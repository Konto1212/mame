// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.ComponentModel
{
    /// <summary>
    /// 
    /// </summary>
    public class CollectionDefaultValueAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public object DefaultValue
        {
            get;
            private set;
        }

        public CollectionDefaultValueAttribute(object defaultValue)
        {
            DefaultValue = defaultValue;
        }
    }
}
