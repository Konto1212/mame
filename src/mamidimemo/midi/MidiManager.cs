using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Smf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using zanac.mamidimemo.instruments;
using zanac.mamidimemo.Properties;

//https://www.g200kg.com/jp/docs/tech/index.html
//http://lib.roland.co.jp/support/jp/manuals/res/1809744/MT-32_j2.pdf
//https://nickfever.com/Music/midi-cc-list

namespace zanac.mamidimemo.midi
{
    public static class MidiManager
    {
        public static BlockingCollection<MidiEvent> MessageQueue = new BlockingCollection<MidiEvent>();

        private static InputDevice inputDevice;

        //
        // 概要:
        //     Occurs when a MIDI event is received.
        public static event EventHandler<MidiEventReceivedEventArgs> MidiEventReceived;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<InputDevice> GetInputMidiDevices()
        {
            return InputDevice.GetAll();
        }

        /// <summary>
        /// 
        /// </summary>
        public static void SetInputMidiDevice(string deviceName)
        {
            if (inputDevice != null)
            {
                inputDevice.StopEventsListening();
                inputDevice.Dispose();
            }
            Settings.Default.MidiIF = deviceName;
            inputDevice = InputDevice.GetByName(deviceName);
            if (inputDevice != null)
            {
                inputDevice.EventReceived += midiEventReceived;
                inputDevice.StartEventsListening();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void midiEventReceived(object sender, MidiEventReceivedEventArgs e)
        {
            MidiEventReceived?.Invoke(sender, e);
        }

        public static byte[] ToByteArray<T>(this T structure) where T : struct
        {
            byte[] bb = new byte[Marshal.SizeOf(typeof(T))];
            GCHandle gch = GCHandle.Alloc(bb, GCHandleType.Pinned);
            Marshal.StructureToPtr(structure, gch.AddrOfPinnedObject(), false);
            gch.Free();
            return bb;
        }

        public static T FromByteArray<T>(byte[] data) where T : struct
        {
            GCHandle? gch = null;
            T str;
            try
            {
                gch = GCHandle.Alloc(data, GCHandleType.Pinned);
                str = Marshal.PtrToStructure<T>(gch.Value.AddrOfPinnedObject());
            }
            finally
            {
                gch?.Free();
            }
            return str;
        }

    }


}
