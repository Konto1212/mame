// copyright-holders:K.Ito
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Mame;

namespace zanac.MAmidiMEmo.Instruments
{
    [MidiHook]
    [DataContract]
    public abstract class InstrumentBase : ContextBoundObject
    {
        /// <summary>
        /// 
        /// </summary>
        [Category("General")]
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("General")]
        public abstract string Group
        {
            get;
        }

        private float f_GainLeft = 1.0f;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("General")]
        [Description("Gain Left ch. (0.0-*)")]
        public float GainLeft
        {
            get
            {
                return f_GainLeft;
            }
            set
            {
                f_GainLeft = value;
                set_output_gain(UnitNumber, SoundInterfaceTagNamePrefix, 0, value);
            }
        }

        private float f_GainRight = 1.0f;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("General")]
        [Description("Gain Right ch. (0.0-*)")]
        public float GainRight
        {
            get
            {
                return f_GainRight;
            }
            set
            {
                f_GainRight = value;
                set_output_gain(UnitNumber, SoundInterfaceTagNamePrefix, 1, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Description("Memo")]
        public string Memo
        {
            get;
            set;
        }

        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
    typeof(UITypeEditor)), Localizable(false)]
        [IgnoreDataMember]
        [JsonIgnore]
        [Description("You can copy and paste this text data to other same type Instrument.")]
        public string SerializeData
        {
            get
            {
                return JsonConvert.SerializeObject(this, Formatting.Indented);
            }
            set
            {
                RestoreFrom(value);
            }
        }

        public abstract void RestoreFrom(string serializeData);

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        public abstract string ImageKey
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        [Category("General")]
        public abstract InstrumentType InstrumentType
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected abstract string SoundInterfaceTagNamePrefix
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("General")]
        public abstract uint DeviceID
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("MIDI")]
        public uint UnitNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("MIDI")]
        [Description("Receving MIDI ch")]
        [TypeConverter(typeof(CustomCollectionConverter))]
        public bool[] Channels
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("MIDI")]
        [Description("Pitch (0 - 8192 - 16383)")]
        [TypeConverter(typeof(CustomCollectionConverter))]
        public ushort[] Pitchs
        {
            get;
        }

        [DataMember]
        [Category("MIDI")]
        [Description("Pitch bend censitivity [half note]")]
        [TypeConverter(typeof(CustomCollectionConverter))]
        public byte[] PitchBendRanges
        {
            get;
        }

        [DataMember]
        [Category("MIDI")]
        [Description("Program number (0-127)")]
        [TypeConverter(typeof(CustomCollectionConverter))]
        public byte[] ProgramNumbers
        {
            get;
        }

        [DataMember]
        [Category("MIDI")]
        [Description("Volume (0-127)")]
        [TypeConverter(typeof(CustomCollectionConverter))]
        public byte[] Volumes
        {
            get;
        }

        [DataMember]
        [Category("MIDI")]
        [Description("Volume (0-127)")]
        [TypeConverter(typeof(CustomCollectionConverter))]
        public byte[] Expressions
        {
            get;
        }

        [DataMember]
        [Category("MIDI")]
        [Description("Volume ((L)0-63(C)64-127(R))")]
        [TypeConverter(typeof(CustomCollectionConverter))]
        public byte[] Panpots
        {
            get;
        }

        [Browsable(false)]
        public byte[] RpnLsb
        {
            get;
        }

        [Browsable(false)]
        public byte[] RpnMsb
        {
            get;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_set_device_enable(uint unitNumber, string tagName, byte enable);

        /// <summary>
        /// 
        /// </summary>
        private static delegate_set_device_enable set_device_enable;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_device_reset(uint unitNumber, string tagName);

        /// <summary>
        /// 
        /// </summary>
        private static delegate_device_reset device_reset;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_set_output_gain(uint unitNumber, string tagName, int channel, float gain);

        /// <summary>
        /// 
        /// </summary>
        private static delegate_set_output_gain set_output_gain
        {
            get;
            set;
        }

        static InstrumentBase()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("set_device_enable");
            if (funcPtr != IntPtr.Zero)
                set_device_enable = Marshal.GetDelegateForFunctionPointer<delegate_set_device_enable>(funcPtr);

            funcPtr = MameIF.GetProcAddress("set_output_gain");
            if (funcPtr != IntPtr.Zero)
                set_output_gain = Marshal.GetDelegateForFunctionPointer<delegate_set_output_gain>(funcPtr);

            funcPtr = MameIF.GetProcAddress("device_reset");
            if (funcPtr != IntPtr.Zero)
                device_reset = Marshal.GetDelegateForFunctionPointer<delegate_device_reset>(funcPtr);
        }

        /// <summary>
        /// 
        /// </summary>
        public InstrumentBase()
        {
            device_reset(UnitNumber, SoundInterfaceTagNamePrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        public InstrumentBase(uint unitNumber)
        {
            UnitNumber = unitNumber;

            Channels = new bool[] {
                    true, true, true,
                    true, true, true,
                    true, true, true,
                    true, true, true,
                    true, true, true,true };

            ProgramNumbers = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0 };
            Volumes = new byte[] {
                    127, 127, 127,
                    127, 127, 127,
                    127, 127, 127,
                    127, 127, 127,
                    127, 127, 127, 127  };
            Expressions = new byte[] {
                    127, 127, 127,
                    127, 127, 127,
                    127, 127, 127,
                    127, 127, 127,
                    127, 127, 127, 127  };
            Panpots = new byte[] {
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64, 64};
            Pitchs = new ushort[] {
                    8192, 8192, 8192,
                    8192, 8192, 8192,
                    8192, 8192, 8192,
                    8192, 8192, 8192,
                    8192, 8192, 8192, 8192};
            PitchBendRanges = new byte[] {
                    2, 2, 2,
                    2, 2, 2,
                    2, 2, 2,
                    2, 2, 2,
                    2, 2, 2, 2};
            RpnLsb = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0};
            RpnMsb = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0};
            RpnMsb = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0};
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
            set_device_enable(UnitNumber, SoundInterfaceTagNamePrefix, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        internal virtual void PrepareSound()
        {
            set_device_enable(UnitNumber, SoundInterfaceTagNamePrefix, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        internal void NotifyMidiEvent(MidiEvent midiEvent)
        {
            OnMidiEvent(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnMidiEvent(MidiEvent midiEvent)
        {
            var non = midiEvent as NoteOnEvent;
            if (non != null)
            {
                if (!Channels[non.Channel])
                    return;

                if (non.Velocity == 0)
                    OnNoteOffEvent(new NoteOffEvent(non.NoteNumber, (SevenBitNumber)0) { Channel = non.Channel, DeltaTime = non.DeltaTime });
                else
                    OnNoteOnEvent(non);
            }
            else
            {
                var noff = midiEvent as NoteOffEvent;
                if (noff != null)
                {
                    if (!Channels[noff.Channel])
                        return;
                    OnNoteOffEvent(noff);
                }
                else
                {
                    var cont = midiEvent as ControlChangeEvent;
                    if (cont != null)
                    {
                        if (!Channels[cont.Channel])
                            return;
                        OnControlChangeEvent(cont);
                    }
                    else
                    {
                        var prog = midiEvent as ProgramChangeEvent;
                        if (prog != null)
                        {
                            if (!Channels[prog.Channel])
                                return;
                            OnProgramChangeEvent(prog);
                        }
                        else
                        {
                            var pitch = midiEvent as PitchBendEvent;
                            if (pitch != null)
                            {
                                if (!Channels[pitch.Channel])
                                    return;
                                OnPitchBendEvent(pitch);
                            }
                            else
                            {
                                //TODO: key/ch pressure
                                var sysex = midiEvent as SysExEvent;
                                if (sysex != null)
                                {
                                    OnSystemExclusiveEvent(sysex);
                                }
                                else
                                {
                                    //TODO: key/ch pressure
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sysex"></param>
        protected virtual void OnSystemExclusiveEvent(SysExEvent sysex)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnNoteOnEvent(NoteOnEvent midiEvent)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnNoteOffEvent(NoteOffEvent midiEvent)
        {

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnControlChangeEvent(ControlChangeEvent midiEvent)
        {
            switch (midiEvent.ControlNumber)
            {
                case 6:    //Data Entry
                    switch (RpnMsb[midiEvent.Channel])
                    {
                        case 0: //PitchBendRanges
                            {
                                PitchBendRanges[midiEvent.Channel] = midiEvent.ControlValue;
                                break;
                            }
                    }
                    break;
                case 7:    //Volume
                    Volumes[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 10:    //Panpot
                    Panpots[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 11:    //Expression
                    Expressions[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 100:    //RPN LSB
                    RpnLsb[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 101:    //RPN MSB
                    RpnMsb[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnProgramChangeEvent(ProgramChangeEvent midiEvent)
        {
            ProgramNumbers[midiEvent.Channel] = midiEvent.ProgramNumber;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnPitchBendEvent(PitchBendEvent midiEvent)
        {
            Pitchs[midiEvent.Channel] = midiEvent.PitchValue;
        }
    }
}
