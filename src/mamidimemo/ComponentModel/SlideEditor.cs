using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace zanac.MAmidiMEmo.ComponentModel
{
    public class SlideEditor : System.Drawing.Design.UITypeEditor
    {
        private ToolTip toolTip = new ToolTip();

        private int? minValue;
        private int? maxValue;

        public SlideEditor()
        {
        }

        public SlideEditor(int minValue, int maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return System.Drawing.Design.UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var service = provider.GetService
                (typeof(System.Windows.Forms.Design.IWindowsFormsEditorService))
                    as System.Windows.Forms.Design.IWindowsFormsEditorService;


            SlideParametersAttribute att = (SlideParametersAttribute)context.PropertyDescriptor.Attributes[typeof(SlideParametersAttribute)];

            TrackBar track = new TrackBar();
            track.ValueChanged += Track_ValueChanged;
            if (att != null)
            {
                track.Maximum = att.SliderMax;
                track.Minimum = att.SliderMin;
            }
            if(maxValue.HasValue)
                track.Maximum = maxValue.Value;
            if (minValue.HasValue)
                track.Minimum = minValue.Value;
            track.Width = 127;
            track.Height = 15;
            int freq = track.Maximum / 50;
            if (freq < 1)
                freq = 1;
            track.TickFrequency = freq;
            switch (value)
            {
                case byte v:
                    track.Value = v;
                    break;
                case sbyte v:
                    track.Value = v;
                    break;
                case short v:
                    track.Value = v;
                    break;
                case ushort v:
                    track.Value = v;
                    break;
                case int v:
                    track.Value = v;
                    break;
            }
            if(att.SliderDynamicSetValue)
                track.Tag = context;
            service.DropDownControl(track);

            switch (value)
            {
                case byte v:
                    return (byte)track.Value;
                case sbyte v:
                    return (sbyte)track.Value;
                case short v:
                    return (short)track.Value;
                case ushort v:
                    return (ushort)track.Value;
                default:
                    return track.Value;
            }
        }

        private void Track_ValueChanged(object sender, EventArgs e)
        {
            TrackBar track = (TrackBar)sender;
            toolTip.SetToolTip(track, track.Value.ToString());
            ITypeDescriptorContext ctx = (ITypeDescriptorContext)track.Tag;
            if (ctx != null)
            {
                switch (ctx.PropertyDescriptor.GetValue(ctx.Instance))
                {
                    case byte v:
                        ctx.PropertyDescriptor.SetValue(ctx.Instance, (byte)track.Value);
                        break;
                    case sbyte v:
                        ctx.PropertyDescriptor.SetValue(ctx.Instance, (sbyte)track.Value);
                        break;
                    case short v:
                        ctx.PropertyDescriptor.SetValue(ctx.Instance, (short)track.Value);
                        break;
                    case ushort v:
                        ctx.PropertyDescriptor.SetValue(ctx.Instance, (ushort)track.Value);
                        break;
                    default:
                        ctx.PropertyDescriptor.SetValue(ctx.Instance, track.Value);
                        break;
                }
            }

        }
    }
}
