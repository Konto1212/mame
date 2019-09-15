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
    public class SlideParametersAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public int SliderMin
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public int SliderMax
        {
            get;
            private set;
        }

        public SlideParametersAttribute(int min, int max)
        {
            SliderMax = max;
            SliderMin= min;
        }
    }
}
