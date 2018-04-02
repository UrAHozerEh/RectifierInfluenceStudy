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
        public Form1()
        {
            InitializeComponent();
            string[] cycleNames = {"TEG Unit #10","TEG Unit #9","TEG Unit #8","TEG Unit #7","Pole Rectifier",
                    "TEG Unit #6","TEG Unit #5","TEG Unit #4","TEG Unit #3","TEG Unit #2","TEG Unit #1","MP 156.13","MP 153.5","MP 148.5"};
            InterruptionCycle cycle = new MultiSetInterruptionCycle("Testing Set 1", 17, 5, 2, cycleNames);
            //string[] files = Directory.GetFiles(@"C:\Users\KevinC.ACCURATECORROSI\Desktop\3510\RIS\Phase 1\SET 2\", "*.csv");
            string[] files = Directory.GetFiles(@"C:\Users\kcron\Desktop\RIS\Phase 1\SET 2\", "*.csv");
            List<RISDataSet> sets = new List<RISDataSet>();
            RISDataSet set;
            double min = double.MaxValue;
            double max = double.MinValue;
            Read read;
            DateTime start;
            foreach (string file in files)
            {
                set = new RISDataSet(file, cycle);
                read = set.Reads[0];
                start = cycle.GetNextCycleStart(read.UTCTime);
                if (set.MinValue < min)
                    min = set.MinValue;
                if (set.MaxValue > max)
                    max = set.MaxValue;
                sets.Add(set);
            }

            RISDesktopGraph graph = new RISDesktopGraph(sets[0].GraphReads);
            graph.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top);
            graph.Size = new Size(300, 150);
            Controls.Add(graph);
        }
    }
}