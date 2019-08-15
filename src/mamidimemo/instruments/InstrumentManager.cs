using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo.Instruments
{
    public static class InstrumentManager
    {
        private static object lockObject = new object();

        public static List<List<InstrumentBase>> Instruments = new List<List<InstrumentBase>>();

        /// <summary>
        /// 
        /// </summary>
        static InstrumentManager()
        {
            Midi.MidiManager.MidiEventReceived += MidiManager_MidiEventReceived;

            for (int i = 0; i < Enum.GetNames(typeof(InstrumentType)).Length; i++)
                Instruments.Add(new List<InstrumentBase>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<InstrumentBase> GetAllInstruments()
        {
            List<InstrumentBase> insts = new List<InstrumentBase>();

            foreach (List<InstrumentBase> i in Instruments)
                insts.AddRange(i);

            return insts.AsEnumerable();
        }

        /// <summary>
        /// 
        /// </summary>
        public static void RestoreSettings(EnvironmentSettings settings)
        {
            if (settings.Instruments != null)
            {
                foreach (int v in Enum.GetValues(typeof(InstrumentType)))
                {
                    if (v < settings.Instruments.Count && settings.Instruments[v] != null)
                    {
                        try
                        {
                            //clear current insts
                            for (int i = Instruments[v].Count - 1; i >= 0; i--)
                            {
                                Instruments[v][i].Dispose();
                                Instruments[v].RemoveAt(i);
                            }
                            //prepare new insts
                            foreach (InstrumentBase inst in settings.Instruments[v])
                                inst.PrepareSound();
                            //set new insts
                            InstrumentManager.Instruments[v] = settings.Instruments[v];
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

            InstrumentChanged?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void SaveSettings(EnvironmentSettings settings)
        {
            settings.Instruments = Instruments;
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
        public static event EventHandler<EventArgs> InstrumentChanged;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="instrumentType"></param>
        public static void AddInstrument(InstrumentType instrumentType)
        {
            lock (lockObject)
            {
                if (Instruments[(int)instrumentType].Count < 8)
                {
                    Assembly asm = Assembly.GetAssembly(typeof(InstrumentType));
                    string name = Enum.GetName(typeof(InstrumentType), instrumentType);
                    Type t = asm.GetType("zanac.MAmidiMEmo.Instruments." + name);

                    var inst = (InstrumentBase)Activator.CreateInstance(t, (uint)Instruments[(int)instrumentType].Count);
                    inst.PrepareSound();
                    Instruments[(int)instrumentType].Add(inst);
                    InstrumentAdded?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="instrumentType"></param>
        public static void RemoveInstrument(InstrumentType instrumentType)
        {
            lock (lockObject)
            {
                var list = Instruments[(int)instrumentType];
                list[list.Count - 1].Dispose();
                list.RemoveAt(list.Count - 1);
                InstrumentRemoved?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
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
                foreach (var i in Instruments)
                    i.ForEach((dev) => { dev.NotifyMidiEvent(e.Event); });
            }
        }

    }
}
