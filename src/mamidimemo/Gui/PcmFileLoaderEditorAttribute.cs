// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Gui
{
    /// <summary>
    /// 
    /// </summary>
    public class PcmFileLoaderEditorAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string Exts
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Rate
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Bits
        {
            get;
            private set;
        }


        /// <summary>
        /// 
        /// </summary>
        public int Channels
        {
            get;
            private set;
        }

        public int MaxSize
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileDialogFilter"></param>
        public PcmFileLoaderEditorAttribute(string fileDialogFilter, int rate, int bits, int channels, int maxSize)
        {
            Exts = fileDialogFilter;
            Rate = rate;
            Bits = bits;
            Channels = channels;
            MaxSize = maxSize;
        }
    }
}
