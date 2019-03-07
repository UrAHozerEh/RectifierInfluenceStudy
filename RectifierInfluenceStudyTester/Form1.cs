using RectifierInfluenceStudy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RectifierInfluenceStudyTester
{
    public partial class Form1 : Form
    {
        private int mCurrentSet;
        private List<RISDataSet> mSets;
        public InterruptionCycle Cycle;
        public string Folder;
        public string[] Files;
        public bool Subfolders = false;
        private Dictionary<string, double> _Offset;

        public Form1()
        {
            InitializeComponent();
            //string[] cycleNames = {"TEG Unit #10","TEG Unit #9","TEG Unit #8","TEG Unit #7","Pole Rectifier",
            //        "TEG Unit #6","TEG Unit #5","TEG Unit #4","TEG Unit #3","TEG Unit #2","TEG Unit #1","MP 156.13","MP 153.5","MP 148.5"};
            //string[] cycleNames = new string[23];
            //for (int i = 1; i <= 23; ++i)
            //    cycleNames[i - 1] = "Cycle " + i;
            //InterruptionCycle cycle = new MultiSetInterruptionCycle("Testing Set 1", 14, 5, 2, cycleNames);
            mSets = new List<RISDataSet>();
            
        }

        public void UpdateGraphs()
        {
            if (!Directory.Exists(Folder) && (Files == null || Files.Length == 0))
                return;
            if (Cycle == null)
                return;
            string[] files;
            if (Directory.Exists(Folder))
                files = Directory.GetFiles(Folder, "*.csv", Subfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            else
                files = Files;
            mSets = new List<RISDataSet>();
            RISDataSet set;
            double min = double.MaxValue;
            double max = double.MinValue;
            Read read;
            DateTime start;
            mCurrentSet = 0;
            string output = "";
            _Offset = new Dictionary<string, double>();
            foreach (string file in files)
            {
                set = new RISDataSet(file, Cycle);
                read = set.DataReads[0];
                start = Cycle.GetNextCycleStart(read.UTCTime);
                if (set.MinValueData < min)
                    min = set.MinValueData;
                if (set.MaxValueData > max)
                    max = set.MaxValueData;
                output += set.FileName + "," + set.Output + "\n";
                mSets.Add(set);
                _Offset.Add(file, 0);
            }
            txtCycle.Text = mSets[mCurrentSet].FileName;
            GraphForm graph = new GraphForm();
            graph.Cycle = Cycle;
            graph.Files = files;
            graph.UpdateSets();
            graph.Show();
        }

        public void UpdateValue(Read pRead, string pCycleName)
        {
            txtCycle.Text = mSets[mCurrentSet].FileName + "\r\n";
            txtCycle.Text += pCycleName;
        }

        private void NextClick(object sender, EventArgs e)
        {
            if (mSets.Count == 0)
                return;
            ++mCurrentSet;
            if (mCurrentSet == mSets.Count)
                mCurrentSet = 0;
            //mGraph.Graph = new RISGraph(mSets[mCurrentSet]);
            txtCycle.Text = mSets[mCurrentSet].FileName;
        }

        private void PreviousClick(object sender, EventArgs e)
        {
            if (mSets.Count == 0)
                return;
            --mCurrentSet;
            if (mCurrentSet == -1)
                mCurrentSet = mSets.Count - 1;
            //mGraph.Graph = new RISGraph(mSets[mCurrentSet]);
            txtCycle.Text = mSets[mCurrentSet].FileName;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            UpdateForm update = new UpdateForm(this, Files, Folder, Cycle);
            update.ShowDialog(this);
        }
    }
}