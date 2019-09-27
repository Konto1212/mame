// copyright-holders:K.Ito
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class MOS8580 : SIDBase
    {

        public override string Name => "MOS8580";

        public override InstrumentType InstrumentType => InstrumentType.MOS8580;

        [Browsable(false)]
        public override string ImageKey => "MOS8580";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "mos8580_";

        [Browsable(false)]
        protected override string WriteProcName => "mos8580_write";

        /// <summary>
        /// 
        /// </summary>
        [Category("MIDI")]
        [Description("MIDI Device ID")]
        [IgnoreDataMember]
        [JsonIgnore]
        public override uint DeviceID
        {
            get
            {
                return 12;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public MOS8580(uint unitNumber) : base(unitNumber)
        {
            
        }

    }
}