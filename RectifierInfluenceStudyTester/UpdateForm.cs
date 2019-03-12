using System;
using RectifierInfluenceStudy;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RectifierInfluenceStudyTester
{
    public partial class UpdateForm : Form
    {
        private Form1 _Form;
        private string[] _Files;

        public UpdateForm(Form1 pForm, string[] pFiles, string pFolder, InterruptionCycle pCycle)
        {
            InitializeComponent();
            _Form = pForm;
            _Files = pFiles;
            txtFolder.Text = pFolder;
            chkSubfolders.Checked = _Form.Subfolders;
            if (_Files != null)
            {
                if (_Files.Length == 1)
                    txtFiles.Text = _Files[0].Split('\\').Last();
                else
                    txtFiles.Text = $"Selected {_Files.Length} files.";
            }
            if (pCycle == null)
                return;
            MultiSetInterruptionCycle multi = pCycle as MultiSetInterruptionCycle;
            txtOn.Text = multi.On + "";
            txtOff.Text = multi.Off + "";
            txtDelay.Text = multi.Delay + "";
            txtCycles.Text = multi.NumSets + "";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            double on, off, delay;
            int cycles;
            if (!double.TryParse(txtOn.Text, out on)) return;
            if (!double.TryParse(txtOff.Text, out off)) return;
            if (!double.TryParse(txtDelay.Text, out delay)) return;
            if (!int.TryParse(txtCycles.Text, out cycles)) return;
            string[] names = new string[cycles];
            for(int i = 1; i <= cycles; ++i)
            {
                names[i - 1] = "Cycle " + i;
            }

            names[0] = "TW Rainbow Valley Off";
            names[1] = "TW Gila River MS Off";
            names[2] = "TW Gila River Off";
            names[3] = "TW Arlington MS Off";
            names[4] = "EPNG-1433 Off";
            names[5] = "EPNG-1190 Off";
            names[6] = "EPNG-1316 Off";
            names[7] = "EPNG-1471 Off";
            names[8] = "EPNG-1129 Off";
            names[9] = "EPNG-2036 Off";
            names[10] = "EPNG-240 Off";
            names[11] = "EPNG-1924 Off";
            names[12] = "EPNG-1579 Off";
            names[13] = "EPNG-1014 Off";
            names[14] = "EPNG-2069 Off";
            names[15] = "EPNG-1015 Off";
            names[16] = "EPNG-1974 Off";
            names[17] = "EPNG-1435 Off";
            names[18] = "EPNG-1971 Off";
            names[19] = "Unused";
            names[20] = "SWG Off";
            names[21] = "EXEL Off";
            names[22] = "Other Off";

            InterruptionCycle cycle = new MultiSetInterruptionCycle("", on, off, delay, names);
            _Form.Cycle = cycle;
            if(string.IsNullOrWhiteSpace(txtFolder.Text))
            {
                _Form.Folder = "";
                _Form.Files = _Files;
            }
            else
            {
                _Form.Folder = txtFolder.Text;
                _Form.Files = null;
            }
            _Form.Subfolders = chkSubfolders.Checked;
            _Form.UpdateGraphs();
            Close();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            if(result != DialogResult.Cancel)
            {
                txtFolder.Text = dialog.SelectedPath;
                _Files = null;
            }
        }

        private void TextChangedValue(object sender, EventArgs e)
        {
            double on, off, delay;
            int cycles;
            if (string.IsNullOrWhiteSpace(txtOn.Text) || !double.TryParse(txtOn.Text, out on))
            {
                txtLength.Text = "";
                return;
            }
            if (string.IsNullOrWhiteSpace(txtOff.Text) || !double.TryParse(txtOff.Text, out off))
            {
                txtLength.Text = "";
                return;
            }
            if (string.IsNullOrWhiteSpace(txtDelay.Text) || !double.TryParse(txtDelay.Text, out delay))
            {
                txtLength.Text = "";
                return;
            }
            if (string.IsNullOrWhiteSpace(txtCycles.Text) || !int.TryParse(txtCycles.Text, out cycles))
            {
                txtLength.Text = "";
                return;
            }
            double length = (on + off + (off + delay) * cycles);
            txtLength.Text = length + "";
        }

        private void btnFileBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "*.csv | *.csv";
            dialog.Multiselect = true;
            DialogResult result = dialog.ShowDialog();
            if(result != DialogResult.Cancel)
            {
                txtFolder.Text = "";
                _Files = dialog.FileNames;
                if (_Files.Length == 1)
                    txtFiles.Text = _Files[0].Split('\\').Last();
                else
                    txtFiles.Text = $"Selected {_Files.Length} files.";
            }
        }
    }
}
