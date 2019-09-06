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
        [IgnoreDataMember]
        [JsonIgnore]
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
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public SN76496Timbre[] Timbres
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override TimbreBase GetTimbre(int channel)
        {
            var pn = (SevenBitNumber)ProgramNumbers[channel];
            return Timbres[pn];
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
        private class SN76496SoundManager : SoundManagerBase
        {
            private SoundList<SN76496Sound> psgOnSounds = new SoundList<SN76496Sound>(3);

            private SoundList<SN76496Sound> noiseOnSounds = new SoundList<SN76496Sound>(1);

            private SN76496 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public SN76496SoundManager(SN76496 parent) : base(parent)
            {
                this.parentModule = parent;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            public override SoundBase SoundOn(NoteOnEvent note)
            {
                int emptySlot = searchEmptySlot(note);
                if (emptySlot < 0)
                    return null;

                var programNumber = (SevenBitNumber)parentModule.ProgramNumbers[note.Channel];
                var timbre = parentModule.Timbres[programNumber];
                SN76496Sound snd = new SN76496Sound(parentModule, this, timbre, note, emptySlot);
                switch (((SN76496Timbre)timbre).SoundType)
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
                snd.KeyOn();

                return snd;
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
                            emptySlot = SearchEmptySlotAndOff(psgOnSounds, note, 3);
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            emptySlot = SearchEmptySlotAndOff(noiseOnSounds, note, 3);
                            break;
                        }
                }
                return emptySlot;
            }


        }


        /// <summary>
        /// 
        /// </summary>
        private class SN76496Sound : SoundBase
        {

            private SN76496 parentModule;

            private SevenBitNumber programNumber;

            private SN76496Timbre timbre;

            private SoundType lastSoundType;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public SN76496Sound(SN76496 parentModule, SN76496SoundManager manager, TimbreBase timbre, NoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.programNumber = (SevenBitNumber)parentModule.ProgramNumbers[noteOnEvent.Channel];
                this.timbre = parentModule.Timbres[programNumber];

                lastSoundType = this.timbre.SoundType;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                UpdateVolume();
                UpdatePitch();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void UpdateVolume()
            {
                switch (lastSoundType)
                {
                    case SoundType.PSG:
                        updatePsgVolume();
                        break;
                    case SoundType.NOISE:
                        updateNoiseVolume();
                        break;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            private void updatePsgVolume()
            {
                byte fv = (byte)((14 - (int)Math.Round(14 * CalcCurrentVolume())) & 0xf);

                Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | Slot << 5 | 0x10 | fv));
            }

            /// <summary>
            /// 
            /// </summary>
            private void updateNoiseVolume()
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
            public override void UpdatePitch()
            {
                switch (lastSoundType)
                {
                    case SoundType.PSG:
                        updatePsgPitch();
                        break;
                    case SoundType.NOISE:
                        updateNoisePitch();
                        break;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            private void updatePsgPitch()
            {
                double freq = CalcCurrentFrequency();
                freq = Math.Round(3579545 / (freq * 32));
                if (freq > 0x3ff)
                    freq = 0x3ff;
                var n = (ushort)freq;
                Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | Slot << 5 | n & 0xf));
                Sn76496WriteData(parentModule.UnitNumber, (byte)((n >> 4) & 0x3f));
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            private void updateNoisePitch()
            {
                int v = NoteOnEvent.NoteNumber % 3;

                Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | (Slot + 3) << 5 | timbre.FB << 2 | v));
            }


            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

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