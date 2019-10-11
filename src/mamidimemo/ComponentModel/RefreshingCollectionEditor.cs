// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace zanac.MAmidiMEmo.ComponentModel
{
    public class RefreshingCollectionEditor : CollectionEditor
    {
        protected PropertyGrid ownerGrid;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public RefreshingCollectionEditor(Type type) : base(type)
        {
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            PropertyInfo ownerGridProperty = provider.GetType().GetProperty("OwnerGrid", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            ownerGrid = (PropertyGrid)ownerGridProperty.GetValue(provider);

            return base.EditValue(context, provider, value);
        }

        protected override CollectionForm CreateCollectionForm()
        {
            CollectionForm cf = base.CreateCollectionForm();
            cf.FormClosing += delegate (object sender, FormClosingEventArgs e)
            {
                ownerGrid.Refresh();
            };

            return cf;
        }
    }
}
