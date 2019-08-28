// copyright-holders:K.Ito
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.ComponentModel;

namespace zanac.MAmidiMEmo.Instruments
{

    /// <summary>
    /// 
    /// </summary>
    public class SoundManagerBase : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        protected SoundList<SoundBase> AllOnSounds
        {
            get;
            private set;
        }

        /// <summary>
        /// ポルタメント用の最後に鳴ったキー番号
        /// </summary>
        private Dictionary<int, int> LastNoteNumbers = new Dictionary<int, int>();

        /// <summary>
        /// 
        /// </summary>
        public SoundManagerBase()
        {
            AllOnSounds = new SoundList<SoundBase>(-1);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
            for (int i = AllOnSounds.Count - 1; i >= 0; i--)
            {
                var removed = AllOnSounds[i];
                AllOnSounds.RemoveAt(i);
                removed.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        public virtual void PitchBend(PitchBendEvent midiEvent)
        {
            foreach (var t in AllOnSounds)
            {
                if (t.NoteOnEvent.Channel == midiEvent.Channel)
                {
                    t.UpdatePitch();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="value"></param>
        public virtual void ControlChange(ControlChangeEvent midiEvent)
        {
            switch (midiEvent.ControlNumber)
            {
                case 1:    //Modulation
                    foreach (var t in AllOnSounds)
                    {
                        if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            t.ModulationEnabled = midiEvent.ControlValue != 0;
                    }
                    break;
                case 6:    //Data Entry
                           //nothing
                    break;
                case 7:    //Volume
                    foreach (var t in AllOnSounds)
                    {
                        if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            t.UpdateVolume();
                    }
                    break;
                case 10:    //Panpot
                    foreach (var t in AllOnSounds)
                    {
                        if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            t.UpdatePanpot();
                    }
                    break;
                case 11:    //Expression
                    foreach (var t in AllOnSounds)
                    {
                        if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            t.UpdateVolume();
                    }
                    break;
                case 120:   //All Sounds Off
                case 123:   //All Note Off
                    {
                        foreach (var snd in AllOnSounds)
                        {
                            var noff = new NoteOffEvent(snd.NoteOnEvent.NoteNumber, (SevenBitNumber)0) { Channel = snd.NoteOnEvent.Channel };
                            NoteOff(noff);
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public int GetLastNoteNumber(int channel)
        {
            if (LastNoteNumbers.ContainsKey(channel))
                return LastNoteNumbers[channel];
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="note"></param>
        public virtual void NoteOn(NoteOnEvent note)
        {
            LastNoteNumbers[note.Channel] = note.NoteNumber;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sound"></param>
        internal void AddOnSound(SoundBase sound)
        {
            AllOnSounds.Add(sound);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="note"></param>
        public virtual SoundBase NoteOff(NoteOffEvent note)
        {
            SoundBase removed = null;
            for (int i = 0; i < AllOnSounds.Count; i++)
            {
                if (AllOnSounds[i].NoteOnEvent.Channel == note.Channel)
                {
                    if (AllOnSounds[i].NoteOnEvent.NoteNumber == note.NoteNumber)
                    {
                        removed = AllOnSounds[i];
                        AllOnSounds.RemoveAt(i);
                        removed.Dispose();
                        break;
                    }
                }
            }

            return removed;
        }

        /// <summary>
        /// 未使用のスロットを検索する
        /// 空が無い場合は最初に鳴った音を消す
        /// </summary>
        /// <param name="onSounds"></param>
        /// <param name="maxSlot"></param>
        /// <returns></returns>
        protected virtual int SearchEmptySlotAndOff<T>(SoundList<T> onSounds, NoteOnEvent newNote, int maxSlot) where T : SoundBase
        {
            int emptySlot = onSounds.GetEmptySlot(maxSlot);

            for (int i = 0; i < onSounds.Count; i++)
            {
                var snd = onSounds[i];
                if (emptySlot == snd.Slot)
                {
                    var noff = new NoteOffEvent(snd.NoteOnEvent.NoteNumber, (SevenBitNumber)0);
                    noff.Channel = snd.NoteOnEvent.Channel;
                    NoteOff(noff);
                    break;
                }
            }

            return emptySlot;
        }

    }
}
