// copyright-holders:K.Ito
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using zanac.MAmidiMEmo.Instruments;

namespace zanac.MAmidiMEmo.Gui
{
    /// <summary>
    /// 
    /// </summary>
    public class WsgITypeEditor : ArrayEditor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public WsgITypeEditor(Type type) : base(type)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="provider"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService editorService = null;

            if (provider != null)
                editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (editorService == null)
                return value;

            var byteObj = context.Instance as IWsgEditorByteCapable;
            var sbyteObj = context.Instance as IWsgEditorSbyteCapable;

            using (FormWsgEditor frm = new FormWsgEditor())
            {
                if (byteObj != null)
                    frm.ByteInstance = byteObj;
                if (sbyteObj != null)
                    frm.SbyteInstance = sbyteObj;

                DialogResult dr = frm.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    if (byteObj != null)
                        byteObj.WsgData = frm.ByteWsgData;
                    if (sbyteObj != null)
                        sbyteObj.WsgData = frm.SbyteWsgData;
                }
            }
            return value;
        }
    }
}
