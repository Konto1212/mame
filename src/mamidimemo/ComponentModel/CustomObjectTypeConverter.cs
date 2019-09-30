// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Globalization;

namespace zanac.MAmidiMEmo.ComponentModel
{
    /// <summary>
    /// </summary>
    public class CustomObjectTypeConverter : TypeConverter
    {
        private int bitMask;

        /// <summary>
        /// 
        /// </summary>
        public CustomObjectTypeConverter()
        {
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return context.PropertyDescriptor.PropertyType.Name;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
