using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms.DataVisualization.Charting;

namespace QuerySessionSummaryControl
{
    /// <summary>
    /// Interaction logic for ReportSummary.xaml
    /// </summary>
    public partial class ReportSummary
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        private Chart _individualRuntimeChart, _cumulativeRuntimeChart;
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        public ReportSummary()
        {
            InitializeComponent();
            _individualRuntimeChart = new Chart();
            _cumulativeRuntimeChart = new Chart();
        }

        public void GenerateChart()
        {
            _individualRuntimeChart.ChartAreas.Add("chtArea");
            ControlHost.Child = _individualRuntimeChart;
            _individualRuntimeChart.ChartAreas[0].AxisX.Title = "Runs";
            _individualRuntimeChart.ChartAreas[0].AxisX.TitleFont = new Font(System.Drawing.FontFamily.GenericSansSerif, 12);
            _individualRuntimeChart.ChartAreas[0].AxisY.Title = "Runtime (Milliseconds)";
            _individualRuntimeChart.ChartAreas[0].AxisY.TitleFont = new Font(System.Drawing.FontFamily.GenericSansSerif, 12);
            _individualRuntimeChart.BackColor = Color.White;
            _individualRuntimeChart.BorderSkin.SkinStyle = BorderSkinStyle.Emboss;
            _individualRuntimeChart.BorderlineColor = Color.Black;
            _individualRuntimeChart.BorderlineWidth = 3;

            _cumulativeRuntimeChart.ChartAreas.Add("chtArea");
            CumulativeGraphControlHost.Child = _cumulativeRuntimeChart;
            _cumulativeRuntimeChart.ChartAreas[0].AxisX.Title = "Runs";
            _cumulativeRuntimeChart.ChartAreas[0].AxisX.TitleFont = new Font(System.Drawing.FontFamily.GenericSansSerif, 12);
            _cumulativeRuntimeChart.ChartAreas[0].AxisY.Title = "Runtime (Milliseconds)";
            _cumulativeRuntimeChart.ChartAreas[0].AxisY.TitleFont = new Font(System.Drawing.FontFamily.GenericSansSerif, 12);
            _cumulativeRuntimeChart.BackColor = Color.White;
            _cumulativeRuntimeChart.BorderSkin.SkinStyle = BorderSkinStyle.Emboss;
            _cumulativeRuntimeChart.BorderlineColor = Color.Black;
            _cumulativeRuntimeChart.BorderlineWidth = 3;
        }

        private void DataLabels_OnChecked(object sender, RoutedEventArgs e)
        {
            foreach (var serie in _individualRuntimeChart.Series)
            {
                serie.IsValueShownAsLabel = ShowValues.IsChecked ?? false;
            }
        }

        private int _index;
        public void AddDataToChart(string request, List<TimeSpan> runtimes)
        {
            _individualRuntimeChart.Legends.Add(request);
            _individualRuntimeChart.Series.Add(request);
            _individualRuntimeChart.Series[_index].ChartType = SeriesChartType.Line;
            var milliseconds = runtimes.Select(x => x.TotalMilliseconds).ToList();
            int trialNum = 0;
            var trials = milliseconds.Select(item => ++trialNum).ToList();
            _individualRuntimeChart.Series[_index].Points.DataBindXY(trials, "Runs", milliseconds, "Runtime (Milliseconds)");
            _individualRuntimeChart.Legends[_index].LegendStyle = LegendStyle.Table;
            _individualRuntimeChart.Legends[_index].TableStyle = LegendTableStyle.Tall;
            _individualRuntimeChart.Legends[_index].Docking = Docking.Bottom;
            _individualRuntimeChart.ChartAreas[_index].AxisY.Minimum = runtimes.Min().TotalMilliseconds;
            _individualRuntimeChart.ChartAreas[_index].AxisY.Maximum = runtimes.Max().TotalMilliseconds;
            _individualRuntimeChart.ChartAreas[_index].AxisX.Minimum = trials.Min();
            _individualRuntimeChart.ChartAreas[_index].AxisX.Maximum = trials.Max();

            _cumulativeRuntimeChart.Legends.Add(request);
            _cumulativeRuntimeChart.Series.Add(request);
            _cumulativeRuntimeChart.Series[_index].ChartType = SeriesChartType.Line;
            var cumulativeMiliseconds = GetAggregate(runtimes).ToList();
            _cumulativeRuntimeChart.Series[_index].Points.DataBindXY(trials, "Runs", cumulativeMiliseconds, "Runtime (Milliseconds)");
            _cumulativeRuntimeChart.Legends[_index].LegendStyle = LegendStyle.Table;
            _cumulativeRuntimeChart.Legends[_index].TableStyle = LegendTableStyle.Tall;
            _cumulativeRuntimeChart.Legends[_index].Docking = Docking.Bottom;
            _cumulativeRuntimeChart.ChartAreas[_index].AxisY.Minimum = 0;
            _cumulativeRuntimeChart.ChartAreas[_index].AxisY.Maximum = cumulativeMiliseconds.Max();
            _cumulativeRuntimeChart.ChartAreas[_index].AxisX.Minimum = 0;
            _cumulativeRuntimeChart.ChartAreas[_index].AxisX.Maximum = trials.Max();

            _index++;
        }

        private IEnumerable<double> GetAggregate(List<TimeSpan> runtimes)
        {
            var aggregates = new List<double>();
            var index = 1;
#pragma warning disable 168
            foreach (var item in runtimes)
#pragma warning restore 168
            {
                aggregates.Add(runtimes.Take(index).Sum(x => x.TotalMilliseconds));
                index++;
            }
            return aggregates;
        }

        public void MergeDataToChart(string request, List<TimeSpan> runtimes)
        {
            var index = _individualRuntimeChart.Series.IndexOf(request);
            var milliseconds = _individualRuntimeChart.Series[index].Points.Select(x => x.YValues.First()).ToList();
            milliseconds.AddRange(runtimes.Select(x => x.TotalMilliseconds).ToList());
            var trialNum = 0;
            var trials = milliseconds.Select(item => ++trialNum).ToList();
            _individualRuntimeChart.Series[index].Points.Clear();
            _individualRuntimeChart.Series[index].Points.DataBindXY(trials, "Runs", milliseconds, "Runtime (Milliseconds)");
            _individualRuntimeChart.Legends[index].LegendStyle = LegendStyle.Table;
            _individualRuntimeChart.Legends[index].TableStyle = LegendTableStyle.Tall;
            _individualRuntimeChart.Legends[index].Docking = Docking.Bottom;
            _individualRuntimeChart.ChartAreas[index].AxisY.Minimum = milliseconds.Min();
            _individualRuntimeChart.ChartAreas[index].AxisY.Maximum = milliseconds.Max();
            _individualRuntimeChart.ChartAreas[index].AxisX.Minimum = trials.Min();
            _individualRuntimeChart.ChartAreas[index].AxisX.Maximum = trials.Max();

            var cumulativeMilliseconds = _cumulativeRuntimeChart.Series[index].Points.Select(x => x.YValues.First()).ToList();
            var max = cumulativeMilliseconds.Max();
            cumulativeMilliseconds.AddRange(GetAggregate(runtimes).Select(x => x + max));
            _cumulativeRuntimeChart.Series[index].Points.Clear();
            _cumulativeRuntimeChart.Series[index].Points.DataBindXY(trials, "Runs", cumulativeMilliseconds, "Runtime (Milliseconds)");
            _cumulativeRuntimeChart.Legends[index].LegendStyle = LegendStyle.Table;
            _cumulativeRuntimeChart.Legends[index].TableStyle = LegendTableStyle.Tall;
            _cumulativeRuntimeChart.Legends[index].Docking = Docking.Bottom;
            _cumulativeRuntimeChart.ChartAreas[index].AxisY.Minimum = 0;
            _cumulativeRuntimeChart.ChartAreas[index].AxisY.Maximum = cumulativeMilliseconds.Max();
            _cumulativeRuntimeChart.ChartAreas[index].AxisX.Minimum = 0;
            _cumulativeRuntimeChart.ChartAreas[index].AxisX.Maximum = trials.Max();
        }

        private void CumulativeDataLabels_OnChecked(object sender, RoutedEventArgs e)
        {
            foreach (var serie in _cumulativeRuntimeChart.Series)
            {
                serie.IsValueShownAsLabel = ShowValues.IsChecked ?? false;
            }
        }
    }
}
