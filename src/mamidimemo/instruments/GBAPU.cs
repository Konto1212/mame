// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Linq;
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
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Mame;
using zanac.MAmidiMEmo.Midi;

//http://mydocuments.g2.xrea.com/html/gb/soundspec.html
//http://bgb.bircd.org/pandocs.htm#soundcontrolregisters
//https://gbdev.gg8.se/wiki/articles/Gameboy_sound_hardware
//http://mydocuments.g2.xrea.com/
//http://marc.rawer.de/Gameboy/Docs/GBCPUman.pdf
//http://www.devrs.com/gb/files/hosted/GBSOUND.txt

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class GB_APU : InstrumentBase
    {
        public override string Name => "GB_APU";

        public override string Group => "PSG";

        public override InstrumentType InstrumentType => InstrumentType.GB_APU;

        [Browsable(false)]
        public override string ImageKey => "GB_APU";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "gbsnd_";

        /// <summary>
        /// 
        /// </summary>
        [Category("MIDI")]
        [Description("MIDI Device ID")]
        public override uint DeviceID
        {
            get
            {
                return 5;
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
        public GBAPUTimbre[] Timbres
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override TimbreBase GetTimbre(int channel)
        {
            var pn = (SevenBitNumber)ProgramNumbers[channel];
            return Timbres[pn];
        }

        private bool f_PartialReserveSPSG;

        [DataMember]
        [Category("Chip")]
        [Description("SPSG partial reserve against with PSG.\r\n" +
            "Sweep PSG shared 1ch with PSG." +
            "So, you can choose whether to give priority to SPSG over PSG")]
        public bool PartialReserveSPSG
        {
            get
            {
                return f_PartialReserveSPSG;
            }
            set
            {
                f_PartialReserveSPSG = value;
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
                var obj = JsonConvert.DeserializeObject<GB_APU>(serializeData);
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
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_gb_apu_write(uint unitNumber, uint address, byte data);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte delegate_gb_apu_read(uint unitNumber, uint address);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_gb_apu_wave_write(uint unitNumber, uint address, byte data);

        /// <summary>
        /// 
        /// </summary>
        private static void GbApuWriteData(uint unitNumber, uint address, byte data)
        {
            try
            {
                Program.SoundUpdating();
                GbApu_write(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_gb_apu_write GbApu_write
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static void GbApuWaveWriteData(uint unitNumber, uint address, byte data)
        {
            try
            {
                Program.SoundUpdating();
                GbApu_wave_write(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_gb_apu_wave_write GbApu_wave_write
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private static byte GbApuReadData(uint unitNumber, uint address)
        {
            try
            {
                Program.SoundUpdating();
                return GbApu_read(unitNumber, address);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_gb_apu_read GbApu_read
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        static GB_APU()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("gb_apu_write");
            if (funcPtr != IntPtr.Zero)
                GbApu_write = (delegate_gb_apu_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_gb_apu_write));
            funcPtr = MameIF.GetProcAddress("gb_apu_read");
            if (funcPtr != IntPtr.Zero)
                GbApu_read = (delegate_gb_apu_read)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_gb_apu_read));
            funcPtr = MameIF.GetProcAddress("gb_apu_wave_write");
            if (funcPtr != IntPtr.Zero)
                GbApu_wave_write = (delegate_gb_apu_wave_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_gb_apu_wave_write));
        }

        private GBSoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public GB_APU(uint unitNumber) : base(unitNumber)
        {
            Timbres = new GBAPUTimbre[128];
            for (int i = 0; i < 128; i++)
                Timbres[i] = new GBAPUTimbre();
            setPresetInstruments();

            this.soundManager = new GBSoundManager(this);
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
            Timbres[0].SoundType = SoundType.PSG;

            Timbres[1].SoundType = SoundType.WAV;
            Timbres[1].WsgData = new byte[]
            {
                 7, 10, 12, 13, 14, 13, 12, 10,
                 7,  4,  2,  1,  0,  1,  2,  4,
                 7, 11, 13, 14, 13, 11,  7,  3,
                 1,  0,  1,  3,  7, 14,  7,  0,  };
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnNoteOnEvent(NoteOnEvent midiEvent)
        {
            soundManager.KeyOn(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnNoteOffEvent(NoteOffEvent midiEvent)
        {
            soundManager.KeyOff(midiEvent);
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
        private class GBSoundManager : SoundManagerBase
        {
            private SoundList<GbSound> spsgOnSounds = new SoundList<GbSound>(1);

            private SoundList<GbSound> psgOnSounds = new SoundList<GbSound>(2);

            private SoundList<GbSound> wavOnSounds = new SoundList<GbSound>(1);

            private SoundList<GbSound> noiseOnSounds = new SoundList<GbSound>(1);


            private GB_APU parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public GBSoundManager(GB_APU parent) : base(parent)
            {
                this.parentModule = parent;

                //Sound On
                GbApuWriteData(parentModule.UnitNumber, 0x16, 0x80);
                GbApuWriteData(parentModule.UnitNumber, 0x14, 0x77);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            public override SoundBase SoundOn(NoteOnEvent note)
            {
                int emptySlot = searchEmptySlot(note);
                if (emptySlot < 0)
                    return null;

                var programNumber = (SevenBitNumber)parentModule.ProgramNumbers[note.Channel];
                var timbre = parentModule.Timbres[programNumber];
                GbSound snd = new GbSound(parentModule, this, timbre, note, emptySlot);
                switch (timbre.SoundType)
                {
                    case SoundType.SPSG:
                        spsgOnSounds.Add(snd);
                        FormMain.OutputDebugLog("KeyOn SPSG ch" + emptySlot + " " + note.ToString());
                        break;
                    case SoundType.PSG:
                        psgOnSounds.Add(snd);
                        FormMain.OutputDebugLog("KeyOn PSG ch" + emptySlot + " " + note.ToString());
                        break;
                    case SoundType.WAV:
                        wavOnSounds.Add(snd);
                        FormMain.OutputDebugLog("KeyOn WAV ch" + emptySlot + " " + note.ToString());
                        break;
                    case SoundType.NOISE:
                        noiseOnSounds.Add(snd);
                        FormMain.OutputDebugLog("KeyOn NOISE ch" + emptySlot + " " + note.ToString());
                        break;
                }
                snd.KeyOn();

                return snd;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            private int searchEmptySlot(NoteOnEvent note)
            {
                int emptySlot = -1;

                var programNumber = (SevenBitNumber)parentModule.ProgramNumbers[note.Channel];
                var timbre = parentModule.Timbres[programNumber];
                switch (timbre.SoundType)
                {
                    case SoundType.SPSG:
                        {
                            emptySlot = SearchEmptySlotAndOff(spsgOnSounds, note, 1);
                            break;
                        }
                    case SoundType.PSG:
                        {
                            if (parentModule.PartialReserveSPSG && spsgOnSounds.Count != 0)
                                emptySlot = SearchEmptySlotAndOff(psgOnSounds, note, 1);
                            else
                                emptySlot = SearchEmptySlotAndOff(psgOnSounds, note, 2);
                            break;
                        }
                    case SoundType.WAV:
                        {
                            emptySlot = SearchEmptySlotAndOff(wavOnSounds, note, 1);
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            emptySlot = SearchEmptySlotAndOff(noiseOnSounds, note, 1);
                            break;
                        }
                }
                return emptySlot;
            }

        }


        /// <summary>
        /// 
        /// </summary>
        private class GbSound : SoundBase
        {

            private GB_APU parentModule;

            private SevenBitNumber programNumber;

            private GBAPUTimbre timbre;

            private SoundType lastSoundType;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public GbSound(GB_APU parentModule, GBSoundManager manager, TimbreBase timbre, NoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.programNumber = (SevenBitNumber)parentModule.ProgramNumbers[noteOnEvent.Channel];
                this.timbre = parentModule.Timbres[programNumber];

                lastSoundType = this.timbre.SoundType;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                switch (lastSoundType)
                {
                    case SoundType.SPSG:
                    case SoundType.PSG:
                        {
                            uint reg = (uint)(Slot * 5);

                            if(lastSoundType == SoundType.SPSG)
                                GbApuWriteData(parentModule.UnitNumber, reg, (byte)(timbre.SweepTime << 4 | timbre.SweepDir << 3 | timbre.SweepNumber));
                            else
                                GbApuWriteData(parentModule.UnitNumber, reg, 0x00);
                            GbApuWriteData(parentModule.UnitNumber, reg + 1, (byte)(timbre.Duty << 6 | timbre.SoundLength));

                            UpdateVolume();

                            UpdatePanpot();

                            UpdatePitch(0x80);

                            break;
                        }
                    case SoundType.WAV:
                        {
                            uint reg = (uint)((Slot + 2) * 5);

                            GbApuWriteData(parentModule.UnitNumber, reg, 0x00);
                            GbApuWriteData(parentModule.UnitNumber, reg + 1, timbre.SoundLength);

                            UpdateVolume();

                            UpdatePanpot();

                            //Wave
                            for (int i = 0; i < 16; i++)
                                GbApuWaveWriteData(parentModule.UnitNumber, (uint)i, (byte)(((timbre.WsgData[i * 2] & 0xf) << 4) | (timbre.WsgData[(i * 2) + 1] & 0xf)));

                            GbApuWriteData(parentModule.UnitNumber, reg, 0x80);

                            UpdatePitch(0x80);

                            break;
                        }
                    case SoundType.NOISE:
                        {
                            uint reg = (uint)((Slot + 3) * 5);

                            //GbApuWriteData(parentModule.UnitNumber, reg, 0x00);
                            GbApuWriteData(parentModule.UnitNumber, reg + 1, timbre.SoundLength);

                            UpdateVolume();

                            UpdatePanpot();

                            UpdatePitch(0x80);

                            break;
                        }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public override void UpdateVolume()
            {
                switch (lastSoundType)
                {
                    case SoundType.SPSG:
                    case SoundType.PSG:
                        {
                            uint reg = (uint)(Slot * 5);
                            byte tl = (byte)Math.Round(timbre.EnvInitialVolume * CalcCurrentVolume());

                            byte edir = (byte)(timbre.EnvDirection << 3);
                            byte elen = timbre.EnvLength;

                            GbApuWriteData(parentModule.UnitNumber, reg + 2, (byte)((tl << 4) | edir | elen));
                            break;
                        }
                    case SoundType.WAV:
                        {
                            uint reg = (uint)((Slot + 2) * 5);
                            byte tl = (byte)(Math.Round(3d * CalcCurrentVolume()));
                            switch (tl)
                            {
                                case 3:
                                    tl = 1;
                                    break;
                                case 2:
                                    break;
                                case 1:
                                    tl = 3;
                                    break;
                            }

                            GbApuWriteData(parentModule.UnitNumber, reg + 2, (byte)(tl << 5));
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            uint reg = (uint)((Slot + 3) * 5);
                            byte tl = (byte)Math.Round(timbre.EnvInitialVolume * CalcCurrentVolume());

                            byte edir = (byte)(timbre.EnvDirection << 3);
                            byte elen = timbre.EnvLength;

                            GbApuWriteData(parentModule.UnitNumber, reg + 2, (byte)((tl << 4) | edir | elen));
                            break;
                        }
                }

            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public override void UpdatePitch()
            {
                UpdatePitch(0x00);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public void UpdatePitch(byte keyOn)
            {
                if (IsSoundOff)
                    return;

                double freq = CalcCurrentFrequency();

                //Freq
                switch (timbre.SoundType)
                {
                    case SoundType.SPSG:
                    case SoundType.PSG:
                        {
                            uint reg = (uint)(Slot * 5);
                            ushort gfreq = convertPsgFrequency(freq);

                            Program.SoundUpdating();
                            GbApuWriteData(parentModule.UnitNumber, reg + 3, (byte)(gfreq & 0xff));
                            GbApuWriteData(parentModule.UnitNumber, reg + 4, (byte)(keyOn | (timbre.EnableLength << 6) | ((gfreq >> 8) & 0x07)));
                            Program.SoundUpdated();

                            break;
                        }
                    case SoundType.WAV:
                        {
                            uint reg = (uint)((Slot + 2) * 5);
                            ushort gfreq = convertWavFrequency(freq);

                            Program.SoundUpdating();
                            GbApuWriteData(parentModule.UnitNumber, reg + 3, (byte)(gfreq & 0xff));
                            GbApuWriteData(parentModule.UnitNumber, reg + 4, (byte)(keyOn | (timbre.EnableLength << 6) | ((gfreq >> 8) & 0x07)));
                            Program.SoundUpdated();

                            break;
                        }
                    case SoundType.NOISE:
                        {
                            var pitch = (int)ParentModule.Pitchs[NoteOnEvent.Channel] - 8192;
                            uint reg = (uint)((Slot + 3) * 5);

                            var nfreq = (int)timbre.NoiseShiftClockFrequency + ((15 * -pitch) / 8192);
                            if (nfreq > 15)
                                nfreq = 15;
                            else if (nfreq < 0)
                                nfreq = 0;

                            Program.SoundUpdating();
                            GbApuWriteData(parentModule.UnitNumber, reg + 3, (byte)(nfreq << 4 | timbre.NoiseCounter << 3 | timbre.NoiseDivRatio));
                            GbApuWriteData(parentModule.UnitNumber, reg + 4, (byte)(keyOn | (timbre.EnableLength << 6)));
                            Program.SoundUpdated();

                            break;
                        }
                }

            }

            /// <summary>
            /// 
            /// </summary>
            public override void UpdatePanpot()
            {
                //Pan
                byte? cpan = GbApuReadData(parentModule.UnitNumber, 0x15);
                if (cpan.HasValue)
                {
                    var rslot = Slot;
                    switch (lastSoundType)
                    {
                        case SoundType.SPSG:
                        case SoundType.PSG:
                            break;
                        case SoundType.WAV:
                            rslot += 2;
                            break;
                        case SoundType.NOISE:
                            rslot += 3;
                            break;
                    }

                    byte mask = (byte)(0x11 << rslot);
                    byte ccpan = (byte)(cpan.Value & (byte)~mask);

                    byte pan = parentModule.Panpots[NoteOnEvent.Channel];
                    if (pan < 32)
                        pan = 0x10;
                    else if (pan > 96)
                        pan = 0x01;
                    else
                        pan = 0x11;
                    pan = (byte)(pan << rslot);
                    ccpan |= pan;

                    GbApuWriteData(parentModule.UnitNumber, 0x15, ccpan);
                }
            }


            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                switch (lastSoundType)
                {
                    case SoundType.SPSG:
                    case SoundType.PSG:
                        {
                            uint reg = (uint)(Slot * 5);

                            GbApuWriteData(parentModule.UnitNumber, reg + 2, 0x00);

                            break;
                        }
                    case SoundType.WAV:
                        {
                            uint reg = (uint)((Slot + 2) * 5);

                            GbApuWriteData(parentModule.UnitNumber, reg + 2, 0x00);

                            break;
                        }
                    case SoundType.NOISE:
                        {
                            uint reg = (uint)((Slot + 3) * 5);

                            GbApuWriteData(parentModule.UnitNumber, reg + 2, 0x00);

                            break;
                        }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            /// <returns></returns>
            private ushort convertPsgFrequency(double freq)
            {
                /*
                 * FF14 - NR14 - Channel 1 Frequency hi (R/W)
                 * Bit 7   - Initial (1=Restart Sound)     (Write Only)
                 * Bit 6   - Counter/consecutive selection (Read/Write)
                 * (1=Stop output when length in NR11 expires)
                 * Bit 2-0 - Frequency's higher 3 bits (x) (Write Only)
                 * Frequency = 131072/(2048-x) Hz
                 */
                double f = (131072d / freq);
                if (f > 2048d)
                    f = 2048d;
                return (ushort)Math.Round(2048d - f);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            /// <returns></returns>
            private ushort convertWavFrequency(double freq)
            {
                double f = (65536d / freq);
                if (f > 2048d)
                    f = 2048d;
                return (ushort)Math.Round(2048d - (65536d / freq));
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<GBAPUTimbre>))]
        [DataContract]
        public class GBAPUTimbre : TimbreBase, IWsgEditorByteCapable
        {
            [DataMember]
            [Category("Sound")]
            [Description("Sound Type (SPSG:Sweep PSG:PSG(2ch) WAV:WAV NOISE:NOISE)")]
            public SoundType SoundType
            {
                get;
                set;
            }

            private byte f_SweepTime;

            [DataMember]
            [Category("Sound(Sweep)")]
            [Description("Sweep Time (0:OFF 1-7:N/128Hz)")]
            public byte SweepTime
            {
                get
                {
                    return f_SweepTime;
                }
                set
                {
                    f_SweepTime = (byte)(value & 7);
                }
            }

            private byte f_SweepDir;

            [DataMember]
            [Category("Sound(Sweep)")]
            [Description("Sweep Increase/Decrease (0: Addition 1: Subtraction)")]
            public byte SweepDir
            {
                get
                {
                    return f_SweepDir;
                }
                set
                {
                    f_SweepDir = (byte)(value & 1);
                }
            }

            private byte f_SweepNumber;

            [DataMember]
            [Category("Sound(Sweep)")]
            [Description("Number of sweep shift (0-7)")]
            public byte SweepNumber
            {
                get
                {
                    return f_SweepNumber;
                }
                set
                {
                    f_SweepNumber = (byte)(value & 7);
                }
            }

            private byte f_Duty;

            [DataMember]
            [Category("Sound")]
            [Description("Duty (0:12.5% 1:25% 2:50% 3:75%)")]
            public byte Duty
            {
                get
                {
                    return f_Duty;
                }
                set
                {
                    f_Duty = (byte)(value & 3);
                }
            }

            private byte f_SoundLength;

            [DataMember]
            [Category("Sound")]
            [Description("Sound Length (0-64,0-255)[(64-N)*(1/256) seconds]")]
            public byte SoundLength
            {
                get
                {
                    if (SoundType == SoundType.WAV)
                        return f_SoundLength;
                    else
                        return (byte)(f_SoundLength & 63);
                }
                set
                {
                    if (SoundType == SoundType.WAV)
                        f_SoundLength = (byte)(value & 255);
                    else
                        f_SoundLength = (byte)(value & 63);
                }
            }


            private byte f_EnableLength;

            [DataMember]
            [Category("Sound")]
            [Description("Whether Sound Length is enable or not (0:Disable 1:Enable)")]
            public byte EnableLength
            {
                get
                {
                    return f_EnableLength;
                }
                set
                {
                    f_EnableLength = (byte)(value & 1);
                }
            }


            private byte f_EnvInitialVolume;

            [DataMember]
            [Category("Sound")]
            [Description("Initial Volume of envelope (0-15(0:No Sound))")]
            public byte EnvInitialVolume
            {
                get
                {
                    return f_EnvInitialVolume;
                }
                set
                {
                    f_EnvInitialVolume = (byte)(value & 0xf);
                }
            }

            private byte f_EnvDirection;

            [DataMember]
            [Category("Sound")]
            [Description("Envelope Direction (0=Decrease, 1=Increase)")]
            public byte EnvDirection
            {
                get
                {
                    return f_EnvDirection;
                }
                set
                {
                    f_EnvDirection = (byte)(value & 1);
                }
            }

            private byte f_EnvLength;

            [DataMember]
            [Category("Sound")]
            [Description("Envelope Length (0-7 (0:Stop)[1step = N*(1/64) sec]")]
            public byte EnvLength
            {
                get
                {
                    return f_EnvLength;
                }
                set
                {
                    f_EnvLength = (byte)(value & 7);
                }
            }

            private byte f_NoiseShiftClockFrequency;

            [DataMember]
            [Category("Sound(Noise)")]
            [Description("Shift Clock Frequency (0-15) This parameter is affected by Pitch Bend MIDI message")]
            public byte NoiseShiftClockFrequency
            {
                get
                {
                    return f_NoiseShiftClockFrequency;
                }
                set
                {
                    f_NoiseShiftClockFrequency = (byte)(value & 15);
                }
            }

            private byte f_NoiseCounter;

            [DataMember]
            [Category("Sound(Noise)")]
            [Description("Counter Step/Width (0=15 bits, 1=7 bits)")]
            public byte NoiseCounter
            {
                get
                {
                    return f_NoiseCounter;
                }
                set
                {
                    f_NoiseCounter = (byte)(value & 1);
                }
            }

            private byte f_NoiseDivRatio;

            [DataMember]
            [Category("Sound(Noise)")]
            [Description("Dividing Ratio of Frequencies(0-7)")]
            public byte NoiseDivRatio
            {
                get
                {
                    return f_NoiseDivRatio;
                }
                set
                {
                    f_NoiseDivRatio = (byte)(value & 7);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            [Browsable(false)]
            [IgnoreDataMember]
            [JsonIgnore]
            public byte WsgBitWide
            {
                get
                {
                    return 4;
                }
            }

            private byte[] f_wavedata = new byte[32];

            [TypeConverter(typeof(ArrayConverter))]
            [Editor(typeof(WsgITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DataMember]
            [Category("Sound")]
            [Description("Wave Table (32 samples, 0-15 levels)")]
            public byte[] WsgData
            {
                get
                {
                    return f_wavedata;
                }
                set
                {
                    f_wavedata = value;
                }
            }


            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            typeof(UITypeEditor)), Localizable(false)]
            [Category("Sound")]
            [Description("Wave Table (32 samples, 0-15 levels)")]
            [IgnoreDataMember]
            [JsonIgnore]
            public string WsgDataSerializeData
            {
                get
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < WsgData.Length; i++)
                    {
                        if (sb.Length != 0)
                            sb.Append(' ');
                        sb.Append(WsgData[i].ToString((IFormatProvider)null));
                    }
                    return sb.ToString();
                }
                set
                {
                    string[] vals = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    List<byte> vs = new List<byte>();
                    foreach (var val in vals)
                    {
                        byte v = 0;
                        if (byte.TryParse(val, out v))
                            vs.Add(v);
                    }
                    for (int i = 0; i < Math.Min(WsgData.Length, vs.Count); i++)
                        WsgData[i] = vs[i] > 15 ? (byte)15 : vs[i];
                }
            }


            public GBAPUTimbre()
            {
                Duty = 2;
                SoundLength = 0;
                EnableLength = 0;
                EnvInitialVolume = 15;
                EnvDirection = 1;
                EnvLength = 7;
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="serializeData"></param>
            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<GBAPUTimbre>(serializeData);
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
        public enum SoundType
        {
            SPSG,
            PSG,
            WAV,
            NOISE,
        }

    }
}
