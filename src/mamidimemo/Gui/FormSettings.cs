using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();
        }


        private static string[] SoundTypes = new string[] { "auto", "dsound", "xaudio2", "portaudio" };

        private static string[] AudioLatency = new string[] { "1", "2", "3", "4" };

        private void buttonOk_Click(object sender, EventArgs e)
        {
            try
            {
                using (var t = File.CreateText(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "mame.ini")))
                {
                    t.WriteLine("sound " + SoundTypes[comboBoxSoundType.SelectedIndex]);
                    t.WriteLine("samplerate " + comboBoxSampleRate.Text);
                    t.WriteLine("audio_latency " + AudioLatency[comboBoxAudioLatency.SelectedIndex]);
                    t.WriteLine("volume 0");
                    t.WriteLine("pa_api " + textBoxPaApi.Text);
                    t.WriteLine("pa_device " + textBoxPaDevice.Text);
                    t.WriteLine("pa_latency " + textBoxPaLatency.Text);
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;

                MessageBox.Show(ex.ToString());
                return;
            }
            DialogResult = DialogResult.OK;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int MessageBeep(uint n);

        private void comboBoxText_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(((ComboBox)sender).Text))
            {
                MessageBeep(0);
                e.Cancel = true;
            }
        }

        private void textBox_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(((TextBox)sender).Text))
            {
                MessageBeep(0);
                e.Cancel = true;
            }
        }
    }
}
