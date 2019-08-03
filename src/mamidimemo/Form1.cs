using Melanchall.DryWetMidi.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.mamidimemo.instruments;
using zanac.mamidimemo.mame;

namespace zanac.mamidimemo
{
    public partial class Form1 : Form
    {
        private YM2612 ym2612 = new YM2612(0);
        private GBAPU gbapu = new GBAPU(0);


        public Form1()
        {
            InitializeComponent();
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            try
            {
                foreach (var dev in InputDevice.GetAll())
                {
                    comboBox1.Items.Add(dev.Name);
                    dev.Dispose();
                }
            }
            catch (Exception ex)
            {
                if (ex is Exception)
                    return;
                if (ex is SystemException)
                    return;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            var dev = InputDevice.GetByName(comboBox1.Text);
            if (dev != null)
            {
                dev.EventReceived += Dev_EventReceived;
                dev.StartEventsListening();
            }
        }

        private void Dev_EventReceived(object sender, MidiEventReceivedEventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(() =>
            {
                ym2612.NotifyMidiEvent(e.Event);
                gbapu.NotifyMidiEvent(e.Event);
            }));
        }
    }
}
