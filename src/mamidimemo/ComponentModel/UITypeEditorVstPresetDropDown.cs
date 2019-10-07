using Jacobi.Vst.Interop.Host;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Instruments.Vst;

namespace zanac.MAmidiMEmo.ComponentModel
{

    public abstract class UITypeEditorVstPresetDropDown : UITypeEditor
    {
        protected abstract VstPluginContextWrapper GetTargetVstPluginContext(ITypeDescriptorContext context);

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        private IWindowsFormsEditorService service;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            if (service == null)
                return value;
            var list = new ListBox();
            list.Click += List_Click;
            lock (InstrumentBase.VstPluginContextLockObject)
            {
                var ctx = GetTargetVstPluginContext(context)?.VstPluginContext;
                if (ctx != null)
                {
                    for (int i = 0; i < ctx.PluginInfo.ProgramCount; i++)
                        list.Items.Add(ctx.PluginCommandStub.GetProgramNameIndexed(i));
                }
            }
            service.DropDownControl(list);
            return (list.SelectedItem != null) ? list.SelectedItem : value;
        }

        private void List_Click(object sender, EventArgs e)
        {
            service?.CloseDropDown();
        }
    }

}
