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

//http://www.citylan.it/wiki/images/3/3e/5232.pdf
//http://sr4.sakura.ne.jp/acsound/taito/taito5232.html

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class MSM5232 : InstrumentBase, IStandardValues
    {

        public override string Name => "MSM5232+TA7630";

        public override string Group => "PSG";

        public override InstrumentType InstrumentType => InstrumentType.MSM5232;

        [Browsable(false)]
        public override string ImageKey => "MSM5232";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "msm5232_";

        /// <summary>
        /// 
        /// </summary>
        [Category("MIDI")]
        [Description("MIDI Device ID")]
        public override uint DeviceID
        {
            get
            {
                return 10;
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
        public MSM5232Timbre[] Timbres
        {
            get;
            private set;
        }

        private double f_Capacitor;

        /// <summary>
        /// 
        /// </summary>
        [Category("Chip")]
        [Description("Set Capacitor capacity [uF]" +
            "The FairyLand Story: 1.0e-6\rn" +
            "Lady Frog: 0.65e-6\r\n" +
            "Equites: 0.47e-6\r\n" +
            "Buggy Challenge: 0.39e-6")]
        [TypeConverter(typeof(StandardValuesDoubleConverter))]
        public double Capacitor
        {
            get => f_Capacitor;
            set
            {
                if (f_Capacitor != value)
                {
                    f_Capacitor = value;
                    MSM5232SetCapasitors(UnitNumber, value, value, value, value, value, value, value, value);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public ICollection GetStandardValues(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(Capacitor):
                    {
                        var l = new List<double>();
                        l.Add(1.0e-6);
                        l.Add(0.65e-6);
                        l.Add(0.47e-6);
                        l.Add(0.39e-6);
                        return l;
                    }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializeData"></param>
        public override void RestoreFrom(string serializeData)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<MSM5232>(serializeData);
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
        private delegate void delegate_MSM5232_write(uint unitNumber, uint address, byte data);

        /// <summary>
        /// 
        /// </summary>
        private static delegate_MSM5232_write MSM5232_write;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitNumber"></param>
        /// <param name="ch"></param>
        /// <param name="data"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_MSM5232_set_volume(uint unitNumber, int ch, byte data);

        /// <summary>
        /// 
        /// </summary>
        private static delegate_MSM5232_set_volume MSM5232_set_volume;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitNumber"></param>
        /// <param name="ch"></param>
        /// <param name="data"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_MSM5232_set_capacitors(uint unitNumber, double cap1, double cap2, double cap3, double cap4, double cap5, double cap6, double cap7, double cap8);

        /// <summary>
        /// 
        /// </summary>
        private static delegate_MSM5232_set_capacitors MSM5232_set_capacitors;

        /// <summary>
        /// 
        /// </summary>
        private static void MSM5232WriteData(uint unitNumber, uint address, byte data)
        {
            try
            {
                Program.SoundUpdating();
                MSM5232_write(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private static void MSM5232SetVolume(uint unitNumber, int ch, byte data)
        {
            try
            {
                Program.SoundUpdating();
                MSM5232_set_volume(unitNumber, ch, data);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private static void MSM5232SetCapasitors(uint unitNumber, double cap1, double cap2, double cap3, double cap4, double cap5, double cap6, double cap7, double cap8)
        {
            try
            {
                Program.SoundUpdating();
                MSM5232_set_capacitors(unitNumber, cap1, cap2, cap3, cap4, cap5, cap6, cap7, cap8);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static void MSM5232SetCapacitors(uint unitNumber, int ch, byte data)
        {
            try
            {
                Program.SoundUpdating();
                MSM5232_set_volume(unitNumber, ch, data);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static MSM5232()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("msm5232_write");
            if (funcPtr != IntPtr.Zero)
            {
                MSM5232_write = (delegate_MSM5232_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_MSM5232_write));
            }
            funcPtr = MameIF.GetProcAddress("msm5232_set_volume");
            if (funcPtr != IntPtr.Zero)
            {
                MSM5232_set_volume = (delegate_MSM5232_set_volume)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_MSM5232_set_volume));
            }
            funcPtr = MameIF.GetProcAddress("msm5232_set_capacitors");
            if (funcPtr != IntPtr.Zero)
            {
                MSM5232_set_capacitors = (delegate_MSM5232_set_capacitors)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_MSM5232_set_capacitors));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            soundManager?.Dispose();
            base.Dispose();
        }

        private MSM5232SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public MSM5232(uint unitNumber) : base(unitNumber)
        {
            Timbres = new MSM5232Timbre[128];
            for (int i = 0; i < 128; i++)
                Timbres[i] = new MSM5232Timbre();
            setPresetInstruments();

            this.soundManager = new MSM5232SoundManager(this);
        }

        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {
            Timbres[0].SoundGroup = SoundGroup.Group1;
        }

        internal override void PrepareSound()
        {
            base.PrepareSound();

            Capacitor = 1.0e-6;
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
        private class MSM5232SoundManager : SoundManagerBase
        {
            private SoundList<MSM5232Sound> chAOnSounds = new SoundList<MSM5232Sound>(4);
            private SoundList<MSM5232Sound> chBOnSounds = new SoundList<MSM5232Sound>(4);

            private MSM5232 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public MSM5232SoundManager(MSM5232 parent)
            {
                this.parentModule = parent;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            public override void KeyOn(NoteOnEvent note)
            {
                int emptySlot = searchEmptySlot(note);
                if (emptySlot < 0)
                    return;

                MSM5232Sound snd = new MSM5232Sound(parentModule, this, note, emptySlot);
                switch (snd.Timbre.SoundGroup)
                {
                    case SoundGroup.Group1:
                        chAOnSounds.Add(snd);
                        FormMain.OutputDebugLog("KeyOn A ch" + emptySlot + " " + note.ToString());
                        break;
                    case SoundGroup.Group2:
                        chBOnSounds.Add(snd);
                        FormMain.OutputDebugLog("KeyOn B ch" + emptySlot + " " + note.ToString());
                        break;
                }
                snd.KeyOn();

                base.KeyOn(note);
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
                switch (timbre.SoundGroup)
                {
                    case SoundGroup.Group1:
                        {
                            emptySlot = SearchEmptySlotAndOff(chAOnSounds, note, 4);
                            break;
                        }
                    case SoundGroup.Group2:
                        {
                            emptySlot = SearchEmptySlotAndOff(chBOnSounds, note, 4);
                            break;
                        }
                }
                return emptySlot;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            public override SoundBase KeyOff(NoteOffEvent note)
            {
                MSM5232Sound removed = (MSM5232Sound)base.KeyOff(note);

                if (removed != null)
                {
                    for (int i = 0; i < chAOnSounds.Count; i++)
                    {
                        if (chAOnSounds[i] == removed)
                        {
                            FormMain.OutputDebugLog("KeyOff A ch" + removed.Slot + " " + note.ToString());
                            chAOnSounds.RemoveAt(i);
                            return removed;
                        }
                    }
                    for (int i = 0; i < chBOnSounds.Count; i++)
                    {
                        if (chBOnSounds[i] == removed)
                        {
                            FormMain.OutputDebugLog("KeyOff B ch" + removed.Slot + " " + note.ToString());
                            chBOnSounds.RemoveAt(i);
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
        private class MSM5232Sound : SoundBase
        {

            private MSM5232 parentModule;

            private SevenBitNumber programNumber;

            public MSM5232Timbre Timbre;

            public int lastGroup = 0;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public MSM5232Sound(MSM5232 parentModule, MSM5232SoundManager manager, NoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.programNumber = (SevenBitNumber)parentModule.ProgramNumbers[noteOnEvent.Channel];
                this.Timbre = parentModule.Timbres[programNumber];
                this.lastGroup = (int)Timbre.SoundGroup;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                SetTimbre();
                //Volume
                UpdateVolume();
                //Freq
                UpdatePitch();
            }

            /// <summary>
            /// 
            /// </summary>
            public void SetTimbre()
            {
                var pn = parentModule.ProgramNumbers[NoteOnEvent.Channel];
                var timbre = parentModule.Timbres[pn];

                MSM5232WriteData(parentModule.UnitNumber, (uint)(0x8 + lastGroup), timbre.AT);
                MSM5232WriteData(parentModule.UnitNumber, (uint)(0xa + lastGroup), timbre.DT);
                MSM5232WriteData(parentModule.UnitNumber, (uint)(0xc + lastGroup), (byte)(timbre.EGE << 5 | timbre.ARM << 4 | timbre.Hormonics));
            }

            /// <summary>
            /// 
            /// </summary>
            public override void UpdateVolume()
            {
                var exp = parentModule.Expressions[NoteOnEvent.Channel] / 127d;
                var vol = parentModule.Volumes[NoteOnEvent.Channel] / 127d;
                var vel = NoteOnEvent.Velocity / 127d;

                byte fv = (byte)((byte)Math.Round(15 * vol * vel * exp) & 0xf);

                MSM5232SetVolume(parentModule.UnitNumber, lastGroup, fv);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public override void UpdatePitch()
            {
                if (!Timbre.NoiseTone)
                {
                    byte noteNum = (byte)NoteOnEvent.NoteNumber;

                    if (noteNum > 0x24)
                        noteNum -= 24;
                    else
                        noteNum = 0;

                    MSM5232WriteData(parentModule.UnitNumber, (uint)(Slot + (lastGroup * 4)), (byte)(0x80 | noteNum));
                }
                else
                {
                    MSM5232WriteData(parentModule.UnitNumber, (uint)(Slot + (lastGroup * 4)), (byte)(0xff));
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOff()
            {
                base.KeyOff();

                MSM5232WriteData(parentModule.UnitNumber, (uint)(Slot + (lastGroup * 4)), 0);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<MSM5232Timbre>))]
        [DataContract]
        public class MSM5232Timbre : TimbreBase
        {
            [DataMember]
            [Category("Sound")]
            [Description("Sound Group")]
            public SoundGroup SoundGroup
            {
                get;
                set;
            }

            private byte f_AT;

            [DataMember]
            [Category("Sound")]
            [Description("Attack Time (0-7)")]
            public byte AT
            {
                get
                {
                    return f_AT;
                }
                set
                {
                    f_AT = (byte)(value & 7);
                }
            }

            private byte f_DT;

            [DataMember]
            [Category("Sound")]
            [Description("Decay Time (0-15)")]
            public byte DT
            {
                get
                {
                    return f_DT;
                }
                set
                {
                    f_DT = (byte)(value & 15);
                }
            }


            private byte f_EGE;

            [Browsable(false)]
            [DataMember]
            [Category("Sound")]
            [Description("Envelope Generator Mode (0:Off 1:On)")]
            public byte EGE
            {
                get
                {
                    return f_EGE;
                }
                set
                {
                    f_EGE = (byte)(value & 1);
                }
            }


            private byte f_ARM = 1;

            [DataMember]
            [Category("Sound")]
            [Description("Attack Release Mode (0:Off 1:On)")]
            public byte ARM
            {
                get
                {
                    return f_ARM;
                }
                set
                {
                    f_ARM = (byte)(value & 1);
                }
            }

            private byte f_Horm = 1;

            [DataMember]
            [Category("Sound")]
            [Description("Hormonics mode (b0:Normal b1:1 Octave b2:2 Octave b3:3 Octave)")]
            public byte Hormonics
            {
                get
                {
                    return f_Horm;
                }
                set
                {
                    f_Horm = (byte)(value & 15);
                }
            }

            private bool f_NoiseTone;

            [DataMember]
            [Category("Sound")]
            [Description("Noise Tone")]
            public bool NoiseTone
            {
                get
                {
                    return f_NoiseTone;
                }
                set
                {
                    f_NoiseTone = value;
                }
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<MSM5232Timbre>(serializeData);
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
        public enum SoundGroup
        {
            Group1,
            Group2,
        }

    }
}