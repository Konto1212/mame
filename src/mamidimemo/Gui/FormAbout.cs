using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.ComponentModel;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();

            labelVer.Text = string.Format(labelVer.Text, Program.FILE_VERSION);

            ImageUtility.AdjustControlImagesDpiScale(this);
        }

    }
}
