using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Instruments;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormProp : Form
    {
        private List<InstrumentBase> instruments;

        /// <summary>
        /// 
        /// </summary>
        public FormProp(InstrumentBase[] insts)
        {
            InitializeComponent();

            instruments = new List<InstrumentBase>(insts);

            propertyGrid.SelectedObjects = instruments.ToArray();

            setTitle();

            InstrumentManager.InstrumentChanged += InstrumentManager_InstrumentChanged;
            InstrumentManager.InstrumentRemoved += InstrumentManager_InstrumentRemoved;
        }

        private void setTitle()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var i in instruments)
            {
                if (sb.Length != 0)
                    sb.Append(", ");
                sb.Append(i.Name + "(" + i.UnitNumber + ")");
            }
            this.Text = sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstrumentManager_InstrumentRemoved(object sender, EventArgs e)
        {
            List<InstrumentBase> insts = new List<InstrumentBase>();
            foreach (var inst in InstrumentManager.GetAllInstruments())
            {
                foreach (var i in instruments)
                {
                    if (i.DeviceID == inst.DeviceID && i.UnitNumber == inst.UnitNumber)
                        insts.Add(inst);
                }
            }

            instruments = insts;
            propertyGrid.SelectedObjects = insts.ToArray();
            setTitle();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstrumentManager_InstrumentChanged(object sender, EventArgs e)
        {
            List<InstrumentBase> insts = new List<InstrumentBase>();
            foreach (var inst in InstrumentManager.GetAllInstruments())
            {
                foreach (var i in instruments)
                {
                    if (i.DeviceID == inst.DeviceID && i.UnitNumber == inst.UnitNumber)
                        insts.Add(inst);
                }
            }

            instruments = insts;
            propertyGrid.SelectedObjects = insts.ToArray();
            setTitle();
        }

        private void toolStripButtonCat_Click(object sender, EventArgs e)
        {
            propertyGrid.PropertySort = PropertySort.Categorized;
            toolStripButtonCat.Checked = true;
            toolStripButtonA2Z.Checked = false;
        }

        private void toolStripButtonA2Z_Click(object sender, EventArgs e)
        {
            propertyGrid.PropertySort = PropertySort.Alphabetical;
            toolStripButtonCat.Checked = false;
            toolStripButtonA2Z.Checked = true;
        }

        private void toolStripButtonPopup_Click(object sender, EventArgs e)
        {
            FormProp fp = new FormProp(instruments.ToArray());
            fp.Show(this);
        }

        private void contextMenuStripProp_Click(object sender, EventArgs e)
        {
            propertyGrid.ResetSelectedProperty();
            propertyGrid.Refresh();
        }
    }
}
