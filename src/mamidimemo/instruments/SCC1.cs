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

//http://bifi.msxnet.org/msxnet/tech/scc.html

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class SCC1 : InstrumentBase
    {

        public override string Name => "SCC1";

        public override string Group => "WSG";

        public override InstrumentType InstrumentType => InstrumentType.SCC1;

        [Browsable(false)]
        public override string ImageKey => "SCC1";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "scc1_";

        /// <summary>
        /// 
        /// </summary>
        [Category("MIDI")]
        [Description("MIDI Device ID")]
        public override uint DeviceID
        {
            get
            {
                return 7;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Timbres (0-127)")]
        [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(CustomCollectionConverter))]
        public SCC1Timbre[] Timbres
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
            var obj = JsonConvert.DeserializeObject<SCC1>(serializeData);
            this.InjectFrom(new LoopInjection(new[] { "SerializeData" }), obj);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_SCC1_w(uint unitNumber, uint address, byte data);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte delegate_SCC1_r(uint unitNumber, uint address);

        private static delegate_SCC1_w SCC1_waveform_w;

        private static delegate_SCC1_w SCC1_volume_w;

        private static delegate_SCC1_w SCC1_frequency_w;

        private static delegate_SCC1_w SCC1_keyonoff_w;

        private static delegate_SCC1_r SCC1_keyonoff_r;

        /// <summary>
        /// 
        /// </summary>
        private static void Scc1VolumeWriteData(uint unitNumber, uint address, byte data)
        {
            try
            {
                Program.SoundUpdating();
                SCC1_volume_w(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static void Scc1FrequencyWriteData(uint unitNumber, uint address, byte data)
        {
            try
            {
                Program.SoundUpdating();
                SCC1_frequency_w(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static void Scc1KeyOnOffWriteData(uint unitNumber, byte data)
        {
            try
            {
                Program.SoundUpdating();
                SCC1_keyonoff_w(unitNumber, 0, data);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static byte Scc1KeyOnOffReadData(uint unitNumber)
        {
            try
            {
                Program.SoundUpdating();
                return SCC1_keyonoff_r(unitNumber, 0);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static void Scc1WriteWaveData(uint unitNumber, uint address, sbyte[] data)
        {
            try
            {
                Program.SoundUpdating();
                for (var i = 0; i < data.Length; i++)
                    SCC1_waveform_w(unitNumber, (uint)(address + i), (byte)data[i]);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static SCC1()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("SCC1_waveform_w");
            if (funcPtr != IntPtr.Zero)
                SCC1_waveform_w = (delegate_SCC1_w)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_SCC1_w));
            funcPtr = MameIF.GetProcAddress("SCC1_volume_w");
            if (funcPtr != IntPtr.Zero)
                SCC1_volume_w = (delegate_SCC1_w)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_SCC1_w));
            funcPtr = MameIF.GetProcAddress("SCC1_frequency_w");
            if (funcPtr != IntPtr.Zero)
                SCC1_frequency_w = (delegate_SCC1_w)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_SCC1_w));
            funcPtr = MameIF.GetProcAddress("SCC1_keyonoff_w");
            if (funcPtr != IntPtr.Zero)
                SCC1_keyonoff_w = (delegate_SCC1_w)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_SCC1_w));
            funcPtr = MameIF.GetProcAddress("SCC1_keyonoff_r");
            if (funcPtr != IntPtr.Zero)
                SCC1_keyonoff_r = (delegate_SCC1_r)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_SCC1_r));
        }

        private SCC1SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public SCC1(uint unitNumber) : base(unitNumber)
        {
            Timbres = new SCC1Timbre[128];
            for (int i = 0; i < 128; i++)
                Timbres[i] = new SCC1Timbre();
            setPresetInstruments();

            this.soundManager = new SCC1SoundManager(this);
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
            Timbres[0].WsgData = new sbyte[] { 8 * 16 - 127, 9 * 16 - 127, 11 * 16 - 127, 12 * 16 - 127, 13 * 16 - 127, 14 * 16 - 127, 15 * 16 - 127, 15 * 16 - 127, 15 * 16 - 127, 15 * 16 - 127, 14 * 16 - 127, 14 * 16 - 127, 13 * 16 - 127, 11 * 16 - 127, 10 * 16 - 127, 9 * 16 - 127, 7 * 16 - 127, 6 * 16 - 127, 4 * 16 - 127, 3 * 16 - 127, 2 * 16 - 127, 1 * 16 - 127, 0 * 16 - 127, 0 * 16 - 127, 0 * 16 - 127, 0 * 16 - 127, 1 * 16 - 127, 1 * 16 - 127, 2 * 16 - 127, 4 * 16 - 127, 5 * 16 - 127, 6 };
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
        private class SCC1SoundManager : SoundManagerBase
        {
            private List<SCC1Sound> sccOnSounds = new List<SCC1Sound>();

            private SCC1 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public SCC1SoundManager(SCC1 parent)
            {
                this.parentModule = parent;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="midiEvent"></param>
            public override void PitchBend(PitchBendEvent midiEvent)
            {
                foreach (SCC1Sound t in AllOnSounds)
                {
                    if (t.NoteOnEvent.Channel == midiEvent.Channel)
                    {
                        t.UpdatePitch();
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
                        foreach (SCC1Sound t in AllOnSounds)
                        {
                            if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            {
                                t.UpdateVolume();
                            }
                        }
                        break;
                    case 10:    //Panpot
                        break;
                    case 11:    //Expression
                        foreach (SCC1Sound t in AllOnSounds)
                        {
                            if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            {
                                t.UpdateVolume();
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

                SCC1Sound snd = new SCC1Sound(parentModule, note, emptySlot);
                AllOnSounds.Add(snd);
                sccOnSounds.Add(snd);
                FormMain.OutputDebugLog("KeyOn SCC ch" + emptySlot + " " + note.ToString());
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
                emptySlot = SearchEmptySlot(sccOnSounds.ToList<SoundBase>(), 5);
                return emptySlot;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            public override void NoteOff(NoteOffEvent note)
            {
                SCC1Sound removed = SearchAndRemoveOnSound(note, AllOnSounds) as SCC1Sound;

                if (removed != null)
                {
                    for (int i = 0; i < sccOnSounds.Count; i++)
                    {
                        if (sccOnSounds[i] == removed)
                        {
                            FormMain.OutputDebugLog("KeyOff SCC ch" + removed.Slot + " " + note.ToString());
                            sccOnSounds.RemoveAt(i);
                            return;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class SCC1Sound : SoundBase
        {

            private SCC1 parentModule;

            private SevenBitNumber programNumber;

            public SCC1Timbre Timbre;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public SCC1Sound(SCC1 parentModule, NoteOnEvent noteOnEvent, int slot) : base(noteOnEvent, slot)
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
                UpdatePitch();
                //Volume
                UpdateVolume();

                byte data = Scc1KeyOnOffReadData(parentModule.UnitNumber);
                data |= (byte)(1 << Slot);
                Scc1KeyOnOffWriteData(parentModule.UnitNumber, data);
            }

            /// <summary>
            /// 
            /// </summary>
            public void SetWsgTimbre()
            {
                Scc1WriteWaveData(parentModule.UnitNumber, (uint)(Slot << 5), Timbre.WsgData);
            }

            /// <summary>
            /// 
            /// </summary>
            public void UpdateVolume()
            {
                var exp = parentModule.Expressions[NoteOnEvent.Channel] / 127d;
                var vol = parentModule.Volumes[NoteOnEvent.Channel] / 127d;
                var vel = NoteOnEvent.Velocity / 127d;

                byte fv = (byte)((int)Math.Round(15 * vol * vel * exp) & 0xf);

                Scc1VolumeWriteData(parentModule.UnitNumber, (uint)Slot, fv);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public void UpdatePitch()
            {
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

                /*
                 *                fclock
                 *     ftone = -------------
                 *             32 * (TP + 1)
                 *             
                 *     fclock is the clock frequency of the computer. 3,579,545 Hz
                 */
                // TP = (fclock / (32 * ftone))-1

                uint n = (uint)Math.Round((3579545 / (32 * freq)) - 1) & 0xfff;

                Scc1FrequencyWriteData(parentModule.UnitNumber, (uint)((Slot << 1)) + 0, (byte)(n & 0xff));
                Scc1FrequencyWriteData(parentModule.UnitNumber, (uint)((Slot << 1)) + 1, (byte)((n >> 8) & 0xf));
            }

            /// <summary>
            /// 
            /// </summary>
            public override void Off()
            {
                byte data = Scc1KeyOnOffReadData(parentModule.UnitNumber);
                data &= (byte)~(1 << Slot);
                Scc1KeyOnOffWriteData(parentModule.UnitNumber, data);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SCC1Timbre>))]
        [DataContract]
        public class SCC1Timbre : TimbreBase , IWsgEditorSbyteCapable
        {
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
                    return 8;
                }
            }

            private sbyte[] f_wavedata = new sbyte[32];

            [TypeConverter(typeof(ArrayConverter))]
            [Editor(typeof(WsgITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DataMember]
            [Category("Sound")]
            [Description("Wave Table (32 samples, 8 bit signed data)")]
            public sbyte[] WsgData
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
            [Description("Wave Table (32 samples, 8 bit signed data)")]
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
                    var vs = new List<sbyte>();
                    foreach (var val in vals)
                    {
                        sbyte v = 0;
                        if (sbyte.TryParse(val, out v))
                            vs.Add(v);
                    }
                    for (int i = 0; i < Math.Min(WsgData.Length, vs.Count); i++)
                        WsgData[i] = vs[i];
                }
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<SCC1Timbre>(serializeData);
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

    }
}