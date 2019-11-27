// copyright-holders:K.Ito
using Newtonsoft.Json;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Instruments.Envelopes;
using static zanac.MAmidiMEmo.Instruments.SoundBase;

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [JsonConverter(typeof(NoTypeConverterJsonConverter<SoundControlChangeSettings>))]
    [DataContract]
    [MidiHook]
    public class SoundControlChangeSettings : ContextBoundObject
    {

        [DataMember]
        [Description("Sound Control 1(Control Change No.70(0x46))\r\n" +
            "Link Data Entry message value with the Timbre property value (Only property that has slider editor)\r\n" +
            "eg) \"DutyCycle,Volume\" ... You can change DutyCycle and Volume property values dynamically via MIDI Control Change No.70 message.")]
        public string SoundControl1
        {
            get;
            set;
        }

        [DataMember]
        [Description("Sound Control 2(Control Change No.71(0x47))")]
        public string SoundControl2
        {
            get;
            set;
        }

        [DataMember]
        [Description("Sound Control 3(Control Change No.72(0x48))")]
        public string SoundControl3
        {
            get;
            set;
        }

        [DataMember]
        [Description("Sound Control 4(Control Change No.73(0x49))")]
        public string SoundControl4
        {
            get;
            set;
        }

        [DataMember]
        [Description("Sound Control 5(Control Change No.74(0x4A))")]
        public string SoundControl5
        {
            get;
            set;
        }

        [DataMember]
        [Description("Sound Control 6(Control Change No.75(0x4B))")]
        public string SoundControl6
        {
            get;
            set;
        }

        /*
        [DataMember]
        [Description("Sound Control 7(Control Change No.76(0x4C))")]
        public string SoundControl7
        {
            get;
            set;
        }

        [DataMember]
        [Description("Sound Control 8(Control Change No.77(0x4D))")]
        public string SoundControl8
        {
            get;
            set;
        }

        [DataMember]
        [Description("Sound Control 9(Control Change No.78(0x4E))")]
        public string SoundControl9
        {
            get;
            set;
        }
        */

        [DataMember]
        [Description("Sound Control 10(Control Change No.79(0x4F))")]
        public string SoundControl10
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public SoundControlChangeSettings()
        {

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="timbre"></param>
        /// <param name="controlNo">1～5</param>
        /// <returns></returns>
        public PropertyInfo[] GetPropertyInfo(TimbreBase timbre, int controlNo)
        {
            switch (controlNo)
            {
                case 1:
                    return getPropertiesInfo(timbre, SoundControl1);
                case 2:
                    return getPropertiesInfo(timbre, SoundControl2);
                case 3:
                    return getPropertiesInfo(timbre, SoundControl3);
                case 4:
                    return getPropertiesInfo(timbre, SoundControl4);
                case 5:
                    return getPropertiesInfo(timbre, SoundControl5);
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timbre"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private PropertyInfo[] getPropertiesInfo(TimbreBase timbre, string propertyNames)
        {
            string[] propNameParts = propertyNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<PropertyInfo> plist = new List<PropertyInfo>();

            foreach (var propertyName in propNameParts)
            {
                var pi = getPropertyInfo(timbre, propertyName);
                if (pi != null)
                {
                    SlideParametersAttribute attribute =
                        Attribute.GetCustomAttribute(pi, typeof(SlideParametersAttribute)) as SlideParametersAttribute;
                    if (attribute != null)
                        plist.Add(pi);
                    DoubleSlideParametersAttribute dattribute =
                        Attribute.GetCustomAttribute(pi, typeof(DoubleSlideParametersAttribute)) as DoubleSlideParametersAttribute;
                    if (dattribute != null)
                        plist.Add(pi);
                }
            }

            return plist.ToArray();
        }

        private PropertyInfo getPropertyInfo(TimbreBase timbre, string propertyName)
        {
            object obj = timbre;

            // Split property name to parts (propertyName could be hierarchical, like obj.subobj.subobj.property
            string[] propertyNameParts = propertyName.Split('.');

            PropertyInfo pi = null;
            foreach (string propertyNamePart in propertyNameParts)
            {
                if (obj == null)
                    return null;

                // propertyNamePart could contain reference to specific 
                // element (by index) inside a collection
                if (!propertyNamePart.Contains("["))
                {
                    pi = obj.GetType().GetProperty(propertyNamePart);
                    if (pi == null)
                        return null;
                    obj = pi.GetValue(obj, null);
                }
                else
                {   // propertyNamePart is areference to specific element 
                    // (by index) inside a collection
                    // like AggregatedCollection[123]
                    //   get collection name and element index
                    int indexStart = propertyNamePart.IndexOf("[") + 1;
                    string collectionPropertyName = propertyNamePart.Substring(0, indexStart - 1);
                    int collectionElementIndex = Int32.Parse(propertyNamePart.Substring(indexStart, propertyNamePart.Length - indexStart - 1));
                    //   get collection object
                    pi = obj.GetType().GetProperty(collectionPropertyName);
                    if (pi == null)
                        return null;
                    object unknownCollection = pi.GetValue(obj, null);
                    //   try to process the collection as array
                    if (unknownCollection.GetType().IsArray)
                    {
                        object[] collectionAsArray = unknownCollection as object[];
                        obj = collectionAsArray[collectionElementIndex];
                    }
                    else
                    {
                        //   try to process the collection as IList
                        System.Collections.IList collectionAsList = unknownCollection as System.Collections.IList;
                        if (collectionAsList != null)
                        {
                            obj = collectionAsList[collectionElementIndex];
                        }
                        else
                        {
                            // ??? Unsupported collection type
                        }
                    }
                }
            }

            return pi;
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
        [Description("You can copy and paste this text data to other same type timber.\r\nNote: Open dropdown editor then copy all text and paste to dropdown editor. Do not copy and paste one liner text.")]
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

}

