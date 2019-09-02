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
        protected Dictionary<int, Arpeggiator> ArpeggiatorsForKeyOn
        {
            get;
            private set;
        }

        /// 
        /// </summary>
        protected Dictionary<int, Arpeggiator> ArpeggiatorsForPitch
        {
            get;
            private set;
        }

        private Action processArpeggiatorKeyOnAction;

        private Action processArpeggiatorPitchAction;

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
            ArpeggiatorsForKeyOn = new Dictionary<int, Arpeggiator>();
            ArpeggiatorsForPitch = new Dictionary<int, Arpeggiator>();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
            {
                foreach (var ch in ArpeggiatorsForKeyOn.Keys)
                {
                    var arp = ArpeggiatorsForKeyOn[ch];
                    var lnote = arp.LastSoundNote;
                    if (lnote != null)
                        keyOffCore(new NoteOffEvent(lnote.NoteNumber, (SevenBitNumber)0) { Channel = lnote.Channel });
                    arp.ClearNotes();
                }
                ArpeggiatorsForKeyOn.Clear();
                if (processArpeggiatorKeyOnAction != null)
                {
                    InstrumentManager.UnsetPeriodicCallback(processArpeggiatorKeyOnAction);
                    processArpeggiatorKeyOnAction = null;
                }
            }
            {
                foreach (var ch in ArpeggiatorsForPitch.Keys)
                {
                    var arp = ArpeggiatorsForPitch[ch];
                    var lnote = arp.FirstAddedNote;
                    if (lnote != null)
                        keyOffCore(new NoteOffEvent(lnote.NoteNumber, (SevenBitNumber)0) { Channel = lnote.Channel });
                    arp.ClearNotes();
                }
                ArpeggiatorsForPitch.Clear();
                if (processArpeggiatorPitchAction != null)
                {
                    InstrumentManager.UnsetPeriodicCallback(processArpeggiatorPitchAction);
                    processArpeggiatorPitchAction = null;
                }
            }

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
                        //clear all arps
                        {
                            foreach (var ch in ArpeggiatorsForKeyOn.Keys)
                            {
                                var arp = ArpeggiatorsForKeyOn[ch];
                                var lnote = arp.LastSoundNote;
                                if (lnote != null)
                                    keyOffCore(new NoteOffEvent(lnote.NoteNumber, (SevenBitNumber)0) { Channel = lnote.Channel });
                                arp.ClearNotes();
                            }
                            ArpeggiatorsForKeyOn.Clear();
                            if (processArpeggiatorKeyOnAction != null)
                            {
                                InstrumentManager.UnsetPeriodicCallback(processArpeggiatorKeyOnAction);
                                processArpeggiatorKeyOnAction = null;
                            }
                        }
                        {
                            foreach (var ch in ArpeggiatorsForPitch.Keys)
                            {
                                var arp = ArpeggiatorsForPitch[ch];
                                var lnote = arp.FirstAddedNote;
                                if (lnote != null)
                                    keyOffCore(new NoteOffEvent(lnote.NoteNumber, (SevenBitNumber)0) { Channel = lnote.Channel });
                                arp.ClearNotes();
                            }
                            ArpeggiatorsForPitch.Clear();
                            if (processArpeggiatorPitchAction != null)
                            {
                                InstrumentManager.UnsetPeriodicCallback(processArpeggiatorPitchAction);
                                processArpeggiatorPitchAction = null;
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


        /// <summary>
        /// 
        /// </summary>
        private void ProcessArpeggiatorsForKeyOn()
        {
            List<int> removeArps = new List<int>();
            foreach (var ch in ArpeggiatorsForKeyOn.Keys)
            {
                var arp = ArpeggiatorsForKeyOn[ch];
                var timbre = parentModule.GetTimbre(ch);
                var sds = timbre.SDS.ARP;
                //end arp
                if (!sds.Enable || sds.ArpMethod != ArpMethod.NoteOn ||
                    (!sds.Hold && arp.ResetNextNoteOn))
                {
                    //off last sound
                    var note = arp.LastSoundNote;
                    if (note != null)
                        keyOffCore(new NoteOffEvent(note.NoteNumber, (SevenBitNumber)0) { Channel = note.Channel });
                    arp.ClearNotes();
                    removeArps.Add(ch);
                    continue;
                }

                arp.StepStyle = sds.StepStyle;
                arp.Range = sds.OctaveRange;
                if (sds.KeySync)
                    arp.RetriggerType = RetriggerType.Note;
                arp.CounterStep = (60d * InstrumentManager.TIMER_HZ / sds.Beat) / (double)sds.ArpResolution;
                if (arp.GateCounter != double.MinValue)
                {
                    arp.GateCounter += InstrumentManager.TIMER_INTERVAL;
                    if (arp.GateCounter >= arp.CounterStep * ((sds.GateTime + 1) / 128d))
                    {
                        arp.GateCounter = double.MinValue;
                        // key off lsat note
                        var note = arp.LastSoundNote;
                        if (note != null)
                            keyOffCore(new NoteOffEvent(note.NoteNumber, (SevenBitNumber)0) { Channel = note.Channel });
                    }
                }
                arp.Counter += InstrumentManager.TIMER_INTERVAL;
                // key on
                if (arp.Counter >= arp.CounterStep)
                {
                    arp.Counter -= arp.CounterStep;
                    arp.GateCounter = arp.Counter;
                    var note = arp.Next();
                    keyOnCore(new NoteOnEvent(note.NoteNumber, note.Velocity) { Channel = note.Channel });
                }
            }

            foreach (int ch in removeArps)
                ArpeggiatorsForKeyOn.Remove(ch);
            //全アルペジオ終了
            if (ArpeggiatorsForKeyOn.Count == 0)
            {
                InstrumentManager.UnsetPeriodicCallback(processArpeggiatorKeyOnAction);
                processArpeggiatorKeyOnAction = null;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private void ProcessArpeggiatorsForPitch()
        {
            List<int> removeArps = new List<int>();
            foreach (var ch in ArpeggiatorsForPitch.Keys)
            {
                var arp = ArpeggiatorsForPitch[ch];
                var timbre = parentModule.GetTimbre(ch);
                var sds = timbre.SDS.ARP;
                //end arp
                if (!sds.Enable || sds.ArpMethod != ArpMethod.Pitch ||
                    (!sds.Hold && arp.ResetNextNoteOn))
                {
                    //off sound
                    var note = arp.FirstAddedNote;
                    if (note != null)
                        keyOffCore(new NoteOffEvent(note.NoteNumber, (SevenBitNumber)0) { Channel = note.Channel });
                    arp.ClearNotes();
                    removeArps.Add(ch);
                    continue;
                }

                arp.StepStyle = sds.StepStyle;
                arp.Range = sds.OctaveRange;
                if (sds.KeySync)
                    arp.RetriggerType = RetriggerType.Note;
                arp.CounterStep = (60d * InstrumentManager.TIMER_HZ / sds.Beat) / (double)sds.ArpResolution;
                if (arp.GateCounter != double.MinValue)
                {
                    arp.GateCounter += InstrumentManager.TIMER_INTERVAL;
                    if (arp.GateCounter >= arp.CounterStep * ((sds.GateTime + 1) / 128d))
                    {
                        arp.GateCounter = double.MinValue;
                        arp.FirstAddedNoteSound.ArpeggiateVolume = 0d;
                    }
                }
                arp.Counter += InstrumentManager.TIMER_INTERVAL;
                // on sound
                if (arp.Counter >= arp.CounterStep)
                {
                    arp.Counter -= arp.CounterStep;
                    arp.GateCounter = arp.Counter;
                    var note = arp.Next();

                    var snd = arp.FirstAddedNoteSound;
                    snd.ArpeggiateDeltaNoteNumber = note.NoteNumber - arp.FirstAddedNote.NoteNumber;
                    snd.ArpeggiateVolume = 1d;
                }
            }

            foreach (int ch in removeArps)
                ArpeggiatorsForPitch.Remove(ch);
            //全アルペジオ終了
            if (ArpeggiatorsForPitch.Count == 0)
            {
                InstrumentManager.UnsetPeriodicCallback(processArpeggiatorPitchAction);
                processArpeggiatorPitchAction = null;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="note"></param>
        public virtual void KeyOn(NoteOnEvent note)
        {
            var ch = note.Channel;
            var timbre = parentModule.GetTimbre(ch);
            var sds = timbre.SDS.ARP;

            if (sds.Enable)
            {
                if (sds.ArpMethod == ArpMethod.NoteOn)
                {
                    //create Arpeggiator
                    if (!ArpeggiatorsForKeyOn.ContainsKey(ch))
                        ArpeggiatorsForKeyOn.Add(ch, new Arpeggiator());

                    var arp = ArpeggiatorsForKeyOn[ch];
                    arp.StepStyle = sds.StepStyle;
                    arp.Range = sds.OctaveRange;
                    if (sds.KeySync)
                        arp.RetriggerType = RetriggerType.Note;
                    arp.CounterStep = (60d * InstrumentManager.TIMER_HZ / sds.Beat) / (double)sds.ArpResolution;
                    //hold reset
                    if (arp.ResetNextNoteOn)
                    {
                        arp.ResetNextNoteOn = false;
                        var lnote = arp.LastSoundNote;
                        if (lnote != null)
                            keyOffCore(new NoteOffEvent(lnote.NoteNumber, (SevenBitNumber)0) { Channel = ch });
                        arp.ClearNotes();
                    }
                    arp.AddNote(note);
                    //enable Arpeggiator
                    if (processArpeggiatorKeyOnAction == null)
                    {
                        processArpeggiatorKeyOnAction = new Action(ProcessArpeggiatorsForKeyOn);
                        InstrumentManager.SetPeriodicCallback(processArpeggiatorKeyOnAction);
                    }
                    return;
                }
                else if (sds.ArpMethod == ArpMethod.Pitch)
                {
                    //create Arpeggiator
                    if (ArpeggiatorsForPitch.ContainsKey(ch))
                    {
                        var arp = ArpeggiatorsForPitch[ch];
                        arp.StepStyle = sds.StepStyle;
                        arp.Range = sds.OctaveRange;
                        if (sds.KeySync)
                            arp.RetriggerType = RetriggerType.Note;
                        arp.CounterStep = (60d * InstrumentManager.TIMER_HZ / sds.Beat) / (double)sds.ArpResolution;
                        //hold reset
                        if (arp.ResetNextNoteOn)
                        {
                            arp.ResetNextNoteOn = false;
                            var lnote = arp.FirstAddedNote;
                            if (lnote != null)
                                keyOffCore(new NoteOffEvent(lnote.NoteNumber, (SevenBitNumber)0) { Channel = ch });
                            arp.ClearNotes();
                            ArpeggiatorsForPitch.Remove(ch);
                        }
                        else
                        {
                            arp.AddNote(note);
                            return;
                        }
                    }
                }
            }

            var snd = keyOnCore(note);

            if (snd != null && sds.Enable && sds.ArpMethod == ArpMethod.Pitch)
            {
                //create Arpeggiator
                if (!ArpeggiatorsForPitch.ContainsKey(note.Channel))
                {
                    ArpeggiatorsForPitch.Add(note.Channel, new Arpeggiator());

                    var arp = ArpeggiatorsForPitch[note.Channel];
                    arp.FirstAddedNoteSound = snd;
                    arp.StepStyle = sds.StepStyle;
                    arp.Range = sds.OctaveRange;
                    if (sds.KeySync)
                        arp.RetriggerType = RetriggerType.Note;
                    arp.CounterStep = (60d * InstrumentManager.TIMER_HZ / sds.Beat) / (double)sds.ArpResolution;

                    arp.AddNote(note);
                    arp.SkipNextNote = true;
                    //enable Arpeggiator
                    if (processArpeggiatorPitchAction == null)
                    {
                        processArpeggiatorPitchAction = new Action(ProcessArpeggiatorsForPitch);
                        InstrumentManager.SetPeriodicCallback(processArpeggiatorPitchAction);
                    }
                }
            }
        }

        private SoundBase keyOnCore(NoteOnEvent note)
        {
            var snd = SoundOn(note);
            if (snd != null)
            {
                AllSounds.Add(snd);
                LastNoteNumbers[note.Channel] = note.NoteNumber;
                return snd;
            }
            return null;
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
            if (ArpeggiatorsForKeyOn.ContainsKey(ch))
            {
                var timbre = parentModule.GetTimbre(note.Channel);
                var sds = timbre.SDS.ARP;
                Arpeggiator arp = ArpeggiatorsForKeyOn[ch];
                if (sds.Enable)
                {
                    //ignore keyoff if hold mode
                    if (sds.Hold)
                    {
                        arp.ResetNextNoteOn = true;
                        return;
                    }
                }
                if (arp.RemoveNote(note))
                {
                    if (ArpeggiatorsForKeyOn[ch].LastSoundNote != null)
                    {
                        var onote = ArpeggiatorsForKeyOn[ch].LastSoundNote;
                        keyOffCore(new NoteOffEvent(onote.NoteNumber, (SevenBitNumber)0) { Channel = onote.Channel });
                    }
                    //全アルペジオ終了
                    if (arp.ArpNotesCount == 0)
                        ArpeggiatorsForKeyOn.Remove(ch);
                    if (ArpeggiatorsForKeyOn.Count == 0)
                    {
                        InstrumentManager.UnsetPeriodicCallback(processArpeggiatorKeyOnAction);
                        processArpeggiatorKeyOnAction = null;
                    }
                    return;
                }
            }
            else if (ArpeggiatorsForPitch.ContainsKey(ch))
            {
                var timbre = parentModule.GetTimbre(note.Channel);
                var sds = timbre.SDS.ARP;
                Arpeggiator arp = ArpeggiatorsForPitch[ch];
                if (sds.Enable)
                {
                    //ignore keyoff if hold mode
                    if (sds.Hold)
                    {
                        arp.ResetNextNoteOn = true;
                        return;
                    }
                }
                if (arp.RemoveNote(note))
                {
                    if (arp.ArpNotesCount == 0)
                    {
                        var onote = ArpeggiatorsForPitch[ch].FirstAddedNote;
                        keyOffCore(new NoteOffEvent(onote.NoteNumber, (SevenBitNumber)0) { Channel = onote.Channel });
                    }
                    //全アルペジオ終了
                    if (arp.ArpNotesCount == 0)
                        ArpeggiatorsForPitch.Remove(ch);
                    if (ArpeggiatorsForPitch.Count == 0)
                    {
                        InstrumentManager.UnsetPeriodicCallback(processArpeggiatorPitchAction);
                        processArpeggiatorPitchAction = null;
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
