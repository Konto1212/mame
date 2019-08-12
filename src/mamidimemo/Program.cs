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
                Instruments.InstrumentManager.RestoreSettings();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Application.Run(new FormMain());

                Instruments.InstrumentManager.SaveSettings();
                Settings.Default.Save();
            }));
            mainThread.Start();
            threadStart.WaitOne();
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

        static ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

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
