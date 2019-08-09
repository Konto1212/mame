using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.mamidimemo.instruments
{
    public static class InstrumentManager
    {
        public static List<YM2151> List_ym2151 = new List<YM2151>();

        public static List<YM2612> List_ym2612 = new List<YM2612>();

        public static List<GBAPU> List_gbapu = new List<GBAPU>();

        public static List<SN76496> List_sn76496 = new List<SN76496>();

        public static List<NAMCO_CUS30> List_namco_cus30 = new List<NAMCO_CUS30>();

        public static IEnumerable<InstrumentBase> GetAllInstruments()
        {
            List<InstrumentBase> insts = new List<InstrumentBase>();
            insts.AddRange(List_ym2151);
            insts.AddRange(List_ym2612);
            insts.AddRange(List_gbapu);
            insts.AddRange(List_sn76496);
            insts.AddRange(List_namco_cus30);

            return insts.AsEnumerable();
        }

        public static event EventHandler<EventArgs> InstrumentAdded;

        public static event EventHandler<EventArgs> InstrumentRemoved;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instrumentType"></param>
        public static void AddInstrument(InstrumentType instrumentType)
        {
            switch (instrumentType)
            {
                case InstrumentType.YM2151:
                    {
                        lock (List_ym2151)
                        {
                            if (List_ym2151.Count < 7)
                            {
                                List_ym2151.Add(new YM2151((uint)List_ym2151.Count));
                                InstrumentAdded?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                            }
                        }
                        break;
                    }
                case InstrumentType.YM2612:
                    {
                        lock (List_ym2612)
                        {
                            if (List_ym2612.Count < 7)
                            {
                                List_ym2612.Add(new YM2612((uint)List_ym2612.Count));
                                InstrumentAdded?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                            }
                        }
                        break;
                    }
                case InstrumentType.GBAPU:
                    {
                        lock (List_gbapu)
                        {
                            if (List_gbapu.Count < 7)
                            {
                                List_gbapu.Add(new GBAPU((uint)List_gbapu.Count));
                                InstrumentAdded?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                            }
                        }
                        break;
                    }
                case InstrumentType.SN76496:
                    {
                        lock (List_sn76496)
                        {
                            if (List_sn76496.Count < 7)
                            {
                                List_sn76496.Add(new SN76496((uint)List_sn76496.Count));
                                InstrumentAdded?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                            }
                        }
                        break;
                    }
                case InstrumentType.NAMCO_CUS30:
                    {
                        lock (List_namco_cus30)
                        {
                            if (List_namco_cus30.Count < 7)
                            {
                                List_namco_cus30.Add(new NAMCO_CUS30((uint)List_namco_cus30.Count));
                                InstrumentAdded?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                            }
                        }
                        break;
                    }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="instrumentType"></param>
        public static void RemoveInstrument(InstrumentBase instrument)
        {
            switch (instrument.InstrumentType)
            {
                case InstrumentType.YM2151:
                    List_ym2151.Remove((YM2151)instrument);
                    InstrumentRemoved?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                    break;
                case InstrumentType.YM2612:
                    List_ym2612.Remove((YM2612)instrument);
                    InstrumentRemoved?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                    break;
                case InstrumentType.GBAPU:
                    List_gbapu.Remove((GBAPU)instrument);
                    InstrumentRemoved?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                    break;
                case InstrumentType.SN76496:
                    List_sn76496.Remove((SN76496)instrument);
                    InstrumentRemoved?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                    break;
                case InstrumentType.NAMCO_CUS30:
                    List_namco_cus30.Remove((NAMCO_CUS30)instrument);
                    InstrumentRemoved?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static InstrumentManager()
        {
            midi.MidiManager.MidiEventReceived += MidiManager_MidiEventReceived;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MidiManager_MidiEventReceived(object sender, Melanchall.DryWetMidi.Devices.MidiEventReceivedEventArgs e)
        {
            lock (List_ym2151)
                List_ym2151.ForEach((dev) => { dev.NotifyMidiEvent(e.Event); });
            lock (List_ym2612)
                List_ym2612.ForEach((dev) => { dev.NotifyMidiEvent(e.Event); });
            lock (List_gbapu)
                List_gbapu.ForEach((dev) => { dev.NotifyMidiEvent(e.Event); });
            lock (List_sn76496)
                List_sn76496.ForEach((dev) => { dev.NotifyMidiEvent(e.Event); });
            lock (List_namco_cus30)
                List_namco_cus30.ForEach((dev) => { dev.NotifyMidiEvent(e.Event); });
        }

    }
}
