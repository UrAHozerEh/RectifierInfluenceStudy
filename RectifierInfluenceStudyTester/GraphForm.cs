using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using RectifierInfluenceStudy;

namespace RectifierInfluenceStudyTester
{
    public partial class GraphForm : Form
    {
        public string Folder;
        public string[] Files;
        private Dictionary<string, double> _Offset;
        private List<RISDataSet> _Sets;
        public InterruptionCycle Cycle;

        public GraphForm()
        {
            InitializeComponent();
            Chart.Series.Clear();
        }

        public void UpdateSets()
        {
            _Sets = new List<RISDataSet>();
            _Offset = new Dictionary<string, double>();
            ListFiles.Items.Clear();
            foreach (string file in Files)
            {
                RISDataSet set = new RISDataSet(file, Cycle);
                _Sets.Add(set);
                _Offset.Add(set.FullFileName, 0);
                ListFiles.Items.Add(set.FileName);
            }
            ChartArea area = Chart.ChartAreas[0];
            area.AxisX.Minimum = 0;
            area.AxisX.Maximum = Cycle.Length.TotalSeconds;
            area.AxisY.IsReversed = true;
            ListFiles.SelectedIndex = 0;
        }

        private void ChangeToSet(int pIndex)
        {
            List<Series> series = CreateSeries(_Sets[pIndex]);
            Chart.Series.Clear();
            //TextOffset.Text = _Offset[_Sets[pIndex].FullFileName].ToString("F1");
            if (series == null) return;
            foreach (Series s in series)
            {
                Chart.Series.Add(s);
            }
        }

        private List<Series> CreateSeries(RISDataSet pSet)
        {
            List<Series> collection = new List<Series>();
            pSet.Offset = _Offset[pSet.FullFileName];
            pSet.CreateGraphReads();
            MultiSetInterruptionCycle cycle = Cycle as MultiSetInterruptionCycle;
            foreach (RectifierSet set in cycle.Sets)
            {
                Series s = new Series(set.Name);
                s.ChartType = SeriesChartType.Line;
                if (set.Name != "All On")
                    s.BorderWidth = 3;
                else
                    s.BorderWidth = 2;
                collection.Add(s);
            }
            int cycleNum = 1;
            if (pSet.GraphReads[0].Count == 0)
                return null;
            double lastTime = pSet.GraphReads[0][0].Time;
            foreach (GraphRead read in pSet.GraphReads[0])
            {
                if (read.Time - lastTime > 1 && cycleNum < pSet.GraphReads.Keys.Count)
                {
                    foreach (GraphRead read2 in pSet.GraphReads[cycleNum])
                    {
                        collection[cycleNum].Points.AddXY(read2.Time, read2.Value);
                        collection[0].Points.AddXY(read2.Time, read2.Value);
                    }
                    ++cycleNum;
                }
                lastTime = read.Time;
                collection[0].Points.AddXY(read.Time, read.Value);

            }
            if (cycleNum < pSet.GraphReads.Keys.Count)
            {
                foreach (GraphRead read2 in pSet.GraphReads[cycleNum])
                {
                    collection[cycleNum].Points.AddXY(read2.Time, read2.Value);
                    collection[0].Points.AddXY(read2.Time, read2.Value);
                }
            }
            foreach (Series s in collection)
                s.Sort(PointSortOrder.Ascending, "X");
            return collection;
        }

        private void ListFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = ListFiles.SelectedIndex;
            ChangeToSet(index);
            TextOffset.Text = _Offset[_Sets[index].FullFileName].ToString("F1");
        }

        private void TextOffset_TextChanged(object sender, EventArgs e)
        {
            double offset;
            string text = TextOffset.Text;
            int index = ListFiles.SelectedIndex;
            if (string.IsNullOrWhiteSpace(text) || !double.TryParse(text, out offset))
                return;
            if (_Offset[_Sets[index].FullFileName] == offset)
                return;
            _Offset[_Sets[index].FullFileName] = offset;
            ChangeToSet(index);
        }
    }
}
