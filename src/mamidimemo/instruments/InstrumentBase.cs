using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.mamidimemo.instruments
{
    public class InstrumentBase
    {

        /// <summary>
        /// 
        /// </summary>
        public InstrumentBase()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        internal void NotifyMidiEvent(MidiEvent midiEvent)
        {
            OnMidiEvent(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnMidiEvent(MidiEvent midiEvent)
        {
            var non = midiEvent as NoteOnEvent;
            if (non != null)
            {
                if (non.Velocity == 0)
                    OnNoteOffEvent(new NoteOffEvent(non.NoteNumber, (SevenBitNumber)0) { Channel = non.Channel, DeltaTime = non.DeltaTime });
                else
                    OnNoteOnEvent(non);
            }
            else
            {
                var noff = midiEvent as NoteOffEvent;
                if (noff != null)
                {
                    OnNoteOffEvent(noff);
                }
                else
                {
                    var cont = midiEvent as ControlChangeEvent;
                    if (cont != null)
                    {
                        OnControlChangeEvent(cont);
                    }
                    else
                    {
                        var prog = midiEvent as ProgramChangeEvent;
                        if (prog != null)
                        {
                            OnProgramChangeEvent(prog);
                        }
                        else
                        {
                            var pitch = midiEvent as PitchBendEvent;
                            if (pitch != null)
                            {
                                OnPitchBendEvent(pitch);
                            }
                            else
                            {
                                //TODO: key/ch pressure
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnNoteOnEvent(NoteOnEvent midiEvent)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnNoteOffEvent(NoteOffEvent midiEvent)
        {

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnControlChangeEvent(ControlChangeEvent midiEvent)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnProgramChangeEvent(ProgramChangeEvent midiEvent)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnPitchBendEvent(PitchBendEvent midiEvent)
        {

        }
    }
}
