using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Instruments
{
    public static class HighPrecisionTimer
    {

        [DllImport("kernel32.dll")]
        public static extern SafeWaitHandle CreateWaitableTimer(IntPtr lpTimerAttributes, bool bManualReset, string lpTimerName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWaitableTimer(SafeWaitHandle hTimer,
            [In] ref long pDueTime, int lPeriod,
            IntPtr pfnCompletionRoutine, IntPtr lpArgToCompletionRoutine, bool fResume);

        [DllImport("kernel32.dll")]
        internal static extern uint WaitForSingleObject(SafeWaitHandle hHandle, uint dwMilliseconds);

        private const int WAIT_TIMEOUT = 120 * 1000;

        /// <summary>
        /// Periodic Action Timer Interval
        /// </summary>
        public const uint TIMER_BASIC_INTERVAL = 10;

        /// <summary>
        /// Periodic Action Timer Hz
        /// </summary>
        public const double TIMER_BASIC_HZ = 1000d / (double)TIMER_BASIC_INTERVAL;

        static HighPrecisionTimer()
        {
            Program.ShuttingDown += Program_ShuttingDown;
        }

        [DllImport("kernel32.dll")]
        public static extern void GetSystemTimeAsFileTime(out long lpSystemTimeAsFileTime);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="periodMs"></param>
        /// <param name="state"></param>    
        public static void SetPeriodicCallback(Func<object, double> action, double periodMs, object state)
        {
            long lpSystemTimeAsFileTime;
            periodMs = action(state);
            GetSystemTimeAsFileTime(out lpSystemTimeAsFileTime);
            double nextTime = lpSystemTimeAsFileTime;
            Thread th = new Thread((object data) =>
            {
                using (SafeWaitHandle handle = CreateWaitableTimer(IntPtr.Zero, false, null))
                {
                    periodMs *= 1000 * 10;
                    while (true)
                    {
                        nextTime += periodMs;
                        long dueTime = (long)Math.Round(nextTime);
                        SetWaitableTimer(handle, ref dueTime, 0, IntPtr.Zero, IntPtr.Zero, false);
                        WaitForSingleObject(handle, WAIT_TIMEOUT);
                        lock (Program.ExclusiveLockObject)
                            periodMs = action(data);
                        if (periodMs < 0 || shutDown)
                            break;
                        periodMs *= 1000 * 10;
                        // Next time is past time?
                        GetSystemTimeAsFileTime(out lpSystemTimeAsFileTime);
                        if(lpSystemTimeAsFileTime > nextTime + periodMs)
                            nextTime = lpSystemTimeAsFileTime;  // adjust to current time
                    }
                }
            });
            th.Start(state);
        }

        private static bool shutDown;

        private static void Program_ShuttingDown(object sender, EventArgs e)
        {
            shutDown = true;
        }

    }
}
