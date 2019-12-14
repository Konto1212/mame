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
    public class BrrFileLoaderEditorAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string Exts
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
        public BrrFileLoaderEditorAttribute(string fileDialogFilter, int maxSize)
        {
            Exts = fileDialogFilter;
            MaxSize = maxSize;
        }
    }
}
