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
        private Dictionary<string, bool> _Approved;
        private List<RISDataSet> _Sets;
        public InterruptionCycle Cycle;
        private double _Minimum = double.MaxValue;
        private double _Maximum = double.MinValue;
        private double _SpanMin = double.MaxValue;
        private double _SpanMax = double.MinValue;
        private bool _AutoScale = true;

        public GraphForm()
        {
            InitializeComponent();
            Chart.Series.Clear();
        }

        public void UpdateSets()
        {
            _Sets = new List<RISDataSet>();
            _Offset = new Dictionary<string, double>();
            _Approved = new Dictionary<string, bool>();
            ListFiles.Items.Clear();
            foreach (string file in Files)
            {
                RISDataSet set = new RISDataSet(file, Cycle);
                if (set.FileName.ToLower().Contains("span"))
                {
                    if (set.MinValueData < _SpanMin)
                        _SpanMin = set.MinValueData;
                    if (set.MaxValueData > _SpanMax)
                        _SpanMax = set.MaxValueData;
                }
                else
                {
                    if (set.MinValueData < _Minimum)
                        _Minimum = set.MinValueData;
                    if (set.MaxValueData > _Maximum)
                        _Maximum = set.MaxValueData;
                }
                _Sets.Add(set);
                _Offset.Add(set.FullFileName, 0);
                _Approved.Add(set.FullFileName, false);
                ListFiles.Items.Add(set.FileName);
            }
            if (_Maximum < 0)
                _Maximum = 0;
            double range = Math.Abs(_Minimum - _Maximum);
            _Minimum = _Minimum - range * 0.1;
            if (_Maximum != 0)
                _Maximum = _Maximum + range * 0.1;

            range = Math.Abs(_SpanMin - _SpanMax);
            _SpanMin = _SpanMin - range * 0.1;
            _SpanMax = _SpanMax + range * 0.1;
            if (Math.Abs(_SpanMin) > Math.Abs(_SpanMax))
                _SpanMax = 0 - _SpanMin;
            else
                _SpanMin = 0 - _SpanMax;
            ChartArea area = Chart.ChartAreas[0];
            area.AxisX.Minimum = 0;
            area.AxisX.Maximum = Cycle.Length.TotalSeconds;
            if (!_AutoScale)
            {
                area.AxisY.Maximum = _Maximum;
                area.AxisY.Minimum = _Minimum;
            }
            area.AxisY.IsReversed = true;
            ListFiles.SelectedIndex = 0;
        }

        private void ChangeToSet(int pIndex)
        {
            List<Series> series = CreateSeries(_Sets[pIndex]);
            Chart.Series.Clear();
            //TextOffset.Text = _Offset[_Sets[pIndex].FullFileName].ToString("F1");
            if (series == null) return;
            ChartArea area = Chart.ChartAreas[0];
            if (!_AutoScale)
            {
                if (_Sets[pIndex].FileName.ToLower().Contains("span"))
                {
                    area.AxisY.Maximum = _SpanMax;
                    area.AxisY.Minimum = _SpanMin;
                }
                else
                {
                    area.AxisY.Maximum = _Maximum;
                    area.AxisY.Minimum = _Minimum;
                }
            }
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
