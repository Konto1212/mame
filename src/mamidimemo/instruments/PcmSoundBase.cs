using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using zanac.MAmidiMEmo.ComponentModel;

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class PcmSoundBase
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String KeyName
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public int NoteNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String SoundName
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public virtual byte[] PcmData
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="noteNumber"></param>
        public PcmSoundBase(int noteNumber)
        {
            NoteNumber = noteNumber;
            var no = new NoteOnEvent((SevenBitNumber)NoteNumber, (SevenBitNumber)0);
            KeyName = no.GetNoteName() + no.GetNoteOctave().ToString();
        }
    }
}