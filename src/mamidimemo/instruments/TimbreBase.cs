using Newtonsoft.Json;
using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using zanac.mamidimemo.ComponentModel;

namespace zanac.mamidimemo.instruments
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [JsonConverter(typeof(NoTypeConverterJsonConverter<TimbreBase>))]
    [DataContract]
    public abstract class TimbreBase
    {
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
                return JsonConvert.SerializeObject(this, Formatting.None);
            }
            set
            {
                RestoreFrom(value);
            }
        }

        public abstract void RestoreFrom(string serializeData);

    }
}
