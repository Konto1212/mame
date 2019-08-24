// copyright-holders:K.Ito
using Melanchall.DryWetMidi.Smf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo.Instruments
{
    public static class InstrumentManager
    {
        private static List<List<InstrumentBase>> instruments = new List<List<InstrumentBase>>();

        private static MultiMediaTimerComponent multiMediaTimerComponent;

        private static Dictionary<Action, object> timerSounds = new Dictionary<Action, object>();

        private static object lockObject = new object();

        public const uint TIMER_INTERVAL = 10;

        public const double TIMER_HZ = 1000d / (double)TIMER_INTERVAL;

        /// <summary>
        /// 
        /// </summary>
        static InstrumentManager()
        {
            Program.ShuttingDown += Program_ShuttingDown;

            Midi.MidiManager.MidiEventReceived += MidiManager_MidiEventReceived;

            for (int i = 0; i < Enum.GetNames(typeof(InstrumentType)).Length; i++)
                instruments.Add(new List<InstrumentBase>());

            multiMediaTimerComponent = new MultiMediaTimerComponent();
            multiMediaTimerComponent.Interval = TIMER_INTERVAL;
            multiMediaTimerComponent.Resolution = 2;
            multiMediaTimerComponent.OnTimer += MultiMediaTimerComponent_OnTimer;
            multiMediaTimerComponent.Enabled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        public static void SetPeriodicCallback(Action action)
        {
            lock (lockObject)
            {
                if (!timerSounds.ContainsKey(action))
                    timerSounds.Add(action, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        public static void UnsetPeriodicCallback(Action action)
        {
            lock (lockObject)
            {
                foreach (var snd in timerSounds.Keys)
                    snd();
                if (timerSounds.ContainsKey(action))
                    timerSounds.Remove(action);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        private static void MultiMediaTimerComponent_OnTimer(object sender)
        {
            lock (lockObject)
            {
                //Gui.FormMain.OutputDebugLogFile("timer enter");
                foreach (var snd in timerSounds.Keys)
                    snd();
                //Gui.FormMain.OutputDebugLogFile("timer leave");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Program_ShuttingDown(object sender, EventArgs e)
        {
            multiMediaTimerComponent?.Dispose();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<InstrumentBase> GetAllInstruments()
        {
            lock (lockObject)
            {
                List<InstrumentBase> insts = new List<InstrumentBase>();

                foreach (List<InstrumentBase> i in instruments)
                    insts.AddRange(i);

                return insts.AsEnumerable();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void RestoreSettings(EnvironmentSettings settings)
        {
            lock (lockObject)
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
                                for (int i = instruments[v].Count - 1; i >= 0; i--)
                                {
                                    instruments[v][i].Dispose();
                                    instruments[v].RemoveAt(i);
                                }
                                //prepare new insts
                                foreach (InstrumentBase inst in settings.Instruments[v])
                                    inst.PrepareSound();
                                //set new insts
                                InstrumentManager.instruments[v] = settings.Instruments[v];
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
        }

        /// <summary>
        /// 
        /// </summary>
        public static void SaveSettings(EnvironmentSettings settings)
        {
            settings.Instruments = instruments;
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
                if (instruments[(int)instrumentType].Count < 8)
                {
                    Assembly asm = Assembly.GetAssembly(typeof(InstrumentType));
                    string name = Enum.GetName(typeof(InstrumentType), instrumentType);
                    Type t = asm.GetType("zanac.MAmidiMEmo.Instruments." + name);

                    var inst = (InstrumentBase)Activator.CreateInstance(t, (uint)instruments[(int)instrumentType].Count);
                    inst.PrepareSound();
                    instruments[(int)instrumentType].Add(inst);
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
                var list = instruments[(int)instrumentType];
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
        private static void MidiManager_MidiEventReceived(object sender, MidiEvent e)
        {
            lock (lockObject)
            {
                foreach (var i in instruments)
                    i.ForEach((dev) => { dev.NotifyMidiEvent(e); });
            }
        }

    }
}
