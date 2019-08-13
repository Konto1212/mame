using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Smf;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Mame;
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo
{
    public static class Program
    {

        private static Thread mainThread;


        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        /// <param name="parentModule">親モジュール</param>
        public static void Main(IntPtr parentModule)
        {
            MameIF.Initialize(parentModule);
            var threadStart = new ManualResetEvent(false);
            mainThread = new Thread(new ThreadStart(() =>
            {
                threadStart.Set();
                Settings.Default.Reload();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                if (!string.IsNullOrEmpty(Settings.Default.EnvironmentSettings))
                {
                    try
                    {
                        var settings = JsonConvert.DeserializeObject<EnvironmentSettings>(StringCompressionUtility.Decompress(Settings.Default.EnvironmentSettings));
                        InstrumentManager.RestoreSettings(settings);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }

                Application.Run(new FormMain());

                Settings.Default.EnvironmentSettings = StringCompressionUtility.Compress(JsonConvert.SerializeObject(SaveEnvironmentSettings(), Formatting.Indented));
                Settings.Default.Save();
            }));
            mainThread.SetApartmentState(ApartmentState.STA);
            mainThread.Start();
            threadStart.WaitOne();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static EnvironmentSettings SaveEnvironmentSettings()
        {
            var es = new EnvironmentSettings();
            try
            {
                InstrumentManager.SaveSettings(es);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return es;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static int HasExited()
        {
            var ret = mainThread.IsAlive ? 0 : 1;
            return ret;
        }

        private static ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static void SoundUpdating()
        {
            lockSlim.EnterWriteLock();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static void SoundUpdated()
        {
            lockSlim.ExitWriteLock();
        }
    }
}
