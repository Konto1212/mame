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
                    var lnote = arp.LastPassedNote;
                    if (lnote != null)
                        keyOffCore(new NoteOffEvent(lnote.NoteNumber, (SevenBitNumber)0) { Channel = lnote.Channel });
                    arp.ClearAddedNotes();
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
                    arp.ClearAddedNotes();
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
                        stopArpForKeyOn();
                        stopArpForPitch();
                        foreach (var snd in AllSounds)
                            KeyOff(new NoteOffEvent(snd.NoteOnEvent.NoteNumber, (SevenBitNumber)0) { Channel = snd.NoteOnEvent.Channel });
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
                if (!sds.Enable || arp.ArpType != sds.ArpType ||
                    sds.ArpMethod != ArpMethod.KeyOn ||
                    (!sds.Hold && arp.ResetNextNoteOn) ||
                    arp.AddedNotesCount == 0)
                {
                    stopArpForKeyOn(arp);
                    removeArps.Add(ch);
                    continue;
                }

                setupArp(sds, arp);

                if (arp.GateCounter != double.MinValue)
                {
                    arp.GateCounter += InstrumentManager.TIMER_INTERVAL;
                    if (arp.GateCounter >= arp.StepCounter * ((sds.GateTime + 1) / 128d))
                    {
                        //Gate Time
                        arp.GateCounter = double.MinValue;
                        keyOffCore(arp.LastPassedNote);
                    }
                }

                arp.Counter += InstrumentManager.TIMER_INTERVAL;
                if (arp.Counter >= arp.StepCounter)
                {
                    // on sound
                    arp.Counter -= arp.StepCounter;
                    arp.GateCounter = arp.Counter;
                    keyOnCore(arp.NextNote());
                }
            }

            //clear finished arps
            foreach (int ch in removeArps)
                ArpeggiatorsForKeyOn.Remove(ch);
            tryToDisableArpTimerForKeyOn();
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
                if (!sds.Enable || arp.ArpType != sds.ArpType ||
                    sds.ArpMethod != ArpMethod.PitchChange ||
                    (!sds.Hold && arp.ResetNextNoteOn) ||
                    arp.AddedNotesCount == 0)
                {
                    //off sound
                    stopArpForPitch(arp);
                    removeArps.Add(ch);
                    continue;
                }

                setupArp(sds, arp);

                //Gate Time
                if (arp.GateCounter != double.MinValue)
                {
                    arp.GateCounter += InstrumentManager.TIMER_INTERVAL;
                    if (arp.GateCounter >= arp.StepCounter * ((sds.GateTime + 1) / 128d))
                    {
                        arp.GateCounter = double.MinValue;
                        arp.FirstSoundForPitch.ArpeggiateVolume = 0d;
                    }
                }
                arp.Counter += InstrumentManager.TIMER_INTERVAL;
                if (arp.Counter >= arp.StepCounter)
                {
                    // on sound
                    arp.Counter -= arp.StepCounter;
                    arp.GateCounter = arp.Counter;
                    var note = arp.NextNote();
                    var snd = arp.FirstSoundForPitch;
                    snd.ArpeggiateDeltaNoteNumber = note.NoteNumber - arp.FirstAddedNote.NoteNumber;
                    snd.ArpeggiateVolume = 1d;
                }
            }

            //clear finished arps
            foreach (int ch in removeArps)
                ArpeggiatorsForPitch.Remove(ch);
            tryToDisableArpTimerForPitch();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="note"></param>
        public virtual void KeyOn(NoteOnEvent note)
        {
            if (preProcessArrpegioForKeyOn(note))
                return;

            var snd = keyOnCore(note);

            postProcessArrpegioForKeyOn(note, snd);
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
            if (preProcessArpeggioForKeyOff(note))
                return;

            keyOffCore(note);
        }

        private void keyOffCore(NoteOnEvent onote)
        {
            if (onote != null)
                keyOffCore(new NoteOffEvent(onote.NoteNumber, (SevenBitNumber)0) { Channel = onote.Channel });
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

        #region アルペジオ関係


        private bool preProcessArrpegioForKeyOn(NoteOnEvent note)
        {
            FourBitNumber ch = note.Channel;
            ARPSettings sds = parentModule.GetTimbre(ch).SDS.ARP;
            if (sds.Enable)
            {
                if (sds.ArpMethod == ArpMethod.KeyOn)
                {
                    //create Arpeggiator
                    if (!ArpeggiatorsForKeyOn.ContainsKey(ch))
                        ArpeggiatorsForKeyOn.Add(ch, new Arpeggiator());
                    var arp = ArpeggiatorsForKeyOn[ch];
                    setupArp(sds, arp);
                    if (sds.ArpType == ArpType.Dynamic)
                    {
                        //hold reset
                        if (arp.ResetNextNoteOn)
                        {
                            arp.ResetNextNoteOn = false;
                            keyOffCore(arp.LastPassedNote);
                            arp.ClearAddedNotes();
                        }
                        arp.AddNote(note);
                        //即音を鳴らすためカウンターを進める
                        if (arp.AddedNotesCount == 1)
                        {
                            arp.Counter = (60d * InstrumentManager.TIMER_HZ / sds.Beat) / (double)sds.ArpResolution;
                            arp.Counter -= InstrumentManager.TIMER_INTERVAL;
                        }
                    }
                    else if (sds.ArpType == ArpType.Static)
                    {
                        stopArpForKeyOn(arp);
                        arp.AddNote(note);
                        //即音を鳴らすためカウンターを進める
                        arp.Counter = (60d * InstrumentManager.TIMER_HZ / sds.Beat) / (double)sds.ArpResolution;
                        arp.Counter -= InstrumentManager.TIMER_INTERVAL;
                        foreach (int i in sds.StaticArpStepKeyNums)
                        {
                            int n = i;
                            if (sds.StaticArpStepType != CustomArpStepType.Fixed)
                                n = (int)note.NoteNumber + i;
                            if (n < 0)
                                n = 0;
                            else if (n > 127)
                                n = 127;
                            arp.AddNote(new NoteOnEvent((SevenBitNumber)n, note.Velocity) { Channel = ch });
                        }
                    }

                    tryToEnableArpTimerForKeyOn();
                    return true;
                }
                else if (sds.ArpMethod == ArpMethod.PitchChange)
                {
                    //create Arpeggiator
                    if (ArpeggiatorsForPitch.ContainsKey(ch))
                    {
                        var arp = ArpeggiatorsForPitch[ch];
                        if (sds.ArpType == ArpType.Dynamic)
                        {
                            setupArp(sds, arp);
                            if (arp.SkipNextNote == true)
                                arp.SkipNextNote = false;

                            //hold reset
                            if (arp.ResetNextNoteOn)
                            {
                                arp.ResetNextNoteOn = false;
                                stopArpForPitch(arp);
                                ArpeggiatorsForPitch.Remove(ch);
                            }
                            else
                            {
                                arp.AddNote(note);
                                return true;
                            }
                        }
                        else if (sds.ArpType == ArpType.Static)
                        {
                            stopArpForPitch(arp);
                            ArpeggiatorsForPitch.Remove(ch);
                        }
                    }
                }
            }
            return false;
        }


        private void postProcessArrpegioForKeyOn(NoteOnEvent note, SoundBase snd)
        {
            FourBitNumber ch = note.Channel;
            ARPSettings sds = parentModule.GetTimbre(ch).SDS.ARP;
            if (snd != null && sds.Enable)
            {
                if (sds.ArpMethod == ArpMethod.PitchChange)
                {
                    //create Arpeggiator
                    if (!ArpeggiatorsForPitch.ContainsKey(note.Channel))
                        ArpeggiatorsForPitch.Add(note.Channel, new Arpeggiator());
                    var arp = ArpeggiatorsForPitch[note.Channel];
                    setupArp(sds, arp);
                    arp.FirstSoundForPitch = snd;
                    arp.AddNote(note);
                    if (sds.ArpType == ArpType.Dynamic)
                    {
                        //すでに音が鳴っているのでスキップする
                        arp.SkipNextNote = true;
                    }
                    else if (sds.ArpType == ArpType.Static)
                    {
                        //カスタム値をすべて登録
                        foreach (int i in sds.StaticArpStepKeyNums)
                        {
                            int n = i;
                            if (sds.StaticArpStepType != CustomArpStepType.Fixed)
                                n = (int)note.NoteNumber + i;
                            if (n < 0)
                                n = 0;
                            else if (n > 127)
                                n = 127;
                            arp.AddNote(new NoteOnEvent((SevenBitNumber)n, note.Velocity) { Channel = ch });
                        }
                    }
                    tryToEnableArpTimerForPitch();
                }
            }
        }

        private bool preProcessArpeggioForKeyOff(NoteOffEvent note)
        {
            int ch = note.Channel;
            if (ArpeggiatorsForKeyOn.ContainsKey(ch))
            {
                var timbre = parentModule.GetTimbre(ch);
                var sds = timbre.SDS.ARP;
                Arpeggiator arp = ArpeggiatorsForKeyOn[ch];
                if (sds.Enable)
                {
                    //ignore keyoff if hold mode
                    if (sds.Hold)
                    {
                        arp.ResetNextNoteOn = true;
                        return true;
                    }
                }
                if (arp.RemoveNote(note))
                {
                    if (ArpeggiatorsForKeyOn[ch].LastPassedNote != null)
                        keyOffCore(ArpeggiatorsForKeyOn[ch].LastPassedNote);
                    if (arp.ArpType == ArpType.Static)
                        arp.ClearAddedNotes();

                    //全アルペジオ終了
                    if (arp.AddedNotesCount == 0)
                        ArpeggiatorsForKeyOn.Remove(ch);
                    tryToDisableArpTimerForKeyOn();
                    return true;
                }
            }
            else if (ArpeggiatorsForPitch.ContainsKey(ch))
            {
                var timbre = parentModule.GetTimbre(ch);
                var sds = timbre.SDS.ARP;
                Arpeggiator arp = ArpeggiatorsForPitch[ch];
                if (sds.Enable)
                {
                    //ignore keyoff if hold mode
                    if (sds.Hold)
                    {
                        arp.ResetNextNoteOn = true;
                        return true;
                    }
                }
                if (arp.ArpType == ArpType.Static)
                {
                    if (arp.FirstAddedNote.NoteNumber == note.NoteNumber)
                    {
                        arp.ClearAddedNotes();
                        keyOffCore(ArpeggiatorsForPitch[ch].FirstAddedNote);
                        ArpeggiatorsForPitch.Remove(ch);
                        tryToDisableArpTimerForPitch();
                        return true;
                    }
                }
                else
                {
                    if (arp.RemoveNote(note))
                    {
                        if (arp.AddedNotesCount == 0)
                        {
                            keyOffCore(ArpeggiatorsForPitch[ch].FirstAddedNote);
                            ArpeggiatorsForPitch.Remove(ch);
                            tryToDisableArpTimerForPitch();
                        }
                        return true;
                    }
                }
            }
            return false;
        }


        private static void setupArp(ARPSettings sds, Arpeggiator arp)
        {
            arp.StepStyle = sds.ArpType == ArpType.Static ? ArpStepStyle.Order : sds.StepStyle;
            arp.Range = sds.OctaveRange;
            arp.RetriggerType = sds.KeySync ? RetriggerType.Note : RetriggerType.Off;
            arp.StepCounter = (60d * InstrumentManager.TIMER_HZ / sds.Beat) / (double)sds.ArpResolution;
            arp.ArpType = sds.ArpType;
            arp.StaticArpStepType = sds.StaticArpStepType;
        }

        private void tryToEnableArpTimerForPitch()
        {
            ProcessArpeggiatorsForPitch();
            if (processArpeggiatorPitchAction == null)
            {
                processArpeggiatorPitchAction = new Action(ProcessArpeggiatorsForPitch);
                InstrumentManager.SetPeriodicCallback(processArpeggiatorPitchAction);
            }
        }

        private void tryToEnableArpTimerForKeyOn()
        {
            ProcessArpeggiatorsForKeyOn();
            if (processArpeggiatorKeyOnAction == null)
            {
                processArpeggiatorKeyOnAction = new Action(ProcessArpeggiatorsForKeyOn);
                InstrumentManager.SetPeriodicCallback(processArpeggiatorKeyOnAction);
            }
        }

        private void tryToDisableArpTimerForPitch()
        {
            //全アルペジオ終了
            if (ArpeggiatorsForPitch.Count != 0)
                return;

            if (processArpeggiatorPitchAction != null)
                InstrumentManager.UnsetPeriodicCallback(processArpeggiatorPitchAction);
            processArpeggiatorPitchAction = null;
        }

        private void tryToDisableArpTimerForKeyOn()
        {
            //全アルペジオ終了
            if (ArpeggiatorsForKeyOn.Count != 0)
                return;

            if (processArpeggiatorKeyOnAction != null)
                InstrumentManager.UnsetPeriodicCallback(processArpeggiatorKeyOnAction);
            processArpeggiatorKeyOnAction = null;
        }

        private void stopArpForPitch()
        {
            foreach (var ch in ArpeggiatorsForPitch.Keys)
            {
                var arp = ArpeggiatorsForPitch[ch];
                stopArpForPitch(arp);
            }
            ArpeggiatorsForPitch.Clear();
            tryToDisableArpTimerForPitch();
        }

        private void stopArpForKeyOn()
        {
            foreach (var ch in ArpeggiatorsForKeyOn.Keys)
            {
                var arp = ArpeggiatorsForKeyOn[ch];
                stopArpForKeyOn(arp);
            }
            ArpeggiatorsForKeyOn.Clear();
            tryToDisableArpTimerForKeyOn();
        }

        private void stopArpForPitch(Arpeggiator arp)
        {
            keyOffCore(arp.FirstAddedNote);
            arp.ClearAddedNotes();
        }

        private void stopArpForKeyOn(Arpeggiator arp)
        {
            keyOffCore(arp.LastPassedNote);
            arp.ClearAddedNotes();
        }


        #endregion

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
