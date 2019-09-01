// copyright-holders:K.Ito
using Newtonsoft.Json;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.ComponentModel;

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [JsonConverter(typeof(NoTypeConverterJsonConverter<SoundDriverSettings>))]
    [DataContract]
    [MidiHook]
    public class SoundDriverSettings : ContextBoundObject
    {
        /// <summary>
        /// 
        /// </summary>
        public SoundDriverSettings()
        {
            ADSR = new ADSRSettings();
            ARP = new ARPSettings();
        }

        #region ADSR 

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Description("ADSR Settings")]
        public ADSRSettings ADSR
        {
            get;
            private set;
        }

        #endregion

        #region Arp

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Description("ARP Settings")]
        public ARPSettings ARP
        {
            get;
            private set;
        }

        #endregion

        #region Etc

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
        [Description("You can copy and paste this text data to other same type timber.")]
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

        public void RestoreFrom(string serializeData)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<SoundDriverSettings>(serializeData);
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

        #endregion

    }

    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [JsonConverter(typeof(NoTypeConverterJsonConverter<ADSRSettings>))]
    [DataContract]
    [MidiHook]
    public class ADSRSettings
    {
        #region ADSR 

        private bool f_Enable;

        [DataMember]
        [Description("Whether enable Sound Driver Level ADSR")]
        public bool Enable
        {
            get
            {
                return f_Enable;
            }
            set
            {
                if (f_Enable != value)
                {
                    f_Enable = value;
                }
            }
        }

        /*  TODO:
        private byte f_KSR;

        /// <summary>
        /// Keyboard scaling rate (0-1)
        /// </summary>
        [DataMember]
        [Description("Sound Driver Level Key Scaling Rate")]
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

        private byte f_KSL;

        /// <summary>
        /// Key Scaling Level(0-3)
        /// </summary>
        [DataMember]
        [Category("Sound")]
        [Description("Sound Driver Level Key Scaling Level (00:No Change 10:1.5dB/8ve 01:3dB/8ve 11:6dB/8ve)")]
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
        */

        private byte f_AR = 110;

        /// <summary>
        /// Attack Rate (0(10s)-127(0s))
        /// 0s - 10s
        /// </summary>
        [DataMember]
        [Category("Sound")]
        [Description("Sound Driver Level Attack Rate (0(max)-127(0s))")]
        public byte AR
        {
            get
            {
                return f_AR;
            }
            set
            {
                f_AR = (byte)(value & 127);
            }
        }

        private byte f_DR = 64;

        /// <summary>
        /// Decay Rate (0(0s)-127(10s))
        /// </summary>
        [DataMember]
        [Category("Sound")]
        [Description("Sound Driver Level Decay Rate  (0(0s)-127(max))")]
        public byte DR
        {
            get
            {
                return f_DR;
            }
            set
            {
                f_DR = (byte)(value & 127);
            }
        }

        private byte f_SL = 16;

        /// <summary>
        /// Sustain Level (0(min)-127(max))
        /// </summary>
        [DataMember]
        [Category("Sound")]
        [Description("Sound Driver Level Sustain Level (0(none)-127(max))")]
        public byte SL
        {
            get
            {
                return f_SL;
            }
            set
            {
                f_SL = (byte)(value & 127);
            }
        }

        private byte f_RR = 48;

        /// <summary>
        /// release rate (0(0s)-127(60s))
        /// </summary>
        [DataMember]
        [Category("Sound")]
        [Description("Sound Driver Level Release Rate (0(0s)-127(max))")]
        public byte RR
        {
            get
            {
                return f_RR;
            }
            set
            {
                f_RR = (byte)(value & 127);
            }
        }

        #endregion

        #region Etc

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
        [Description("You can copy and paste this text data to other same type timber.")]
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

        public void RestoreFrom(string serializeData)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<ADSRSettings>(serializeData);
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

        #endregion
    }

    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [JsonConverter(typeof(NoTypeConverterJsonConverter<ARPSettings>))]
    [DataContract]
    [MidiHook]
    public class ARPSettings
    {
        private bool f_Enable;

        [DataMember]
        [Description("Whether enable Sound Driver Level Arpeggio")]
        public bool Enable
        {
            get
            {
                return f_Enable;
            }
            set
            {
                if (f_Enable != value)
                {
                    f_Enable = value;
                }
            }
        }

        private bool f_Hold;

        [DataMember]
        [Description("Select whether Arpeggio key hold or no")]
        public bool Hold
        {
            get
            {
                return f_Hold;
            }
            set
            {
                if (f_Hold != value)
                {
                    f_Hold = value;
                }
            }
        }

        private ArpType f_ArpType;

        [DataMember]
        [Description("Select Arpeggio Type (Dynamic or Static)")]
        public ArpType ArpType
        {
            get
            {
                return f_ArpType;
            }
            set
            {
                if (f_ArpType != value)
                {
                    f_ArpType = value;
                }
            }
        }

        private ArpMethod f_ArpMethod;

        [DataMember]
        [Description("Select Arpeggio Method (Note On or Pitch Change)")]
        public ArpMethod ArpMethod
        {
            get
            {
                return f_ArpMethod;
            }
            set
            {
                if (f_ArpMethod != value)
                {
                    f_ArpMethod = value;
                }
            }
        }

        private ArpStepStyle f_StepStyle;

        [DataMember]
        [Description("Select Arpeggio Step Style")]
        public ArpStepStyle StepStyle
        {
            get
            {
                return f_StepStyle;
            }
            set
            {
                if (f_StepStyle != value)
                {
                    f_StepStyle = value;
                }
            }
        }

        private int f_OctaveRange = 1;

        [DataMember]
        [Description("Select Arpeggio Octave Range (1-4)")]
        public int OctaveRange
        {
            get
            {
                return f_OctaveRange;
            }
            set
            {
                if (f_OctaveRange != value && value >= 1 && value <= 4)
                {
                    f_OctaveRange = value;
                }
            }
        }

        private int f_Beat = 120;

        [DataMember]
        [Description("Select Arpeggio Beat (20-300)")]
        public int Beat
        {
            get
            {
                return f_Beat;
            }
            set
            {
                if (f_Beat != value && value >= 20 && value <= 300)
                {
                    f_Beat = value;
                }
            }
        }

        private ArpResolution f_ArpResolution = ArpResolution.QuarterNote;

        [DataMember]
        [Description("Select Arpeggio Tempo Resolution")]
        public ArpResolution ArpResolution
        {
            get
            {
                return f_ArpResolution;
            }
            set
            {
                if (f_ArpResolution != value)
                {
                    f_ArpResolution = value;
                    ArpStep = (60d * InstrumentManager.TIMER_HZ / Beat) / (double)ArpResolution;
                }
            }
        }

        [Browsable(false)]
        [IgnoreDataMember]
        public double ArpStep
        {
            get;
            private set;
        }


        private int f_GateTime = 127;

        [DataMember]
        [Description("Select Arpeggio Note Gate Time (0(0%)-127(100%))")]
        public int GateTime
        {
            get
            {
                return f_GateTime;
            }
            set
            {
                value = value & 127;
                if (f_GateTime != value)
                {
                    f_GateTime = value;
                    ArpStep = (60d * InstrumentManager.TIMER_HZ / Beat) / (double)ArpResolution;
                }
            }
        }


        private bool f_KeySync;

        [DataMember]
        [Description("Select Use Key Sync Mode ")]
        public bool KeySync
        {
            get
            {
                return f_KeySync;
            }
            set
            {
                if (f_KeySync != value)
                {
                    f_KeySync = value;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ARPSettings()
        {
            ArpStep = (60d * InstrumentManager.TIMER_HZ / Beat) / (double)ArpResolution;
        }

        #region Etc

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
        [Description("You can copy and paste this text data to other same type timber.")]
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

        public void RestoreFrom(string serializeData)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<ARPSettings>(serializeData);
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

        #endregion

    }
}

