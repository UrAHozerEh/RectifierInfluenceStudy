﻿using RectifierInfluenceStudy;
using RectifierInfluenceStudy.DesktopGraphing;
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
    public partial class Form1 : Form, IGraphWatcher
    {
        private RISDesktopGraph mGraph;
        private int mCurrentSet;
        private List<RISDataSet> mSets;
        public InterruptionCycle Cycle;
        public string Folder;
        public string[] Files;
        public bool Subfolders = false;

        public Form1()
        {
            InitializeComponent();
            //string[] cycleNames = {"TEG Unit #10","TEG Unit #9","TEG Unit #8","TEG Unit #7","Pole Rectifier",
            //        "TEG Unit #6","TEG Unit #5","TEG Unit #4","TEG Unit #3","TEG Unit #2","TEG Unit #1","MP 156.13","MP 153.5","MP 148.5"};
            //string[] cycleNames = new string[23];
            //for (int i = 1; i <= 23; ++i)
            //    cycleNames[i - 1] = "Cycle " + i;
            //InterruptionCycle cycle = new MultiSetInterruptionCycle("Testing Set 1", 14, 5, 2, cycleNames);
            mGraph = new RISDesktopGraph(null, this);
            mGraph.Location = new Point(13, 13);
            mGraph.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top);
            mGraph.Size = new Size(533, 292);
            Controls.Add(mGraph);
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
            foreach (string file in files)
            {

                set = new RISDataSet(file, Cycle);
                read = set.DataReads[0];
                start = Cycle.GetNextCycleStart(read.UTCTime);
                if (set.MinValueData < min)
                    min = set.MinValueData;
                if (set.MaxValueData > max)
                    max = set.MaxValueData;
                set.GetPaths();
                output += set.FileName + "," + set.Output + "\n";
                mSets.Add(set);
            }
            txtCycle.Text = mSets[mCurrentSet].FileName;
            mGraph.Graph = new RISGraph(mSets[mCurrentSet]);
            mGraph.Invalidate();
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
            mGraph.Graph = new RISGraph(mSets[mCurrentSet]);
            mGraph.Invalidate();
            txtCycle.Text = mSets[mCurrentSet].FileName;
        }

        private void PreviousClick(object sender, EventArgs e)
        {
            if (mSets.Count == 0)
                return;
            --mCurrentSet;
            if (mCurrentSet == -1)
                mCurrentSet = mSets.Count - 1;
            mGraph.Graph = new RISGraph(mSets[mCurrentSet]);
            mGraph.Invalidate();
            txtCycle.Text = mSets[mCurrentSet].FileName;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            UpdateForm update = new UpdateForm(this, Files, Folder, Cycle);
            update.ShowDialog(this);
        }
    }
}