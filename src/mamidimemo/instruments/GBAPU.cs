using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class GBAPU : InstrumentBase
    {
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
        private static delegate_gb_apu_write GbApi_write
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_gb_apu_read GbApi_read
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
        static GBAPU()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("gb_apu_write");
            if (funcPtr != IntPtr.Zero)
            {
                GbApi_write = (delegate_gb_apu_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_gb_apu_write));
                GbApi_read = (delegate_gb_apu_read)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_gb_apu_read));
            }
        }

        private GBSoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public GBAPU(uint unitNumber)
        {
            UnitNumber = unitNumber;
            this.soundManager = new GBSoundManager(this);
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
        private class GBSoundManager : SoundManagerBase
        {
            private List<GbSound> allOnSounds = new List<GbSound>();

            private List<GbSound> wavOnSounds = new List<GbSound>();

            private List<GbSound> psgOnSounds = new List<GbSound>();

            private GBAPU parentModule;

            public SevenBitNumber[] PitchBendRanges;

            public SevenBitNumber[] ProgramNumbers;

            public SevenBitNumber[] Volumes;

            public SevenBitNumber[] Expressions;

            public SevenBitNumber[] Panpots;

            public GBAPUTimbre[] Timbres;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public GBSoundManager(GBAPU parent)
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

                Timbres = new GBAPUTimbre[128];

                //Sound On
                GbApi_write?.Invoke(parentModule.UnitNumber, 0x16, 0x80);
                GbApi_write?.Invoke(parentModule.UnitNumber, 0x14, 0x77);
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

                GbSound snd = new GbSound(parentModule, note, emptySlot);
                allOnSounds.Add(snd);
                switch (snd.Timbre.SoundType)
                {
                    case SoundType.PSG:
                        psgOnSounds.Add(snd);
                        break;
                    case SoundType.WAV:
                        wavOnSounds.Add(snd);
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
                    case SoundType.PSG:
                        {
                            emptySlot = SearchEmptySlot(psgOnSounds.ToList<SoundBase>(), 2);
                            break;
                        }
                    case SoundType.WAV:
                        {
                            emptySlot = SearchEmptySlot(wavOnSounds.ToList<SoundBase>(), 1);
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
                GbSound removed = SearchAndRemoveOnSound(note, allOnSounds);

                if (removed != null)
                {
                    for (int i = 0; i < psgOnSounds.Count; i++)
                    {
                        if (psgOnSounds[i] == removed)
                        {
                            psgOnSounds.RemoveAt(i);
                            return;
                        }
                    }
                    for (int i = 0; i < wavOnSounds.Count; i++)
                    {
                        if (wavOnSounds[i] == removed)
                        {
                            wavOnSounds.RemoveAt(i);
                            return;
                        }
                    }
                }
            }

        }


        /// <summary>
        /// 
        /// </summary>
        private class GbSound : SoundBase
        {

            private GBAPU parentModule;

            private SevenBitNumber programNumber;

            public GBAPUTimbre Timbre;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public GbSound(GBAPU parentModule, NoteOnEvent noteOnEvent, int slot) : base(noteOnEvent, slot)
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
                    case SoundType.PSG:
                        {
                            uint reg = (uint)(Slot * 5);

                            GbApi_write?.Invoke(parentModule.UnitNumber, reg, 0x00);
                            GbApi_write?.Invoke(parentModule.UnitNumber, reg + 1, 0x00);


                            //SetTimbre(Slot);

                            //Freq
                            ushort freq = convertPsgFrequency(NoteOnEvent);

                            Console.WriteLine(freq);

                            //Volume
                            byte vol = parentModule.soundManager.Volumes[NoteOnEvent.Channel];
                            vol = (byte)((vol >> 3) & 0xf);
                            GbApi_write?.Invoke(parentModule.UnitNumber, reg + 2, (byte)((vol << 4) | 0x00));

                            //Pan
                            byte? cpan = GbApi_read?.Invoke(parentModule.UnitNumber, 0x15);
                            if (cpan.HasValue)
                            {
                                byte mask = (byte)(0x11 << Slot);
                                byte ccpan = (byte)(cpan.Value & (byte)~mask);

                                byte pan = parentModule.soundManager.Panpots[NoteOnEvent.Channel];
                                
                                pan = 0;

                                if (pan < 32)
                                    pan = 0x10;
                                else if (pan > 96)
                                    pan = 0x01;
                                else
                                    pan = 0x11;
                                pan = (byte)(pan << Slot);
                                ccpan |= pan;

                                GbApi_write?.Invoke(parentModule.UnitNumber, 0x15, ccpan);
                            }

                            GbApi_write?.Invoke(parentModule.UnitNumber, reg + 3, (byte)(freq & 0xff));
                            GbApi_write?.Invoke(parentModule.UnitNumber, reg + 4, (byte)(0x80 | ((freq >> 8) & 0x03)));

                            break;
                        }
                    case SoundType.WAV:
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

            }

            /// <summary>
            /// 
            /// </summary>
            public override void Off()
            {
                switch (Timbre.SoundType)
                {
                    case SoundType.PSG:
                        {
                            uint reg = (uint)(Slot * 5);

                            GbApi_write?.Invoke(parentModule.UnitNumber, reg + 2, 0x00);

                            break;
                        }
                    case SoundType.WAV:
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
            private ushort convertPsgFrequency(NoteOnEvent note)
            {
                var realfreq = 440.0 * Math.Pow(2.0, (note.NoteNumber - 69.0) / 12.0);

                var gbfreq = 2048d - (131072d / realfreq);
                /*
                 * FF14 - NR14 - Channel 1 Frequency hi (R/W)
                 * Bit 7   - Initial (1=Restart Sound)     (Write Only)
                 * Bit 6   - Counter/consecutive selection (Read/Write)
                 * (1=Stop output when length in NR11 expires)
                 * Bit 2-0 - Frequency's higher 3 bits (x) (Write Only)
                 * Frequency = 131072/(2048-x) Hz
                 */

                return (ushort)Math.Round(gbfreq);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private struct GBAPUTimbre
        {
            public SoundType SoundType;

        }

        /// <summary>
        /// 
        /// </summary>
        private enum SoundType
        {
            PSG,
            WAV,
            NOISE,
        }

    }
}
