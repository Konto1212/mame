// copyright-holders:K.Ito
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Smf;
using Newtonsoft.Json;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Mame;
using zanac.MAmidiMEmo.Midi;

//http://hp.vector.co.jp/authors/VA042397/nes/apu.html
//https://wiki.nesdev.com/w/index.php/APU

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public partial class RP2A03 : InstrumentBase
    {

        public override string Name => "RP2A03";

        public override string Group => "PSG";

        public override InstrumentType InstrumentType => InstrumentType.RP2A03;

        [Browsable(false)]
        public override string ImageKey => "RP2A03";

        /// <summary>
        /// 
        /// </summary>
        [Category("MIDI")]
        [Description("MIDI Device ID")]
        public override uint DeviceID
        {
            get
            {
                return 6;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix
        {
            get
            {
                return "nes_apu_";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Timbres (0-127)")]
        [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public RP2A03Timbre[] Timbres
        {
            get;
            private set;
        }


        [DataMember]
        [Category("Chip")]
        [Description("Delta PCM Data (Max 4081 bytes)")]
        [Editor(typeof(PcmUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [PcmEditor("DMC File|*.dmc")]
        public DPcmSoundTable DeltaPcmSoundTable
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializeData"></param>
        public override void RestoreFrom(string serializeData)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<RP2A03>(serializeData);
                this.InjectFrom(new LoopInjection(new[] { "SerializeData" }), obj);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;


                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_RP2A03_write(uint unitNumber, uint address, byte data);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte delegate_RP2A03_read(uint unitNumber, uint address);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_RP2A03_SetDPCM(uint unitNumber, byte[] data, uint length);

        /// <summary>
        /// 
        /// </summary>
        private static delegate_RP2A03_write RP2A03_write
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_RP2A03_read RP2A03_read
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_RP2A03_SetDPCM RP2A03_SetDPCM
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static void RP2A03WriteData(uint unitNumber, uint address, byte data)
        {
            try
            {
                Program.SoundUpdating();
                RP2A03_write(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static byte RP2A03ReadData(uint unitNumber, uint address)
        {
            try
            {
                Program.SoundUpdating();
                return RP2A03_read(unitNumber, address);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }


        /// 
        /// </summary>
        private static void RP2A03SetDpcm(uint unitNumber, byte[] data)
        {
            try
            {
                Program.SoundUpdating();
                RP2A03_SetDPCM(unitNumber, data, (uint)data.Length);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static RP2A03()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("nes_apu_regwrite");
            if (funcPtr != IntPtr.Zero)
            {
                RP2A03_write = Marshal.GetDelegateForFunctionPointer<delegate_RP2A03_write>(funcPtr);
            }
            funcPtr = MameIF.GetProcAddress("nes_apu_regread");
            if (funcPtr != IntPtr.Zero)
            {
                RP2A03_read = Marshal.GetDelegateForFunctionPointer<delegate_RP2A03_read>(funcPtr);
            }
            funcPtr = MameIF.GetProcAddress("nes_apu_set_dpcm");
            if (funcPtr != IntPtr.Zero)
            {
                RP2A03_SetDPCM = Marshal.GetDelegateForFunctionPointer<delegate_RP2A03_SetDPCM>(funcPtr);
            }
        }

        private RP2A03SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public RP2A03(uint unitNumber) : base(unitNumber)
        {
            Timbres = new RP2A03Timbre[128];
            for (int i = 0; i < 128; i++)
                Timbres[i] = new RP2A03Timbre();
            DeltaPcmSoundTable = new DPcmSoundTable();

            setPresetInstruments();

            this.soundManager = new RP2A03SoundManager(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            soundManager?.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {
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
        private class RP2A03SoundManager : SoundManagerBase
        {
            private SoundList<RP2A03Sound> sqOnSounds = new SoundList<RP2A03Sound>(2);

            private SoundList<RP2A03Sound> triOnSounds = new SoundList<RP2A03Sound>(1);

            private SoundList<RP2A03Sound> noiseOnSounds = new SoundList<RP2A03Sound>(1);

            private SoundList<RP2A03Sound> dpcmOnSounds = new SoundList<RP2A03Sound>(1);

            private RP2A03 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public RP2A03SoundManager(RP2A03 parent)
            {
                this.parentModule = parent;
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

                RP2A03Sound snd = new RP2A03Sound(parentModule, this, note, emptySlot);
                switch (snd.Timbre.ToneType)
                {
                    case ToneType.SQUARE:
                        sqOnSounds.Add(snd);
                        FormMain.OutputDebugLog("KeyOn SQ ch" + emptySlot + " " + note.ToString());
                        break;
                    case ToneType.TRIANGLE:
                        triOnSounds.Add(snd);
                        FormMain.OutputDebugLog("KeyOn Tri ch" + emptySlot + " " + note.ToString());
                        break;
                    case ToneType.NOISE:
                        noiseOnSounds.Add(snd);
                        FormMain.OutputDebugLog("KeyOn Noise ch" + emptySlot + " " + note.ToString());
                        break;
                    case ToneType.DPCM:
                        dpcmOnSounds.Add(snd);
                        FormMain.OutputDebugLog("KeyOn DPCM ch" + emptySlot + " " + note.ToString());
                        break;
                }
                snd.On();

                base.NoteOn(note);
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
                switch (timbre.ToneType)
                {
                    case ToneType.SQUARE:
                        {
                            emptySlot = SearchEmptySlotAndOff(sqOnSounds, note, 2);
                            break;
                        }
                    case ToneType.TRIANGLE:
                        {
                            emptySlot = SearchEmptySlotAndOff(triOnSounds, note, 1);
                            break;
                        }
                    case ToneType.NOISE:
                        {
                            emptySlot = SearchEmptySlotAndOff(noiseOnSounds, note, 1);
                            break;
                        }
                    case ToneType.DPCM:
                        {
                            emptySlot = SearchEmptySlotAndOff(dpcmOnSounds, note, 1);
                            break;
                        }
                }
                return emptySlot;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            public override SoundBase NoteOff(NoteOffEvent note)
            {
                RP2A03Sound removed = (RP2A03Sound)base.NoteOff(note);

                if (removed != null)
                {
                    for (int i = 0; i < sqOnSounds.Count; i++)
                    {
                        if (sqOnSounds[i] == removed)
                        {
                            FormMain.OutputDebugLog("KeyOff SQ ch" + removed.Slot + " " + note.ToString());
                            sqOnSounds.RemoveAt(i);
                            return removed;
                        }
                    }
                    for (int i = 0; i < triOnSounds.Count; i++)
                    {
                        if (sqOnSounds[i] == removed)
                        {
                            FormMain.OutputDebugLog("KeyOff Tri ch" + removed.Slot + " " + note.ToString());
                            sqOnSounds.RemoveAt(i);
                            return removed;
                        }
                    }
                    for (int i = 0; i < noiseOnSounds.Count; i++)
                    {
                        if (sqOnSounds[i] == removed)
                        {
                            FormMain.OutputDebugLog("KeyOff Noise ch" + removed.Slot + " " + note.ToString());
                            sqOnSounds.RemoveAt(i);
                            return removed;
                        }
                    }
                    for (int i = 0; i < dpcmOnSounds.Count; i++)
                    {
                        if (sqOnSounds[i] == removed)
                        {
                            FormMain.OutputDebugLog("KeyOff DPCM ch" + removed.Slot + " " + note.ToString());
                            sqOnSounds.RemoveAt(i);
                            return removed;
                        }
                    }
                }

                return removed;
            }


        }


        /// <summary>
        /// 
        /// </summary>
        private class RP2A03Sound : SoundBase
        {

            private RP2A03 parentModule;

            private SevenBitNumber programNumber;

            public RP2A03Timbre Timbre;

            private ToneType lastToneType;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public RP2A03Sound(RP2A03 parentModule, RP2A03SoundManager manager, NoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.programNumber = (SevenBitNumber)parentModule.ProgramNumbers[noteOnEvent.Channel];
                this.Timbre = parentModule.Timbres[programNumber];

                lastToneType = Timbre.ToneType;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void On()
            {
                base.On();

                switch (lastToneType)
                {
                    case ToneType.SQUARE:
                        {
                            var pn = parentModule.ProgramNumbers[NoteOnEvent.Channel];
                            var timbre = parentModule.Timbres[pn];

                            byte data = (byte)(RP2A03ReadData(parentModule.UnitNumber, 0x15) & ~(1 << Slot));
                            RP2A03WriteData(parentModule.UnitNumber, 0x15, (byte)(data | (1 << Slot)));

                            RP2A03WriteData(parentModule.UnitNumber, (uint)((Slot * 4) + 0x01),
                                (byte)(timbre.SQSweepEnable << 7 | timbre.SQSweepUpdateRate << 4 |
                                timbre.SQSweepDirection << 3 | timbre.SQSweepRange));

                            //Volume
                            updateSqVolume();

                            //Freq
                            updateSqPitch();

                            break;
                        }
                    case ToneType.TRIANGLE:
                        {
                            var pn = parentModule.ProgramNumbers[NoteOnEvent.Channel];
                            var timbre = parentModule.Timbres[pn];

                            byte data = (byte)(RP2A03ReadData(parentModule.UnitNumber, 0x15) & ~(1 << 2));
                            RP2A03WriteData(parentModule.UnitNumber, 0x15, (byte)(data | (1 << 2)));

                            RP2A03WriteData(parentModule.UnitNumber, (uint)((2 * 4) + 0x00),
                                (byte)(timbre.LengthCounterDisable << 7 | timbre.TriCounterLength));

                            //Freq
                            updateTriPitch();

                            break;
                        }
                    case ToneType.NOISE:
                        {
                            byte data = (byte)(RP2A03ReadData(parentModule.UnitNumber, 0x15) & ~(1 << 3));
                            RP2A03WriteData(parentModule.UnitNumber, 0x15, (byte)(data | 8));

                            //Volume
                            updateNoiseVolume();

                            //Freq
                            UpdateNoisePitch();

                            break;
                        }
                    case ToneType.DPCM:
                        {
                            //https://wiki.nesdev.com/w/index.php/APU_DMC

                            var pn = parentModule.ProgramNumbers[NoteOnEvent.Channel];
                            var timbre = parentModule.Timbres[pn];
                            int noteNum = NoteOnEvent.NoteNumber;

                            //keyoff
                            byte data = (byte)(RP2A03ReadData(parentModule.UnitNumber, 0x15) & ~(1 << 4));
                            RP2A03WriteData(parentModule.UnitNumber, 0x15, (byte)data);

                            // Loop / Smple Rate
                            RP2A03WriteData(parentModule.UnitNumber, (uint)0x10,
                                (byte)(timbre.DeltaPcmLoopEnable << 6 | timbre.DeltaPcmBitRate));

                            //Size
                            if (parentModule.DeltaPcmSoundTable.PcmTimbres[noteNum].PcmData != null)
                            {
                                RP2A03SetDpcm(parentModule.UnitNumber, parentModule.DeltaPcmSoundTable.PcmTimbres[noteNum].PcmData);
                                int sz = parentModule.DeltaPcmSoundTable.PcmTimbres[noteNum].PcmData.Length - 1;
                                if (sz > 4081)
                                    sz = 4081;
                                if (sz >= 16)
                                {
                                    RP2A03WriteData(parentModule.UnitNumber, (uint)0x13, (byte)((sz / 16) & 0xff));
                                    //keyon
                                    RP2A03WriteData(parentModule.UnitNumber, 0x15, (byte)(data | 16));
                                }
                            }

                            break;
                        }
                }
            }

            public override void UpdateVolume()
            {
                switch (lastToneType)
                {
                    case ToneType.SQUARE:
                        updateSqVolume();
                        break;
                    case ToneType.NOISE:
                        updateNoiseVolume();
                        break;
                    case ToneType.DPCM:
                        updateDpcmVolume();
                        break;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            private void updateSqVolume()
            {
                var exp = parentModule.Expressions[NoteOnEvent.Channel] / 127d;
                var vol = parentModule.Volumes[NoteOnEvent.Channel] / 127d;
                var vel = NoteOnEvent.Velocity / 127d;

                byte fv = (byte)((byte)Math.Round(Timbre.Volume * vol * vel * exp) & 0xf);

                byte dd = Timbre.DecayDisable;
                byte ld = Timbre.LengthCounterDisable;
                byte dc = Timbre.SQDutyCycle;

                RP2A03WriteData(parentModule.UnitNumber, (uint)((Slot * 4) + 0x00), (byte)(dc << 6 | ld << 5 | dd << 4 | fv));
            }

            /// <summary>
            /// 
            /// </summary>
            private void updateNoiseVolume()
            {
                var exp = parentModule.Expressions[NoteOnEvent.Channel] / 127d;
                var vol = parentModule.Volumes[NoteOnEvent.Channel] / 127d;
                var vel = NoteOnEvent.Velocity / 127d;

                byte fv = (byte)((byte)Math.Round(Timbre.Volume * vol * vel * exp) & 0xf);

                byte dd = Timbre.DecayDisable;
                byte ld = Timbre.LengthCounterDisable;

                RP2A03WriteData(parentModule.UnitNumber, (uint)(0x0c), (byte)(ld << 5 | dd << 4 | fv));
            }

            /// <summary>
            /// 
            /// </summary>
            private void updateDpcmVolume()
            {
                var vol = parentModule.Volumes[NoteOnEvent.Channel];

                RP2A03WriteData(parentModule.UnitNumber, (uint)(0x11), vol);
            }

            /// <summary>
            /// 
            /// </summary>
            public override void UpdatePitch()
            {
                switch (lastToneType)
                {
                    case ToneType.SQUARE:
                        updateSqPitch();
                        break;
                    case ToneType.TRIANGLE:
                        updateTriPitch();
                        break;
                    case ToneType.NOISE:
                        UpdateNoisePitch();
                        break;
                }
            }

            private void updateSqPitch()
            {
                double freq = CalcCurrentFrequency();

                var n = (ushort)((ushort)(Math.Round(1790000d / (freq * 32)) - 1) & 0x7ff);
                Program.SoundUpdating();
                RP2A03WriteData(parentModule.UnitNumber, (uint)((Slot * 4) + 0x02), (byte)(n & 0xff));
                RP2A03WriteData(parentModule.UnitNumber, (uint)((Slot * 4) + 0x03), (byte)((Timbre.PlayLength << 3) | (n >> 8) & 0x7));
                Program.SoundUpdated();
            }

            private void updateTriPitch()
            {
                double freq = CalcCurrentFrequency();

                var n = (ushort)((ushort)(Math.Round(1790000d / (freq * 32)) - 1) & 0x7ff);
                Program.SoundUpdating();
                RP2A03WriteData(parentModule.UnitNumber, (uint)((2 * 4) + 0x02), (byte)(n & 0xff));
                RP2A03WriteData(parentModule.UnitNumber, (uint)((2 * 4) + 0x03), (byte)((Timbre.PlayLength << 3) | (n >> 8) & 0x7));
                Program.SoundUpdated();
            }

            public void UpdateNoisePitch()
            {
                var pitch = (int)(parentModule.Pitchs[NoteOnEvent.Channel] - 8192) / (8192 / 32);
                int n = 31 - ((NoteOnEvent.NoteNumber + pitch) % 32);

                Program.SoundUpdating();
                RP2A03WriteData(parentModule.UnitNumber, (uint)((3 * 4) + 0x02), (byte)((Timbre.NoiseType << 7) | (n & 0xf)));
                RP2A03WriteData(parentModule.UnitNumber, (uint)((3 * 4) + 0x03), (byte)(Timbre.PlayLength << 3));
                Program.SoundUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void Off()
            {
                switch (lastToneType)
                {
                    case ToneType.SQUARE:
                        {
                            byte data = (byte)(RP2A03ReadData(parentModule.UnitNumber, 0x15) & ~(1 << Slot));
                            Program.SoundUpdating();
                            RP2A03WriteData(parentModule.UnitNumber, 0x15, data);
                            RP2A03WriteData(parentModule.UnitNumber, 0x15, (byte)(data | (1 << Slot)));
                            Program.SoundUpdated();
                            break;
                        }
                    case ToneType.TRIANGLE:
                        {
                            byte data = (byte)(RP2A03ReadData(parentModule.UnitNumber, 0x15) & ~(1 << 2));
                            Program.SoundUpdating();
                            RP2A03WriteData(parentModule.UnitNumber, 0x15, data);
                            RP2A03WriteData(parentModule.UnitNumber, 0x15, (byte)(data | (1 << 2)));
                            Program.SoundUpdated();
                            break;
                        }
                    case ToneType.NOISE:
                        {
                            byte data = (byte)(RP2A03ReadData(parentModule.UnitNumber, 0x15) & ~8);
                            Program.SoundUpdating();
                            RP2A03WriteData(parentModule.UnitNumber, 0x15, data);
                            RP2A03WriteData(parentModule.UnitNumber, 0x15, (byte)(data | 8));
                            Program.SoundUpdated();
                            break;
                        }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<RP2A03Timbre>))]
        [DataContract]
        public class RP2A03Timbre : TimbreBase
        {

            [DataMember]
            [Category("Sound")]
            [Description("Sound Type")]
            public ToneType ToneType
            {
                get;
                set;
            }

            private byte f_Volume = 15;

            [DataMember]
            [Category("Sound(SQ/Noise)")]
            [Description("Square/Noise Volume (0-15)")]
            public byte Volume
            {
                get
                {
                    return f_Volume;
                }
                set
                {
                    f_Volume = (byte)(value & 15);
                }
            }

            private byte f_DecayDisable = 1;

            [DataMember]
            [Category("Sound(SQ)")]
            [Description("Square Envelope Decay Disable (0:Enable 1:Disable)")]
            public byte DecayDisable
            {
                get
                {
                    return f_DecayDisable;
                }
                set
                {
                    f_DecayDisable = (byte)(value & 1);
                }
            }


            private byte f_LengthDisable = 1;

            [DataMember]
            [Category("Sound(SQ/Tri)")]
            [Description("Square/Tri Length Counter Clock Disable (0:Enable 1:Disable)")]
            public byte LengthCounterDisable
            {
                get
                {
                    return f_LengthDisable;
                }
                set
                {
                    f_LengthDisable = (byte)(value & 1);
                }
            }


            private byte f_NoiseEnvDecayEnable;

            [DataMember]
            [Category("Sound(Noise)")]
            [Description("Noise Envelope Decay Looping Enable (0:Disable 1:Enable)")]
            public byte NoiseEnvDecayEnable
            {
                get
                {
                    return f_NoiseEnvDecayEnable;
                }
                set
                {
                    f_NoiseEnvDecayEnable = (byte)(value & 1);
                }
            }

            private byte f_SQDutyCycle;

            [DataMember]
            [Category("Sound(SQ)")]
            [Description("Square Duty Cycle (0:87.5% 1:75% 2:50% 3:25%)")]
            public byte SQDutyCycle
            {
                get
                {
                    return f_SQDutyCycle;
                }
                set
                {
                    f_SQDutyCycle = (byte)(value & 3);
                }
            }

            private byte f_SQSweepEnable;

            [DataMember]
            [Category("Sound(SQ)")]
            [Description("Square Sweep Enable (0:Disable 1:Enable)")]
            public byte SQSweepEnable
            {
                get
                {
                    return f_SQSweepEnable;
                }
                set
                {
                    f_SQSweepEnable = (byte)(value & 1);
                }
            }


            private byte f_SQSweepUpdateRate;

            [DataMember]
            [Category("Sound(SQ)")]
            [Description("Square Sweep Update Rate (0-7)")]
            public byte SQSweepUpdateRate
            {
                get
                {
                    return f_SQSweepUpdateRate;
                }
                set
                {
                    f_SQSweepUpdateRate = (byte)(value & 7);
                }
            }

            private byte f_SQSweepDirection;

            [DataMember]
            [Category("Sound(SQ)")]
            [Description("Wave Length (0:Decrease 1:Increse)")]
            public byte SQSweepDirection
            {
                get
                {
                    return f_SQSweepDirection;
                }
                set
                {
                    f_SQSweepDirection = (byte)(value & 1);
                }
            }

            private byte f_SQSweepRange;

            [DataMember]
            [Category("Sound(SQ)")]
            [Description("Wave Length (0-7)")]
            public byte SQSweepRange
            {
                get
                {
                    return f_SQSweepRange;
                }
                set
                {
                    f_SQSweepRange = (byte)(value & 7);
                }
            }

            private byte f_NoiseType;

            [DataMember]
            [Category("Sound(Noise)")]
            [Description("Noise Type (0:32k bit 1:93bit)")]
            public byte NoiseType
            {
                get
                {
                    return f_NoiseType;
                }
                set
                {
                    f_NoiseType = (byte)(value & 1);
                }
            }


            private byte f_PlayLength;

            [DataMember]
            [Category("Sound")]
            [Description("Square/Tri Play Length (0-31)")]
            public byte PlayLength
            {
                get
                {
                    return f_PlayLength;
                }
                set
                {
                    f_PlayLength = (byte)(value & 31);
                }
            }


            private byte f_TriCounterLength = 127;

            [DataMember]
            [Category("Sound(Tri)")]
            [Description("Tri Linear Counter Length (0-127)")]
            public byte TriCounterLength
            {
                get
                {
                    return f_TriCounterLength;
                }
                set
                {
                    f_TriCounterLength = (byte)(value & 127);
                }
            }


            private byte f_DeltaPcmBitRate = 15;

            [DataMember]
            [Category("Sound(DPCM)")]
            [Description("DPCM Sample Bit Rate (0:4KHz-15:32KHz)")]
            public byte DeltaPcmBitRate
            {
                get
                {
                    return f_DeltaPcmBitRate;
                }
                set
                {
                    f_DeltaPcmBitRate = (byte)(value & 15);
                }
            }

            private byte f_DeltaPcmLoop = 0;

            [DataMember]
            [Category("Sound(DPCM)")]
            [Description("DPCM Loop Play (0:Off 1:On)")]
            public byte DeltaPcmLoopEnable
            {
                get
                {
                    return f_DeltaPcmLoop;
                }
                set
                {
                    f_DeltaPcmLoop = (byte)(value & 1);
                }
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="serializeData"></param>
            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<RP2A03Timbre>(serializeData);
                    this.InjectFrom(new LoopInjection(new[] { "SerializeData" }), obj);
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(Exception))
                        throw;
                    else if (ex.GetType() == typeof(SystemException))
                        throw;


                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public enum ToneType
        {
            SQUARE,
            TRIANGLE,
            NOISE,
            DPCM,
        }

        /// <summary>
        /// 
        /// </summary>
        [DataContract]
        public class DPcmSoundTable : PcmTimbreTableBase
        {
            /// <summary>
            /// 
            /// </summary>
            public DPcmSoundTable()
            {
                for (int i = 0; i < 128; i++)
                    PcmTimbres[i] = new DeltaPcmSound(i);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [DataContract]
        public class DeltaPcmSound : PcmTimbreBase
        {

            private byte[] f_DeltaPcmData;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Browsable(false)]
            public override byte[] PcmData
            {
                get
                {
                    return f_DeltaPcmData;
                }
                set
                {
                    if (value != null)
                    {
                        List<byte> al = new List<byte>(value);
                        //Max 4081
                        if (al.Count > 4081)
                            al.RemoveRange(4081, al.Count - 4081);
                        //Pad
                        if ((al.Count - 1) % 16 != 0)
                        {
                            for (int i = 0; i < (al.Count - 1) % 16; i++)
                                al.Add(0);
                        }
                        f_DeltaPcmData = al.ToArray();
                    }
                    else
                    {
                        f_DeltaPcmData = value;
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="noteNumber"></param>
            public DeltaPcmSound(int noteNumber) : base(noteNumber)
            {
            }
        }

    }
}