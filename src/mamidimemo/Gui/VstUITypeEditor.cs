// copyright-holders:K.Ito
using Jacobi.Vst.Interop.Host;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Instruments.Vst;

namespace zanac.MAmidiMEmo.Gui
{
    /// <summary>
    /// 
    /// </summary>
    public class VstUITypeEditor : UITypeEditor
    {

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
            VstSettings vs = (VstSettings)value;

            if (provider != null)
                editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (editorService == null || vs == null)
                return value;

            using (FormVstEditorFrame dlg = new FormVstEditorFrame())
            {
                lock (InstrumentBase.VstPluginContextLockObject)
                {
                    var ctx = vs.VstPluginContext;
                    if (ctx != null)
                        dlg.PluginCommandStub = ctx.VstPluginContext.PluginCommandStub;
                }
                dlg.ShowDialog(null);
            }
            return value;
        }
    }
}
