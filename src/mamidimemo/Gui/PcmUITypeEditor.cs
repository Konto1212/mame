using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace zanac.MAmidiMEmo.Gui
{
    /// <summary>
    /// 
    /// </summary>
    public class PcmUITypeEditor : UITypeEditor
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

            if (provider != null)
                editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            PcmEditorAttribute att = (PcmEditorAttribute)context.PropertyDescriptor.Attributes[typeof(PcmEditorAttribute)];

            if (editorService == null)
                return value;

            // CurrencyValueEditorForm を使用したプロパティエディタの表示
            using (FormPcmEditor frm = new FormPcmEditor())
            {
                /*
                frm.CurrencyValue = (long)value;    // 現在のプロパティ値をエディタ側に設定
                DialogResult dr = editorService.ShowDialog(frm);
                if (dr == DialogResult.OK)
                {
                    return frm.CurrencyValue;       // 新しい設定値をプロパティ値として返す
                }
                else
                {
                    return value;                   // エディタ呼び出し直前の設定値をそのまま返す
                }*/
            }
            return null;
        }
    }
}
