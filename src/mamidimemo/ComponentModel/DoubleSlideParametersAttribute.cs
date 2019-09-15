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
    public class DoubleSlideParametersAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public double SliderMin
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public double SliderMax
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public double Precision
        {
            get;
            private set;
        }

        public DoubleSlideParametersAttribute(double min, double max, double precision)
        {
            SliderMax = max;
            SliderMin = min;
            Precision = precision;
        }
    }
}
