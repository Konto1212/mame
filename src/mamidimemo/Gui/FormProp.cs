using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
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
using zanac.MAmidiMEmo.Midi;

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

            toolStripComboBox1.SelectedIndex = 0;
            toolStripComboBox2.SelectedIndex = 0;

            InstrumentManager.InstrumentChanged += InstrumentManager_InstrumentChanged;
            InstrumentManager.InstrumentRemoved += InstrumentManager_InstrumentRemoved;

            pianoControl1.NoteOn += PianoControl1_NoteOn;
            pianoControl1.NoteOff += PianoControl1_NoteOff;
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
            fp.Show(Owner);
        }

        private void contextMenuStripProp_Click(object sender, EventArgs e)
        {
            propertyGrid.ResetSelectedProperty();
            propertyGrid.Refresh();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.W | Keys.Control))
            {
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var r = MessageBox.Show(this, "Are you sure you want to close " + Text + " ?", "Qeuestion", MessageBoxButtons.OKCancel);
            if (r == DialogResult.Cancel)
                e.Cancel = true;
            base.OnClosing(e);
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pianoControl1.SetMouseChannel(toolStripComboBox1.SelectedIndex);
            for (int i = 0; i < 16; i++)
                pianoControl1.SetReceiveChannel(i, false);
            pianoControl1.SetReceiveChannel(toolStripComboBox1.SelectedIndex, true);
        }

        private void PianoControl1_NoteOn(object sender, NoteOnEvent e)
        {
            if (toolStripComboBox2.SelectedIndex != 0)
            {
                //Program change
                var pe = new ProgramChangeEvent((SevenBitNumber)(toolStripComboBox2.SelectedIndex - 1));
                foreach (var i in instruments)
                    i.NotifyMidiEvent(pe);
            }
            foreach (var i in instruments)
                i.NotifyMidiEvent(e);
        }

        private void PianoControl1_NoteOff(object sender, NoteOffEvent e)
        {
            foreach (var i in instruments)
                i.NotifyMidiEvent(e);
        }
    }
}
