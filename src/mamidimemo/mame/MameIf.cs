// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Mame
{
    public static class MameIF
    {
        /// <summary>
        /// 
        /// </summary>
        public static IntPtr ParentModule
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hModule"></param>
        /// <param name="procName"></param>
        /// <returns></returns>
        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiName"></param>
        /// <returns></returns>
        public static IntPtr GetProcAddress(string apiName)
        {
            return GetProcAddress(ParentModule, apiName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentModule"></param>
        internal static void Initialize(IntPtr parentModule)
        {
            ParentModule = parentModule;
        }
    }
}
