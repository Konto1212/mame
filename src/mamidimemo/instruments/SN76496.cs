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

//http://www.smspower.org/Development/SN76489
//http://www.st.rim.or.jp/~nkomatsu/peripheral/SN76489.html

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class SN76496 : InstrumentBase
    {

        public override string Name => "SN76496";

        public override string Group => "PSG";

        public override InstrumentType InstrumentType => InstrumentType.SN76496;

        [Browsable(false)]
        public override string ImageKey => "SN76496";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "sn76496_";

        /// <summary>
        /// 
        /// </summary>
        [Category("MIDI")]
        [Description("MIDI Device ID")]
        public override uint DeviceID
        {
            get
            {
                return 3;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Timbres (0-127)")]
        [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(CustomCollectionConverter))]
        public SN76496Timbre[] Timbres
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializeData"></param>
        public override void RestoreFrom(string serializeData)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<SN76496>(serializeData);
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
        private delegate void delegate_sn76496_write(uint unitNumber, byte data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_sn76496_write Sn76496_write
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static void Sn76496WriteData(uint unitNumber, byte data)
        {
            try
            {
                Program.SoundUpdating();
                Sn76496_write(unitNumber, data);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static SN76496()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("sn76496_write");
            if (funcPtr != IntPtr.Zero)
            {
                Sn76496_write = (delegate_sn76496_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_sn76496_write));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            soundManager?.Dispose();
            base.Dispose();
        }

        private SN76496SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public SN76496(uint unitNumber) : base(unitNumber)
        {
            Timbres = new SN76496Timbre[128];
            for (int i = 0; i < 128; i++)
                Timbres[i] = new SN76496Timbre();
            setPresetInstruments();

            this.soundManager = new SN76496SoundManager(this);
        }

        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {
            Timbres[0].SoundType = SoundType.PSG;
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
        private class SN76496SoundManager : SoundManagerBase
        {
            private List<SN76496Sound> psgOnSounds = new List<SN76496Sound>();

            private List<SN76496Sound> noiseOnSounds = new List<SN76496Sound>();

            private SN76496 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public SN76496SoundManager(SN76496 parent)
            {
                this.parentModule = parent;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="midiEvent"></param>
            public override void PitchBend(PitchBendEvent midiEvent)
            {
                foreach (SN76496Sound t in AllOnSounds)
                {
                    if (t.NoteOnEvent.Channel == midiEvent.Channel)
                    {
                        switch (t.Timbre.SoundType)
                        {
                            case SoundType.PSG:
                                t.UpdatePsgPitch();
                                break;
                            case SoundType.NOISE:
                                t.UpdateNoisePitch();
                                break;
                        }
                    }
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
                        foreach (SN76496Sound t in AllOnSounds)
                        {
                            if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            {
                                switch (t.Timbre.SoundType)
                                {
                                    case SoundType.PSG:
                                        t.UpdatePsgVolume();
                                        break;
                                    case SoundType.NOISE:
                                        t.UpdateNoiseVolume();
                                        break;
                                }
                            }
                        }
                        break;
                    case 10:    //Panpot
                        break;
                    case 11:    //Expression
                        foreach (SN76496Sound t in AllOnSounds)
                        {
                            if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            {
                                switch (t.Timbre.SoundType)
                                {
                                    case SoundType.PSG:
                                        t.UpdatePsgVolume();
                                        break;
                                    case SoundType.NOISE:
                                        t.UpdateNoiseVolume();
                                        break;
                                }
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

                SN76496Sound snd = new SN76496Sound(parentModule, note, emptySlot);
                AllOnSounds.Add(snd);
                switch (snd.Timbre.SoundType)
                {
                    case SoundType.PSG:
                        psgOnSounds.Add(snd);
                        FormMain.OutputDebugLog("KeyOn PSG ch" + emptySlot + " " + note.ToString());
                        break;
                    case SoundType.NOISE:
                        noiseOnSounds.Add(snd);
                        FormMain.OutputDebugLog("KeyOn NOISE ch" + emptySlot + " " + note.ToString());
                        break;
                }
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
                switch (timbre.SoundType)
                {
                    case SoundType.PSG:
                        {
                            emptySlot = SearchEmptySlot(psgOnSounds.ToList<SoundBase>(), 3);
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            emptySlot = SearchEmptySlot(noiseOnSounds.ToList<SoundBase>(), 1);
                            break;
                        }
                }
                return emptySlot;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            public override void NoteOff(NoteOffEvent note)
            {
                SN76496Sound removed = (SN76496Sound)SearchAndRemoveOnSound(note, AllOnSounds);

                if (removed != null)
                {
                    for (int i = 0; i < psgOnSounds.Count; i++)
                    {
                        if (psgOnSounds[i] == removed)
                        {
                            FormMain.OutputDebugLog("KeyOff PSG ch" + removed.Slot + " " + note.ToString());
                            psgOnSounds.RemoveAt(i);
                            return;
                        }
                    }
                    for (int i = 0; i < noiseOnSounds.Count; i++)
                    {
                        if (noiseOnSounds[i] == removed)
                        {
                            noiseOnSounds.RemoveAt(i);
                            return;
                        }
                    }
                }
            }


        }


        /// <summary>
        /// 
        /// </summary>
        private class SN76496Sound : SoundBase
        {

            private SN76496 parentModule;

            private SevenBitNumber programNumber;

            public SN76496Timbre Timbre;

            private SoundType lastSoundType;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public SN76496Sound(SN76496 parentModule, NoteOnEvent noteOnEvent, int slot) : base(noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.programNumber = (SevenBitNumber)parentModule.ProgramNumbers[noteOnEvent.Channel];
                this.Timbre = parentModule.Timbres[programNumber];

                lastSoundType = Timbre.SoundType;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void On()
            {
                switch (Timbre.SoundType)
                {
                    case SoundType.PSG:
                        {
                            //Freq
                            UpdatePsgPitch();
                            //Volume
                            UpdatePsgVolume();
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            //Freq
                            UpdateNoisePitch();
                            //Volume
                            UpdateNoiseVolume();
                            break;
                        }
                }
            }


            /// <summary>
            /// 
            /// </summary>
            public void UpdatePsgVolume()
            {
                var exp = parentModule.Expressions[NoteOnEvent.Channel] / 127d;
                var vol = parentModule.Volumes[NoteOnEvent.Channel] / 127d;
                var vel = NoteOnEvent.Velocity / 127d;

                byte fv = (byte)((14 - (int)Math.Round(14 * vol * vel * exp)) & 0xf);

                Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | Slot << 5 | 0x10 | fv));
            }

            /// <summary>
            /// 
            /// </summary>
            public void UpdateNoiseVolume()
            {
                var exp = parentModule.Expressions[NoteOnEvent.Channel] / 127d;
                var vol = parentModule.Volumes[NoteOnEvent.Channel] / 127d;
                var vel = NoteOnEvent.Velocity / 127d;

                byte fv = (byte)((14 - (int)Math.Round(14 * vol * vel * exp)) & 0xf);

                Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | (Slot + 3) << 5 | 0x10 | fv));
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public void UpdatePsgPitch()
            {
                var pitch = (int)parentModule.Pitchs[NoteOnEvent.Channel] - 8192;
                var range = (int)parentModule.PitchBendRanges[NoteOnEvent.Channel];

                int noteNum = NoteOnEvent.NoteNumber;
                double freq = 440.0 * Math.Pow(2.0, (NoteOnEvent.NoteNumber - 69.0) / 12.0);
                ushort n = 0;
                if (pitch > 0)
                {
                    var nfreq = 440.0 * Math.Pow(2.0, (NoteOnEvent.NoteNumber + range - 69.0) / 12.0);
                    var dfreq = (nfreq - freq) * ((double)pitch / (double)8192);
                    freq = (ushort)Math.Round(freq + dfreq);
                }
                else if (pitch < 0)
                {
                    var nfreq = 440.0 * Math.Pow(2.0, (NoteOnEvent.NoteNumber - range - 69.0) / 12.0);
                    var dfreq = (nfreq - freq) * ((double)-pitch / (double)8192);
                    freq = (ushort)Math.Round(freq + dfreq);
                }
                n = (ushort)((ushort)Math.Round(3579545 / (freq * 32)) & 0x3ff);
                Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | Slot << 5 | n & 0xf));
                Sn76496WriteData(parentModule.UnitNumber, (byte)((n >> 4) & 0x3f));
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public void UpdateNoisePitch()
            {
                int v = NoteOnEvent.NoteNumber % 3;

                Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | (Slot + 3) << 5 | Timbre.FB << 2 | v));
            }


            /// <summary>
            /// 
            /// </summary>
            public override void Off()
            {
                switch (lastSoundType)
                {
                    case SoundType.PSG:
                        {
                            Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | Slot << 5 | 0x1f));
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | (Slot + 3) << 5 | 0x1f));
                            break;
                        }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SN76496Timbre>))]
        [DataContract]
        public class SN76496Timbre : TimbreBase
        {
            [DataMember]
            [Category("Sound")]
            [Description("Sound Type")]
            public SoundType SoundType
            {
                get;
                set;
            }

            private byte f_FB;

            [DataMember]
            [Category("Sound")]
            [Description("Feedback (0-1)")]
            public byte FB
            {
                get
                {
                    return f_FB;
                }
                set
                {
                    f_FB = (byte)(value & 1);
                }
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<SN76496Timbre>(serializeData);
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
        public enum SoundType
        {
            PSG,
            NOISE,
        }

    }
}