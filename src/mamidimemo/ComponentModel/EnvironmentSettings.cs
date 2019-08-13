using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.Instruments;

namespace zanac.MAmidiMEmo.ComponentModel
{
    /// <summary>
    /// 
    /// </summary>
    public class EnvironmentSettings
    {
        [DataMember]
        public List<YM2151> YM2151
        {
            get;
            set;
        }

        [DataMember]
        public List<YM2612> YM2612
        {
            get;
            set;
        }

        [DataMember]
        public List<NAMCO_CUS30> NAMCO_CUS30
        {
            get;
            set;
        }

        [DataMember]
        public List<SN76496> SN76496
        {
            get;
            set;
        }

        [DataMember]
        public List<GB_APU> GB_APU
        {
            get;
            set;
        }

        [DataMember]
        public List<RP2A03> RP2A03
        {
            get;
            set;
        }
    }

}
