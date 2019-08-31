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
    public abstract class SoundManagerBase : IDisposable
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
        /// 
        /// </summary>
        protected Dictionary<int, Arpeggiator> Arpeggiators
        {
            get;
            private set;
        }

        /// <summary>
        /// ポルタメント用の最後に鳴ったキー番号
        /// </summary>
        private Dictionary<int, int> LastNoteNumbers = new Dictionary<int, int>();

        private InstrumentBase parentModule;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SoundManagerBase(InstrumentBase parentModule)
        {
            this.parentModule = parentModule;
            AllSounds = new SoundList<SoundBase>(-1);
            Arpeggiators = new Dictionary<int, Arpeggiator>();
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
                        {
                            foreach (var ch in Arpeggiators.Keys)
                            {
                                var arp = Arpeggiators[ch];
                                var lnote = arp.LastNote;
                                if (lnote != null)
                                    keyOffCore(new NoteOffEvent(lnote.NoteNumber, (SevenBitNumber)0) { Channel = lnote.Channel });
                                arp.ClearNotes();
                            }
                            Arpeggiators.Clear();
                            if (processArpeggiatorAction != null)
                            {
                                InstrumentManager.UnsetPeriodicCallback(processArpeggiatorAction);
                                processArpeggiatorAction = null;
                            }
                        }
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

        private Action processArpeggiatorAction = null;

        /// <summary>
        /// 
        /// </summary>
        private void ProcessArpeggiators()
        {
            List<int> removeArps = new List<int>();
            foreach (var ch in Arpeggiators.Keys)
            {
                var arp = Arpeggiators[ch];
                var timbre = parentModule.GetTimbre(ch);
                var sds = timbre.SDS;
                //end arp
                if (!sds.ArpEnable || sds.ArpMethod != ArpMethod.NoteOn)
                {
                    if (Arpeggiators[ch].LastNote != null)
                    {
                        var note = arp.LastNote;
                        KeyOff(new NoteOffEvent(note.NoteNumber, (SevenBitNumber)0) { Channel = note.Channel });
                    }
                    arp.ClearNotes();
                    removeArps.Add(ch);
                    continue;
                }

                arp.StepStyle = sds.ArpStepStyle;
                arp.Range = sds.ArpRange;
                if (sds.ArpKeySync)
                    arp.RetriggerType = RetriggerType.Note;
                arp.CounterStep = (60d * InstrumentManager.TIMER_HZ / sds.ArpTempo) / (double)sds.ArpResolution;
                if (arp.GateCounter != double.MinValue)
                {
                    arp.GateCounter += InstrumentManager.TIMER_INTERVAL;
                    if (arp.GateCounter >= arp.CounterStep * ((sds.ArpGate + 1) / 128d))
                    {
                        arp.GateCounter = double.MinValue;
                        //if (arp.ArpNotesCount > 1)
                        {
                            // key off lsat note
                            var note = arp.LastNote;
                            if (note != null)// && arp.ArpNotesCount > 1)  //1つしかなければ消さない
                                keyOffCore(new NoteOffEvent(note.NoteNumber, (SevenBitNumber)0) { Channel = note.Channel });
                        }
                    }
                }
                arp.Counter += InstrumentManager.TIMER_INTERVAL;
                if (arp.Counter >= arp.CounterStep)
                {
                    arp.Counter -= arp.CounterStep;
                    arp.GateCounter = arp.Counter;
                    //key on new note(同じ音の場合は繰り返さない)
                    var lnote = arp.LastNote;
                    var note = Arpeggiators[ch].Next();
                    //if (lnote == null || note.NoteNumber != lnote.NoteNumber)
                    keyOnCore(new NoteOnEvent(note.NoteNumber, note.Velocity) { Channel = note.Channel });
                }
            }

            foreach (int ch in removeArps)
                Arpeggiators.Remove(ch);
            //全アルペジオ終了
            if (Arpeggiators.Count == 0)
            {
                InstrumentManager.UnsetPeriodicCallback(processArpeggiatorAction);
                processArpeggiatorAction = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="note"></param>
        public virtual void KeyOn(NoteOnEvent note)
        {
            var timbre = parentModule.GetTimbre(note.Channel);
            var sds = timbre.SDS;
            if (sds.ArpEnable && sds.ArpMethod == ArpMethod.NoteOn)
            {
                //create Arpeggiator
                if (!Arpeggiators.ContainsKey(note.Channel))
                    Arpeggiators.Add(note.Channel, new Arpeggiator());

                var arp = Arpeggiators[note.Channel];
                arp.StepStyle = sds.ArpStepStyle;
                arp.Range = sds.ArpRange;
                if(sds.ArpKeySync)
                    arp.RetriggerType = RetriggerType.Note;
                arp.CounterStep = (60d * InstrumentManager.TIMER_HZ / sds.ArpTempo) / (double)sds.ArpResolution;
                //hold reset
                if (arp.ResetNextNoteOn)
                {
                    arp.ResetNextNoteOn = false;
                    var lnote = arp.LastNote;
                    if (lnote != null)
                        keyOffCore(new NoteOffEvent(lnote.NoteNumber, (SevenBitNumber)0) { Channel = lnote.Channel });
                    arp.ClearNotes();
                }
                arp.AddNote(note);
                //enable Arpeggiator
                if (processArpeggiatorAction == null)
                {
                    ProcessArpeggiators();
                    processArpeggiatorAction = new Action(ProcessArpeggiators);
                    InstrumentManager.SetPeriodicCallback(processArpeggiatorAction);
                }
                return;
            }

            keyOnCore(note);
        }

        private void keyOnCore(NoteOnEvent note)
        {
            var snd = SoundOn(note);
            if (snd != null)
            {
                AllSounds.Add(snd);
                LastNoteNumbers[note.Channel] = note.NoteNumber;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="note"></param>
        public virtual SoundBase SoundOn(NoteOnEvent note)
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="note"></param>
        public virtual void KeyOff(NoteOffEvent note)
        {
            int ch = note.Channel;
            if (Arpeggiators.ContainsKey(ch))
            {
                var timbre = parentModule.GetTimbre(note.Channel);
                var arp = Arpeggiators[ch];
                if (timbre.SDS.ArpEnable && timbre.SDS.ArpMethod == ArpMethod.NoteOn)
                {
                    //ignore keyoff if hold mode
                    if (timbre.SDS.ArpHold)
                    {
                        arp.ResetNextNoteOn = true;
                        return;
                    }
                }
                if (arp.RemoveNote(note))
                {
                    if (Arpeggiators[ch].LastNote != null)
                    {
                        var onote = Arpeggiators[ch].LastNote;
                        keyOffCore(new NoteOffEvent(onote.NoteNumber, (SevenBitNumber)0) { Channel = onote.Channel });
                    }
                    //全アルペジオ終了
                    if (arp.ArpNotesCount == 0)
                        Arpeggiators.Remove(ch);
                    if (Arpeggiators.Count == 0)
                    {
                        InstrumentManager.UnsetPeriodicCallback(processArpeggiatorAction);
                        processArpeggiatorAction = null;
                    }
                    return;
                }
            }

            keyOffCore(note);
        }

        private SoundBase keyOffCore(NoteOffEvent note)
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
