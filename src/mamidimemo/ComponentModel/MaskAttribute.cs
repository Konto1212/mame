// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.ComponentModel
{
    /// <summary>
    /// 
    /// </summary>
    public class MaskAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public uint MaskValue
        {
            get;
            private set;
        }

        public MaskAttribute(uint maskValue)
        {
            MaskValue = maskValue;
        }
    }
}
