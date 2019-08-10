using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Smf;
using Newtonsoft.Json;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;
using zanac.mamidimemo.ComponentModel;
using zanac.mamidimemo.mame;
using zanac.mamidimemo.midi;

//https://www16.atwiki.jp/mxdrv/pages/24.html

namespace zanac.mamidimemo.instruments
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class YM2151 : InstrumentBase
    {

        public override string Name => "YM2151";

        public override InstrumentType InstrumentType => InstrumentType.YM2151;

        [Browsable(false)]
        public override string ImageKey => "YM2151";

        [DataMember]
        [Category("Chip")]
        [Description("Timbres (0-127)")]
        public YM2151Timbre[] Timbres
        {
            get;
            set;
        }

                /// <summary>
        /// 
        /// </summary>
        /// <param name="serializeData"></param>
        public override void RestoreFrom(string serializeData)
        {
            var obj = JsonConvert.DeserializeObject<YM2151>(serializeData);
            this.InjectFrom(new LoopInjection(new[] { "SerializeData" }), obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_ym2151_write(uint unitNumber, uint address, byte data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_ym2151_write Ym2151_write
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static void Ym2151WriteData(uint unitNumber, byte address, int op, int slot, byte data)
        {
            try
            {
                Program.SoundUpdating();
                switch (op)
                {
                    case 0:
                        op = 0;
                        break;
                    case 1:
                        op = 2;
                        break;
                    case 2:
                        op = 1;
                        break;
                    case 3:
                        op = 3;
                        break;
                }
                Ym2151_write(unitNumber, 0, (byte)(address + (op * 8) + slot));
                Ym2151_write(unitNumber, 1, data);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static YM2151()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("ym2151_write");
            if (funcPtr != IntPtr.Zero)
            {
                Ym2151_write = (delegate_ym2151_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_ym2151_write));
            }
        }

        private YM2151SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public YM2151(uint unitNumber) : base(unitNumber)
        {
            Timbres = new YM2151Timbre[128];
            for (int i = 0; i < 128; i++)
                Timbres[i] = new YM2151Timbre();
            setPresetInstruments();

            this.soundManager = new YM2151SoundManager(this);
        }

        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {
            //Dist Gt
            Timbres[0].PMS = 0;
            Timbres[0].AMS = 0;
            Timbres[0].FB = 0;
            Timbres[0].ALG = 3;

            Timbres[0].Ops[0].Enable = 1;
            Timbres[0].Ops[0].AR = 31;
            Timbres[0].Ops[0].D1R = 0;
            Timbres[0].Ops[0].SL = 0;
            Timbres[0].Ops[0].D2R = 0;
            Timbres[0].Ops[0].RR = 6;

            Timbres[0].Ops[0].MUL = 8;
            Timbres[0].Ops[0].RS = 0;
            Timbres[0].Ops[0].DT1 = 7;
            Timbres[0].Ops[0].AM = 0;
            Timbres[0].Ops[0].DT2 = 0;
            Timbres[0].Ops[0].TL = 56;

            Timbres[0].Ops[1].Enable = 1;
            Timbres[0].Ops[1].AR = 31;
            Timbres[0].Ops[1].D1R = 18;
            Timbres[0].Ops[1].SL = 0;
            Timbres[0].Ops[1].D2R = 0;
            Timbres[0].Ops[1].RR = 6;

            Timbres[0].Ops[1].MUL = 3;
            Timbres[0].Ops[1].RS = 0;
            Timbres[0].Ops[1].DT1 = 7;
            Timbres[0].Ops[1].AM = 0;
            Timbres[0].Ops[1].DT2 = 0;
            Timbres[0].Ops[1].TL = 19;

            Timbres[0].Ops[2].Enable = 1;
            Timbres[0].Ops[2].AR = 31;
            Timbres[0].Ops[2].D1R = 0;
            Timbres[0].Ops[2].SL = 0;
            Timbres[0].Ops[2].D2R = 0;
            Timbres[0].Ops[2].RR = 6;

            Timbres[0].Ops[2].MUL = 3;
            Timbres[0].Ops[2].RS = 0;
            Timbres[0].Ops[2].DT1 = 4;
            Timbres[0].Ops[2].AM = 0;
            Timbres[0].Ops[2].DT2 = 0;
            Timbres[0].Ops[2].TL = 8;

            Timbres[0].Ops[3].Enable = 1;
            Timbres[0].Ops[3].AR = 20;
            Timbres[0].Ops[3].D1R = 17;
            Timbres[0].Ops[3].SL = 0;
            Timbres[0].Ops[3].D2R = 0;
            Timbres[0].Ops[3].RR = 5;

            Timbres[0].Ops[3].MUL = 2;
            Timbres[0].Ops[3].RS = 0;
            Timbres[0].Ops[3].DT1 = 4;
            Timbres[0].Ops[3].AM = 0;
            Timbres[0].Ops[3].DT2 = 0;
            Timbres[0].Ops[3].TL = 24;
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
        private class YM2151SoundManager : SoundManagerBase
        {
            private List<YM2151Sound> allOnSounds = new List<YM2151Sound>();

            private List<YM2151Sound> fmOnSounds = new List<YM2151Sound>();

            private List<YM2151Sound> psgOnSounds = new List<YM2151Sound>();

            private YM2151 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public YM2151SoundManager(YM2151 parent)
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
                        t.UpdateFmPitch();
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
                                t.UpdateFmVolume();
                            }
                        }
                        break;
                    case 10:    //Panpot
                        foreach (var t in allOnSounds)
                        {
                            if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            {
                                t.UpdatePanpot();
                            }
                        }
                        break;
                    case 11:    //Expression
                        foreach (var t in allOnSounds)
                        {
                            if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            {
                                t.UpdateFmVolume();
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

                YM2151Sound snd = new YM2151Sound(parentModule, note, emptySlot);
                allOnSounds.Add(snd);
                fmOnSounds.Add(snd);
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
                emptySlot = SearchEmptySlot(fmOnSounds.ToList<SoundBase>(), 6);
                return emptySlot;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            public override void NoteOff(NoteOffEvent note)
            {
                YM2151Sound removed = SearchAndRemoveOnSound(note, allOnSounds);

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
        private class YM2151Sound : SoundBase
        {

            private YM2151 parentModule;

            private SevenBitNumber programNumber;

            public YM2151Timbre Timbre;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public YM2151Sound(YM2151 parentModule, NoteOnEvent noteOnEvent, int slot) : base(noteOnEvent, slot)
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
                //
                SetFmTimbre();
                //Freq
                UpdateFmPitch();
                //Volume
                UpdateFmVolume();
                //On
                byte op = (byte)(Timbre.Ops[0].Enable << 3 | Timbre.Ops[2].Enable << 4 | Timbre.Ops[1].Enable << 5 | Timbre.Ops[3].Enable << 6);
                Ym2151WriteData(parentModule.UnitNumber, 0x08, 0, 0, (byte)(op | (Slot % 3)));
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
                        ops.Add(1);
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
                    //$60+: total level
                    Ym2151WriteData(parentModule.UnitNumber, 0x60, op, Slot, (byte)(127 - Math.Round((127 - timbre.Ops[op].TL) * vol * vel * exp)));
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
                if (pitch > 0)
                {
                    //0x1fff 8192 13bit 0x3f 63 6bit
                    double range1 = 8192d / range;
                    int kf = (int)Math.Round(63d * (((double)pitch % range1) / range1));

                    int noted = (int)(pitch / range1);
                    int noteNum = NoteOnEvent.NoteNumber + noted;
                    if (noteNum > 127)
                        noteNum = 127;
                    var nnOn = new NoteOnEvent((SevenBitNumber)noteNum, (SevenBitNumber)127);

                    byte nn = getNoteNum(nnOn.GetNoteName());
                    byte octave = (byte)nnOn.GetNoteOctave();
                    if (nn == 0xff)
                    {
                        if (octave > 0)
                        {
                            octave -= 1;
                            nn = 14;
                        }
                    }
                    if (octave > 0)
                        octave -= 1;
                    Ym2151WriteData(parentModule.UnitNumber, 0x28, 0, Slot, (byte)((octave << 4) | nn));
                    Ym2151WriteData(parentModule.UnitNumber, 0x30, 0, Slot, (byte)(kf << 2));
                }
                else if (pitch < 0)
                {
                    pitch = -pitch;
                    double range1 = 8192d / range;
                    int kf = (int)Math.Round(63d - (63d * (((double)pitch % range1) / range1)));

                    int noted = (int)(pitch / range1) + 1;
                    int noteNum = NoteOnEvent.NoteNumber - noted;
                    if (noteNum > 127)
                        noteNum = 127;
                    var nnOn = new NoteOnEvent((SevenBitNumber)noteNum, (SevenBitNumber)127);

                    byte nn = getNoteNum(nnOn.GetNoteName());
                    byte octave = (byte)nnOn.GetNoteOctave();
                    if (nn == 0xff)
                    {
                        if (octave > 0)
                        {
                            octave -= 1;
                            nn = 14;
                        }
                    }
                    if (octave > 0)
                        octave -= 1;
                    Ym2151WriteData(parentModule.UnitNumber, 0x28, 0, Slot, (byte)((octave << 4) | nn));
                    Ym2151WriteData(parentModule.UnitNumber, 0x30, 0, Slot, (byte)(kf << 2));
                }
                else
                {
                    byte nn = getNoteNum(NoteOnEvent.GetNoteName());
                    byte octave = (byte)NoteOnEvent.GetNoteOctave();
                    if (nn == 0xff)
                    {
                        if (octave > 0)
                        {
                            octave -= 1;
                            nn = 14;
                        }
                    }
                    if (octave > 0)
                        octave -= 1;
                    Ym2151WriteData(parentModule.UnitNumber, 0x28, 0, Slot, (byte)((octave << 4) | nn));
                    Ym2151WriteData(parentModule.UnitNumber, 0x30, 0, Slot, (byte)0);
                }
            }

            private byte getNoteNum(NoteName noteName)
            {
                byte nn = 0;
                switch (noteName)
                {
                    case NoteName.C:
                        nn = 0xff;
                        break;
                    case NoteName.CSharp:
                        nn = 0;
                        break;
                    case NoteName.D:
                        nn = 1;
                        break;
                    case NoteName.DSharp:
                        nn = 2;
                        break;
                    case NoteName.E:
                        nn = 4;
                        break;
                    case NoteName.F:
                        nn = 5;
                        break;
                    case NoteName.FSharp:
                        nn = 6;
                        break;
                    case NoteName.G:
                        nn = 8;
                        break;
                    case NoteName.GSharp:
                        nn = 9;
                        break;
                    case NoteName.A:
                        nn = 10;
                        break;
                    case NoteName.ASharp:
                        nn = 12;
                        break;
                    case NoteName.B:
                        nn = 13;
                        break;
                }

                return nn;
            }

            /// <summary>
            /// 
            /// </summary>
            public void UpdatePanpot()
            {
                var pn = parentModule.ProgramNumbers[NoteOnEvent.Channel];
                var timbre = parentModule.Timbres[pn];

                byte pan = parentModule.Panpots[NoteOnEvent.Channel];
                if (pan < 32)
                    pan = 0x2;
                else if (pan > 96)
                    pan = 0x1;
                else
                    pan = 0x3;
                Ym2151WriteData(parentModule.UnitNumber, 0x20, 0, Slot, (byte)(pan << 6 | (timbre.FB << 3) | timbre.ALG));
            }

            /// <summary>
            /// 
            /// </summary>
            public void SetFmTimbre()
            {
                var pn = parentModule.ProgramNumbers[NoteOnEvent.Channel];
                var timbre = parentModule.Timbres[pn];

                Ym2151WriteData(parentModule.UnitNumber, 0x30, 0, Slot, (byte)((timbre.PMS << 4 | timbre.AMS)));
                for (int op = 0; op < 4; op++)
                {
                    Ym2151WriteData(parentModule.UnitNumber, 0x40, op, Slot, (byte)((timbre.Ops[op].DT1 << 4 | timbre.Ops[op].MUL)));
                    Ym2151WriteData(parentModule.UnitNumber, 0x60, op, Slot, (byte)timbre.Ops[op].TL);
                    Ym2151WriteData(parentModule.UnitNumber, 0x80, op, Slot, (byte)((timbre.Ops[op].RS << 6 | timbre.Ops[op].AR)));
                    Ym2151WriteData(parentModule.UnitNumber, 0xa0, op, Slot, (byte)((timbre.Ops[op].AM << 7 | timbre.Ops[op].D1R)));
                    Ym2151WriteData(parentModule.UnitNumber, 0xc0, op, Slot, (byte)((timbre.Ops[op].DT2 << 7 | timbre.Ops[op].D2R)));
                    Ym2151WriteData(parentModule.UnitNumber, 0xe0, op, Slot, (byte)((timbre.Ops[op].SL << 7 | timbre.Ops[op].RR)));
                }

                UpdatePanpot();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void Off()
            {
                Ym2151WriteData(parentModule.UnitNumber, 0x08, 0, 0, (byte)(0x00 | Slot));
            }

        }


        /// <summary>
        /// 
        /// </summary>
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2151Timbre>))]
        [DataContract]
        public class YM2151Timbre : TimbreBase
        {
            #region FM Symth

            private byte f_FB;

            [DataMember]
            [Category("Sound")]
            [Description("Feedback (0-7)")]
            public byte FB
            {
                get
                {
                    return f_FB;
                }
                set
                {
                    f_FB = (byte)(value & 7);
                }
            }

            private byte f_ALG;

            [DataMember]
            [Category("Sound")]
            [Description("Algorithm (0-7)")]
            public byte ALG
            {
                get
                {
                    return f_ALG;
                }
                set
                {
                    f_ALG = (byte)(value & 7);
                }
            }

            private byte f_AMS;

            [DataMember]
            [Category("Sound")]
            [Description("Amplitude Modulation Sensitivity (0-3)")]
            public byte AMS
            {
                get
                {
                    return f_AMS;
                }
                set
                {
                    f_AMS = (byte)(value & 3);
                }
            }

            private byte f_PMS;

            [DataMember]
            [Category("Sound")]
            [Description("Phase Modulation Sensitivity (0-7)")]
            public byte PMS
            {
                get
                {
                    return f_PMS;
                }
                set
                {
                    f_PMS = (byte)(value & 7);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Operators")]
            public YM2151Operator[] Ops
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            public YM2151Timbre()
            {
                Ops = new YM2151Operator[] { new YM2151Operator(), new YM2151Operator(), new YM2151Operator(), new YM2151Operator() };
            }

            #endregion

            public override void RestoreFrom(string serializeData)
            {
                var obj = JsonConvert.DeserializeObject<YM2151Timbre>(serializeData);
                this.InjectFrom(obj);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2151Operator>))]
        [DataContract]
        public class YM2151Operator
        {
            private byte f_Enable;

            /// <summary>
            /// Enable(0-1)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Whether this operator enable or not")]
            public byte Enable
            {
                get
                {
                    return f_Enable;
                }
                set
                {
                    f_Enable = (byte)(value & 1);
                }
            }

            private byte f_DT1;

            /// <summary>
            /// Detune1(0-7)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("DeTune 1 (1-4-7)")]
            public byte DT1
            {
                get
                {
                    return f_DT1;
                }
                set
                {
                    f_DT1 = (byte)(value & 7);
                }
            }

            private byte f_MUL;

            /// <summary>
            /// Multiply(0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Multiply (0-15)")]
            public byte MUL
            {
                get
                {
                    return f_MUL;
                }
                set
                {
                    f_MUL = (byte)(value & 15);
                }
            }

            private byte f_TL;

            /// <summary>
            /// Total Level(0-127)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Total Level (0-127)")]
            public byte TL
            {
                get
                {
                    return f_TL;
                }
                set
                {
                    f_TL = (byte)(value & 127);
                }
            }

            private byte f_RS;

            /// <summary>
            /// Rate Scaling(0-3)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Rate Scaling (0-3)")]
            public byte RS
            {
                get
                {
                    return f_RS;
                }
                set
                {
                    f_RS = (byte)(value & 3);
                }
            }

            private byte f_AR;

            /// <summary>
            /// Attack Rate(0-31)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Attack Rate (0-31)")]
            public byte AR
            {
                get
                {
                    return f_AR;
                }
                set
                {
                    f_AR = (byte)(value & 31);
                }
            }

            private byte f_AM;

            /// <summary>
            /// amplitude modulation sensivity(0-1)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Amplitude Modulation Sensivity (0-1)")]
            public byte AM
            {
                get
                {
                    return f_AM;
                }
                set
                {
                    f_AM = (byte)(value & 1);
                }
            }

            private byte f_D1R;

            /// <summary>
            /// 1st decay rate(0-31)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("1st Decay Rate (0-31)")]
            public byte D1R
            {
                get
                {
                    return f_D1R;
                }
                set
                {
                    f_D1R = (byte)(value & 31);
                }
            }

            private byte f_D2R;

            /// <summary>
            /// 2nd decay rate(0-31)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("2nd Decay Rate (0-31)")]
            public byte D2R
            {
                get
                {
                    return f_D2R;
                }
                set
                {
                    f_D2R = (byte)(value & 31);
                }
            }

            private byte f_SL;

            /// <summary>
            /// sustain level(0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Sustain Level(0-15)")]
            public byte SL
            {
                get
                {
                    return f_SL;
                }
                set
                {
                    f_SL = (byte)(value & 15);
                }
            }

            private byte f_RR;

            /// <summary>
            /// release rate(0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Release Rate (0-15)")]
            public byte RR
            {
                get
                {
                    return f_RR;
                }
                set
                {
                    f_RR = (byte)(value & 15);
                }
            }

            private byte f_DT2;

            /// <summary>
            /// DT2(0-3)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Detune 2 (0-3)")]
            public byte DT2
            {
                get
                {
                    return f_DT2;
                }
                set
                {
                    f_DT2 = (byte)(value & 3);
                }
            }
        }

    }


}