﻿// copyright-holders:K.Ito
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

            WsgBitWideAttribute watt = (WsgBitWideAttribute)context.PropertyDescriptor.Attributes[typeof(WsgBitWideAttribute)];

            using (FormWsgEditor frm = new FormWsgEditor())
            {
                frm.WsgBitWide = watt.BitWide;

                if(value.GetType() == typeof(byte[]))
                    frm.ByteWsgData = (byte[])value;
                else if (value.GetType() == typeof(sbyte[]))
                    frm.SbyteWsgData = (sbyte[])value;

                DialogResult dr = frm.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    if (value.GetType() == typeof(byte[]))
                        value = frm.ByteWsgData;
                    else if (value.GetType() == typeof(sbyte[]))
                        value = frm.SbyteWsgData;
                }
            }
            return value;
        }
    }
}
