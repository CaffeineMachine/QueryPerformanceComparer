using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;

namespace QuerySessionSummaryControl
{
    /// <summary>
    /// Interaction logic for ReportSummary.xaml
    /// </summary>
    public partial class ReportSummary : UserControl
    {
        private Chart _chart;
        public ReportSummary()
        {
            InitializeComponent();
            _chart = new Chart();
        }

        public void GenerateChart()
        {
            _chart.ChartAreas.Add("chtArea");
            controlHost.Child = _chart;
            _chart.ChartAreas[0].AxisX.Title = "Runs";
            _chart.ChartAreas[0].AxisX.TitleFont = new Font(System.Drawing.FontFamily.GenericSansSerif, 12);
            _chart.ChartAreas[0].AxisY.Title = "Runtime (Milliseconds)";
            _chart.ChartAreas[0].AxisY.TitleFont = new Font(System.Drawing.FontFamily.GenericSansSerif, 12);
            _chart.BackColor = Color.White;
            _chart.BorderSkin.SkinStyle = BorderSkinStyle.Emboss;
            _chart.BorderlineColor = Color.Black;
            _chart.BorderlineWidth = 3;
        }

        private void DataLabels_OnChecked(object sender, RoutedEventArgs e)
        {
            foreach (var serie in _chart.Series)
            {
                serie.IsValueShownAsLabel = showValues.IsChecked ?? false;
            }
        }

        private int _index = 0;
        public void AddDataToChart(string request, List<TimeSpan> runtimes)
        {
            _chart.Legends.Add(request);
            _chart.Series.Add(request);
            _chart.Series[_index].ChartType = SeriesChartType.Line;
            var milliseconds = runtimes.Select(x => x.TotalMilliseconds).ToList();
            int trialNum = 0;
            var trials = milliseconds.Select(item => ++trialNum).ToList();
            _chart.Series[_index].Points.DataBindXY(trials, "Runs", milliseconds, "Runtime (Milliseconds)");
            _chart.Legends[_index].LegendStyle = LegendStyle.Table;
            _chart.Legends[_index].TableStyle = LegendTableStyle.Tall;
            _chart.Legends[_index].Docking = Docking.Bottom;
            _chart.ChartAreas[_index].AxisY.Minimum = runtimes.Min().TotalMilliseconds;
            _chart.ChartAreas[_index].AxisY.Maximum = runtimes.Max().TotalMilliseconds;
            _chart.ChartAreas[_index].AxisX.Minimum = trials.Min();
            _chart.ChartAreas[_index].AxisX.Maximum = trials.Max();
            _index++;
        }

        public void MergeDataToChart(string request, List<TimeSpan> runtimes)
        {
            var index = _chart.Series.IndexOf(request);
            var milliseconds = _chart.Series[index].Points.Select(x => x.YValues.First()).ToList();
            milliseconds.AddRange(runtimes.Select(x => x.TotalMilliseconds).ToList());
            int trialNum = 0;
            var trials = milliseconds.Select(item => ++trialNum).ToList();
            _chart.Series[index].Points.Clear();
            _chart.Series[index].Points.DataBindXY(trials, "Runs", milliseconds, "Runtime (Milliseconds)");
            _chart.Legends[index].LegendStyle = LegendStyle.Table;
            _chart.Legends[index].TableStyle = LegendTableStyle.Tall;
            _chart.Legends[index].Docking = Docking.Bottom;
            _chart.ChartAreas[index].AxisY.Minimum = milliseconds.Min();
            _chart.ChartAreas[index].AxisY.Maximum = milliseconds.Max();
            _chart.ChartAreas[index].AxisX.Minimum = trials.Min();
            _chart.ChartAreas[index].AxisX.Maximum = trials.Max();
        }
    }
}
