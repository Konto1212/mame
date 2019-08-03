using System;
using System.Collections.Generic;
using System.Linq;
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
        public uint UnitNumber
        {
            get;
            private set;
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
        public YM2612(uint unitNumber)
        {
            UnitNumber = unitNumber;
            this.soundManager = new YM2612SoundManager(this);
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
        protected override void OnProgramChangeEvent(ProgramChangeEvent midiEvent)
        {
            base.OnProgramChangeEvent(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnControlChangeEvent(ControlChangeEvent midiEvent)
        {
            soundManager.ControlChange(midiEvent);
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

            public SevenBitNumber[] PitchBendRanges;

            public SevenBitNumber[] ProgramNumbers;

            public SevenBitNumber[] Volumes;

            public SevenBitNumber[] Expressions;

            public SevenBitNumber[] Panpots;

            public YM2612Timbre[] Timbres;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public YM2612SoundManager(YM2612 parent)
            {
                ProgramNumbers = new SevenBitNumber[] {
                    (SevenBitNumber)0, (SevenBitNumber)0, (SevenBitNumber)0,
                    (SevenBitNumber)0, (SevenBitNumber)0, (SevenBitNumber)0,
                    (SevenBitNumber)0, (SevenBitNumber)0, (SevenBitNumber)0,
                    (SevenBitNumber)0, (SevenBitNumber)0, (SevenBitNumber)0,
                    (SevenBitNumber)0, (SevenBitNumber)0, (SevenBitNumber)0, (SevenBitNumber)0 };
                Volumes = new SevenBitNumber[] {
                    (SevenBitNumber)127, (SevenBitNumber)127, (SevenBitNumber)127,
                    (SevenBitNumber)127, (SevenBitNumber)127, (SevenBitNumber)127,
                    (SevenBitNumber)127, (SevenBitNumber)127, (SevenBitNumber)127,
                    (SevenBitNumber)127, (SevenBitNumber)127, (SevenBitNumber)127,
                    (SevenBitNumber)127, (SevenBitNumber)127, (SevenBitNumber)127, (SevenBitNumber)127  };
                Expressions = new SevenBitNumber[] {
                    (SevenBitNumber)127, (SevenBitNumber)127, (SevenBitNumber)127,
                    (SevenBitNumber)127, (SevenBitNumber)127, (SevenBitNumber)127,
                    (SevenBitNumber)127, (SevenBitNumber)127, (SevenBitNumber)127,
                    (SevenBitNumber)127, (SevenBitNumber)127, (SevenBitNumber)127,
                    (SevenBitNumber)127, (SevenBitNumber)127, (SevenBitNumber)127, (SevenBitNumber)127  };
                Panpots = new SevenBitNumber[] {
                    (SevenBitNumber)64, (SevenBitNumber)64, (SevenBitNumber)64,
                    (SevenBitNumber)64, (SevenBitNumber)64, (SevenBitNumber)64,
                    (SevenBitNumber)64, (SevenBitNumber)64, (SevenBitNumber)64,
                    (SevenBitNumber)64, (SevenBitNumber)64, (SevenBitNumber)64,
                    (SevenBitNumber)64, (SevenBitNumber)64, (SevenBitNumber)64, (SevenBitNumber)64};
                PitchBendRanges = new SevenBitNumber[] {
                    (SevenBitNumber)2, (SevenBitNumber)2, (SevenBitNumber)2,
                    (SevenBitNumber)2, (SevenBitNumber)2, (SevenBitNumber)2,
                    (SevenBitNumber)2, (SevenBitNumber)2, (SevenBitNumber)2,
                    (SevenBitNumber)2, (SevenBitNumber)2, (SevenBitNumber)2,
                    (SevenBitNumber)2, (SevenBitNumber)2, (SevenBitNumber)2, (SevenBitNumber)2};

                this.parentModule = parent;

                Timbres = new YM2612Timbre[128];
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
                    case 39:    //Volume
                        Volumes[midiEvent.Channel] = midiEvent.ControlValue;
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
                snd.On();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            private int searchEmptySlot(NoteOnEvent note)
            {
                int emptySlot = -1;

                var pn = ProgramNumbers[note.Channel];

                var timbre = parentModule.soundManager.Timbres[pn];
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
                this.programNumber = parentModule.soundManager.ProgramNumbers[noteOnEvent.Channel];
                this.Timbre = parentModule.soundManager.Timbres[programNumber];
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
                            ushort freq = convertFmFrequency(NoteOnEvent);

                            //Freq
                            byte octave = (byte)((NoteOnEvent.GetNoteOctave() + 1) << 3);
                            uint reg = (uint)(Slot / 3) * 2;
                            Ym2612_write?.Invoke(parentModule.UnitNumber, reg, (byte)(0xa4 + Slot));
                            Ym2612_write?.Invoke(parentModule.UnitNumber, reg + 1, (byte)(octave | (freq >> 8)));
                            Ym2612_write?.Invoke(parentModule.UnitNumber, reg, (byte)(0xa0 + Slot));
                            Ym2612_write?.Invoke(parentModule.UnitNumber, reg + 1, (byte)(0xff & freq));

                            SetTimbre(Slot);

                            //On
                            Ym2612_write?.Invoke(parentModule.UnitNumber, 0, 0x28);
                            Ym2612_write?.Invoke(parentModule.UnitNumber, 1, (byte)(0xf0 | reg << 1 | (byte)(Slot % 3)));

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
            /// <param name="slot"></param>
            public void SetTimbre(int slot)
            {
                var pn = parentModule.soundManager.ProgramNumbers[NoteOnEvent.Channel];
                var timbre = parentModule.soundManager.Timbres[pn];

                uint reg = (uint)(slot / 3) * 2;
                /*
                //$90+: SSG-EG

                //$B0+: algorithm and feedback
                Ym2612_write?.Invoke(parentModule.UnitNumber, reg, (byte)(0xB0 + (slot % 3)));
                Ym2612_write?.Invoke(parentModule.UnitNumber, reg + 1, (byte)((timbre.FB << 3) | timbre.ALG));
                */
                //$B4+: panning, FMS, AMS
                Ym2612_write?.Invoke(parentModule.UnitNumber, reg, (byte)(0xB4 + (slot % 3)));
                byte pan = parentModule.soundManager.Panpots[NoteOnEvent.Channel];

                pan = 0;

                if (pan < 32)
                    pan = 0x2;
                else if (pan > 96)
                    pan = 0x1;
                else
                    pan = 0x3;

                Ym2612_write?.Invoke(parentModule.UnitNumber, reg + 1, (byte)(pan << 6 | (timbre.AMS << 4) | timbre.FMS));
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
                            Ym2612_write?.Invoke(parentModule.UnitNumber, 0, 0x28);
                            uint reg = (uint)(Slot / 3);
                            Ym2612_write?.Invoke(parentModule.UnitNumber, 1, (byte)(0x00 | (reg << 2) | (byte)(Slot % 3)));
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
        private struct YM2612Timbre
        {
            public SoundType SoundType;

            public byte FB;
            public byte ALG;
            public byte AMS;
            public byte FMS;

            public YM2612Operator Op1;
            public YM2612Operator Op2;
            public YM2612Operator Op3;
            public YM2612Operator Op4;

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
        private struct YM2612Operator
        {
            public byte DT1;
            public byte MUL;
            public byte TL;
            public byte RS;
            public byte AR;
            public byte AM;
            public byte D1R;
            public byte D2R;
            public byte D1L;
            public byte RR;
            public byte SSG_EG;
        }

        /// <summary>
        /// 
        /// </summary>
        private enum SoundType
        {
            FM,
            PSG,
        }

    }



}
