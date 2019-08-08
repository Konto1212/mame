using Melanchall.DryWetMidi.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.mamidimemo.instruments;
using zanac.mamidimemo.mame;
using zanac.mamidimemo.midi;
using zanac.mamidimemo.Properties;

namespace zanac.mamidimemo
{
    public partial class FormMain : Form
    {

        private static ListView outputListView;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        //[Conditional("DEBUG")]
        public static void OutputLog(String log)
        {
            outputListView?.BeginInvoke(new MethodInvoker(() =>
            {
                var item = outputListView.Items.Add(log);
                outputListView.EnsureVisible(item.Index);
            }), null);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FormMain()
        {
            InitializeComponent();
            imageList1.Images.Add("YM2612", ImageResource.YM2612);
            foreach (var dev in InputDevice.GetAll())
            {
                int idx = toolStripComboBoxMidiIf.Items.Add(dev.Name);
                if (string.Equals(dev.Name, Settings.Default.MidiIF))
                    toolStripComboBoxMidiIf.SelectedIndex = idx;
                dev.Dispose();
            }
            if (toolStripComboBoxMidiIf.Items.Count >= 0)
                toolStripComboBoxMidiIf.SelectedIndex = 0;
            outputListView = listView1;

            InstrumentManager.InstrumentAdded += InstrumentManager_InstrumentAdded;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstrumentManager_InstrumentAdded(object sender, EventArgs e)
        {
            listViewIntruments.Clear();
            foreach (var inst in InstrumentManager.List_ym2612)
            {
                var lvi = new ListViewItem(inst.Name, inst.ImageKey);
                var item = listViewIntruments.Items.Add(lvi);
                item.Tag = inst;
            }
            foreach (var inst in InstrumentManager.List_sn76496)
            {
                var lvi = new ListViewItem(inst.Name, inst.ImageKey);
                var item = listViewIntruments.Items.Add(lvi);
                item.Tag = inst;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripComboBox1_DropDown(object sender, EventArgs e)
        {
            toolStripComboBoxMidiIf.Items.Clear();
            try
            {
                foreach (var dev in MidiManager.GetInputMidiDevices())
                {
                    toolStripComboBoxMidiIf.Items.Add(dev.Name);
                    dev.Dispose();
                }
            }
            catch (Exception ex)
            {
                if (ex is Exception)
                    return;
                if (ex is SystemException)
                    return;

                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                MidiManager.SetInputMidiDevice(toolStripComboBoxMidiIf.Text);
            }
            catch (Exception ex)
            {
                if (ex is Exception)
                    return;
                if (ex is SystemException)
                    return;

                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewIntruments_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            List<InstrumentBase> insts = new List<InstrumentBase>();
            foreach (ListViewItem item in listViewIntruments.SelectedItems)
                insts.Add((InstrumentBase)item.Tag);
            propertyGrid.SelectedObjects = insts.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addYM2612ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.YM2612);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addSN76496ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.SN76496);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
