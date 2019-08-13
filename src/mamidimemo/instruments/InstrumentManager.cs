using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo.Instruments
{
    public static class InstrumentManager
    {
        private static object lockObject = new object();

        public static List<YM2151> List_ym2151 = new List<YM2151>();

        public static List<YM2612> List_ym2612 = new List<YM2612>();

        public static List<GB_APU> List_gbapu = new List<GB_APU>();

        public static List<SN76496> List_sn76496 = new List<SN76496>();

        public static List<NAMCO_CUS30> List_namco_cus30 = new List<NAMCO_CUS30>();

        public static List<RP2A03> List_RP2A03 = new List<RP2A03>();

        public static IEnumerable<InstrumentBase> GetAllInstruments()
        {
            List<InstrumentBase> insts = new List<InstrumentBase>();

            insts.AddRange(List_ym2151);
            insts.AddRange(List_ym2612);
            insts.AddRange(List_gbapu);
            insts.AddRange(List_sn76496);
            insts.AddRange(List_namco_cus30);
            insts.AddRange(List_RP2A03);

            return insts.AsEnumerable();
        }

        /// <summary>
        /// 
        /// </summary>
        public static void RestoreSettings()
        {
            try
            {
                var List_ym2151 = JsonConvert.DeserializeObject<List<YM2151>>(Settings.Default.YM2151);
                if (List_ym2151 != null)
                    InstrumentManager.List_ym2151 = List_ym2151;
            }
            catch (Exception ex)
            {
                if (ex is Exception)
                    return;
                if (ex is SystemException)
                    return;

                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
            try
            {
                var List_ym2612 = JsonConvert.DeserializeObject<List<YM2612>>(Settings.Default.YM2612);
                if (List_ym2612 != null)
                    InstrumentManager.List_ym2612 = List_ym2612;
            }
            catch (Exception ex)
            {
                if (ex is Exception)
                    return;
                if (ex is SystemException)
                    return;

                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
            try
            {
                var List_gbapu = JsonConvert.DeserializeObject<List<GB_APU>>(Settings.Default.GB_APU);
                if (List_gbapu != null)
                    InstrumentManager.List_gbapu = List_gbapu;
            }
            catch (Exception ex)
            {
                if (ex is Exception)
                    return;
                if (ex is SystemException)
                    return;

                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
            try
            {
                var List_sn76496 = JsonConvert.DeserializeObject<List<SN76496>>(Settings.Default.SN76496);
                if (List_sn76496 != null)
                    InstrumentManager.List_sn76496 = List_sn76496;
            }
            catch (Exception ex)
            {
                if (ex is Exception)
                    return;
                if (ex is SystemException)
                    return;

                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
            try
            {
                var List_namco_cus30 = JsonConvert.DeserializeObject<List<NAMCO_CUS30>>(Settings.Default.NAMCO_CUS30);
                if (List_namco_cus30 != null)
                    InstrumentManager.List_namco_cus30 = List_namco_cus30;
            }
            catch (Exception ex)
            {
                if (ex is Exception)
                    return;
                if (ex is SystemException)
                    return;

                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
            try
            {
                var List_RP2A03 = JsonConvert.DeserializeObject<List<RP2A03>>(Settings.Default.RP2A03);
                if (List_RP2A03 != null)
                    InstrumentManager.List_RP2A03 = List_RP2A03;
            }
            catch (Exception ex)
            {
                if (ex is Exception)
                    return;
                if (ex is SystemException)
                    return;

                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }

        public static void SaveSettings()
        {
            Settings.Default.YM2151 = JsonConvert.SerializeObject(List_ym2151, Formatting.Indented);
            Settings.Default.YM2612 = JsonConvert.SerializeObject(List_ym2612, Formatting.Indented);
            Settings.Default.NAMCO_CUS30 = JsonConvert.SerializeObject(List_namco_cus30, Formatting.Indented);
            Settings.Default.SN76496 = JsonConvert.SerializeObject(List_sn76496, Formatting.Indented);
            Settings.Default.GB_APU = JsonConvert.SerializeObject(List_gbapu, Formatting.Indented);
            Settings.Default.RP2A03 = JsonConvert.SerializeObject(List_RP2A03, Formatting.Indented);
        }


        /// <summary>
        /// 
        /// </summary>
        public static event EventHandler<EventArgs> InstrumentAdded;

        /// <summary>
        /// 
        /// </summary>
        public static event EventHandler<EventArgs> InstrumentRemoved;

        /// <summary>
        /// 
        /// </summary>
        static InstrumentManager()
        {
            Midi.MidiManager.MidiEventReceived += MidiManager_MidiEventReceived;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instrumentType"></param>
        public static void AddInstrument(InstrumentType instrumentType)
        {
            lock (lockObject)
            {
                switch (instrumentType)
                {
                    case InstrumentType.YM2151:
                        {
                            if (List_ym2151.Count < 7)
                            {
                                List_ym2151.Add(new YM2151((uint)List_ym2151.Count));
                                InstrumentAdded?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                            }
                            break;
                        }
                    case InstrumentType.YM2612:
                        {
                            if (List_ym2612.Count < 7)
                            {
                                List_ym2612.Add(new YM2612((uint)List_ym2612.Count));
                                InstrumentAdded?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                            }
                            break;
                        }
                    case InstrumentType.GB_APU:
                        {
                            if (List_gbapu.Count < 7)
                            {
                                List_gbapu.Add(new GB_APU((uint)List_gbapu.Count));
                                InstrumentAdded?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                            }
                            break;
                        }
                    case InstrumentType.SN76496:
                        {
                            if (List_sn76496.Count < 7)
                            {
                                List_sn76496.Add(new SN76496((uint)List_sn76496.Count));
                                InstrumentAdded?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                            }
                            break;
                        }
                    case InstrumentType.NAMCO_CUS30:
                        {
                            if (List_namco_cus30.Count < 7)
                            {
                                List_namco_cus30.Add(new NAMCO_CUS30((uint)List_namco_cus30.Count));
                                InstrumentAdded?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                            }
                            break;
                        }
                    case InstrumentType.RP2A03:
                        {
                            if (List_RP2A03.Count < 7)
                            {
                                List_RP2A03.Add(new RP2A03((uint)List_RP2A03.Count));
                                InstrumentAdded?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                            }
                            break;
                        }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="instrumentType"></param>
        public static void RemoveInstrument(InstrumentType type)
        {
            lock (lockObject)
            {
                switch (type)
                {
                    case InstrumentType.YM2151:
                        List_ym2151[List_ym2151.Count - 1].Dispose();
                        List_ym2151.RemoveAt(List_ym2151.Count - 1);
                        InstrumentRemoved?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                        break;
                    case InstrumentType.YM2612:
                        List_ym2612.RemoveAt(List_ym2612.Count - 1);
                        InstrumentRemoved?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                        break;
                    case InstrumentType.GB_APU:
                        List_gbapu.RemoveAt(List_gbapu.Count - 1);
                        InstrumentRemoved?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                        break;
                    case InstrumentType.SN76496:
                        List_sn76496.RemoveAt(List_sn76496.Count - 1);
                        InstrumentRemoved?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                        break;
                    case InstrumentType.NAMCO_CUS30:
                        List_namco_cus30.RemoveAt(List_namco_cus30.Count - 1);
                        InstrumentRemoved?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                        break;
                    case InstrumentType.RP2A03:
                        List_RP2A03.RemoveAt(List_RP2A03.Count - 1);
                        InstrumentRemoved?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MidiManager_MidiEventReceived(object sender, Melanchall.DryWetMidi.Devices.MidiEventReceivedEventArgs e)
        {
            lock (lockObject)
            {
                List_ym2151.ForEach((dev) => { dev.NotifyMidiEvent(e.Event); });
                List_ym2612.ForEach((dev) => { dev.NotifyMidiEvent(e.Event); });
                List_gbapu.ForEach((dev) => { dev.NotifyMidiEvent(e.Event); });
                List_sn76496.ForEach((dev) => { dev.NotifyMidiEvent(e.Event); });
                List_namco_cus30.ForEach((dev) => { dev.NotifyMidiEvent(e.Event); });
                List_RP2A03.ForEach((dev) => { dev.NotifyMidiEvent(e.Event); });
            }
        }

    }
}
