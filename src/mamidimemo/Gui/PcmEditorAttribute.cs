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
    public class PcmEditorAttribute : Attribute
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
        /// <param name="fileDialogFilter"></param>
        /// <param name="maxSize"></param>
        public PcmEditorAttribute(string fileDialogFilter)
        {
            Exts = fileDialogFilter;
        }
    }
}
