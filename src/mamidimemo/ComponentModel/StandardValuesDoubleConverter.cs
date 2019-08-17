// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace zanac.MAmidiMEmo.ComponentModel
{
    public class StandardValuesDoubleConverter : DoubleConverter
    {
        public override Boolean GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override Boolean GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            IStandardValues sv = context.Instance as IStandardValues;
            if (sv != null)
                return new StandardValuesCollection(sv.GetStandardValues(context.PropertyDescriptor.Name));
            else
                return null;
        }

    }
}
