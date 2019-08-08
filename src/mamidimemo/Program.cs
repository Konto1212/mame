using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Smf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.mamidimemo.mame;
using zanac.mamidimemo.Properties;

namespace zanac.mamidimemo
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

            mainThread = new Thread(new ThreadStart(() =>
            {
                Settings.Default.Reload();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Application.Run(new FormMain());

                Settings.Default.Save();
            }));
            mainThread.Start();
            while (mainThread.ThreadState != ThreadState.Running) ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static int HasExited()
        {
            if (mainThread == null)
                return 1;
            return (mainThread.ThreadState != ThreadState.Running) ? 1 : 0;
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
