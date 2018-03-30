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
        public Form1()
        {
            InitializeComponent();
            string[] files = Directory.GetFiles(@"C:\Users\KevinC.ACCURATECORROSI\Desktop\3510\RIS\Phase 1\SET 2\", "*.csv");
            List<RISDataSet> sets = new List<RISDataSet>();
            RISDataSet set;
            double min = double.MaxValue;
            double max = double.MinValue;
            foreach(string file in files)
            {
                set = new RISDataSet(file);
                if (set.MinValue < min)
                    min = set.MinValue;
                if (set.MaxValue > max)
                    max = set.MaxValue;
                sets.Add(set);
            }
        }
    }
}