// copyright-holders:K.Ito
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
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
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Mame;
using zanac.MAmidiMEmo.Midi;

//http://fpga.blog.shinobi.jp/fpga/おんげん！
//https://www.walkofmind.com/programming/pie/wsg3.htm

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class NAMCO_CUS30 : InstrumentBase
    {

        public override string Name => "NAMCO_CUS30";

        public override InstrumentType InstrumentType => InstrumentType.NAMCO_CUS30;

        [Browsable(false)]
        public override string ImageKey => "NAMCO_CUS30";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "namco_cus30_";

        /// <summary>
        /// 
        /// </summary>
        [Category("MIDI")]
        [Description("MIDI Device ID")]
        public override uint DeviceID
        {
            get
            {
                return 4;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Timbres (0-127)")]
        [TypeConverter(typeof(CustomCollectionConverter))]
        public NAMCO_CUS30Timbre[] Timbres
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
            var obj = JsonConvert.DeserializeObject<NAMCO_CUS30>(serializeData);
            this.InjectFrom(new LoopInjection(new[] { "SerializeData" }), obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_namco_cus30_w(uint unitNumber, uint address, byte data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_namco_cus30_w namco_cus30_w
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static void NamcoCus30WriteData(uint unitNumber, uint address, byte data)
        {
            try
            {
                Program.SoundUpdating();
                namco_cus30_w(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte delegate_namco_cus30_r(uint unitNumber, uint address);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_namco_cus30_r namco_cus30_r
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static byte NamcoCus30ReadData(uint unitNumber, uint address)
        {
            try
            {
                Program.SoundUpdating();
                return namco_cus30_r(unitNumber, address);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static NAMCO_CUS30()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("namco_cus30_w");
            if (funcPtr != IntPtr.Zero)
                namco_cus30_w = (delegate_namco_cus30_w)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_namco_cus30_w));
            funcPtr = MameIF.GetProcAddress("namco_cus30_r");
            if (funcPtr != IntPtr.Zero)
                namco_cus30_r = (delegate_namco_cus30_r)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_namco_cus30_r));
        }

        private NAMCO_CUS30SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public NAMCO_CUS30(uint unitNumber) : base(unitNumber)
        {
            Timbres = new NAMCO_CUS30Timbre[128];
            for (int i = 0; i < 128; i++)
                Timbres[i] = new NAMCO_CUS30Timbre();
            setPresetInstruments();

            this.soundManager = new NAMCO_CUS30SoundManager(this);
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
            Timbres[0].SoundType = SoundType.WSG;
            Timbres[0].WaveData = new byte[] { 8, 9, 11, 12, 13, 14, 15, 15, 15, 15, 14, 14, 13, 11, 10, 9, 7, 6, 4, 3, 2, 1, 0, 0, 0, 0, 1, 1, 2, 4, 5, 6 };

            Timbres[1].SoundType = SoundType.WSG;
            Timbres[1].WaveData = new byte[]
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
        private class NAMCO_CUS30SoundManager : SoundManagerBase
        {
            private List<NAMCO_CUS30Sound> wsgOnSounds = new List<NAMCO_CUS30Sound>();

            private NAMCO_CUS30 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public NAMCO_CUS30SoundManager(NAMCO_CUS30 parent)
            {
                this.parentModule = parent;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="midiEvent"></param>
            public override void PitchBend(PitchBendEvent midiEvent)
            {
                foreach (NAMCO_CUS30Sound t in AllOnSounds)
                {
                    if (t.NoteOnEvent.Channel == midiEvent.Channel)
                    {
                        t.UpdateWsgPitch();
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
                        foreach (NAMCO_CUS30Sound t in AllOnSounds)
                        {
                            if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            {
                                t.UpdateWsgVolume();
                            }
                        }
                        break;
                    case 10:    //Panpot
                        foreach (NAMCO_CUS30Sound t in AllOnSounds)
                        {
                            if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            {
                                t.UpdateWsgVolume();
                            }
                        }
                        break;
                    case 11:    //Expression
                        foreach (NAMCO_CUS30Sound t in AllOnSounds)
                        {
                            if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            {
                                t.UpdateWsgVolume();
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

                NAMCO_CUS30Sound snd = new NAMCO_CUS30Sound(parentModule, note, emptySlot);
                AllOnSounds.Add(snd);
                wsgOnSounds.Add(snd);
                FormMain.OutputDebugLog("KeyOn WSG ch" + emptySlot + " " + note.ToString());
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
                emptySlot = SearchEmptySlot(wsgOnSounds.ToList<SoundBase>(), 8);
                return emptySlot;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            public override void NoteOff(NoteOffEvent note)
            {
                NAMCO_CUS30Sound removed = SearchAndRemoveOnSound(note, AllOnSounds) as NAMCO_CUS30Sound;

                if (removed != null)
                {
                    for (int i = 0; i < wsgOnSounds.Count; i++)
                    {
                        if (wsgOnSounds[i] == removed)
                        {
                            FormMain.OutputDebugLog("KeyOff WSG ch" + removed.Slot + " " + note.ToString());
                            wsgOnSounds.RemoveAt(i);
                            return;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class NAMCO_CUS30Sound : SoundBase
        {

            private NAMCO_CUS30 parentModule;

            private SevenBitNumber programNumber;

            public NAMCO_CUS30Timbre Timbre;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public NAMCO_CUS30Sound(NAMCO_CUS30 parentModule, NoteOnEvent noteOnEvent, int slot) : base(noteOnEvent, slot)
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
                SetWsgTimbre();
                //Freq
                UpdateWsgPitch();
                //Volume
                UpdateWsgVolume();
            }

            /// <summary>
            /// 
            /// </summary>
            public void SetWsgTimbre()
            {
                var pn = parentModule.ProgramNumbers[NoteOnEvent.Channel];
                var timbre = parentModule.Timbres[pn];

                byte[] wdata = timbre.WaveData;
                for (int i = 0; i < 16; i++)
                    NamcoCus30WriteData(parentModule.UnitNumber, (uint)((Slot * 16) + i), (byte)(((wdata[i * 2 + 1] & 0xf) << 4) | (wdata[i * 2] & 0xf)));
            }

            /// <summary>
            /// 
            /// </summary>
            public void UpdateWsgVolume()
            {
                var pn = parentModule.ProgramNumbers[NoteOnEvent.Channel];
                var timbre = parentModule.Timbres[pn];

                var exp = parentModule.Expressions[NoteOnEvent.Channel] / 127d;
                var vol = parentModule.Volumes[NoteOnEvent.Channel] / 127d;
                var vel = NoteOnEvent.Velocity / 127d;

                byte fv_l = (byte)((int)Math.Round(15 * vol * vel * exp) & 0xf);
                byte fv_r = fv_l;

                var pan = parentModule.Panpots[NoteOnEvent.Channel] / 127d;

                if (pan < 0.5)   //left
                    fv_r = (byte)((byte)(fv_r * pan / 63) & 0xf);
                else if (pan > 64)  //right
                    fv_l = (byte)((byte)(fv_l * (127 - pan) / 63) & 0xf);

                fv_r |= (byte)(NamcoCus30ReadData(parentModule.UnitNumber, 0x100 + (uint)Slot * 8 + 0x04) & 0x80);

                byte noise = NamcoCus30ReadData(parentModule.UnitNumber, 0x100 + (uint)(((Slot - 1) * 8) & 0x3f) + 0x04);
                noise &= 0x7f;
                if (timbre.SoundType == SoundType.NOISE)
                    noise |= 0x80;

                NamcoCus30WriteData(parentModule.UnitNumber, 0x100 + (uint)Slot * 8 + 0x00, fv_l);
                NamcoCus30WriteData(parentModule.UnitNumber, 0x100 + (uint)Slot * 8 + 0x04, fv_r);
                NamcoCus30WriteData(parentModule.UnitNumber, 0x100 + (uint)(((Slot - 1) * 8) & 0x3f) + 0x04, noise);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public void UpdateWsgPitch()
            {
                var pn = parentModule.ProgramNumbers[NoteOnEvent.Channel];
                var timbre = parentModule.Timbres[pn];

                var pitch = (int)parentModule.Pitchs[NoteOnEvent.Channel] - 8192;
                var range = (int)parentModule.PitchBendRanges[NoteOnEvent.Channel];

                int noteNum = NoteOnEvent.NoteNumber;
                double freq = 440.0 * Math.Pow(2.0, (NoteOnEvent.NoteNumber - 69.0) / 12.0);

                if (pitch > 0)
                {
                    var nfreq = 440.0 * Math.Pow(2.0, (NoteOnEvent.NoteNumber + range - 69.0) / 12.0);
                    var dfreq = (nfreq - freq) * ((double)pitch / (double)8192);
                    freq = (ushort)Math.Round(freq + dfreq);
                }
                else if (pitch < 0)
                {
                    var nfreq = 440.0 * Math.Pow(2.0, (NoteOnEvent.NoteNumber - range - 69.0) / 12.0);
                    var dfreq = (nfreq - freq) * ((double)-pitch / (double)8192);
                    freq = (ushort)Math.Round(freq + dfreq);
                }
                //max 1048575(20bit)
                //midi 8.175798915643707 ～ 12543.853951415975Hz
                // A4 440 -> 440 * 500 = 440000
                // A6 1760 -> 1760 * 500 = 880000

                //adjust
                double xfreq = 29.00266666666667 * Math.Pow(2.0, (NoteOnEvent.NoteNumber - 69.0) / 12.0);

                uint n = ((uint)Math.Round((freq - xfreq) * 93.75)) & (uint)0xfffff;

                NamcoCus30WriteData(parentModule.UnitNumber, 0x100 + (uint)Slot * 8 + 0x01, (byte)((byte)((Slot & 0xf) << 4) | ((n >> 16) & 0xf)));
                NamcoCus30WriteData(parentModule.UnitNumber, 0x100 + (uint)Slot * 8 + 0x02, (byte)((n >> 8) & 0xff));
                NamcoCus30WriteData(parentModule.UnitNumber, 0x100 + (uint)Slot * 8 + 0x03, (byte)(n & 0xff));
            }

            /// <summary>
            /// 
            /// </summary>
            public override void Off()
            {
                NamcoCus30WriteData(parentModule.UnitNumber, 0x100 + (uint)Slot * 8 + 0x00, 0x0);
                byte org = (byte)(NamcoCus30ReadData(parentModule.UnitNumber, 0x100 + (uint)Slot * 8 + 0x04) & 0x80);
                NamcoCus30WriteData(parentModule.UnitNumber, 0x100 + (uint)Slot * 8 + 0x04, org);
            }

        }

        /* namcos1 register map
    0x00        ch 0    left volume 0-15
    0x01        ch 0    waveform select((data >> 4) & 15) & frequency ((data & 15) << 16)
    0x02-0x03   ch 0    frequency (0x02 << 8 | 0x03)
    0x04        ch 0    right volume AND (data & 0x0f;)
    0x04        ch 1    noise sw ((data & 0x80) >> 7)

    0x08        ch 1    left volume
    0x09        ch 1    waveform select & frequency
    0x0a-0x0b   ch 1    frequency
    0x0c        ch 1    right volume AND
    0x0c        ch 2    noise sw

    .
    .
    .

    0x38        ch 7    left volume
    0x39        ch 7    waveform select & frequency
    0x3a-0x3b   ch 7    frequency
    0x3c        ch 7    right volume AND
    0x3c        ch 0    noise sw
*/

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<NAMCO_CUS30Timbre>))]
        [DataContract]
        public class NAMCO_CUS30Timbre : TimbreBase
        {
            private SoundType f_SoundType;

            /// <summary>
            /// 
            /// </summary>
            [Category("Sound")]
            [Description("Sound Type")]
            public SoundType SoundType
            {
                get
                {
                    return f_SoundType;
                }
                set
                {
                    f_SoundType = value;
                }
            }

            private byte[] f_wavedata = new byte[32];

            [TypeConverter(typeof(ArrayConverter))]
            [Editor(typeof(DummyEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DataMember]
            [Category("Sound")]
            [Description("Wave Table (32 samples, 0-15 levels)")]
            public byte[] WaveData
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
            public string WaveDataSerializeData
            {
                get
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < WaveData.Length; i++)
                    {
                        if (sb.Length != 0)
                            sb.Append(' ');
                        sb.Append(WaveData[i].ToString((IFormatProvider)null));
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
                    for (int i = 0; i < Math.Min(WaveData.Length, vs.Count); i++)
                        WaveData[i] = vs[i] > 15 ? (byte)15 : vs[i];
                }
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<NAMCO_CUS30Timbre>(serializeData);
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
            WSG,
            NOISE,
        }

    }
}