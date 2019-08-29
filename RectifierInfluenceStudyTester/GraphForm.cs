using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using RectifierInfluenceStudy;
using Microsoft.Office.Interop.Excel;
using ChartAreaForm = System.Windows.Forms.DataVisualization.Charting.ChartArea;
using SeriesForm = System.Windows.Forms.DataVisualization.Charting.Series;
using FontForm = System.Drawing.Font;
using ExcelApplication = Microsoft.Office.Interop.Excel.Application;
using ExcelChart = Microsoft.Office.Interop.Excel.Chart;
using ExcelAxis = Microsoft.Office.Interop.Excel.Axis;

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
        private int SelectedIndex => ListFiles.SelectedIndices.Count == 0 ? -1 : ListFiles.SelectedIndices[0];
        private string SelectedFullName => _Sets[SelectedIndex].FullFileName;
        private int _OldIndex = -1;
        private ExcelApplication _Excel = new ExcelApplication();

        public GraphForm()
        {
            InitializeComponent();
            Chart.Series.Clear();
            _Excel.Visible = false;
            _Excel.DisplayAlerts = false;
        }

        public void UpdateSets()
        {
            _Sets = new List<RISDataSet>();
            _Offset = new Dictionary<string, double>();
            _Approved = new Dictionary<string, bool>();
            ListFiles.Items.Clear();
            string csv = "Latitude,Longitude,Name";
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
                csv += $"{set.Latitude},{set.Longitude},{set.FileName}\n";
                _Sets.Add(set);
                _Offset.Add(set.FullFileName, 0);
                _Approved.Add(set.FullFileName, false);
                ListFiles.Items.Add(set.FileName);
            }
            Clipboard.SetText(csv.Trim());
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
            ChartAreaForm area = Chart.ChartAreas[0];
            area.AxisX.Minimum = 0;
            area.AxisX.Maximum = Cycle.Length.TotalSeconds;
            if (!_AutoScale)
            {
                area.AxisY.Maximum = _Maximum;
                area.AxisY.Minimum = _Minimum;
            }
            area.AxisY.IsReversed = true;
            //ListFiles.SelectedIndices = 0;
        }

        private void ChangeToSet(int pIndex)
        {
            List<SeriesForm> series = CreateSeries(_Sets[pIndex]);
            Chart.Series.Clear();
            //TextOffset.Text = _Offset[_Sets[pIndex].FullFileName].ToString("F1");
            if (series == null) return;
            ChartAreaForm area = Chart.ChartAreas[0];
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
            else
            {
                area.RecalculateAxesScale();
                area.AxisX.Minimum = 0;
                area.AxisX.Maximum = Cycle.Length.TotalSeconds;
            }
            foreach (SeriesForm s in series)
            {
                Chart.Series.Add(s);
            }
        }

        private List<SeriesForm> CreateSeries(RISDataSet pSet)
        {
            List<SeriesForm> collection = new List<SeriesForm>();
            pSet.Offset = _Offset[pSet.FullFileName];
            pSet.CreateGraphReads();
            MultiSetInterruptionCycle cycle = Cycle as MultiSetInterruptionCycle;
            foreach (RectifierSet set in cycle.Sets)
            {
                SeriesForm s = new SeriesForm(set.Name);
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
            foreach (SeriesForm s in collection)
                s.Sort(PointSortOrder.Ascending, "X");
            return collection;
        }

        private void ListFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedIndex == -1)
                return;
            ChangeToSet(SelectedIndex);
            TextOffset.Text = _Offset[SelectedFullName].ToString("F1");
        }

        private void TextOffset_TextChanged(object sender, EventArgs e)
        {
            double offset;
            string text = TextOffset.Text;
            if (string.IsNullOrWhiteSpace(text) || !double.TryParse(text, out offset))
                return;
            if (_Offset[SelectedFullName] == offset)
                return;
            _Offset[SelectedFullName] = offset;
            ChangeToSet(SelectedIndex);
        }

        private void CheckApproved_CheckedChanged(object sender, EventArgs e)
        {
            bool approved = CheckApproved.Checked;
            if (approved == _Approved[SelectedFullName])
                return;
            _Approved[SelectedFullName] = approved;
            ListFiles.Items[SelectedIndex].BackColor = approved ? Color.LightGreen : Color.White;
            ListFiles.Items[SelectedIndex].Selected = true;
        }

        private void ListFiles_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (SelectedIndex == -1)
                return;
            ChangeToSet(SelectedIndex);
            TextOffset.Text = _Offset[SelectedFullName].ToString("F1");
            FontForm font = new FontForm(ListFiles.Items[SelectedIndex].Font, FontStyle.Bold);
            if (_OldIndex != -1)
                ListFiles.Items[_OldIndex].Font = ListFiles.Items[SelectedIndex].Font;
            ListFiles.Items[SelectedIndex].Font = font;
            CheckApproved.Checked = _Approved[SelectedFullName];
            _OldIndex = SelectedIndex;
        }

        private void GraphForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'a')
                CheckApproved.Checked = !CheckApproved.Checked;
            if (e.KeyChar == 'v')
            {
                double offset;
                if (!double.TryParse(Clipboard.GetText(), out offset)) return;
                TextOffset.Text = offset + "";
            }
        }

        private void ButtonExportOne_Click(object sender, EventArgs e)
        {
            RISDataSet set = _Sets[SelectedIndex];
            ExcelExport(set);
            MessageBox.Show("Done!");
        }

        private void ExcelExport(RISDataSet pSet)
        {
            string excelFileName = Path.ChangeExtension(pSet.FullFileName, ".xlsx");
            string pdfFileName = Path.ChangeExtension(pSet.FullFileName, ".pdf");
            Workbook workbook = _Excel.Workbooks.Add(Type.Missing);
            Worksheet worksheet = workbook.ActiveSheet as Worksheet;

            worksheet.Name = "Data";

            List<SeriesForm> series = CreateSeries(pSet);
            object[,] data = new object[series[0].Points.Count + 1, series.Count + 1];
            int cycleNum = 1;
            double lastX = double.MinValue;
            int row = 1;
            int pointIndex = 0;
            int columns = series.Count + 1;

            data[0, 0] = "Time (Seconds)";
            for (int i = 0; i < series.Count; ++i)
            {
                data[0, 1 + i] = series[i].Name;
            }

            foreach (DataPoint point in series[0].Points)
            {
                if (cycleNum < series.Count && series[cycleNum].Points.Count == pointIndex)
                {
                    ++cycleNum;
                    pointIndex = 0;
                }
                if (cycleNum < series.Count && series[cycleNum].Points.Count > pointIndex && point.XValue == series[cycleNum].Points[pointIndex].XValue)
                {
                    data[row, cycleNum + 1] = series[cycleNum].Points[pointIndex].YValues[0];
                    //worksheet.Cells[row, cycleNum + 2] = series[cycleNum].Points[pointIndex].YValues[0];
                    ++pointIndex;
                }
                if (point.XValue < lastX)
                    continue;
                data[row, 0] = point.XValue;
                data[row, 1] = point.YValues[0];
                //worksheet.Cells[row, 1] = point.XValue;
                //worksheet.Cells[row, 2] = point.YValues[0];
                ++row;
                lastX = point.XValue;
            }
            Range dataRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[row + 1, series.Count + 1]];
            dataRange.Value = data;
            Range chartRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[row + 1, series.Count + 1]];
            ExcelChart chart = workbook.Charts.Add(Type.Missing) as ExcelChart;
            chart.Name = "Chart";
            chart.ChartType = XlChartType.xlXYScatterLinesNoMarkers;
            chart.ChartStyle = 227;
            chart.SetSourceData(chartRange);
            chart.HasTitle = true;
            chart.ChartTitle.Text = pSet.FileName;
            chart.ChartArea.Border.LineStyle = XlLineStyle.xlLineStyleNone;
            chart.Legend.Position = XlLegendPosition.xlLegendPositionTop;

            ExcelAxis yAxis = chart.Axes(XlAxisType.xlValue);
            yAxis.ReversePlotOrder = true;
            yAxis.HasMinorGridlines = true;
            yAxis.MinorTickMark = XlTickMark.xlTickMarkCross;
            yAxis.HasTitle = true;
            yAxis.AxisTitle.Text = (pSet.FileName.ToLower().Contains("span") ? "Current Span" : "Pipe-to-Soil Potenital") + " (Volts)";
            if (pSet.MaxValueData < 0)
                yAxis.MaximumScale = 0;

            ExcelAxis xAxis = chart.Axes(XlAxisType.xlCategory);
            xAxis.MajorUnitIsAuto = false;
            xAxis.MajorUnit = 20;
            xAxis.TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionLow;
            xAxis.MaximumScaleIsAuto = false;
            xAxis.MaximumScale = Cycle.Length.TotalSeconds;
            xAxis.HasMajorGridlines = true;
            xAxis.HasTitle = true;
            xAxis.AxisTitle.Text = "Time (Seconds)";

            chart.PageSetup.RightFooter = $"{pSet.TimeStart.ToShortDateString()} {pSet.TimeStart.ToLongTimeString()}\n";
            chart.PageSetup.RightFooter += $"GPS: {pSet.Latitude.ToString("F8")},{pSet.Longitude.ToString("F8")},{pSet.Altitude}";
            chart.PageSetup.TopMargin = 0.1;

            workbook.SaveAs(excelFileName);
            chart.ExportAsFixedFormat(XlFixedFormatType.xlTypePDF, pdfFileName, XlFixedFormatQuality.xlQualityStandard, true, false);
            workbook.Close();
            worksheet = null;
            workbook = null;
        }

        private void ButtonExportApproved_Click(object sender, EventArgs e)
        {
            int count = 0;
            StringBuilder output = new StringBuilder();
            output.AppendLine("");
            foreach (RISDataSet set in _Sets)
                if (_Approved[set.FullFileName])
                {
                    ExcelExport(set);
                    ++count;
                }
            MessageBox.Show($"Done exporting {count} files!");
        }
    }
}
