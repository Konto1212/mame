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
        protected SoundList<SoundBase> AllSounds
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
            AllSounds = new SoundList<SoundBase>(-1);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
            for (int i = AllSounds.Count - 1; i >= 0; i--)
            {
                var removed = AllSounds[i];
                AllSounds.RemoveAt(i);
                removed.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        public virtual void PitchBend(PitchBendEvent midiEvent)
        {
            foreach (var t in AllSounds)
            {
                if (t.NoteOnEvent.Channel == midiEvent.Channel)
                    t.UpdatePitch();
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
                    foreach (var t in AllSounds)
                    {
                        if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            t.ModulationEnabled = midiEvent.ControlValue != 0;
                    }
                    break;
                case 6:    //Data Entry
                           //nothing
                    break;
                case 7:    //Volume
                    foreach (var t in AllSounds)
                    {
                        if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            t.UpdateVolume();
                    }
                    break;
                case 10:    //Panpot
                    foreach (var t in AllSounds)
                    {
                        if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            t.UpdatePanpot();
                    }
                    break;
                case 11:    //Expression
                    foreach (var t in AllSounds)
                    {
                        if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            t.UpdateVolume();
                    }
                    break;
                case 120:   //All Sounds Off
                case 123:   //All Note Off
                    {
                        foreach (var snd in AllSounds)
                        {
                            var noff = new NoteOffEvent(snd.NoteOnEvent.NoteNumber, (SevenBitNumber)0) { Channel = snd.NoteOnEvent.Channel };
                            KeyOff(noff);
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
        public virtual void KeyOn(NoteOnEvent note)
        {
            LastNoteNumbers[note.Channel] = note.NoteNumber;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sound"></param>
        internal void AddKeyOnSound(SoundBase sound)
        {
            AllSounds.Add(sound);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="note"></param>
        public virtual SoundBase KeyOff(NoteOffEvent note)
        {
            SoundBase offsnd = null;
            for (int i = 0; i < AllSounds.Count; i++)
            {
                if (AllSounds[i].IsKeyOff)
                    continue;

                if (AllSounds[i].NoteOnEvent.Channel == note.Channel)
                {
                    if (AllSounds[i].NoteOnEvent.NoteNumber == note.NoteNumber)
                    {
                        offsnd = AllSounds[i];
                        offsnd.KeyOff();
                        break;
                    }
                }
            }

            return offsnd;
        }

        /// <summary>
        /// 未使用のスロットを検索する
        /// 空が無い場合は最初に鳴った音を消す
        /// </summary>
        /// <param name="onSounds"></param>
        /// <param name="maxSlot"></param>
        /// <returns></returns>
        protected virtual int SearchEmptySlotAndOff<T>(List<T> onSounds, NoteOnEvent newNote, int maxSlot) where T : SoundBase
        {
            Dictionary<int, bool> usedTable = new Dictionary<int, bool>();
            for (int i = 0; i < onSounds.Count; i++)
                usedTable.Add(onSounds[i].Slot, true);
            //使っていないスロットがあればそれを返す
            for (int i = 0; i < maxSlot; i++)
            {
                if (!usedTable.ContainsKey(i))
                    return i;
            }

            //一番古いキーオフされたスロットを探す
            for (int i = 0; i < onSounds.Count; i++)
            {
                var snd = onSounds[i];
                if (snd.Slot < maxSlot && snd.IsKeyOff)
                {
                    AllSounds.Remove(snd);
                    onSounds.RemoveAt(i);
                    snd.Dispose();
                    return snd.Slot;
                }
            }
            //一番古いキーオンされたスロットを探す
            for (int i = 0; i < onSounds.Count; i++)
            {
                var snd = onSounds[i];
                if (snd.Slot < maxSlot)
                {
                    AllSounds.Remove(snd);
                    onSounds.RemoveAt(i);
                    snd.Dispose();
                    return snd.Slot;
                }
            }
 
            return -1;
        }

    }
}
