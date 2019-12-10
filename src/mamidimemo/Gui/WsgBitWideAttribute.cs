// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Gui
{
    /// <summary>
    /// 
    /// </summary>
    public class WsgBitWideAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public int BitWide
        {
            get;
            private set;
        }

        public WsgBitWideAttribute(int bitWide)
        {
            BitWide = bitWide;
        }

    }
}
