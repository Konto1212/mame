using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Smf;
using zanac.mamidimemo.mame;
using zanac.mamidimemo.midi;

namespace zanac.mamidimemo.instruments
{
    /// <summary>
    /// 
    /// </summary>
    public class YM2612 : InstrumentBase
    {

        public override string Name => "YM2612";

        public override InstrumentType InstrumentType => InstrumentType.YM2612;

        [Browsable(false)]
        public override string ImageKey => "YM2612";


        public YM2612Timbre[] Timbres
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_ym2612_write(uint unitNumber, uint address, byte data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_ym2612_write Ym2612_write
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static void Ym2612WriteData(uint unitNumber, byte address, int op, int slot, byte data)
        {
            uint reg = (uint)(slot / 3) * 2;
            Ym2612_write?.Invoke(unitNumber, reg + 0, (byte)(address + (op * 4) + (slot % 3)));
            Ym2612_write?.Invoke(unitNumber, reg + 1, data);
        }

        /// <summary>
        /// 
        /// </summary>
        static YM2612()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("ym2612_write");
            if (funcPtr != IntPtr.Zero)
            {
                Ym2612_write = (delegate_ym2612_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_ym2612_write));
            }
        }

        private YM2612SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public YM2612(uint unitNumber) : base(unitNumber)
        {
            Timbres = new YM2612Timbre[128];
            setPresetInstruments();

            this.soundManager = new YM2612SoundManager(this);
        }

        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {
            //Brass Section.dmp
            Timbres[0].SoundType = SoundType.FM;

            Timbres[0].FMS = 1;
            Timbres[0].AMS = 0;
            Timbres[0].FB = 7;
            Timbres[0].ALG = 3;

            Timbres[0]._Op1.AR = 31;
            Timbres[0]._Op1.D1R = 6;
            Timbres[0]._Op1.SL = 15;
            Timbres[0]._Op1.D2R = 0;
            Timbres[0]._Op1.RR = 7;

            Timbres[0]._Op1.MUL = 1;
            Timbres[0]._Op1.RS = 0;
            Timbres[0]._Op1.DT1 = 7;
            Timbres[0]._Op1.AM = 0;
            Timbres[0]._Op1.SSG_EG = 0;
            Timbres[0]._Op1.TL = 20;

            Timbres[0]._Op2.AR = 31;
            Timbres[0]._Op2.D1R = 7;
            Timbres[0]._Op2.SL = 4;
            Timbres[0]._Op2.D2R = 0;
            Timbres[0]._Op2.RR = 15;

            Timbres[0]._Op2.MUL = 2;
            Timbres[0]._Op2.RS = 0;
            Timbres[0]._Op2.DT1 = 6;
            Timbres[0]._Op2.AM = 0;
            Timbres[0]._Op2.SSG_EG = 0;
            Timbres[0]._Op2.TL = 21;

            Timbres[0]._Op3.AR = 31;
            Timbres[0]._Op3.D1R = 7;
            Timbres[0]._Op3.SL = 4;
            Timbres[0]._Op3.D2R = 0;
            Timbres[0]._Op3.RR = 15;

            Timbres[0]._Op3.MUL = 1;
            Timbres[0]._Op3.RS = 0;
            Timbres[0]._Op3.DT1 = 2;
            Timbres[0]._Op3.AM = 0;
            Timbres[0]._Op3.SSG_EG = 0;
            Timbres[0]._Op3.TL = 12;

            Timbres[0]._Op4.AR = 31;
            Timbres[0]._Op4.D1R = 0;
            Timbres[0]._Op4.SL = 0;
            Timbres[0]._Op4.D2R = 0;
            Timbres[0]._Op4.RR = 15;

            Timbres[0]._Op4.MUL = 1;
            Timbres[0]._Op4.RS = 0;
            Timbres[0]._Op4.DT1 = 4;
            Timbres[0]._Op4.AM = 0;
            Timbres[0]._Op4.SSG_EG = 0;
            Timbres[0]._Op4.TL = 12;

            //Additive Chimes A.dmp
            Timbres[2].SoundType = SoundType.FM;

            Timbres[2].FMS = 0;
            Timbres[2].AMS = 0;
            Timbres[2].FB = 0;
            Timbres[2].ALG = 7;

            Timbres[2]._Op1.AR = 31;
            Timbres[2]._Op1.D1R = 4;
            Timbres[2]._Op1.SL = 15;
            Timbres[2]._Op1.D2R = 0;
            Timbres[2]._Op1.RR = 4;

            Timbres[2]._Op1.MUL = 1;
            Timbres[2]._Op1.RS = 0;
            Timbres[2]._Op1.DT1 = 4;
            Timbres[2]._Op1.AM = 0;
            Timbres[2]._Op1.SSG_EG = 0;
            Timbres[2]._Op1.TL = 20;

            Timbres[2]._Op2.AR = 31;
            Timbres[2]._Op2.D1R = 7;
            Timbres[2]._Op2.SL = 15;
            Timbres[2]._Op2.D2R = 0;
            Timbres[2]._Op2.RR = 5;

            Timbres[2]._Op2.MUL = 4;
            Timbres[2]._Op2.RS = 0;
            Timbres[2]._Op2.DT1 = 4;
            Timbres[2]._Op2.AM = 0;
            Timbres[2]._Op2.SSG_EG = 0;
            Timbres[2]._Op2.TL = 20;

            Timbres[2]._Op3.AR = 31;
            Timbres[2]._Op3.D1R = 10;
            Timbres[2]._Op3.SL = 15;
            Timbres[2]._Op3.D2R = 0;
            Timbres[2]._Op3.RR = 6;

            Timbres[2]._Op3.MUL = 7;
            Timbres[2]._Op3.RS = 0;
            Timbres[2]._Op3.DT1 = 4;
            Timbres[2]._Op3.AM = 0;
            Timbres[2]._Op3.SSG_EG = 0;
            Timbres[2]._Op3.TL = 20;

            Timbres[2]._Op4.AR = 31;
            Timbres[2]._Op4.D1R = 13;
            Timbres[2]._Op4.SL = 15;
            Timbres[2]._Op4.D2R = 0;
            Timbres[2]._Op4.RR = 7;

            Timbres[2]._Op4.MUL = 10;
            Timbres[2]._Op4.RS = 0;
            Timbres[2]._Op4.DT1 = 0;
            Timbres[2]._Op4.AM = 0;
            Timbres[2]._Op4.SSG_EG = 0;
            Timbres[2]._Op4.TL = 20;

            //DX Piano1
            Timbres[1].SoundType = SoundType.FM;
            Timbres[1].FMS = 0;
            Timbres[1].AMS = 0;
            Timbres[1].FB = 0;
            Timbres[1].ALG = 1;

            Timbres[1]._Op1.AR = 31;
            Timbres[1]._Op1.D1R = 9;
            Timbres[1]._Op1.SL = 15;
            Timbres[1]._Op1.D2R = 0;
            Timbres[1]._Op1.RR = 5;

            Timbres[1]._Op1.MUL = 9;
            Timbres[1]._Op1.RS = 2;
            Timbres[1]._Op1.DT1 = 7;
            Timbres[1]._Op1.AM = 0;
            Timbres[1]._Op1.SSG_EG = 0;
            Timbres[1]._Op1.TL = 60;

            Timbres[1]._Op2.AR = 31;
            Timbres[1]._Op2.D1R = 9;
            Timbres[1]._Op2.SL = 15;
            Timbres[1]._Op2.D2R = 0;
            Timbres[1]._Op2.RR = 5;

            Timbres[1]._Op2.MUL = 9;
            Timbres[1]._Op2.RS = 2;
            Timbres[1]._Op2.DT1 = 1;
            Timbres[1]._Op2.AM = 0;
            Timbres[1]._Op2.SSG_EG = 0;
            Timbres[1]._Op2.TL = 60;

            Timbres[1]._Op3.AR = 31;
            Timbres[1]._Op3.D1R = 7;
            Timbres[1]._Op3.SL = 15;
            Timbres[1]._Op3.D2R = 0;
            Timbres[1]._Op3.RR = 5;

            Timbres[1]._Op3.MUL = 0;
            Timbres[1]._Op3.RS = 2;
            Timbres[1]._Op3.DT1 = 4;
            Timbres[1]._Op3.AM = 0;
            Timbres[1]._Op3.SSG_EG = 0;
            Timbres[1]._Op3.TL = 28;

            Timbres[1]._Op4.AR = 31;
            Timbres[1]._Op4.D1R = 3;
            Timbres[1]._Op4.SL = 15;
            Timbres[1]._Op4.D2R = 0;
            Timbres[1]._Op4.RR = 5;

            Timbres[1]._Op4.MUL = 0;
            Timbres[1]._Op4.RS = 2;
            Timbres[1]._Op4.DT1 = 4;
            Timbres[1]._Op4.AM = 0;
            Timbres[1]._Op4.SSG_EG = 0;
            Timbres[1]._Op4.TL = 10;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnNoteOnEvent(NoteOnEvent midiEvent)
        {
            soundManager.NoteOn(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnNoteOffEvent(NoteOffEvent midiEvent)
        {
            soundManager.NoteOff(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnControlChangeEvent(ControlChangeEvent midiEvent)
        {
            base.OnControlChangeEvent(midiEvent);

            soundManager.ControlChange(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnPitchBendEvent(PitchBendEvent midiEvent)
        {
            base.OnPitchBendEvent(midiEvent);

            soundManager.PitchBend(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        private class YM2612SoundManager : SoundManagerBase
        {
            private List<YM2612Sound> allOnSounds = new List<YM2612Sound>();

            private List<YM2612Sound> fmOnSounds = new List<YM2612Sound>();

            private List<YM2612Sound> psgOnSounds = new List<YM2612Sound>();

            private YM2612 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public YM2612SoundManager(YM2612 parent)
            {
                this.parentModule = parent;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="midiEvent"></param>
            public override void PitchBend(PitchBendEvent midiEvent)
            {
                foreach (var t in allOnSounds)
                {
                    if (t.NoteOnEvent.Channel == midiEvent.Channel)
                    {
                        switch (t.Timbre.SoundType)
                        {
                            case SoundType.FM:
                                t.UpdateFmPitch();
                                break;
                            case SoundType.PSG:
                                //TODO:
                                break;
                        }
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="channel"></param>
            /// <param name="value"></param>
            public override void ControlChange(ControlChangeEvent midiEvent)
            {
                switch (midiEvent.ControlNumber)
                {
                    case 6:    //Data Entry
                        //nothing
                        break;
                    case 7:    //Volume
                        foreach (var t in allOnSounds)
                        {
                            if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            {
                                switch (t.Timbre.SoundType)
                                {
                                    case SoundType.FM:
                                        t.UpdateFmVolume();
                                        break;
                                    case SoundType.PSG:
                                        //TODO:
                                        break;
                                }
                            }
                        }
                        break;
                    case 10:    //Panpot
                        foreach (var t in allOnSounds)
                        {
                            if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            {
                                switch (t.Timbre.SoundType)
                                {
                                    case SoundType.FM:
                                        t.UpdatePanpot();
                                        break;
                                    case SoundType.PSG:
                                        //TODO:
                                        break;
                                }
                            }
                        }
                        break;
                    case 11:    //Expression
                        foreach (var t in allOnSounds)
                        {
                            if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            {
                                switch (t.Timbre.SoundType)
                                {
                                    case SoundType.FM:
                                        t.UpdateFmVolume();
                                        break;
                                    case SoundType.PSG:
                                        //TODO:
                                        break;
                                }
                            }
                        }
                        break;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            public override void NoteOn(NoteOnEvent note)
            {
                int emptySlot = searchEmptySlot(note);
                if (emptySlot < 0)
                    return;

                YM2612Sound snd = new YM2612Sound(parentModule, note, emptySlot);
                allOnSounds.Add(snd);
                switch (snd.Timbre.SoundType)
                {
                    case SoundType.FM:
                        fmOnSounds.Add(snd);
                        break;
                    case SoundType.PSG:
                        psgOnSounds.Add(snd);
                        break;
                }
                FormMain.OutputLog("KeyOn FM ch" + emptySlot + " " + note.ToString());
                snd.On();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            private int searchEmptySlot(NoteOnEvent note)
            {
                int emptySlot = -1;

                var pn = parentModule.ProgramNumbers[note.Channel];

                var timbre = parentModule.Timbres[pn];
                switch (timbre.SoundType)
                {
                    case SoundType.FM:
                        {
                            emptySlot = SearchEmptySlot(fmOnSounds.ToList<SoundBase>(), 6);
                            break;
                        }
                    case SoundType.PSG:
                        {
                            emptySlot = SearchEmptySlot(psgOnSounds.ToList<SoundBase>(), 3);
                            break;
                        }
                }
                return emptySlot;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            public override void NoteOff(NoteOffEvent note)
            {
                YM2612Sound removed = SearchAndRemoveOnSound(note, allOnSounds);

                if (removed != null)
                {
                    for (int i = 0; i < fmOnSounds.Count; i++)
                    {
                        if (fmOnSounds[i] == removed)
                        {
                            FormMain.OutputLog("KeyOff FM ch" + removed.Slot + " " + note.ToString());
                            fmOnSounds.RemoveAt(i);
                            return;
                        }
                    }
                    for (int i = 0; i < psgOnSounds.Count; i++)
                    {
                        if (psgOnSounds[i] == removed)
                        {
                            psgOnSounds.RemoveAt(i);
                            return;
                        }
                    }
                }
            }


        }


        /// <summary>
        /// 
        /// </summary>
        private class YM2612Sound : SoundBase
        {

            private YM2612 parentModule;

            private SevenBitNumber programNumber;

            public YM2612Timbre Timbre;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public YM2612Sound(YM2612 parentModule, NoteOnEvent noteOnEvent, int slot) : base(noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.programNumber = (SevenBitNumber)parentModule.ProgramNumbers[noteOnEvent.Channel];
                this.Timbre = parentModule.Timbres[programNumber];
            }

            /// <summary>
            /// 
            /// </summary>
            public override void On()
            {
                switch (Timbre.SoundType)
                {
                    case SoundType.FM:
                        {
                            //
                            SetFmTimbre();
                            //Freq
                            UpdateFmPitch();
                            //Volume
                            UpdateFmVolume();
                            //On
                            uint reg = (uint)(Slot / 3) * 2;
                            Ym2612WriteData(parentModule.UnitNumber, 0x28, 0, 0, (byte)(0xf0 | (reg << 1) | (byte)(Slot % 3)));
                            break;
                        }
                    case SoundType.PSG:
                        {

                            break;
                        }
                }
            }


            /// <summary>
            /// 
            /// </summary>
            public void UpdateFmVolume()
            {
                var pn = parentModule.ProgramNumbers[NoteOnEvent.Channel];
                var timbre = parentModule.Timbres[pn];

                List<int> ops = new List<int>();
                switch (timbre.ALG)
                {
                    case 0:
                        ops.Add(3);
                        break;
                    case 1:
                        ops.Add(3);
                        break;
                    case 2:
                        ops.Add(3);
                        break;
                    case 3:
                        ops.Add(3);
                        break;
                    case 4:
                        ops.Add(2);
                        ops.Add(3);
                        break;
                    case 5:
                        ops.Add(1);
                        ops.Add(2);
                        ops.Add(3);
                        break;
                    case 6:
                        ops.Add(1);
                        ops.Add(2);
                        ops.Add(3);
                        break;
                    case 7:
                        ops.Add(0);
                        ops.Add(1);
                        ops.Add(2);
                        ops.Add(3);
                        break;
                }
                var exp = parentModule.Expressions[NoteOnEvent.Channel] / 127d;
                var vol = parentModule.Volumes[NoteOnEvent.Channel] / 127d;
                var vel = NoteOnEvent.Velocity / 127d;
                foreach (int op in ops)
                {
                    //$40+: total level
                    Ym2612WriteData(parentModule.UnitNumber, 0x40, op, Slot, (byte)(127 - Math.Round((127 - timbre.GetOperator(op).TL) * vol * vel)));
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public void UpdateFmPitch()
            {
                var pn = parentModule.ProgramNumbers[NoteOnEvent.Channel];
                var timbre = parentModule.Timbres[pn];

                var pitch = (int)parentModule.Pitchs[NoteOnEvent.Channel] - 8192;
                var range = (int)parentModule.PitchBendRanges[NoteOnEvent.Channel];

                int noteNum = NoteOnEvent.NoteNumber;
                if (pitch > 0)
                {
                    noteNum = NoteOnEvent.NoteNumber + range;
                    if (noteNum > 127)
                        noteNum = 127;
                    var nnOn = new NoteOnEvent((SevenBitNumber)noteNum, (SevenBitNumber)127);

                    double cfreq = (double)convertFmFrequency(NoteOnEvent);
                    double nfreq = (double)convertFmFrequency(nnOn);
                    var doctave = nnOn.GetNoteOctave() - NoteOnEvent.GetNoteOctave() + 1;
                    nfreq = nfreq * (double)doctave;

                    var dfreq = (nfreq - cfreq) * ((double)pitch / (double)8192);
                    var freq = (ushort)Math.Round(cfreq + dfreq);
                    byte octave = (byte)((NoteOnEvent.GetNoteOctave()) << 3);

                    Ym2612WriteData(parentModule.UnitNumber, 0xa4, 0, Slot, (byte)(octave | (freq >> 8)));
                    Ym2612WriteData(parentModule.UnitNumber, 0xa0, 0, Slot, (byte)(0xff & freq));
                }
                else if (pitch < 0)
                {
                    noteNum = NoteOnEvent.NoteNumber - range;
                    if (noteNum < 0)
                        noteNum = 0;
                    var nnOn = new NoteOnEvent((SevenBitNumber)noteNum, (SevenBitNumber)127);

                    double cfreq = (double)convertFmFrequency(NoteOnEvent);
                    double nfreq = (double)convertFmFrequency(nnOn);
                    var doctave = NoteOnEvent.GetNoteOctave() - nnOn.GetNoteOctave() + 1;
                    nfreq = nfreq / (double)doctave;

                    var dfreq = (nfreq - cfreq) * ((double)pitch / (double)8192);
                    var freq = (ushort)Math.Round(cfreq + dfreq);
                    byte octave = (byte)((NoteOnEvent.GetNoteOctave()) << 3);

                    Ym2612WriteData(parentModule.UnitNumber, 0xa4, 0, Slot, (byte)(octave | (freq >> 8)));
                    Ym2612WriteData(parentModule.UnitNumber, 0xa0, 0, Slot, (byte)(0xff & freq));
                }
                else
                {
                    var freq = convertFmFrequency(NoteOnEvent);
                    byte octave = (byte)((NoteOnEvent.GetNoteOctave()) << 3);
                    Ym2612WriteData(parentModule.UnitNumber, 0xa4, 0, Slot, (byte)(octave | (freq >> 8)));
                    Ym2612WriteData(parentModule.UnitNumber, 0xa0, 0, Slot, (byte)(0xff & freq));
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public void UpdatePanpot()
            {
                var pn = parentModule.ProgramNumbers[NoteOnEvent.Channel];
                var timbre = parentModule.Timbres[pn];

                //$B4+: panning, FMS, AMS
                byte pan = parentModule.Panpots[NoteOnEvent.Channel];
                if (pan < 32)
                    pan = 0x2;
                else if (pan > 96)
                    pan = 0x1;
                else
                    pan = 0x3;
                Ym2612WriteData(parentModule.UnitNumber, 0xB4, 0, Slot, (byte)(pan << 6 | (timbre.AMS << 4) | timbre.FMS));
            }

            /// <summary>
            /// 
            /// </summary>
            public void SetFmTimbre()
            {
                var pn = parentModule.ProgramNumbers[NoteOnEvent.Channel];
                var timbre = parentModule.Timbres[pn];

                for (int op = 0; op < 4; op++)
                {
                    //$30+: multiply and detune
                    Ym2612WriteData(parentModule.UnitNumber, 0x30, op, Slot, (byte)((timbre.GetOperator(op).DT1 << 4 | timbre.GetOperator(op).MUL)));
                    //$40+: total level
                    Ym2612WriteData(parentModule.UnitNumber, 0x40, op, Slot, (byte)timbre.GetOperator(op).TL);
                    //$50+: attack rate and rate scaling
                    Ym2612WriteData(parentModule.UnitNumber, 0x50, op, Slot, (byte)((timbre.GetOperator(op).RS << 6 | timbre.GetOperator(op).AR)));
                    //$60+: 1st decay rate and AM enable
                    Ym2612WriteData(parentModule.UnitNumber, 0x60, op, Slot, (byte)((timbre.GetOperator(op).AM << 7 | timbre.GetOperator(op).D1R)));
                    //$70+: 2nd decay rate
                    Ym2612WriteData(parentModule.UnitNumber, 0x70, op, Slot, (byte)timbre.GetOperator(op).D2R);
                    //$80+: release rate and sustain level
                    Ym2612WriteData(parentModule.UnitNumber, 0x80, op, Slot, (byte)((timbre.GetOperator(op).SL << 4 | timbre.GetOperator(op).RR)));
                    //$90+: SSG-EG
                    Ym2612WriteData(parentModule.UnitNumber, 0x90, op, Slot, (byte)timbre.GetOperator(op).SSG_EG);
                }

                //$B0+: algorithm and feedback
                Ym2612WriteData(parentModule.UnitNumber, 0xB0, 0, Slot, (byte)(timbre.FB << 3 | timbre.ALG));

                UpdatePanpot();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void Off()
            {
                switch (Timbre.SoundType)
                {
                    case SoundType.FM:
                        {
                            uint reg = (uint)(Slot / 3) * 2;
                            Ym2612WriteData(parentModule.UnitNumber, 0x28, 0, 0, (byte)(0x00 | (reg << 1) | (byte)(Slot % 3)));
                            break;
                        }
                    case SoundType.PSG:
                        {

                            break;
                        }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            /// <param name="freq"></param>
            /// <returns></returns>
            private ushort convertFmFrequency(NoteOnEvent note)
            {
                ushort freq = 0;
                switch (note.GetNoteName())
                {
                    case NoteName.C:
                        freq = 644;
                        break;
                    case NoteName.CSharp:
                        freq = 681;
                        break;
                    case NoteName.D:
                        freq = 722;
                        break;
                    case NoteName.DSharp:
                        freq = 765;
                        break;
                    case NoteName.E:
                        freq = 810;
                        break;
                    case NoteName.F:
                        freq = 858;
                        break;
                    case NoteName.FSharp:
                        freq = 910;
                        break;
                    case NoteName.G:
                        freq = 964;
                        break;
                    case NoteName.GSharp:
                        freq = 1021;
                        break;
                    case NoteName.A:
                        freq = 1081;
                        break;
                    case NoteName.ASharp:
                        freq = 1146;
                        break;
                    case NoteName.B:
                        freq = 1214;
                        break;
                }

                return freq;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TypeConverter(typeof(ValueTypeTypeConverter<YM2612Timbre>))]
        public struct YM2612Timbre
        {
            public SoundType SoundType;

            #region FM Symth

            public byte _FB;

            public byte FB
            {
                get
                {
                    return _FB;
                }
                set
                {
                    _FB = value;
                }
            }

            public byte _ALG;

            public byte ALG
            {
                get
                {
                    return _ALG;
                }
                set
                {
                    _ALG = value;
                }
            }

            public byte _AMS;

            public byte AMS
            {
                get
                {
                    return _AMS;
                }
                set
                {
                    _AMS = value;
                }
            }

            public byte _FMS;

            public byte FMS
            {
                get
                {
                    return _FMS;
                }
                set
                {
                    _FMS = value;
                }
            }

            public YM2612Operator _Op1;

            public YM2612Operator Op1
            {
                get
                {
                    return _Op1;
                }
                set
                {
                    _Op1 = value;
                }
            }

            public YM2612Operator _Op2;

            public YM2612Operator Op2
            {
                get
                {
                    return _Op2;
                }
                set
                {
                    _Op2 = value;
                }
            }

            public YM2612Operator _Op3;

            public YM2612Operator Op3
            {
                get
                {
                    return _Op3;
                }
                set
                {
                    _Op3 = value;
                }
            }

            public YM2612Operator _Op4;

            public YM2612Operator Op4
            {
                get
                {
                    return _Op4;
                }
                set
                {
                    _Op4 = value;
                }
            }

            #endregion

            /// <summary>
            /// 
            /// </summary>
            /// <param name="idx"></param>
            public YM2612Operator GetOperator(int idx)
            {
                switch (idx)
                {
                    case 0:
                        return Op1;
                    case 1:
                        return Op2;
                    case 2:
                        return Op3;
                    case 3:
                        return Op4;
                }
                return Op1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TypeConverter(typeof(ValueTypeTypeConverter<YM2612Operator>))]
        public struct YM2612Operator
        {
            /// <summary>
            /// Detune1(0-7)
            /// </summary>
            public byte _DT1;

            public byte DT1
            {
                get
                {
                    return _DT1;
                }
                set
                {
                    _DT1 = value;
                }
            }

            /// <summary>
            /// Multiply(0-15)
            /// </summary>
            public byte _MUL;

            public byte MUL
            {
                get
                {
                    return _MUL;
                }
                set
                {
                    _MUL = value;
                }
            }

            /// <summary>
            /// Total Level(0-127)
            /// </summary>
            public byte _TL;

            public byte TL
            {
                get
                {
                    return _TL;
                }
                set
                {
                    _TL = (byte)(value & 127);
                }
            }

            /// <summary>
            /// Rate Scaling(0-3)
            /// </summary>
            public byte _RS;

            public byte RS
            {
                get
                {
                    return _RS;
                }
                set
                {
                    _RS = (byte)(value & 3);
                }
            }

            /// <summary>
            /// Attack Rate(0-31)
            /// </summary>
            public byte _AR;

            public byte AR
            {
                get
                {
                    return _AR;
                }
                set
                {
                    _AR = (byte)(value & 31);
                }
            }

            /// <summary>
            /// amplitude modulation sensivity(0-1)
            /// </summary>
            public byte _AM;

            public byte AM
            {
                get
                {
                    return _AM;
                }
                set
                {
                    _AM = (byte)(value & 1);
                }
            }

            /// <summary>
            /// 1st decay rate(0-31)
            /// </summary>
            public byte _D1R;

            public byte D1R
            {
                get
                {
                    return _D1R;
                }
                set
                {
                    _D1R = (byte)(value & 31);
                }
            }

            /// <summary>
            /// 2nd decay rate(0-31)
            /// </summary>
            public byte _D2R;

            public byte D2R
            {
                get
                {
                    return _D2R;
                }
                set
                {
                    _D2R = (byte)(value & 31);
                }
            }

            /// <summary>
            /// sustain level(0-15)
            /// </summary>
            public byte _SL;

            public byte SL
            {
                get
                {
                    return _SL;
                }
                set
                {
                    _SL = (byte)(value & 15);
                }
            }

            /// <summary>
            /// release rate(0-15)
            /// </summary>
            public byte _RR;

            public byte RR
            {
                get
                {
                    return _RR;
                }
                set
                {
                    _RR = (byte)(value & 15);
                }
            }

            /// <summary>
            /// SSG-EG(0-15)
            /// </summary>
            public byte _SSG_EG;

            public byte SSG_EG
            {
                get
                {
                    return _SSG_EG;
                }
                set
                {
                    _SSG_EG = (byte)(value & 15);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public enum SoundType
        {
            FM,
            PSG,
        }

        public class ValueTypeTypeConverter<T> : ExpandableObjectConverter where T : struct
        {
            public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
            {
                if (propertyValues == null)
                    throw new ArgumentNullException("propertyValues");

                T ret = default(T);
                object boxed = ret;
                foreach (DictionaryEntry entry in propertyValues)
                {
                    PropertyInfo pi = ret.GetType().GetProperty(entry.Key.ToString());
                    if (pi != null && pi.CanWrite)
                    {
                        pi.SetValue(boxed, Convert.ChangeType(entry.Value, pi.PropertyType), null);
                    }
                }
                return (T)boxed;
            }
        }
    }
}