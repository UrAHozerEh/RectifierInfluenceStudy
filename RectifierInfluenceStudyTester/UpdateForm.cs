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
using System.IO;

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
                names[i - 1] = "Cycle " + i + " of " + cycles;
            }
            if (names.Length == 14)
            {
                names[0] = "Evan Hewes";
                names[1] = "Dunaway";
                names[2] = "Foxglove";
                names[3] = "Derrick";
                names[4] = "Sillsbee";
                names[5] = "Ross";
                names[6] = "Nichols";
                names[7] = "Villa";
                names[8] = "Sly Rec 2";
            }

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

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "*.txt | *.txt";
            dialog.Multiselect = false;
            DialogResult result = dialog.ShowDialog();
            if (result != DialogResult.Cancel)
            {
                double on, off, delay;
                string[] names;
                InterruptionCycle cycle = null;
                List<string> fileLines = new List<string>();
                using (StreamReader sr = new StreamReader(dialog.OpenFile()))
                {
                    while(!sr.EndOfStream)
                    {
                        fileLines.Add(sr.ReadLine());
                    }
                    on = double.Parse(fileLines[4]);
                    off = double.Parse(fileLines[5]);
                    delay = double.Parse(fileLines[6]);
                    names = fileLines[3].Split(',');
                    cycle = new MultiSetInterruptionCycle("", on, off, delay, names);
                }

                if (cycle == null)
                    return;
                _Form.Cycle = cycle;
                if (string.IsNullOrWhiteSpace(txtFolder.Text))
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
        }
    }
}
