using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ActivityTracker
{
    public partial class FormHelp : Form
    {
        public FormHelp()
        {
            InitializeComponent();
        }

        private string dataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ActivityTracker";
        private void FormHelp_Load(object sender, EventArgs e)
        {
            

            labelPathShow.Text = "Data is saved as soon as you change tab or when you quit the App\n\nData is currently saved under \"" + dataPath + "\"";
            labelPathShow.LinkArea = new LinkArea(labelPathShow.Text.Length - dataPath.Length - 1, dataPath.Length);
        }

        private void labelPathShow_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(dataPath);
            MessageBox.Show("Path copied to your clipboard", "Activity Tracker" + " - Confirmation", MessageBoxButtons.OK);
        }
    }
}
