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

//https://www.waitingforfriday.com/?p=661#6581_SID_Block_Diagram

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class MOS8580 : InstrumentBase
    {

        public override string Name => "MOS8580";

        public override string Group => "PSG";

        public override InstrumentType InstrumentType => InstrumentType.MOS8580;

        [Browsable(false)]
        public override string ImageKey => "MOS8580";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "mos8580_";

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
                return 12;
            }
        }

        private byte f_RES;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Resonance (0-15)")]
        public byte RES
        {
            get => f_RES;
            set
            {
                if (f_RES != value)
                {
                    f_RES = (byte)(value & 15);
                    Mos8580WriteData(UnitNumber, 23, (byte)(f_RES << 4 | (int)PassThrough));
                }
            }
        }

        private ushort f_FC;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Cutoff (or Center) Frequency (0-2047)(30Hz - 10KHz)")]
        public ushort FC
        {
            get => f_FC;
            set
            {
                if (f_FC != value)
                {
                    f_FC = (ushort)(value & 2047);
                    Program.SoundUpdating();
                    Mos8580WriteData(UnitNumber, 21, (byte)(f_FC & 0x7));
                    Mos8580WriteData(UnitNumber, 22, (byte)(f_FC >> 3));
                    Program.SoundUpdated();
                }
            }
        }


        private PassThroughFilter f_PassThrough;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [DefaultValue(PassThroughFilter.None)]
        [TypeConverter(typeof(FlagsEnumConverter))]
        [Description("Pass Through Filter Ch")]
        public PassThroughFilter PassThrough
        {
            get => f_PassThrough;
            set
            {
                if (f_PassThrough != value)
                {
                    f_PassThrough = value;
                    Mos8580WriteData(UnitNumber, 23, (byte)(RES << 4 | (int)PassThrough));
                }
            }
        }

        private FilterTypes f_FILT;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [DefaultValue(FilterTypes.None)]
        [Description("Filter Type")]
        [TypeConverter(typeof(FlagsEnumConverter))]
        public FilterTypes FILT
        {
            get => f_FILT;
            set
            {
                if (f_FILT != value)
                {
                    f_FILT = value;
                    Mos8580WriteData(UnitNumber, 24, (byte)((int)f_FILT << 4 | (int)Volume));
                }
            }
        }

        private byte f_Volume;

        [IgnoreDataMember]
        [JsonIgnore]
        [Browsable(false)]
        public byte Volume
        {
            get => f_Volume;
            set
            {
                if (f_Volume != value)
                {
                    f_Volume = value;
                    Mos8580WriteData(UnitNumber, 24, (byte)((int)FILT << 4 | (int)f_Volume));
                }
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
        public MOS8580Timbre[] Timbres
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
                var obj = JsonConvert.DeserializeObject<MOS8580>(serializeData);
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
        private delegate void delegate_mos8580_write(uint unitNumber, int address, byte data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_mos8580_write Mos8580_write
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static void Mos8580WriteData(uint unitNumber, int address, byte data)
        {
            try
            {
                Program.SoundUpdating();
                Mos8580_write(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static MOS8580()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("mos8580_write");
            if (funcPtr != IntPtr.Zero)
            {
                Mos8580_write = (delegate_mos8580_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_mos8580_write));
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

        private MOS8580SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public MOS8580(uint unitNumber) : base(unitNumber)
        {
            Timbres = new MOS8580Timbre[128];
            for (int i = 0; i < 128; i++)
                Timbres[i] = new MOS8580Timbre();
            setPresetInstruments();

            this.soundManager = new MOS8580SoundManager(this);
        }

        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {

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
        private class MOS8580SoundManager : SoundManagerBase
        {
            private SoundList<MOS8580Sound> psgOnSounds = new SoundList<MOS8580Sound>(3);

            private MOS8580 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public MOS8580SoundManager(MOS8580 parent) : base(parent)
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
                MOS8580Sound snd = new MOS8580Sound(parentModule, this, timbre, note, emptySlot);
                psgOnSounds.Add(snd);
                FormMain.OutputDebugLog("KeyOn PSG ch" + emptySlot + " " + note.ToString());
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
                switch (timbre.PhysicalChannel)
                {
                    case PhysicalChannel.Indeterminatene:
                        {
                            emptySlot = SearchEmptySlotAndOff(psgOnSounds, note, 3);
                            break;
                        }
                    case PhysicalChannel.Ch1:
                    case PhysicalChannel.Ch2:
                    case PhysicalChannel.Ch3:
                        {
                            emptySlot = SearchEmptySlotAndOff(psgOnSounds, note, 1, (int)timbre.PhysicalChannel - 1);
                            break;
                        }
                }
                return emptySlot;
            }

        }


        /// <summary>
        /// 
        /// </summary>
        private class MOS8580Sound : SoundBase
        {

            private MOS8580 parentModule;

            private SevenBitNumber programNumber;

            private MOS8580Timbre timbre;

            private Waveforms lastWaveform;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public MOS8580Sound(MOS8580 parentModule, MOS8580SoundManager manager, TimbreBase timbre, NoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.programNumber = (SevenBitNumber)parentModule.ProgramNumbers[noteOnEvent.Channel];
                this.timbre = parentModule.Timbres[programNumber];

                lastWaveform = this.timbre.Waveform;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                var gs = timbre.GlobalSettings;
                if (gs.Enable)
                {
                    Program.SoundUpdating();
                    parentModule.FC = gs.FC;
                    parentModule.RES = gs.RES;
                    parentModule.PassThrough = gs.PassThrough;
                    Program.SoundUpdated();
                }

                SetTimbre();
                UpdateVolume();
                UpdatePitch();
            }


            /// <summary>
            /// 
            /// </summary>
            public void SetTimbre()
            {
                var pn = parentModule.ProgramNumbers[NoteOnEvent.Channel];
                var timbre = parentModule.Timbres[pn];

                Mos8580WriteData(parentModule.UnitNumber, Slot * 7 + 2, (byte)(timbre.PW & 0xff));
                Mos8580WriteData(parentModule.UnitNumber, Slot * 7 + 3, (byte)(timbre.PW >> 8));
                byte data = (byte)((int)timbre.Waveform << 4 | timbre.RING << 2 | timbre.SYNC << 1 | 1);
                Mos8580WriteData(parentModule.UnitNumber, Slot * 7 + 4, data);
                Mos8580WriteData(parentModule.UnitNumber, Slot * 7 + 5, (byte)(timbre.ATK << 4 | timbre.DCY));
                Mos8580WriteData(parentModule.UnitNumber, Slot * 7 + 6, (byte)(timbre.STN << 4 | timbre.RIS));
            }


            /// <summary>
            /// 
            /// </summary>
            public override void UpdateVolume()
            {
                double v = 1;
                v *= ParentModule.Expressions[NoteOnEvent.Channel] / 127d;
                v *= ParentModule.Volumes[NoteOnEvent.Channel] / 127d;
                parentModule.Volume = (byte)Math.Round(15 * v);
            }

            /// <summary>
            /// 
            /// </summary>
            public override void UpdatePitch()
            {
                double freq = CalcCurrentFrequency();
                int f = (int)Math.Round(16777216d * freq / (14318181d / 14d));
                if (f > 0xffff)
                    f = 0xffff;

                Program.SoundUpdating();
                Mos8580WriteData(parentModule.UnitNumber, Slot * 7 + 1, (byte)(f >> 8));
                Mos8580WriteData(parentModule.UnitNumber, Slot * 7 + 0, (byte)(f & 0xff));
                Program.SoundUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                byte data = (byte)((int)timbre.Waveform << 4 | timbre.RING << 2 | timbre.SYNC << 1 | 0);
                Mos8580WriteData(parentModule.UnitNumber, Slot * 7 + 4, data);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<MOS8580Timbre>))]
        [DataContract]
        public class MOS8580Timbre : TimbreBase
        {
            [DataMember]
            [Category("Sound")]
            [Description("Physical Channel")]
            public PhysicalChannel PhysicalChannel
            {
                get;
                set;
            }

            private Waveforms f_Waveform = Waveforms.Pulse;

            [DataMember]
            [Category("Sound")]
            [Description("Sound Type")]
            [DefaultValue(Waveforms.Pulse)]
            [TypeConverter(typeof(FlagsEnumConverter))]
            public Waveforms Waveform
            {
                get
                {
                    return f_Waveform;
                }
                set
                {
                    var f = value;
                    if ((f & Waveforms.Noise) == Waveforms.Noise)
                        f = Waveforms.Noise;

                    if (f_Waveform != f)
                        f_Waveform = f;
                }
            }

            private byte f_ATK;

            [DataMember]
            [Category("Sound")]
            [Description("Attack (0-15)")]
            public byte ATK
            {
                get
                {
                    return f_ATK;
                }
                set
                {
                    f_ATK = (byte)(value & 15);
                }
            }


            private byte f_DCY = 15;

            [DataMember]
            [Category("Sound")]
            [DefaultValue((byte)15)]
            [Description("Decay (0-15)")]
            public byte DCY
            {
                get
                {
                    return f_DCY;
                }
                set
                {
                    f_DCY = (byte)(value & 15);
                }
            }

            private byte f_STN = 15;

            [DataMember]
            [Category("Sound")]
            [DefaultValue((byte)15)]
            [Description("Sustain (0-15)")]
            public byte STN
            {
                get
                {
                    return f_STN;
                }
                set
                {
                    f_STN = (byte)(value & 15);
                }
            }


            private byte f_RIS;

            [DataMember]
            [Category("Sound")]
            [Description("Release Rate (0-15)")]
            public byte RIS
            {
                get
                {
                    return f_RIS;
                }
                set
                {
                    f_RIS = (byte)(value & 15);
                }
            }


            private ushort f_PW = 2047;

            [DataMember]
            [Category("Sound")]
            [DefaultValue((ushort)2047)]
            [Description("Pulse Width (0-4095)(0% - 100%)")]
            public ushort PW
            {
                get
                {
                    return f_PW;
                }
                set
                {
                    f_PW = (ushort)(value & 4095);
                }
            }

            private byte f_RING;

            [DataMember]
            [Category("Sound")]
            [Description("Ring Modulation (0-1)")]
            public byte RING
            {
                get
                {
                    return f_RING;
                }
                set
                {
                    f_RING = (byte)(value & 1);
                }
            }

            private byte f_SYNC;

            [DataMember]
            [Category("Sound")]
            [Description("Synchronize Oscillator (0-1)")]
            public byte SYNC
            {
                get
                {
                    return f_SYNC;
                }
                set
                {
                    f_SYNC = (byte)(value & 1);
                }
            }

            [DataMember]
            [Category("Chip")]
            [Description("Global Settings")]
            public GlobalSettings GlobalSettings
            {
                get;
                set;
            }

            /// <summary>
            /// 
            /// </summary>
            public MOS8580Timbre()
            {
                GlobalSettings = new GlobalSettings();
                this.SDS.FxS = new BasicFxSettings();
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<MOS8580Timbre>(serializeData);
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


        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<GlobalSettings>))]
        [DataContract]
        [MidiHook]
        public class GlobalSettings : ContextBoundObject
        {

            [DataMember]
            [Category("Chip")]
            [Description("Apply parameters as global settings")]
            public bool Enable
            {
                get;
                set;
            }

            private byte f_RES;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [Description("Resonance (0-15)")]
            public byte RES
            {
                get => f_RES;
                set
                {
                    if (f_RES != value)
                        f_RES = (byte)(value & 15);
                }
            }

            private ushort f_FC;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [Description("Cutoff (or Center) Frequency (0-2047)(30Hz - 10KHz)")]
            public ushort FC
            {
                get => f_FC;
                set
                {
                    if (f_FC != value)
                        f_FC = (ushort)(value & 2047);
                }
            }


            private PassThroughFilter f_PassThrough;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [DefaultValue(PassThroughFilter.None)]
            [Description("Pass Through Filter Ch")]
            public PassThroughFilter PassThrough
            {
                get => f_PassThrough;
                set
                {
                    if (f_PassThrough != value)
                        f_PassThrough = value;
                }
            }


        }

        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum Waveforms
        {
            None = 0,
            Triangle = 1,
            Saw = 2,
            Pulse = 4,
            Noise = 8,
        }

        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum FilterTypes
        {
            None = 0,
            LowPass = 1,
            BandPass = 2,
            HighPass = 4,
        }


        /// <summary>
        /// 
        /// </summary>
        public enum PassThroughFilter
        {
            None = 0,
            Ch1 = 1,
            Ch2 = 2,
            Ch3 = 4,
        }

        /// <summary>
        /// 
        /// </summary>
        public enum PhysicalChannel
        {
            Indeterminatene = 0,
            Ch1 = 1,
            Ch2 = 2,
            Ch3 = 3,
        }

    }
}