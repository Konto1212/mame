using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using zanac.MAmidiMEmo.ComponentModel;

//http://www.smspower.org/Development/SN76489
//http://www.st.rim.or.jp/~nkomatsu/peripheral/SN76489.html

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [MidiHook]
    public class PcmTimbreTableBase : ContextBoundObject
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public PcmTimbreBase[] PcmTimbres
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public PcmTimbreTableBase()
        {
            PcmTimbres = new PcmTimbreBase[128];
        }
    }
}