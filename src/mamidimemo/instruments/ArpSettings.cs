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

    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [JsonConverter(typeof(NoTypeConverterJsonConverter<ArpSettings>))]
    [DataContract]
    [MidiHook]
    public class ArpSettings : ContextBoundObject
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
        [Description("Set arpeggio type (Dynamic or Static)")]
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
        [Description("Set arpeggio method (Note On or Pitch Change)")]
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
        [Description("Set arpeggio step style *Dynamic Arp Only")]
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
        [Description("Set arpeggio octave range (1-4) *Dynamic Arp Only")]
        [DefaultValue(1)]
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
        [Description("Set arpeggio tempo (20-300)")]
        [DefaultValue(120)]
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

        private ArpResolution f_ArpResolution = ArpResolution.SixteenthNote;

        [DataMember]
        [Description("Set arpeggio resolution")]
        [DefaultValue(ArpResolution.SixteenthNote)]
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
                    ArpStep = (60d * HighPrecisionTimer.TIMER_BASIC_HZ / Beat) / (double)ArpResolution;
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
        [Description("Arpeggio Gate Time of NoteOn (0(0%)-127(100%))")]
        [DefaultValue(127)]
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
                    ArpStep = (60d * HighPrecisionTimer.TIMER_BASIC_HZ / Beat) / (double)ArpResolution;
                }
            }
        }

        private bool f_KeySync;

        [DataMember]
        [Description("When you press a key, arpeggio restart from first. ")]
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

        public string f_StaticArpSteps;

        [DataMember]
        [Description("Set static arp steps by text. Input note number and split it with space like the Famitracker.\r\n" +
                    "Absolute/Relative -64～0～+63\r\n" +
                    "Fixed is 0～127")]
        public string StaticArpSteps
        {
            get
            {
                return f_StaticArpSteps;
            }
            set
            {
                if (f_StaticArpSteps != value)
                {
                    if (value == null)
                    {
                        StaticArpStepKeyNums = new int[] { };
                        return;
                    }

                    string[] vals = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    List<int> vs = new List<int>();
                    foreach (var val in vals)
                    {
                        int v = 0;
                        if (int.TryParse(val, out v))
                        {
                            if (v < -64)
                                v = 64;
                            else if (v > 63)
                                v = 63;
                            vs.Add(v);
                        }
                    }
                    StaticArpStepKeyNums = vs.ToArray();

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < StaticArpStepKeyNums.Length; i++)
                    {
                        if (sb.Length != 0)
                            sb.Append(' ');
                        sb.Append(StaticArpStepKeyNums[i].ToString((IFormatProvider)null));
                    }
                    f_StaticArpSteps = sb.ToString();

                }
            }
        }

        [IgnoreDataMember]
        [JsonIgnore]
        [Browsable(false)]
        public int[] StaticArpStepKeyNums { get; set; } = new int[] { };

        private CustomArpStepType f_CustomArpStepType;


        [DataMember]
        [Description("Set static arp step type.")]
        public CustomArpStepType StaticArpStepType
        {
            get
            {
                return f_CustomArpStepType;
            }
            set
            {
                if (f_CustomArpStepType != value)
                {
                    f_CustomArpStepType = value;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ArpSettings()
        {
            ArpStep = (60d * HighPrecisionTimer.TIMER_BASIC_HZ / Beat) / (double)ArpResolution;
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
                var obj = JsonConvert.DeserializeObject<ArpSettings>(serializeData);
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
