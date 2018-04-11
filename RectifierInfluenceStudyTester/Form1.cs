using RectifierInfluenceStudy;
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
    public partial class Form1 : Form
    {
        private RISDesktopGraph mGraph;
        private int mCurrentSet;
        private List<RISDataSet> mSets;

        public Form1()
        {
            InitializeComponent();
            string[] cycleNames = {"TEG Unit #10","TEG Unit #9","TEG Unit #8","TEG Unit #7","Pole Rectifier",
                    "TEG Unit #6","TEG Unit #5","TEG Unit #4","TEG Unit #3","TEG Unit #2","TEG Unit #1","MP 156.13","MP 153.5","MP 148.5"};
            InterruptionCycle cycle = new MultiSetInterruptionCycle("Testing Set 1", 17, 5, 2, cycleNames);
            string[] files = Directory.GetFiles(@"C:\Users\KevinC.ACCURATECORROSI\Desktop\3510\RIS\Phase 1\SET 2\", "*.csv");
            //string[] files = Directory.GetFiles(@"C:\Users\kcron\Desktop\RIS\Phase 1\SET 2\", "*.csv");
            mSets = new List<RISDataSet>();
            RISDataSet set;
            double min = double.MaxValue;
            double max = double.MinValue;
            Read read;
            DateTime start;
            mCurrentSet = 0;
            foreach (string file in files)
            {
                set = new RISDataSet(file, cycle);
                read = set.DataReads[0];
                start = cycle.GetNextCycleStart(read.UTCTime);
                if (set.MinValue < min)
                    min = set.MinValue;
                if (set.MaxValue > max)
                    max = set.MaxValue;
                mSets.Add(set);
            }

            mGraph = new RISDesktopGraph(mSets[mCurrentSet]);
            mGraph.Location = new Point(5, 5);
            mGraph.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top);
            mGraph.Size = new Size(300, 150);
            Controls.Add(mGraph);
        }

        private void NextClick(object sender, EventArgs e)
        {
            ++mCurrentSet;
            if (mCurrentSet == mSets.Count)
                mCurrentSet = 0;
            mGraph.DataSet = mSets[mCurrentSet];
            mGraph.Invalidate();
        }

        private void PreviousClick(object sender, EventArgs e)
        {
            --mCurrentSet;
            if (mCurrentSet == -1)
                mCurrentSet = mSets.Count - 1;
            mGraph.DataSet = mSets[mCurrentSet];
            mGraph.Invalidate();
        }
    }
}