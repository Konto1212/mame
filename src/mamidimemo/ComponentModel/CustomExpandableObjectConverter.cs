// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace zanac.MAmidiMEmo.ComponentModel
{
    /// <summary>
    /// </summary>
    public class CustomExpandableObjectConverter : ExpandableObjectConverter
    {

        public CustomExpandableObjectConverter()
        {
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (destinationType == null)
                throw new ArgumentNullException("destinationType");

            if (destinationType == typeof(string))
                return context.PropertyDescriptor.PropertyType.Name + context.PropertyDescriptor.DisplayName;

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            // TypeDescriptorを使用してプロパティ一覧を取得する
            PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(value, attributes);

            // プロパティ一覧をリフレクションから取得
            Type type = value.GetType();
            List<string> list = new List<string>();
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                if (propertyInfo.Name.Equals("Enable"))
                    list.Insert(0, propertyInfo.Name);
                list.Add(propertyInfo.Name);
            }
            // リフレクションから取得した順でソート
            return pdc.Sort(list.ToArray());
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

    }
}
