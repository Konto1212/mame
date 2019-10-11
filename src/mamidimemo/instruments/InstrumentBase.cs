// copyright-holders:K.Ito
using Jacobi.Vst.Core;
using Jacobi.Vst.Interop.Host;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Instruments.Vst;
using zanac.MAmidiMEmo.Mame;
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo.Instruments
{
    [JsonConverter(typeof(NoTypeConverterJsonConverter<InstrumentBase>))]
    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [MidiHook]
    [DataContract]
    public abstract class InstrumentBase : ContextBoundObject
    {
        public static readonly object VstPluginContextLockObject = new object();

        /// <summary>
        /// 
        /// </summary>
        [Category("General")]
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("General")]
        public abstract string Group
        {
            get;
        }

        private float f_GainLeft = 1.0f;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("General")]
        [Description("Gain Left ch. (0.0-*) of this Instrument")]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(0d, 10d, 0.1d, true)]
        public float GainLeft
        {
            get
            {
                return f_GainLeft;
            }
            set
            {
                if (f_GainLeft != value)
                {
                    f_GainLeft = value;
                    Program.SoundUpdating();
                    set_output_gain(UnitNumber, SoundInterfaceTagNamePrefix, 0, value);
                    Program.SoundUpdated();
                }
            }
        }

        public virtual bool ShouldSerializeGainLeft()
        {
            return GainLeft != 1.0f;
        }

        public virtual void ResetGainLeft()
        {
            GainLeft = 1.0f;
        }

        private float f_GainRight = 1.0f;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("General")]
        [Description("Gain Right ch. (0.0-*) of this Instrument")]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(0d, 10d, 0.1d, true)]
        public float GainRight
        {
            get
            {
                return f_GainRight;
            }
            set
            {
                if (f_GainRight != value)
                {
                    f_GainRight = value;
                    Program.SoundUpdating();
                    set_output_gain(UnitNumber, SoundInterfaceTagNamePrefix, 1, value);
                    Program.SoundUpdated();
                }
            }
        }

        public virtual bool ShouldSerializeGainRight()
        {
            return GainRight != 1.0f;
        }

        public virtual void ResetGainRight()
        {
            GainRight = 1.0f;
        }

        private FilterMode f_FilterMode;

        [DataMember]
        [Category("Filter")]
        [Description("Audio Filter Type")]
        [DefaultValue(FilterMode.None)]
        public FilterMode FilterMode
        {
            get => f_FilterMode;
            set
            {
                if (f_FilterMode != value)
                {
                    f_FilterMode = value;
                    Program.SoundUpdating();
                    set_filter(UnitNumber, SoundInterfaceTagNamePrefix, f_FilterMode, FilterCutoff, FilterResonance);
                    Program.SoundUpdated();
                }
            }
        }

        private double f_FilterCutOff = 0.99d;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Filter")]
        [Description("Audio Cutoff Filter (0.1-0.99) of this Instrument")]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(0.01d, 0.99d, 0.01d, true)]
        [DefaultValue(0.99d)]
        public double FilterCutoff
        {
            get
            {
                return f_FilterCutOff;
            }
            set
            {
                double v = value;
                if (v < 0.01d)
                    v = 0.01d;
                else if (v > 0.99d)
                    v = 0.99d;
                if (f_FilterCutOff != v)
                {
                    f_FilterCutOff = v;
                    Program.SoundUpdating();
                    set_filter(UnitNumber, SoundInterfaceTagNamePrefix, FilterMode, f_FilterCutOff, FilterResonance);
                    Program.SoundUpdated();
                }
            }
        }


        private double f_FilterResonance = 0.01d;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Filter")]
        [Description("Audio Cutoff Filter (0.01-1.00) of this Instrument")]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(0.01d, 1.00d, 0.01d, true)]
        [DefaultValue(0.01d)]
        public double FilterResonance
        {
            get
            {
                return f_FilterResonance;
            }
            set
            {
                double v = value;
                if (v < 0.00d)
                    v = 0.00d;
                else if (v > 1.00d)
                    v = 1.00d;
                if (f_FilterResonance != v)
                {
                    f_FilterResonance = v;
                    Program.SoundUpdating();
                    set_filter(UnitNumber, SoundInterfaceTagNamePrefix, FilterMode, FilterCutoff, f_FilterResonance);
                    Program.SoundUpdated();
                }
            }
        }

        [DataMember]
        [Category("VST")]
        [Description("Set VST effect plugins. Effects are applied in order from the first VST to the last VST")]
        public VSTPluginCollection VSTPlugins
        {
            get;
            set;
        } = new VSTPluginCollection();

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Description("Memo")]
        public string Memo
        {
            get;
            set;
        }

        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
    typeof(UITypeEditor)), Localizable(false)]
        [IgnoreDataMember]
        [JsonIgnore]
        [Description("You can copy and paste this text data to other same type Instrument.")]
        public string SerializeData
        {
            get
            {
                return JsonConvert.SerializeObject(this, Formatting.Indented);
            }
            set
            {
                RestoreFrom(value);
            }
        }

        public abstract void RestoreFrom(string serializeData);

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        public abstract string ImageKey
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        public abstract InstrumentType InstrumentType
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected abstract string SoundInterfaceTagNamePrefix
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("General")]
        public abstract uint DeviceID
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("MIDI")]
        public uint UnitNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("MIDI")]
        [Description("Receving MIDI ch <MIDI 16ch>")]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public bool[] Channels
        {
            get;
            private set;
        }

        public bool ShouldSerializeChannels()
        {
            foreach (var dt in Channels)
            {
                if (dt != true)
                    return true;
            }
            return false;
        }

        public void ResetChannels()
        {
            for (int i = 0; i < Channels.Length; i++)
                Channels[i] = false;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("MIDI")]
        [Description("Pitch (0 - 8192 - 16383) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(16383)]
        public ushort[] Pitchs
        {
            get;
            private set;
        }

        public bool ShouldSerializePitchs()
        {
            foreach (var dt in Pitchs)
            {
                if (dt != 8192)
                    return true;
            }
            return false;
        }

        public void ResetPitchs()
        {
            for (int i = 0; i < Pitchs.Length; i++)
                Pitchs[i] = 8192;
        }

        [DataMember]
        [Category("MIDI")]
        [Description("Pitch bend sensitivity [half note] <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        public byte[] PitchBendRanges
        {
            get;
            private set;
        }

        public bool ShouldSerializePitchBendRanges()
        {
            foreach (var dt in PitchBendRanges)
            {
                if (dt != 2)
                    return true;
            }
            return false;
        }

        public void ResetPitchBendRanges()
        {
            for (int i = 0; i < PitchBendRanges.Length; i++)
                Pitchs[i] = 2;
        }

        [DataMember]
        [Category("MIDI")]
        [Description("Program number (0-127) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        public byte[] ProgramNumbers
        {
            get;
            private set;
        }

        public bool ShouldSerializeProgramNumbers()
        {
            foreach (var dt in ProgramNumbers)
            {
                if (dt != 0)
                    return true;
            }
            return false;
        }

        public void ResetProgramNumbers()
        {
            for (int i = 0; i < ProgramNumbers.Length; i++)
                Pitchs[i] = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract TimbreBase GetTimbre(int channel);


        [DataMember]
        [Category("MIDI")]
        [Description("Volume (0-127) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        public byte[] Volumes
        {
            get;
            private set;
        }

        public bool ShouldSerializeVolumes()
        {
            foreach (var dt in Volumes)
            {
                if (dt != 127)
                    return true;
            }
            return false;
        }

        public void ResetVolumes()
        {
            for (int i = 0; i < Volumes.Length; i++)
                Volumes[i] = 127;
        }

        [DataMember]
        [Category("MIDI")]
        [Description("Volume (0-127) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        public byte[] Expressions
        {
            get;
            private set;
        }


        public bool ShouldSerializeExpressions()
        {
            foreach (var dt in Expressions)
            {
                if (dt != 127)
                    return true;
            }
            return false;
        }

        public void ResetExpressions()
        {
            for (int i = 0; i < Expressions.Length; i++)
                Expressions[i] = 127;
        }

        [DataMember]
        [Category("MIDI")]
        [Description("Volume ((L)0-63(C)64-127(R)) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        public byte[] Panpots
        {
            get;
            private set;
        }

        public bool ShouldSerializePanpots()
        {
            foreach (var dt in Panpots)
            {
                if (dt != 64)
                    return true;
            }
            return false;
        }

        public void ResetPanpots()
        {
            for (int i = 0; i < Panpots.Length; i++)
                Panpots[i] = 64;
        }

        [DataMember]
        [Category("MIDI")]
        [Description("Modulation (0-127) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        public byte[] Modulations
        {
            get;
            private set;
        }

        public bool ShouldSerializeModulations()
        {
            foreach (var dt in Modulations)
            {
                if (dt != 0)
                    return true;
            }
            return false;
        }

        public void ResetModulations()
        {
            for (int i = 0; i < Modulations.Length; i++)
                Modulations[i] = 0;
        }


        [DataMember]
        [Category("MIDI")]
        [Description("Modulation Rate (0-64-127) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        public byte[] ModulationRates
        {
            get;
            private set;
        }

        public bool ShouldSerializeModulationRates()
        {
            foreach (var dt in ModulationRates)
            {
                if (dt != 64)
                    return true;
            }
            return false;
        }

        public void ResetModulationRates()
        {
            for (int i = 0; i < ModulationRates.Length; i++)
                ModulationRates[i] = 64;
        }

        /// <summary>
        /// Hz
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public virtual double GetModulationRateHz(int channel)
        {
            byte val = ModulationRates[channel];

            double rate = Math.Pow(((double)val / 64d), 2) * 6;

            return rate;
        }

        [DataMember]
        [Category("MIDI")]
        [Description("Modulation Depth (0-64-127) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        public byte[] ModulationDepthes
        {
            get;
            private set;
        }

        public bool ShouldSerializeModulationDepthes()
        {
            foreach (var dt in ModulationDepthes)
            {
                if (dt != 64)
                    return true;
            }
            return false;
        }

        public void ResetModulationDepthes()
        {
            for (int i = 0; i < ModulationDepthes.Length; i++)
                ModulationDepthes[i] = 64;
        }

        [DataMember]
        [Category("MIDI")]
        [Description("Modulation Delay (0-64-127) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        public byte[] ModulationDelays
        {
            get;
            private set;
        }

        public bool ShouldSerializeModulationDelays()
        {
            foreach (var dt in ModulationDelays)
            {
                if (dt != 64)
                    return true;
            }
            return false;
        }

        public void ResetModulationDelays()
        {
            for (int i = 0; i < ModulationDelays.Length; i++)
                ModulationDelays[i] = 64;
        }

        /// <summary>
        /// Hz
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public virtual double GetModulationDelaySec(int channel)
        {
            byte val = ModulationDelays[channel];

            double rate = Math.Pow(((double)val / 64d), 3.25) - 1;
            if (rate < 0)
                rate = 0;

            return rate;
        }


        [DataMember]
        [Category("MIDI")]
        [Description("Modulation Depth Range[Half Note] (0-127) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        public byte[] ModulationDepthRangesNote
        {
            get;
            private set;
        }

        public bool ShouldSerializeModulationDepthRangesNote()
        {
            foreach (var dt in ModulationDepthRangesNote)
            {
                if (dt != 0)
                    return true;
            }
            return false;
        }

        public void ResetModulationDepthRangesNote()
        {
            for (int i = 0; i < ModulationDepthRangesNote.Length; i++)
                ModulationDepthRangesNote[i] = 64;
        }


        [DataMember]
        [Category("MIDI")]
        [Description("Modulation Depth Range[Cent] (0-127) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        public byte[] ModulationDepthRangesCent
        {
            get;
            private set;
        }


        public bool ShouldSerializeModulationDepthRangesCent()
        {
            foreach (var dt in ModulationDepthRangesCent)
            {
                if (dt != 0x40)
                    return true;
            }
            return false;
        }

        public void ResetModulationDepthRangesCent()
        {
            for (int i = 0; i < ModulationDepthRangesCent.Length; i++)
                ModulationDepthRangesCent[i] = 64;
        }


        [DataMember]
        [Category("MIDI")]
        [Description("Portamento (0:Off 64:On) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        public byte[] Portamentos
        {
            get;
            private set;
        }


        public bool ShouldSerializePortamentos()
        {
            foreach (var dt in Portamentos)
            {
                if (dt != 0)
                    return true;
            }
            return false;
        }

        public void ResetPortamentos()
        {
            for (int i = 0; i < Portamentos.Length; i++)
                Portamentos[i] = 0;
        }


        [DataMember]
        [Category("MIDI")]
        [Description("Portamento Time (0-127) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        public byte[] PortamentoTimes
        {
            get;
            private set;
        }


        public bool ShouldSerializePortamentoTimes()
        {
            foreach (var dt in PortamentoTimes)
            {
                if (dt != 0)
                    return true;
            }
            return false;
        }

        public void ResetPortamentoTimes()
        {
            for (int i = 0; i < PortamentoTimes.Length; i++)
                PortamentoTimes[i] = 0;
        }


        [Browsable(false)]
        public byte[] RpnLsb
        {
            get;
        }

        [Browsable(false)]
        public byte[] RpnMsb
        {
            get;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_set_device_enable(uint unitNumber, string tagName, byte enable);

        private static delegate_set_device_enable set_device_enable;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_device_reset(uint unitNumber, string tagName);

        private static delegate_device_reset device_reset;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_set_output_gain(uint unitNumber, string tagName, int channel, float gain);

        private static delegate_set_output_gain set_output_gain;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_set_filter(uint unitNumber, string tagName, FilterMode filterMode, double cutoff, double resonance);

        private static delegate_set_filter set_filter;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_set_clock(uint unitNumber, string tagName, uint clock);

        /// <summary>
        /// 
        /// </summary>
        private static delegate_set_clock set_clock;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitNumber"></param>
        /// <param name="tagName"></param>
        /// <param name="clock"></param>
        protected void SetClock(uint unitNumber, string tagName, uint clock)
        {
            try
            {
                Program.SoundUpdating();
                set_clock(unitNumber, tagName, clock);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delg_vst_fx_callback(
            IntPtr buffer,
            int samples);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_set_vst_fx_callback(uint unitNumber, string name, delg_vst_fx_callback callback);

        /// <summary>
        /// 
        /// </summary>
        private static void SetVstFxCallback(uint unitNumber, string name, delg_vst_fx_callback callback)
        {
            try
            {
                Program.SoundUpdating();
                set_vst_fx_callback(unitNumber, name, callback);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_set_vst_fx_callback set_vst_fx_callback
        {
            get;
            set;
        }

        private delg_vst_fx_callback f_vst_fx_callback;

        /// <summary>
        /// 
        /// </summary>
        static InstrumentBase()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("set_device_enable");
            if (funcPtr != IntPtr.Zero)
                set_device_enable = Marshal.GetDelegateForFunctionPointer<delegate_set_device_enable>(funcPtr);

            funcPtr = MameIF.GetProcAddress("set_output_gain");
            if (funcPtr != IntPtr.Zero)
                set_output_gain = Marshal.GetDelegateForFunctionPointer<delegate_set_output_gain>(funcPtr);

            funcPtr = MameIF.GetProcAddress("device_reset");
            if (funcPtr != IntPtr.Zero)
                device_reset = Marshal.GetDelegateForFunctionPointer<delegate_device_reset>(funcPtr);

            funcPtr = MameIF.GetProcAddress("set_filter");
            if (funcPtr != IntPtr.Zero)
                set_filter = Marshal.GetDelegateForFunctionPointer<delegate_set_filter>(funcPtr);

            funcPtr = MameIF.GetProcAddress("set_clock");
            if (funcPtr != IntPtr.Zero)
                set_clock = Marshal.GetDelegateForFunctionPointer<delegate_set_clock>(funcPtr);

            funcPtr = MameIF.GetProcAddress("set_vst_fx_callback");
            if (funcPtr != IntPtr.Zero)
                set_vst_fx_callback = Marshal.GetDelegateForFunctionPointer<delegate_set_vst_fx_callback>(funcPtr);
        }

        /// <summary>
        /// 
        /// </summary>
        public InstrumentBase()
        {
            device_reset(UnitNumber, SoundInterfaceTagNamePrefix);

            set_output_gain(UnitNumber, SoundInterfaceTagNamePrefix, 0, GainLeft);
            set_output_gain(UnitNumber, SoundInterfaceTagNamePrefix, 1, GainRight);
            set_filter(UnitNumber, SoundInterfaceTagNamePrefix, FilterMode, FilterCutoff, FilterResonance);

            f_vst_fx_callback = new delg_vst_fx_callback(vst_fx_callback);
            SetVstFxCallback(UnitNumber, SoundInterfaceTagNamePrefix, f_vst_fx_callback);
        }

        /// <summary>
        /// 
        /// </summary>
        public InstrumentBase(uint unitNumber) : this()
        {
            UnitNumber = unitNumber;

            Channels = new bool[] {
                    true, true, true,
                    true, true, true,
                    true, true, true,
                    true, true, true,
                    true, true, true,true };

            ProgramNumbers = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0 };
            Volumes = new byte[] {
                    127, 127, 127,
                    127, 127, 127,
                    127, 127, 127,
                    127, 127, 127,
                    127, 127, 127, 127  };
            Expressions = new byte[] {
                    127, 127, 127,
                    127, 127, 127,
                    127, 127, 127,
                    127, 127, 127,
                    127, 127, 127, 127  };
            Panpots = new byte[] {
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64, 64};
            Pitchs = new ushort[] {
                    8192, 8192, 8192,
                    8192, 8192, 8192,
                    8192, 8192, 8192,
                    8192, 8192, 8192,
                    8192, 8192, 8192, 8192};
            PitchBendRanges = new byte[] {
                    2, 2, 2,
                    2, 2, 2,
                    2, 2, 2,
                    2, 2, 2,
                    2, 2, 2, 2};
            RpnLsb = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0};
            RpnMsb = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0};
            Modulations = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0};
            ModulationRates = new byte[] {
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64, 64};
            ModulationDepthes = new byte[] {
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64, 64};
            ModulationDelays = new byte[] {
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64, 64};
            ModulationDepthRangesNote = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0};
            ModulationDepthRangesCent = new byte[] {
                    0x40, 0x40, 0x40,
                    0x40, 0x40, 0x40,
                    0x40, 0x40, 0x40,
                    0x40, 0x40, 0x40,
                    0x40, 0x40, 0x40, 0x40};
            Portamentos = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0};
            PortamentoTimes = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0};
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
            set_device_enable(UnitNumber, SoundInterfaceTagNamePrefix, 0);

            SetVstFxCallback(UnitNumber, SoundInterfaceTagNamePrefix, null);
            lock (InstrumentBase.VstPluginContextLockObject)
            {
                foreach (var vp in VSTPlugins)
                    vp.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal virtual void PrepareSound()
        {
            set_device_enable(UnitNumber, SoundInterfaceTagNamePrefix, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        internal void NotifyMidiEvent(MidiEvent midiEvent)
        {
            OnMidiEvent(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnMidiEvent(MidiEvent midiEvent)
        {
            var non = midiEvent as NoteOnEvent;
            if (non != null)
            {
                if (!Channels[non.Channel])
                    return;

                if (non.Velocity == 0)
                    OnNoteOffEvent(new NoteOffEvent(non.NoteNumber, (SevenBitNumber)0) { Channel = non.Channel, DeltaTime = non.DeltaTime });
                else
                    OnNoteOnEvent(non);
            }
            else
            {
                var noff = midiEvent as NoteOffEvent;
                if (noff != null)
                {
                    if (!Channels[noff.Channel])
                        return;
                    OnNoteOffEvent(noff);
                }
                else
                {
                    var cont = midiEvent as ControlChangeEvent;
                    if (cont != null)
                    {
                        if (!Channels[cont.Channel])
                            return;
                        OnControlChangeEvent(cont);
                    }
                    else
                    {
                        var prog = midiEvent as ProgramChangeEvent;
                        if (prog != null)
                        {
                            if (!Channels[prog.Channel])
                                return;
                            OnProgramChangeEvent(prog);
                        }
                        else
                        {
                            var pitch = midiEvent as PitchBendEvent;
                            if (pitch != null)
                            {
                                if (!Channels[pitch.Channel])
                                    return;
                                OnPitchBendEvent(pitch);
                            }
                            else
                            {
                                //TODO: key/ch pressure
                                var sysex = midiEvent as SysExEvent;
                                if (sysex != null)
                                {
                                    OnSystemExclusiveEvent(sysex);
                                }
                                else
                                {
                                    //TODO: key/ch pressure
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sysex"></param>
        protected virtual void OnSystemExclusiveEvent(SysExEvent sysex)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnNoteOnEvent(NoteOnEvent midiEvent)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnNoteOffEvent(NoteOffEvent midiEvent)
        {

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnControlChangeEvent(ControlChangeEvent midiEvent)
        {
            switch (midiEvent.ControlNumber)
            {
                case 1:    //Modulation
                    Modulations[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 6:    //Data Entry
                    switch (RpnMsb[midiEvent.Channel])
                    {
                        case 0:
                            {
                                switch (RpnLsb[midiEvent.Channel])
                                {
                                    case 0: //PitchBendRanges Half Note
                                        {
                                            PitchBendRanges[midiEvent.Channel] = midiEvent.ControlValue;
                                            break;
                                        }
                                    case 5: //Mod Depth
                                        {
                                            ModulationDepthRangesNote[midiEvent.Channel] = midiEvent.ControlValue;
                                            break;
                                        }
                                }
                                break;
                            }
                    }
                    break;
                case 38:    //Data Entry
                    switch (RpnMsb[midiEvent.Channel])
                    {
                        case 0:
                            {
                                switch (RpnLsb[midiEvent.Channel])
                                {
                                    case 0: //PitchBendRanges Cent
                                        {
                                            break;
                                        }
                                    case 5: //Mod Depth
                                        {
                                            ModulationDepthRangesCent[midiEvent.Channel] = midiEvent.ControlValue;
                                            break;
                                        }
                                }
                                break;
                            }
                    }
                    break;
                case 5:    //Portamento Time
                    PortamentoTimes[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 7:    //Volume
                    Volumes[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 10:    //Panpot
                    Panpots[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 11:    //Expression
                    Expressions[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 65:    //Portamento
                    Portamentos[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 76:    //Modulation Rate
                    ModulationRates[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 77:    //Modulation Depth
                    ModulationDepthes[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 78:    //Modulation Delay
                    ModulationDelays[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 100:    //RPN LSB
                    RpnLsb[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 101:    //RPN MSB
                    RpnMsb[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 121:    //Reset All Controller
                    for (int i = 0; i < 16; i++)
                    {
                        Volumes[i] = 127;
                        Panpots[i] = 64;
                        Expressions[i] = 127;
                        PitchBendRanges[i] = 2;
                        Pitchs[i] = 8192;
                        Modulations[i] = 0;
                        ModulationRates[i] = 64;
                        ModulationDepthes[i] = 64;
                        ModulationDelays[i] = 64;
                        ModulationDepthRangesNote[i] = 0;
                        ModulationDepthRangesCent[i] = 0x40;
                        Portamentos[i] = 0;
                        PortamentoTimes[i] = 0;
                    }
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnProgramChangeEvent(ProgramChangeEvent midiEvent)
        {
            ProgramNumbers[midiEvent.Channel] = midiEvent.ProgramNumber;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnPitchBendEvent(PitchBendEvent midiEvent)
        {
            Pitchs[midiEvent.Channel] = midiEvent.PitchValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pn"></param>
        /// <param name="pos"></param>
        private void vst_fx_callback(IntPtr buffer, int samples)
        {
            if (samples == 0)
                return;

            int[][] buf = new int[2][] { new int[samples], new int[samples] };
            IntPtr[] pt = new IntPtr[] { Marshal.ReadIntPtr(buffer), Marshal.ReadIntPtr(buffer + IntPtr.Size) };
            Marshal.Copy(pt[0], buf[0], 0, samples);
            Marshal.Copy(pt[1], buf[1], 0, samples);

            using (VstAudioBufferManager bufA = new VstAudioBufferManager(2, samples))
            using (VstAudioBufferManager bufB = new VstAudioBufferManager(2, samples))
            {
                lock (InstrumentBase.VstPluginContextLockObject)
                {
                    bool processed = false;
                    foreach (var vp in VSTPlugins)
                    {
                        var ctx = vp.PluginContext;
                        if (ctx != null)
                        {
                            int idx = 0;
                            foreach (VstAudioBuffer vab in bufA)
                            {
                                Parallel.ForEach(Partitioner.Create(0, samples), range =>
                                {
                                    for (var i = range.Item1; i < range.Item2; i++)
                                        vab[i] = (float)buf[idx][i] / (float)int.MaxValue;
                                });
                                //for (int i = 0; i < samples; i++)
                                //    vab[i] = (float)buf[idx][i] / (float)int.MaxValue;
                                idx++;
                            }
                            break;
                        }
                    }

                    VstAudioBufferManager bufa = bufA;
                    VstAudioBufferManager bufb = bufA;
                    foreach (var vp in VSTPlugins)
                    {
                        var ctx = vp.PluginContext;
                        if (ctx != null)
                        {
                            ctx.Context.PluginCommandStub.SetBlockSize(samples);
                            ctx.Context.PluginCommandStub.ProcessReplacing(bufa.ToArray<VstAudioBuffer>(), bufb.ToArray<VstAudioBuffer>());
                            processed = true;
                        }
                        var tmp = bufa;
                        bufa = bufb;
                        bufb = tmp;
                    }

                    if (processed)
                    {
                        int idx = 0;
                        foreach (VstAudioBuffer vab in bufb)
                        {
                            Parallel.ForEach(Partitioner.Create(0, samples), range =>
                            {
                                for (var i = range.Item1; i < range.Item2; i++)
                                    buf[idx][i] = (int)(vab[i] * int.MaxValue);
                            });
                            //for (int i = 0; i < samples; i++)
                            //    buf[idx][i] = (int)(vab[i] * int.MaxValue);
                            idx++;
                        }
                        Marshal.Copy(buf[0], 0, pt[0], samples);
                        Marshal.Copy(buf[1], 0, pt[1], samples);
                    }
                }
            }

        }

    }

    public enum FilterMode
    {
        None = 0,
        LowPass,
        HighPass,
        BandPass,
    };
}
