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
        [TypeConverter(typeof(ExpandableCollectionConverter))]
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

                    OnControlChangeEvent(new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0));

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
        [Description("Select Frequency Accuracy Mode (False:3.6MHz mode(Not accurate) True:3.579545MHz mode(Accurate)")]
        public bool FrequencyAccuracyMode
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
            soundManager.KeyOn(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnNoteOffEvent(NoteOffEvent midiEvent)
        {
            soundManager.KeyOff(midiEvent);
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
            private List<YM2413Sound> fmOnSounds = new List<YM2413Sound>(9);

            private List<YM2413Sound> drumOnSounds = new List<YM2413Sound>(9);

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
            /// <param name="note"></param>
            public override void KeyOn(NoteOnEvent note)
            {
                int emptySlot = searchEmptySlot(note);
                if (emptySlot < 0)
                    return;

                var pn = parentModule.ProgramNumbers[note.Channel];
                var timbre = parentModule.Timbres[pn];
                YM2413Sound snd = new YM2413Sound(parentModule, this, timbre, note, emptySlot);
                if (parentModule.RHY == 0)
                {
                    fmOnSounds.Add(snd);
                }
                else
                {
                    if (timbre.ToneType != ToneType.DrumSet)
                        fmOnSounds.Add(snd);
                }
                FormMain.OutputDebugLog("KeyOn FM ch" + emptySlot + " " + note.ToString());
                snd.KeyOn();

                base.KeyOn(note);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            private int searchEmptySlot(NoteOnEvent note)
            {
                int emptySlot = -1;
                if (parentModule.RHY == 0)
                {
                    emptySlot = SearchEmptySlotAndOff(fmOnSounds, note, 9);
                }
                else
                {
                    var pn = parentModule.ProgramNumbers[note.Channel];
                    var timbre = parentModule.Timbres[pn];
                    if (timbre.ToneType != ToneType.DrumSet)
                        emptySlot = SearchEmptySlotAndOff(fmOnSounds, note, 6);
                    else
                        emptySlot = SearchEmptySlotAndOff(drumOnSounds, note, 6);
                }
                return emptySlot;
            }

        }


        /// <summary>
        /// 
        /// </summary>
        private class YM2413Sound : SoundBase
        {
            private YM2413 parentModule;

            private SevenBitNumber programNumber;

            private YM2413Timbre timbre;

            private byte lastFreqData;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public YM2413Sound(YM2413 parentModule, YM2413SoundManager manager, TimbreBase timbre, NoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.programNumber = (SevenBitNumber)parentModule.ProgramNumbers[noteOnEvent.Channel];

                this.timbre = parentModule.Timbres[programNumber];
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                //
                SetTimbre();
                //Volume
                UpdateVolume();
                //Freq & kon
                UpdatePitch();
            }


            /// <summary>
            /// 
            /// </summary>
            public override void UpdateVolume()
            {
                byte tl = (byte)(15 - (byte)Math.Round(15 * CalcCurrentVolume()));
                if (timbre.ToneType != ToneType.DrumSet)
                {
                    if (timbre.ToneType != ToneType.DrumSet)
                        YM2413WriteData(parentModule.UnitNumber, 0x30, Slot, (byte)((int)timbre.ToneType << 4 | tl));
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
            public override void UpdatePitch()
            {
                if (timbre.ToneType != ToneType.DrumSet)
                {
                    double d = CalcCurrentPitch();

                    int noteNum = NoteOnEvent.NoteNumber + (int)d;
                    if (noteNum > 127)
                        noteNum = 127;
                    else if (noteNum < 0)
                        noteNum = 0;
                    var nnOn = new NoteOnEvent((SevenBitNumber)noteNum, (SevenBitNumber)127);
                    ushort freq = convertFmFrequency(nnOn);
                    byte octave = (byte)(nnOn.GetNoteOctave() << 1);

                    if (d != 0)
                        freq += (ushort)(((double)(convertFmFrequency(nnOn, (d < 0) ? false : true) - freq)) * Math.Abs(d - Math.Truncate(d)));

                    //keyon
                    byte kon = IsKeyOff ? (byte)0: (byte)0x10;
                    lastFreqData = (byte)(timbre.SUS << 5 | kon | octave | ((freq >> 8) & 1));

                    Program.SoundUpdating();
                    YM2413WriteData(parentModule.UnitNumber, (byte)(0x10 + Slot), 0, (byte)(0xff & freq));
                    YM2413WriteData(parentModule.UnitNumber, (byte)(0x20 + Slot), 0, lastFreqData);
                    Program.SoundUpdated();
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
                        Program.SoundUpdating();
                        YM2413WriteData(parentModule.UnitNumber, 0xe, 0, (byte)(0x20 | (parentModule.lastDrumKeyOn & ~kon)));  //off
                        parentModule.lastDrumKeyOn |= (byte)kon;
                        YM2413WriteData(parentModule.UnitNumber, 0xe, 0, (byte)(0x20 | parentModule.lastDrumKeyOn));  //on
                        Program.SoundUpdated();
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public void SetTimbre()
            {
                if (timbre.ToneType != ToneType.Custom)
                    return;

                YM2413Modulator m = timbre.Modulator;
                YM2413Career c = timbre.Career;

                //$00+:
                YM2413WriteData(parentModule.UnitNumber, 0x00, 0, (byte)((m.AM << 7 | m.VIB << 6 | m.EG << 5 | m.KSR << 4 | m.MUL)));
                YM2413WriteData(parentModule.UnitNumber, 0x01, 0, (byte)((c.AM << 7 | c.VIB << 6 | c.EG << 5 | c.KSR << 4 | c.MUL)));
                //$02+:
                YM2413WriteData(parentModule.UnitNumber, 0x02, 0, (byte)((m.KSL << 6 | m.TL)));
                YM2413WriteData(parentModule.UnitNumber, 0x03, 0, (byte)((c.KSL << 6 | c.DIST << 4 | m.DIST << 3 | timbre.FB)));
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
            public override void SoundOff()
            {
                base.SoundOff();

                if (timbre.ToneType != ToneType.DrumSet)
                    YM2413WriteData(parentModule.UnitNumber, (byte)(0x20 + Slot), 0, (byte)(timbre.SUS << 5 | lastFreqData & 0x0f));
            }


            private ushort[] freqTable1 = new ushort[] {
                326/2,
                173,
                183,
                194,
                205,
                217,
                230,
                244,
                258,
                274,
                290,
                307,
                326,
                173*2,
            };

            private ushort[] freqTable2 = new ushort[] {
                323/2,
                172,
                181,
                192,
                204,
                216,
                229,
                242,
                257,
                272,
                288,
                305,
                323,
                172*2,
            };

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            /// <param name="freq"></param>
            /// <returns></returns>
            private ushort convertFmFrequency(NoteOnEvent note)
            {
                if (parentModule.FrequencyAccuracyMode)
                    return freqTable1[(int)note.GetNoteName() + 1];
                else
                    return freqTable2[(int)note.GetNoteName() + 1];
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            /// <param name="freq"></param>
            /// <returns></returns>
            private ushort convertFmFrequency(NoteOnEvent note, bool plus)
            {
                if (plus)
                {
                    if (parentModule.FrequencyAccuracyMode)
                        return freqTable1[(int)note.GetNoteName() + 2];
                    else
                        return freqTable2[(int)note.GetNoteName() + 2];
                }
                else
                {
                    if (parentModule.FrequencyAccuracyMode)
                        return freqTable1[(int)note.GetNoteName()];
                    else
                        return freqTable2[(int)note.GetNoteName()];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
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
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2413Career>))]
        [DataContract]
        public class YM2413Career : YM2413Operator
        {


        }
    }


}