// copyright-holders:K.Ito
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Smf;
using Newtonsoft.Json;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Mame;
using zanac.MAmidiMEmo.Midi;

//http://d4.princess.ne.jp/msx/datas/OPLL/YM2413AP.html#31
//http://www.smspower.org/maxim/Documents/YM2413ApplicationManual
//http://hp.vector.co.jp/authors/VA054130/yamaha_curse.html

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class YM2413 : InstrumentBase
    {

        public override string Name => "YM2413";

        public override string Group => "FM";

        public override InstrumentType InstrumentType => InstrumentType.YM2413;

        [Browsable(false)]
        public override string ImageKey => "YM2413";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "ym2413_";

        /// <summary>
        /// 
        /// </summary>
        [Category("MIDI")]
        [Description("MIDI Device ID")]
        public override uint DeviceID
        {
            get
            {
                return 9;
            }
        }

        [DataMember]
        [Category("Chip")]
        [Description("Timbres (0-127)")]
        [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(CustomCollectionConverter))]
        public YM2413Timbre[] Timbres
        {
            get;
            private set;
        }

        private byte f_RHY;

        /// <summary>
        /// Rhythm mode
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Rhythm mode (0:Off(9ch) 1:On(6ch))\r\n" +
            "Set DrumSet to ToneType in Timbre to output")]
        public byte RHY
        {
            get
            {
                return f_RHY;
            }
            set
            {
                var v = (byte)(value & 1);
                if (f_RHY != v)
                {
                    f_RHY = v;
                    soundManager.NoteOffAll();
                    YM2413WriteData(UnitNumber, 0x0e, 0, (byte)(RHY << 5));
                    if (RHY == 1)
                    {
                        YM2413WriteData(UnitNumber, (byte)(0x16), 0, 0x20);
                        YM2413WriteData(UnitNumber, (byte)(0x17), 0, 0x50);
                        YM2413WriteData(UnitNumber, (byte)(0x18), 0, 0xc0);
                        YM2413WriteData(UnitNumber, (byte)(0x26), 0, 0x05);
                        YM2413WriteData(UnitNumber, (byte)(0x27), 0, 0x05);
                        YM2413WriteData(UnitNumber, (byte)(0x28), 0, 0x01);
                    }
                }
            }
        }

        /// <summary>
        /// FrequencyCalculationMode
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Select Frequency Calculation Mode (False:3.6MHz mode(Not accurate) True:3.579545MHz mode(More accurate)")]
        public bool FrequencyCalculationMode
        {
            get;
            set;
        }


        private byte lastDrumKeyOn;
        private byte lastDrumVolume37;
        private byte lastDrumVolume38;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializeData"></param>
        public override void RestoreFrom(string serializeData)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<YM2413>(serializeData);
                this.InjectFrom(new LoopInjection(new[] { "SerializeData" }), obj);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;


                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_YM2413_write(uint unitNumber, uint address, byte data);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte delegate_YM2413_read(uint unitNumber, uint address);

        /// <summary>
        /// 
        /// </summary>
        private static delegate_YM2413_write YM2413_write
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_YM2413_read YM2413_read
        {
            get;
            set;
        }

        private static byte[] addressTable = new byte[] { 0x00, 0x01, 0x02, 0x08, 0x09, 0x0a, 0x10, 0x11, 0x12 };

        /// <summary>
        /// 
        /// </summary>
        private static void YM2413WriteData(uint unitNumber, byte address, int slot, byte data)
        {
            try
            {
                Program.SoundUpdating();
                YM2413_write(unitNumber, 0, (byte)(address + slot));
                YM2413_write(unitNumber, 1, data);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static YM2413()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("ym2413_write");
            if (funcPtr != IntPtr.Zero)
                YM2413_write = (delegate_YM2413_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_YM2413_write));
            /*
            funcPtr = MameIF.GetProcAddress("ym2413_read");
            if (funcPtr != IntPtr.Zero)
                YM2413_read = (delegate_YM2413_read)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_YM2413_read));*/
        }

        private YM2413SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public YM2413(uint unitNumber) : base(unitNumber)
        {
            GainLeft = 3.0f;
            GainRight = 3.0f;

            Timbres = new YM2413Timbre[128];
            for (int i = 0; i < 128; i++)
                Timbres[i] = new YM2413Timbre();
            setPresetInstruments();

            this.soundManager = new YM2413SoundManager(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            soundManager?.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {
            Timbres[0].Career.AR = 15;
            Timbres[0].Career.DIST = 1;
            Timbres[0].Modulator.AR = 14;
            Timbres[0].Modulator.VIB = 1;
        }

        internal override void PrepareSound()
        {
            base.PrepareSound();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnNoteOnEvent(NoteOnEvent midiEvent)
        {
            soundManager.NoteOn(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnNoteOffEvent(NoteOffEvent midiEvent)
        {
            soundManager.NoteOff(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnControlChangeEvent(ControlChangeEvent midiEvent)
        {
            base.OnControlChangeEvent(midiEvent);

            soundManager.ControlChange(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnPitchBendEvent(PitchBendEvent midiEvent)
        {
            base.OnPitchBendEvent(midiEvent);

            soundManager.PitchBend(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        private class YM2413SoundManager : SoundManagerBase
        {
            private List<YM2413Sound> fmOnSounds = new List<YM2413Sound>();

            private List<YM2413Sound> rhyOnSounds = new List<YM2413Sound>();

            private YM2413 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public YM2413SoundManager(YM2413 parent)
            {
                this.parentModule = parent;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="midiEvent"></param>
            public override void PitchBend(PitchBendEvent midiEvent)
            {
                foreach (YM2413Sound t in fmOnSounds)
                {
                    if (t.NoteOnEvent.Channel == midiEvent.Channel)
                        t.UpdateFmPitch();
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="channel"></param>
            /// <param name="value"></param>
            public override void ControlChange(ControlChangeEvent midiEvent)
            {
                switch (midiEvent.ControlNumber)
                {
                    case 6:    //Data Entry
                        //nothing
                        break;
                    case 7:    //Volume
                        foreach (YM2413Sound t in AllOnSounds)
                        {
                            if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            {
                                t.UpdateFmVolume();
                            }
                        }
                        break;
                    case 10:    //Panpot
                        break;
                    case 11:    //Expression
                        foreach (YM2413Sound t in AllOnSounds)
                        {
                            if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            {
                                t.UpdateFmVolume();
                            }
                        }
                        break;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            public override void NoteOn(NoteOnEvent note)
            {
                int emptySlot = searchEmptySlot(note);
                if (emptySlot < 0)
                    return;
                var pn = parentModule.ProgramNumbers[note.Channel];
                var timbre = parentModule.Timbres[pn];

                YM2413Sound snd = new YM2413Sound(parentModule, note, emptySlot);
                AllOnSounds.Add(snd);
                if (parentModule.RHY == 0)
                {
                    fmOnSounds.Add(snd);
                }
                else
                {
                    if (timbre.ToneType != ToneType.DrumSet)
                        fmOnSounds.Add(snd);
                    else
                        rhyOnSounds.Add(snd);
                }
                FormMain.OutputDebugLog("KeyOn FM ch" + emptySlot + " " + note.ToString());
                snd.On();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            private int searchEmptySlot(NoteOnEvent note)
            {
                int emptySlot = -1;
                var pn = parentModule.ProgramNumbers[note.Channel];
                var timbre = parentModule.Timbres[pn];

                if (parentModule.RHY == 0)
                {
                    emptySlot = SearchEmptySlot(fmOnSounds.ToList<SoundBase>(), 9);
                }
                else
                {
                    if (timbre.ToneType != ToneType.DrumSet)
                        emptySlot = SearchEmptySlot(fmOnSounds.ToList<SoundBase>(), 6);
                    else
                        emptySlot = SearchEmptySlot(rhyOnSounds.ToList<SoundBase>(), 6);
                }
                return emptySlot;
            }

            /// <summary>
            /// 
            /// </summary>
            public void NoteOffAll()
            {
                foreach (var n in AllOnSounds)
                    n.Dispose();
                AllOnSounds.Clear();
                fmOnSounds.Clear();
                rhyOnSounds.Clear();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            public override void NoteOff(NoteOffEvent note)
            {
                YM2413Sound removed = SearchAndRemoveOnSound(note, AllOnSounds) as YM2413Sound;

                if (removed != null)
                {
                    for (int i = 0; i < fmOnSounds.Count; i++)
                    {
                        if (fmOnSounds[i] == removed)
                        {
                            FormMain.OutputDebugLog("KeyOff FM ch" + removed.Slot + " " + note.ToString());
                            fmOnSounds.RemoveAt(i);
                            return;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class YM2413Sound : SoundBase
        {
            private YM2413 parentModule;

            private SevenBitNumber programNumber;

            public YM2413Timbre Timbre;

            private byte lastFreqData;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public YM2413Sound(YM2413 parentModule, NoteOnEvent noteOnEvent, int slot) : base(noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.programNumber = (SevenBitNumber)parentModule.ProgramNumbers[noteOnEvent.Channel];

                this.Timbre = parentModule.Timbres[programNumber];
            }

            /// <summary>
            /// 
            /// </summary>
            public override void On()
            {
                //
                SetFmTimbre();
                //Volume
                UpdateFmVolume();
                //Freq & kon
                UpdateFmPitch();
            }


            /// <summary>
            /// 
            /// </summary>
            public void UpdateFmVolume()
            {
                var exp = parentModule.Expressions[NoteOnEvent.Channel] / 127d;
                var vol = parentModule.Volumes[NoteOnEvent.Channel] / 127d;
                var vel = NoteOnEvent.Velocity / 127d;

                byte tl = (byte)(15 - (byte)Math.Round(15 * vol * vel * exp));
                if (Timbre.ToneType != ToneType.DrumSet)
                {
                    if (Timbre.ToneType != ToneType.DrumSet)
                        YM2413WriteData(parentModule.UnitNumber, 0x30, Slot, (byte)((int)Timbre.ToneType << 4 | tl));
                }
                else if (parentModule.RHY == 1)
                {
                    switch (NoteOnEvent.GetNoteName())
                    {
                        case NoteName.C:    //BD
                            YM2413WriteData(parentModule.UnitNumber, 0x36, 0, tl);
                            break;
                        case NoteName.D:    //SD
                            parentModule.lastDrumVolume37 = (byte)(tl | (parentModule.lastDrumVolume37 & 0xf0));
                            YM2413WriteData(parentModule.UnitNumber, 0x37, 0, parentModule.lastDrumVolume37);
                            break;
                        case NoteName.F:    //TOM
                            parentModule.lastDrumVolume38 = (byte)(tl << 4 | (parentModule.lastDrumVolume38 & 0x0f));
                            YM2413WriteData(parentModule.UnitNumber, 0x38, 0, parentModule.lastDrumVolume38);
                            break;
                        case NoteName.FSharp:    //HH
                            parentModule.lastDrumVolume37 = (byte)(tl << 4 | (parentModule.lastDrumVolume37 & 0x0f));
                            YM2413WriteData(parentModule.UnitNumber, 0x37, 0, parentModule.lastDrumVolume37);
                            break;
                        case NoteName.ASharp:    //Symbal
                            parentModule.lastDrumVolume38 = (byte)(tl | (parentModule.lastDrumVolume38 & 0xf0));
                            YM2413WriteData(parentModule.UnitNumber, 0x38, 0, parentModule.lastDrumVolume38);
                            break;
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public void UpdateFmPitch()
            {
                if (Timbre.ToneType != ToneType.DrumSet)
                {
                    var pitch = (int)parentModule.Pitchs[NoteOnEvent.Channel] - 8192;
                    var range = (int)parentModule.PitchBendRanges[NoteOnEvent.Channel];

                    int noteNum = NoteOnEvent.NoteNumber;
                    if (pitch > 0)
                    {
                        noteNum = NoteOnEvent.NoteNumber + range;
                        if (noteNum > 127)
                            noteNum = 127;
                        var nnOn = new NoteOnEvent((SevenBitNumber)noteNum, (SevenBitNumber)127);

                        double cfreq = (double)convertFmFrequency(NoteOnEvent);
                        double nfreq = (double)convertFmFrequency(nnOn);
                        var doctave = nnOn.GetNoteOctave() - NoteOnEvent.GetNoteOctave() + 1;
                        nfreq = nfreq * (double)doctave;

                        var dfreq = (nfreq - cfreq) * ((double)pitch / (double)8192);
                        var freq = (ushort)Math.Round(cfreq + dfreq);
                        byte octave = (byte)((NoteOnEvent.GetNoteOctave()) << 1);

                        YM2413WriteData(parentModule.UnitNumber, (byte)(0x10 + Slot), 0, (byte)(0xff & freq));
                        //keyon
                        lastFreqData = (byte)(Timbre.SUS << 5 | 0x10 | octave | ((freq >> 8) & 1));
                        YM2413WriteData(parentModule.UnitNumber, (byte)(0x20 + Slot), 0, lastFreqData);
                    }
                    else if (pitch < 0)
                    {
                        noteNum = NoteOnEvent.NoteNumber - range;
                        if (noteNum < 0)
                            noteNum = 0;
                        var nnOn = new NoteOnEvent((SevenBitNumber)noteNum, (SevenBitNumber)127);

                        double cfreq = (double)convertFmFrequency(NoteOnEvent);
                        double nfreq = (double)convertFmFrequency(nnOn);
                        var doctave = NoteOnEvent.GetNoteOctave() - nnOn.GetNoteOctave() + 1;
                        nfreq = nfreq / (double)doctave;

                        var dfreq = (nfreq - cfreq) * ((double)-pitch / (double)8192);
                        var freq = (ushort)Math.Round(cfreq + dfreq);
                        byte octave = (byte)((NoteOnEvent.GetNoteOctave()) << 1);

                        YM2413WriteData(parentModule.UnitNumber, (byte)(0x10 + Slot), 0, (byte)(0xff & freq));
                        //keyon
                        lastFreqData = (byte)(Timbre.SUS << 5 | 0x10 | octave | ((freq >> 8) & 1));
                        YM2413WriteData(parentModule.UnitNumber, (byte)(0x20 + Slot), 0, lastFreqData);
                    }
                    else
                    {
                        var freq = convertFmFrequency(NoteOnEvent);
                        byte octave = (byte)((NoteOnEvent.GetNoteOctave()) << 1);
                        YM2413WriteData(parentModule.UnitNumber, (byte)(0x10 + Slot), 0, (byte)(0xff & freq));
                        //keyon
                        lastFreqData = (byte)(Timbre.SUS << 5 | 0x10 | octave | ((freq >> 8) & 1));
                        YM2413WriteData(parentModule.UnitNumber, (byte)(0x20 + Slot), 0, lastFreqData);
                    }
                }
                else if (parentModule.RHY == 1)
                {
                    byte kon = 0;
                    switch (NoteOnEvent.GetNoteName())
                    {
                        case NoteName.C:    //BD
                            kon = 0x10;
                            break;
                        case NoteName.D:    //SD
                            kon = 0x08;
                            break;
                        case NoteName.F:    //TOM
                            kon = 0x04;
                            break;
                        case NoteName.FSharp:    //HH
                            kon = 0x01;
                            break;
                        case NoteName.ASharp:    //Symbal
                            kon = 0x02;
                            break;
                    }
                    if (kon != 0)
                    {
                        YM2413WriteData(parentModule.UnitNumber, 0xe, 0, (byte)(0x20 | (parentModule.lastDrumKeyOn & ~kon)));  //off
                        parentModule.lastDrumKeyOn |= (byte)kon;
                        YM2413WriteData(parentModule.UnitNumber, 0xe, 0, (byte)(0x20 | parentModule.lastDrumKeyOn));  //on
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public void SetFmTimbre()
            {
                if (Timbre.ToneType != ToneType.Custom)
                    return;

                YM2413Modulator m = Timbre.Modulator;
                YM2413Career c = Timbre.Career;

                //$00+:
                YM2413WriteData(parentModule.UnitNumber, 0x00, 0, (byte)((m.AM << 7 | m.VIB << 6 | m.EG << 5 | m.KSR << 4 | m.MUL)));
                YM2413WriteData(parentModule.UnitNumber, 0x01, 0, (byte)((c.AM << 7 | c.VIB << 6 | c.EG << 5 | c.KSR << 4 | c.MUL)));
                //$02+:
                YM2413WriteData(parentModule.UnitNumber, 0x02, 0, (byte)((m.KSL << 6 | m.TL)));
                YM2413WriteData(parentModule.UnitNumber, 0x03, 0, (byte)((c.KSL << 6 | c.DIST << 4 | m.DIST << 3 | Timbre.FB)));
                //$04+:
                YM2413WriteData(parentModule.UnitNumber, 0x04, 0, (byte)((m.AR << 4 | m.DR)));
                YM2413WriteData(parentModule.UnitNumber, 0x05, 0, (byte)((c.AR << 4 | c.DR)));
                //$06+:
                YM2413WriteData(parentModule.UnitNumber, 0x06, 0, (byte)((m.SR << 4 | m.RR)));
                YM2413WriteData(parentModule.UnitNumber, 0x07, 0, (byte)((c.SR << 4 | c.RR)));
            }

            /// <summary>
            /// 
            /// </summary>
            public override void Off()
            {
                if (Timbre.ToneType != ToneType.DrumSet)
                    YM2413WriteData(parentModule.UnitNumber, (byte)(0x20 + Slot), 0, (byte)(Timbre.SUS << 5 | lastFreqData & 0x0f));
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            /// <param name="freq"></param>
            /// <returns></returns>
            private ushort convertFmFrequency(NoteOnEvent note)
            {
                ushort freq = 0;
                if (parentModule.FrequencyCalculationMode)
                {
                    switch (note.GetNoteName())
                    {
                        case NoteName.C:
                            freq = (ushort)(173);
                            break;
                        case NoteName.CSharp:
                            freq = (ushort)(183);
                            break;
                        case NoteName.D:
                            freq = (ushort)(194);
                            break;
                        case NoteName.DSharp:
                            freq = (ushort)(205);
                            break;
                        case NoteName.E:
                            freq = (ushort)(217);
                            break;
                        case NoteName.F:
                            freq = (ushort)(230);
                            break;
                        case NoteName.FSharp:
                            freq = (ushort)(244);
                            break;
                        case NoteName.G:
                            freq = (ushort)(258);
                            break;
                        case NoteName.GSharp:
                            freq = (ushort)(274);
                            break;
                        case NoteName.A:
                            freq = (ushort)(290);
                            break;
                        case NoteName.ASharp:
                            freq = (ushort)(307);
                            break;
                        case NoteName.B:
                            freq = (ushort)(326);
                            break;
                    }
                }
                else
                {
                    switch (note.GetNoteName())
                    {
                        case NoteName.C:
                            freq = (ushort)(172);
                            break;
                        case NoteName.CSharp:
                            freq = (ushort)(181);
                            break;
                        case NoteName.D:
                            freq = (ushort)(192);
                            break;
                        case NoteName.DSharp:
                            freq = (ushort)(204);
                            break;
                        case NoteName.E:
                            freq = (ushort)(216);
                            break;
                        case NoteName.F:
                            freq = (ushort)(229);
                            break;
                        case NoteName.FSharp:
                            freq = (ushort)(242);
                            break;
                        case NoteName.G:
                            freq = (ushort)(257);
                            break;
                        case NoteName.GSharp:
                            freq = (ushort)(272);
                            break;
                        case NoteName.A:
                            freq = (ushort)(288);
                            break;
                        case NoteName.ASharp:
                            freq = (ushort)(305);
                            break;
                        case NoteName.B:
                            freq = (ushort)(323);
                            break;
                    }
                }

                return freq;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2413Timbre>))]
        [DataContract]
        public class YM2413Timbre : TimbreBase
        {
            #region FM Symth


            private ToneType f_ToneType;

            [DataMember]
            [Category("Sound")]
            [Description("Tone type")]
            public ToneType ToneType
            {
                get
                {
                    return f_ToneType;
                }
                set
                {
                    f_ToneType = value;
                }
            }

            private byte f_FB;

            [DataMember]
            [Category("Sound")]
            [Description("Feedback (0-7)")]
            public byte FB
            {
                get
                {
                    return f_FB;
                }
                set
                {
                    f_FB = (byte)(value & 7);
                }
            }

            private byte f_SUS;

            /// <summary>
            /// Sustain Mode (0:Disalbe 1:Enable)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Sustain Mode (0:Disalbe 1:Enable)")]
            public byte SUS
            {
                get
                {
                    return f_SUS;
                }
                set
                {
                    f_SUS = (byte)(value & 1);
                }
            }

            #endregion


            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Operators")]
            public YM2413Modulator Modulator
            {
                get;
                private set;
            }


            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Operators")]
            public YM2413Career Career
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            public YM2413Timbre()
            {
                Modulator = new YM2413Modulator();
                Career = new YM2413Career();
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<YM2413Timbre>(serializeData);
                    this.InjectFrom(new LoopInjection(new[] { "SerializeData" }), obj);
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(Exception))
                        throw;
                    else if (ex.GetType() == typeof(SystemException))
                        throw;


                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public enum ToneType
        {
            Custom,
            Violin,
            Guitor,
            Piano,
            Flute,
            Clarinet,
            Oboe,
            Trumpet,
            Organ,
            Horn,
            Symthesizer,
            Harpsichord,
            Vibraphone,
            SynthesizerBass,
            AcousticBass,
            ElectricGuitar,
            DrumSet,
        }

        /// <summary>
        /// 
        /// </summary>
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2413Operator>))]
        [DataContract]
        public class YM2413Operator
        {

            private byte f_AM;

            /// <summary>
            /// Apply amplitude modulation(0-1)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Apply amplitude modulation (0:Off 1:On)")]
            public byte AM
            {
                get
                {
                    return f_AM;
                }
                set
                {
                    f_AM = (byte)(value & 1);
                }
            }

            private byte f_VIB;

            /// <summary>
            /// Apply vibrato(0-1)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Vibrato (0:Off 1:On)")]
            public byte VIB
            {
                get
                {
                    return f_VIB;
                }
                set
                {
                    f_VIB = (byte)(value & 1);
                }
            }

            private byte f_EG;

            /// <summary>
            /// EG Type (0-1)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("EG Type (0:the sound begins to decay immediately after hitting the SUSTAIN phase 1:the sustain level of the voice is maintained until released")]
            public byte EG
            {
                get
                {
                    return f_EG;
                }
                set
                {
                    f_EG = (byte)(value & 1);
                }
            }

            private byte f_KSR;

            /// <summary>
            /// Keyboard scaling rate (0-1)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Keyboard scaling rate (1: the sound's envelope is foreshortened as it rises in pitch.")]
            public byte KSR
            {
                get
                {
                    return f_KSR;
                }
                set
                {
                    f_KSR = (byte)(value & 1);
                }
            }


            private byte f_MUL;

            /// <summary>
            /// Multiply (0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Multiply (0-15)")]
            public byte MUL
            {
                get
                {
                    return f_MUL;
                }
                set
                {
                    f_MUL = (byte)(value & 15);
                }
            }

            private byte f_KSL;

            /// <summary>
            /// Key Scaling Level(0-3)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Key Scaling Level (00:No Change 10:1.5dB/8ve 01:3dB/8ve 11:6dB/8ve)")]
            public byte KSL
            {
                get
                {
                    return f_KSL;
                }
                set
                {
                    f_KSL = (byte)(value & 3);
                }
            }


            private byte f_DIST;

            /// <summary>
            /// Distortion (0:Off 1:On)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Distortion (0:Off 1:On)")]
            public byte DIST
            {
                get
                {
                    return f_DIST;
                }
                set
                {
                    f_DIST = (byte)(value & 1);
                }
            }

            private byte f_AR;

            /// <summary>
            /// Attack Rate (0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Attack Rate (0-15)")]
            public byte AR
            {
                get
                {
                    return f_AR;
                }
                set
                {
                    f_AR = (byte)(value & 15);
                }
            }

            private byte f_DR;

            /// <summary>
            /// Decay Rate (0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Decay Rate (0-15)")]
            public byte DR
            {
                get
                {
                    return f_DR;
                }
                set
                {
                    f_DR = (byte)(value & 15);
                }
            }

            private byte f_SR;

            /// <summary>
            /// Sustain Rate (0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Sustain Level (0-15)")]
            public byte SR
            {
                get
                {
                    return f_SR;
                }
                set
                {
                    f_SR = (byte)(value & 15);
                }
            }

            private byte f_RR;

            /// <summary>
            /// release rate(0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Release Rate (0-15)")]
            public byte RR
            {
                get
                {
                    return f_RR;
                }
                set
                {
                    f_RR = (byte)(value & 15);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2413Modulator>))]
        [DataContract]
        public class YM2413Modulator : YM2413Operator
        {

            private byte f_TL;

            /// <summary>
            /// Total Level (0-63)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Total Level (0-63)")]
            public byte TL
            {
                get
                {
                    return f_TL;
                }
                set
                {
                    f_TL = (byte)(value & 63);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2413Career>))]
        [DataContract]
        public class YM2413Career : YM2413Operator
        {


        }
    }


}