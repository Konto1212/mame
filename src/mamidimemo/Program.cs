// copyright-holders:K.Ito
using Accessibility;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Smf;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Mame;
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo
{
    public static class Program
    {
        /// <summary>
        /// 
        /// </summary>
        public const string FILE_VERSION = "0.5.0.0";

        public static readonly JsonSerializerSettings JsonAutoSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, DefaultValueHandling = DefaultValueHandling.Ignore };

        private static Thread mainThread;

        internal static string RestartRequiredApplication;

        public static void RestartApplication()
        {
            if (RestartRequiredApplication != null)
                Process.Start(RestartRequiredApplication);
        }

        public static event EventHandler ShuttingDown;

#pragma warning disable CS0414
        /// <summary>
        /// ダミー(遅延Assemblyロード回避)
        /// </summary>
        private static MultilineStringEditor dummyEditor = new MultilineStringEditor();

        /// <summary>
        /// ダミー(遅延Assemblyロード回避)
        /// </summary>
        private static AnnoScope dummyAnnoScope = AnnoScope.ANNO_CONTAINER;
#pragma warning restore  CS0414

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        /// <param name="parentModule">親モジュール</param>
        public static void Main(IntPtr parentModule)
        {
            System.Resources.ResourceManager rm =
                new System.Resources.ResourceManager("System", typeof(UriFormat).Assembly);
            string dummy = rm.GetString("Arg_EmptyOrNullString");

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
                        var dso = StringCompressionUtility.Decompress(Settings.Default.EnvironmentSettings);
                        var settings = JsonConvert.DeserializeObject<EnvironmentSettings>(dso, JsonAutoSettings);
                        InstrumentManager.RestoreSettings(settings);
                    }
                    catch (Exception ex)
                    {
                        if (ex.GetType() == typeof(Exception))
                            throw;
                        else if (ex.GetType() == typeof(SystemException))
                            throw;

                        MessageBox.Show(ex.ToString());
                    }
                }

                try
                {
                    Application.Run(new FormMain());

                    var so = JsonConvert.SerializeObject(SaveEnvironmentSettings(), Formatting.Indented, JsonAutoSettings);
                    Settings.Default.EnvironmentSettings = StringCompressionUtility.Compress(so);
                    Settings.Default.Save();
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(Exception))
                        throw;
                    else if (ex.GetType() == typeof(SystemException))
                        throw;

                    MessageBox.Show(ex.ToString());
                }

                ShuttingDown?.Invoke(typeof(Program), EventArgs.Empty);
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
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;

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

        private static ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// MAMEとMAmiの排他制御
        /// MAmiがMAMEのレジスタを書き換えるときに呼び出す必要がある
        /// </summary>
        public static void SoundUpdating()
        {
            lockSlim.EnterWriteLock();
        }

        /// <summary>
        /// MAMEとMAmiの排他制御
        /// MAmiがMAMEのレジスタを書き換えたあとに呼び出す必要がある
        /// </summary>
        public static void SoundUpdated()
        {
            lockSlim.ExitWriteLock();
        }


    }
}
