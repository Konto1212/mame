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
//http://www.bellesondes.fr/wiki/doku.php?id=mos6581#mos6581_sound_interface_device_sid
//https://www.sfpgmr.net/blog/entry/mos-sid-6581を調べた.html

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public abstract class SIDBase : InstrumentBase
    {

        public override string Group => "PSG";

        private byte f_RES;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Resonance (0-15)")]
        [SlideParametersAttribute(0,15, true)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public byte RES
        {
            get => f_RES;
            set
            {
                if (f_RES != value)
                {
                    f_RES = (byte)(value & 15);
                    SidWriteData(UnitNumber, 23, (byte)(f_RES << 4 | (int)FILT));
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
        [SlideParametersAttribute(0, 2047, true)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public ushort FC
        {
            get => f_FC;
            set
            {
                var v = (ushort)(value & 2047);
                if (f_FC != v)
                {
                    f_FC = v;
                    Program.SoundUpdating();
                    SidWriteData(UnitNumber, 21, (byte)(f_FC & 0x7));
                    SidWriteData(UnitNumber, 22, (byte)(f_FC >> 3));
                    Program.SoundUpdated();
                }
            }
        }


        private FilterChannel f_FILT;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [DefaultValue(FilterChannel.None)]
        [TypeConverter(typeof(FlagsEnumConverter))]
        [Description("Apply Filter Ch")]
        public FilterChannel FILT
        {
            get => f_FILT;
            set
            {
                if (f_FILT != value)
                {
                    f_FILT = value;
                    SidWriteData(UnitNumber, 23, (byte)(OFF3 << 7 | RES << 4 | (int)f_FILT));
                }
            }
        }


        private byte f_Off3;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Disable ch 3 sound (0:Enable 1:Disable)")]
        [SlideParametersAttribute(0, 1, true)]
        [EditorAttribute(typeof(SlideEditor), typeof(UITypeEditor))]
        public byte OFF3
        {
            get
            {
                return f_Off3;
            }
            set
            {
                byte v = (byte)(value & 1);
                if (f_Off3 != v)
                {
                    f_Off3 = v;
                    SidWriteData(UnitNumber, 24, (byte)(f_Off3 << 7 | (int)FilterType << 4 | Volume));
                }
            }
        }

        private FilterTypes f_FilterType;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [DefaultValue(FilterTypes.None)]
        [Description("Filter Type")]
        [TypeConverter(typeof(FlagsEnumConverter))]
        public FilterTypes FilterType
        {
            get => f_FilterType;
            set
            {
                if (f_FilterType != value)
                {
                    f_FilterType = value;
                    SidWriteData(UnitNumber, 24, (byte)(OFF3 << 7 | (int)f_FilterType << 4 | Volume));
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
                byte v = (byte)(value & 15);
                if (f_Volume != v)
                {
                    f_Volume = v;
                    SidWriteData(UnitNumber, 24, (byte)((int)FilterType << 4 | (int)f_Volume));
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
        public SIDTimbre[] Timbres
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
                var obj = JsonConvert.DeserializeObject(serializeData, this.GetType());
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
        private delegate void delegate_sid_write(uint unitNumber, int address, byte data);


        /// <summary>
        /// 
        /// </summary>
        private delegate_sid_write Sid_write
        {
            get;
            set;
        }

        [Browsable(false)]
        protected abstract string WriteProcName
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        private void SidWriteData(uint unitNumber, int address, byte data)
        {
            try
            {
                Program.SoundUpdating();
                Sid_write(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
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

        private SIDSoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public SIDBase(uint unitNumber) : base(unitNumber)
        {
            FilterMode = FilterMode.LowPass;
            FilterCutoff = 0.9d;
            FilterResonance = 0.1d;

            Timbres = new SIDTimbre[128];
            for (int i = 0; i < 128; i++)
                Timbres[i] = new SIDTimbre();
            setPresetInstruments();

            IntPtr funcPtr = MameIF.GetProcAddress(WriteProcName);
            if (funcPtr != IntPtr.Zero)
                Sid_write = (delegate_sid_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_sid_write));

            this.soundManager = new SIDSoundManager(this);
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
        private class SIDSoundManager : SoundManagerBase
        {
            private SoundList<SIDSound> psgOnSounds = new SoundList<SIDSound>(3);

            private SIDBase parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public SIDSoundManager(SIDBase parent) : base(parent)
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
                SIDSound snd = new SIDSound(parentModule, this, timbre, note, emptySlot);
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
        private class SIDSound : SoundBase
        {

            private SIDBase parentModule;

            private SevenBitNumber programNumber;

            private SIDTimbre timbre;

            private Waveforms lastWaveform;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public SIDSound(SIDBase parentModule, SIDSoundManager manager, TimbreBase timbre, NoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, noteOnEvent, slot)
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
                    parentModule.OFF3 = gs.OFF3;
                    parentModule.FILT = gs.FILT;
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

                parentModule.SidWriteData(parentModule.UnitNumber, Slot * 7 + 5, (byte)(timbre.ATK << 4 | timbre.DCY));
                parentModule.SidWriteData(parentModule.UnitNumber, Slot * 7 + 6, (byte)(timbre.STN << 4 | timbre.RIS));
            }


            /// <summary>
            /// 
            /// </summary>
            public override void UpdateVolume()
            {
                double v = 1;
                v *= ParentModule.Expressions[NoteOnEvent.Channel] / 127d;
                v *= ParentModule.Volumes[NoteOnEvent.Channel] / 127d;

                if (FxEngine != null)
                    v *= FxEngine.OutputLevel;

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

                var pw = timbre.PW;
                var res = parentModule.RES;
                var fc = parentModule.FC;
                var w = timbre.Waveform;
                if (FxEngine != null && FxEngine.Active)
                {
                    var eng = (SidFxEngine)FxEngine;
                    if (eng.DutyValue != null)
                        pw = eng.DutyValue.Value;
                    if (eng.WaveFormValue != null)
                        w = eng.WaveFormValue.Value;
                    if (timbre.GlobalSettings.Enable)
                    {
                        if (eng.ResonanceValue != null)
                            res = eng.ResonanceValue.Value;
                        if (eng.CutOffValue != null)
                            fc = eng.CutOffValue.Value;
                    }
                }
                var un = parentModule.UnitNumber;
                Program.SoundUpdating();
                parentModule.RES = res;
                parentModule.FC = fc;
                parentModule.SidWriteData(un, Slot * 7 + 2, (byte)(pw & 0xff));
                parentModule.SidWriteData(un, Slot * 7 + 3, (byte)(pw >> 8));
                byte data = (byte)((int)w << 4 | timbre.RING << 2 | timbre.SYNC << 1 | (IsSoundOff ? 0 : 1));
                parentModule.SidWriteData(un, Slot * 7 + 4, data);

                parentModule.SidWriteData(un, Slot * 7 + 1, (byte)(f >> 8));
                parentModule.SidWriteData(un, Slot * 7 + 0, (byte)(f & 0xff));
                Program.SoundUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                byte data = (byte)((int)timbre.Waveform << 4 | timbre.RING << 2 | timbre.SYNC << 1 | 0);
                parentModule.SidWriteData(parentModule.UnitNumber, Slot * 7 + 4, data);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SIDTimbre>))]
        [DataContract]
        public class SIDTimbre : TimbreBase
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

                    if ((f & Waveforms.Noise) == Waveforms.Noise && (f_Waveform & Waveforms.Noise) != Waveforms.Noise)
                        f &= Waveforms.Noise;
                    else if ((f & (Waveforms)7) != 0 && (f_Waveform & Waveforms.Noise) == Waveforms.Noise)
                        f &= (Waveforms)7;

                    if (f_Waveform != f)
                        f_Waveform = f;
                }
            }

            private byte f_ATK;

            [DataMember]
            [Category("Sound")]
            [Description("Attack (0-15)")]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
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
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
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
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
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
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
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
            [SlideParametersAttribute(0, 4095)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
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
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
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
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
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
            public SIDTimbre()
            {
                GlobalSettings = new GlobalSettings();
                this.SDS.FxS = new SidFxSettings();
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<SIDTimbre>(serializeData);
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
            [Description("Override global settings")]
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
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(UITypeEditor))]
            public byte RES
            {
                get => f_RES;
                set
                {
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
            [SlideParametersAttribute(0, 2047)]
            [EditorAttribute(typeof(SlideEditor), typeof(UITypeEditor))]
            public ushort FC
            {
                get => f_FC;
                set
                {
                    f_FC = (ushort)(value & 2047);
                }
            }

            private byte f_Off3;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [Description("Disable ch 3 sound (0:Enable 1:Disable)")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(UITypeEditor))]
            public byte OFF3
            {
                get => f_Off3;
                set
                {
                    f_Off3 = (byte)(value & 1);
                }
            }

            private FilterChannel f_FILT;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [DefaultValue(FilterChannel.None)]
            [Description("Apply Filter Ch")]
            public FilterChannel FILT
            {
                get => f_FILT;
                set
                {
                    f_FILT = value;
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
        [Flags]
        public enum FilterChannel
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


        /// <summary>
        /// 
        /// </summary>
        public class SidFxEngine : BasicFxEngine
        {
            private SidFxSettings settings;

            /// <summary>
            /// 
            /// </summary>
            public SidFxEngine(SidFxSettings settings) : base(settings)
            {
                this.settings = settings;
            }

            private uint f_dutyCounter;

            public ushort? DutyValue
            {
                get;
                private set;
            }

            public byte? ResonanceValue
            {
                get;
                private set;
            }

            public ushort? CutOffValue
            {
                get;
                private set;
            }

            public Waveforms? WaveFormValue
            {
                get;
                private set;
            }

            protected override void ProcessCore(SoundBase sound, bool isKeyOff, bool isSoundOff)
            {
                base.ProcessCore(sound, isKeyOff, isSoundOff);

                DutyValue = null;
                if (settings.DutyEnvelopesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.DutyEnvelopesNums.Length;
                        if (settings.DutyEnvelopesReleasePoint >= 0)
                            vm = settings.DutyEnvelopesReleasePoint;
                        if (f_dutyCounter >= vm)
                        {
                            if (settings.DutyEnvelopesRepeatPoint >= 0)
                                f_dutyCounter = (uint)settings.DutyEnvelopesRepeatPoint;
                            else
                                f_dutyCounter = (uint)vm;

                            if (f_dutyCounter >= settings.DutyEnvelopesNums.Length)
                                f_dutyCounter = (uint)(settings.DutyEnvelopesNums.Length - 1);
                        }
                    }
                    else
                    {
                        if (settings.DutyEnvelopesRepeatPoint < 0)
                            f_dutyCounter = (uint)settings.DutyEnvelopesNums.Length;

                        if (f_dutyCounter >= settings.DutyEnvelopesNums.Length)
                            f_dutyCounter = (uint)(settings.DutyEnvelopesNums.Length - 1);
                    }
                    int vol = settings.DutyEnvelopesNums[f_dutyCounter++];

                    DutyValue = (ushort)vol;
                }

                ResonanceValue = null;
                if (settings.ResonanceEnvelopesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.ResonanceEnvelopesNums.Length;
                        if (settings.ResonanceEnvelopesReleasePoint >= 0)
                            vm = settings.ResonanceEnvelopesReleasePoint;
                        if (f_dutyCounter >= vm)
                        {
                            if (settings.ResonanceEnvelopesRepeatPoint >= 0)
                                f_dutyCounter = (uint)settings.ResonanceEnvelopesRepeatPoint;
                            else
                                f_dutyCounter = (uint)vm;

                            if (f_dutyCounter >= settings.ResonanceEnvelopesNums.Length)
                                f_dutyCounter = (uint)(settings.ResonanceEnvelopesNums.Length - 1);
                        }
                    }
                    else
                    {
                        if (settings.ResonanceEnvelopesRepeatPoint < 0)
                            f_dutyCounter = (uint)settings.ResonanceEnvelopesNums.Length;

                        if (f_dutyCounter >= settings.ResonanceEnvelopesNums.Length)
                            f_dutyCounter = (uint)(settings.ResonanceEnvelopesNums.Length - 1);
                    }
                    int vol = settings.ResonanceEnvelopesNums[f_dutyCounter++];

                    ResonanceValue = (byte)vol;
                }

                CutOffValue = null;
                if (settings.CutOffEnvelopesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.CutOffEnvelopesNums.Length;
                        if (settings.CutOffEnvelopesReleasePoint >= 0)
                            vm = settings.CutOffEnvelopesReleasePoint;
                        if (f_dutyCounter >= vm)
                        {
                            if (settings.CutOffEnvelopesRepeatPoint >= 0)
                                f_dutyCounter = (uint)settings.CutOffEnvelopesRepeatPoint;
                            else
                                f_dutyCounter = (uint)vm;

                            if (f_dutyCounter >= settings.CutOffEnvelopesNums.Length)
                                f_dutyCounter = (uint)(settings.CutOffEnvelopesNums.Length - 1);
                        }
                    }
                    else
                    {
                        if (settings.CutOffEnvelopesRepeatPoint < 0)
                            f_dutyCounter = (uint)settings.CutOffEnvelopesNums.Length;

                        if (f_dutyCounter >= settings.CutOffEnvelopesNums.Length)
                            f_dutyCounter = (uint)(settings.CutOffEnvelopesNums.Length - 1);
                    }
                    int vol = settings.CutOffEnvelopesNums[f_dutyCounter++];

                    CutOffValue = (byte)vol;
                }

                WaveFormValue = null;
                if (settings.WaveFormEnvelopesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.WaveFormEnvelopesNums.Length;
                        if (settings.WaveFormEnvelopesReleasePoint >= 0)
                            vm = settings.WaveFormEnvelopesReleasePoint;
                        if (f_dutyCounter >= vm)
                        {
                            if (settings.WaveFormEnvelopesRepeatPoint >= 0)
                                f_dutyCounter = (uint)settings.WaveFormEnvelopesRepeatPoint;
                            else
                                f_dutyCounter = (uint)vm;

                            if (f_dutyCounter >= settings.WaveFormEnvelopesNums.Length)
                                f_dutyCounter = (uint)(settings.WaveFormEnvelopesNums.Length - 1);
                        }
                    }
                    else
                    {
                        if (settings.WaveFormEnvelopesRepeatPoint < 0)
                            f_dutyCounter = (uint)settings.WaveFormEnvelopesNums.Length;

                        if (f_dutyCounter >= settings.WaveFormEnvelopesNums.Length)
                            f_dutyCounter = (uint)(settings.WaveFormEnvelopesNums.Length - 1);
                    }
                    int vol = settings.WaveFormEnvelopesNums[f_dutyCounter++];

                    WaveFormValue = (Waveforms)vol;
                }
            }

        }

        [JsonConverter(typeof(NoTypeConverterJsonConverter<SidFxSettings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [MidiHook]
        public class SidFxSettings : BasicFxSettings
        {

            private string f_DutyEnvelopes;

            [DataMember]
            [Description("Set duty envelop by text. Input duty value and split it with space like the Famitracker.\r\n" +
                       "0 ～ 4095 \"|\" is repeat point. \"/\" is release point.")]
            public string DutyEnvelopes
            {
                get
                {
                    return f_DutyEnvelopes;
                }
                set
                {
                    if (f_DutyEnvelopes != value)
                    {
                        DutyEnvelopesRepeatPoint = -1;
                        DutyEnvelopesReleasePoint = -1;
                        if (value == null)
                        {
                            DutyEnvelopesNums = new int[] { };
                            f_DutyEnvelopes = string.Empty;
                            return;
                        }
                        f_DutyEnvelopes = value;
                        string[] vals = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                DutyEnvelopesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                DutyEnvelopesReleasePoint = vs.Count;
                            else
                            {
                                int v;
                                if (int.TryParse(val, out v))
                                {
                                    if (v < 0)
                                        v = 0;
                                    else if (v > 4095)
                                        v = 4095;
                                    vs.Add(v);
                                }
                            }
                        }
                        DutyEnvelopesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < DutyEnvelopesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (DutyEnvelopesRepeatPoint == i)
                                sb.Append("| ");
                            if (DutyEnvelopesReleasePoint == i)
                                sb.Append("/ ");
                            sb.Append(DutyEnvelopesNums[i].ToString((IFormatProvider)null));
                        }
                        f_DutyEnvelopes = sb.ToString();
                    }
                }
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] DutyEnvelopesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int DutyEnvelopesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int DutyEnvelopesReleasePoint { get; set; } = -1;


            private string f_ResonanceEnvelopes;

            [DataMember]
            [Description("Set resonance envelop by text. Input resonance value and split it with space like the Famitracker.\r\n" +
                       "0 ～ 15 \"|\" is repeat point. \"/\" is release point.")]
            public string ResonanceEnvelopes
            {
                get
                {
                    return f_ResonanceEnvelopes;
                }
                set
                {
                    if (f_ResonanceEnvelopes != value)
                    {
                        ResonanceEnvelopesRepeatPoint = -1;
                        ResonanceEnvelopesReleasePoint = -1;
                        if (value == null)
                        {
                            ResonanceEnvelopesNums = new int[] { };
                            f_ResonanceEnvelopes = string.Empty;
                            return;
                        }
                        f_ResonanceEnvelopes = value;
                        string[] vals = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                ResonanceEnvelopesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                ResonanceEnvelopesReleasePoint = vs.Count;
                            else
                            {
                                int v;
                                if (int.TryParse(val, out v))
                                {
                                    if (v < 0)
                                        v = 0;
                                    else if (v > 15)
                                        v = 15;
                                    vs.Add(v);
                                }
                            }
                        }
                        ResonanceEnvelopesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < ResonanceEnvelopesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (ResonanceEnvelopesRepeatPoint == i)
                                sb.Append("| ");
                            if (ResonanceEnvelopesReleasePoint == i)
                                sb.Append("/ ");
                            sb.Append(ResonanceEnvelopesNums[i].ToString((IFormatProvider)null));
                        }
                        f_ResonanceEnvelopes = sb.ToString();
                    }
                }
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] ResonanceEnvelopesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int ResonanceEnvelopesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int ResonanceEnvelopesReleasePoint { get; set; } = -1;


            private string f_CutOffEnvelopes;

            [DataMember]
            [Description("Set resonance envelop by text. Input resonance value and split it with space like the Famitracker.\r\n" +
                       "0 ～ 2047 \"|\" is repeat point. \"/\" is release point.")]
            public string CutOffEnvelopes
            {
                get
                {
                    return f_CutOffEnvelopes;
                }
                set
                {
                    if (f_CutOffEnvelopes != value)
                    {
                        CutOffEnvelopesRepeatPoint = -1;
                        CutOffEnvelopesReleasePoint = -1;
                        if (value == null)
                        {
                            CutOffEnvelopesNums = new int[] { };
                            f_CutOffEnvelopes = string.Empty;
                            return;
                        }
                        f_CutOffEnvelopes = value;
                        string[] vals = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                CutOffEnvelopesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                CutOffEnvelopesReleasePoint = vs.Count;
                            else
                            {
                                int v;
                                if (int.TryParse(val, out v))
                                {
                                    if (v < 0)
                                        v = 0;
                                    else if (v > 2047)
                                        v = 15;
                                    vs.Add(v);
                                }
                            }
                        }
                        CutOffEnvelopesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < CutOffEnvelopesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (CutOffEnvelopesRepeatPoint == i)
                                sb.Append("| ");
                            if (CutOffEnvelopesReleasePoint == i)
                                sb.Append("/ ");
                            sb.Append(CutOffEnvelopesNums[i].ToString((IFormatProvider)null));
                        }
                        f_CutOffEnvelopes = sb.ToString();
                    }
                }
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] CutOffEnvelopesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int CutOffEnvelopesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int CutOffEnvelopesReleasePoint { get; set; } = -1;

            private string f_WaveFormEnvelopes;

            [DataMember]
            [Description("Set resonance envelop by text. Input resonance value and split it with space like the Famitracker.\r\n" +
                       "1 ～ 8(Tri:1 Saw:2 Pulse:4 Noise:8) \"|\" is repeat point. \"/\" is release point.")]
            public string WaveFormEnvelopes
            {
                get
                {
                    return f_WaveFormEnvelopes;
                }
                set
                {
                    if (f_WaveFormEnvelopes != value)
                    {
                        WaveFormEnvelopesRepeatPoint = -1;
                        WaveFormEnvelopesReleasePoint = -1;
                        if (value == null)
                        {
                            WaveFormEnvelopesNums = new int[] { };
                            f_WaveFormEnvelopes = string.Empty;
                            return;
                        }
                        f_WaveFormEnvelopes = value;
                        string[] vals = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                WaveFormEnvelopesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                WaveFormEnvelopesReleasePoint = vs.Count;
                            else
                            {
                                int v;
                                if (int.TryParse(val, out v))
                                {
                                    if (v < 1)
                                        v = 1;
                                    else if (v > 15)
                                        v = 15;
                                    vs.Add(v);
                                }
                            }
                        }
                        WaveFormEnvelopesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < WaveFormEnvelopesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (WaveFormEnvelopesRepeatPoint == i)
                                sb.Append("| ");
                            if (WaveFormEnvelopesReleasePoint == i)
                                sb.Append("/ ");
                            sb.Append(WaveFormEnvelopesNums[i].ToString((IFormatProvider)null));
                        }
                        f_WaveFormEnvelopes = sb.ToString();
                    }
                }
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] WaveFormEnvelopesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int WaveFormEnvelopesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int WaveFormEnvelopesReleasePoint { get; set; } = -1;


            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override AbstractFxEngine CreateEngine()
            {
                return new SidFxEngine(this);
            }

        }

    }
}