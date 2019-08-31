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
            ArpStep = (60d * InstrumentManager.TIMER_HZ / ArpTempo) / (double)ArpResolution;
        }

        #region ADSR 

        private bool f_EnableAdsr;

        [DataMember]
        [Description("Whether enable Sound Driver Level ADSR")]
        public bool ADSREnable
        {
            get
            {
                return f_EnableAdsr;
            }
            set
            {
                if (f_EnableAdsr != value)
                {
                    f_EnableAdsr = value;
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

        #region Arp

        private bool f_ArpEnable;

        [DataMember]
        [Description("Whether enable Sound Driver Level Arpeggio")]
        public bool ArpEnable
        {
            get
            {
                return f_ArpEnable;
            }
            set
            {
                if (f_ArpEnable != value)
                {
                    f_ArpEnable = value;
                }
            }
        }

        private bool f_ArpHold;

        [DataMember]
        [Description("Select whether Arpeggio key hold or no")]
        public bool ArpHold
        {
            get
            {
                return f_ArpHold;
            }
            set
            {
                if (f_ArpHold != value)
                {
                    f_ArpHold = value;
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

        private ArpStepStyle f_ArpStepStyle;

        [DataMember]
        [Description("Select Arpeggio Step Style")]
        public ArpStepStyle ArpStepStyle
        {
            get
            {
                return f_ArpStepStyle;
            }
            set
            {
                if (f_ArpStepStyle != value)
                {
                    f_ArpStepStyle = value;
                }
            }
        }


        private int f_ArpRange = 1;

        [DataMember]
        [Description("Select Arpeggio Range (1-4)")]
        public int ArpRange
        {
            get
            {
                return f_ArpRange;
            }
            set
            {
                if (f_ArpRange != value && value >= 1 && value <= 4)
                {
                    f_ArpRange = value;
                }
            }
        }

        private int f_ArpTempo = 120;

        [DataMember]
        [Description("Select Arpeggio Tempo (20-300)")]
        public int ArpTempo
        {
            get
            {
                return f_ArpTempo;
            }
            set
            {
                if (f_ArpTempo != value && value >= 20 && value <= 300)
                {
                    f_ArpTempo = value;
                }
            }
        }


        private int f_ArpGate = 127;

        [DataMember]
        [Description("Select Arpeggio Gate Time (0(0%)-127(100%))")]
        public int ArpGate
        {
            get
            {
                return f_ArpGate;
            }
            set
            {
                value = value & 127;
                if (f_ArpGate != value)
                {
                    f_ArpGate = value;
                    ArpStep = (60d * InstrumentManager.TIMER_HZ / ArpTempo) / (double)ArpResolution;
                }
            }
        }
        
        private ArpResolution f_ArpResolution = ArpResolution.QuarterNote;

        [DataMember]
        [Description("Select Arpeggio Resolution")]
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
                    ArpStep = (60d * InstrumentManager.TIMER_HZ / ArpTempo) / (double)ArpResolution;
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

        private bool f_ArpKeySync;

        [DataMember]
        [Description("Select Arpeggio Key Sync Mode ")]
        public bool ArpKeySync
        {
            get
            {
                return f_ArpKeySync;
            }
            set
            {
                if (f_ArpKeySync != value)
                {
                    f_ArpKeySync = value;
                }
            }
        }

        #endregion

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

    }

}
