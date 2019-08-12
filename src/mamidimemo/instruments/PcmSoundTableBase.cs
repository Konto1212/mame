using System.ComponentModel;
using System.Runtime.Serialization;

//http://www.smspower.org/Development/SN76489
//http://www.st.rim.or.jp/~nkomatsu/peripheral/SN76489.html

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class PcmSoundTableBase
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public PcmSoundBase[] PcmSounds
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public PcmSoundTableBase()
        {
            PcmSounds = new PcmSoundBase[128];
        }
    }
}