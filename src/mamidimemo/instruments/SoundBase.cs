using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Smf;

namespace zanac.MAmidiMEmo.Instruments
{

    /// <summary>
    /// 
    /// </summary>
    public abstract class SoundBase : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public int Slot
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public NoteOnEvent NoteOnEvent
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slot"></param>
        protected SoundBase(NoteOnEvent noteOnEvent, int slot)
        {
            NoteOnEvent = noteOnEvent;
            Slot = slot;
        }

        /// <summary>
        /// 
        /// </summary>
        public abstract void On();

        /// <summary>
        /// 
        /// </summary>
        public abstract void Off();

        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
            Off();
        }
    }
}
