using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.Integration;
using QuerySessionSummaryLib;

namespace QuerySessionSummaryControl
{
    /// <summary>
    /// Interaction logic for ReportSummary.xaml
    /// </summary>
    public partial class ReportSummary
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        private Chart _individualRuntimeChart, _cumulativeRuntimeChart, _runtimeDistributionChart;
        private List<TimeSpan> _runtimes;
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        public ReportSummary()
        {
            InitializeComponent();
            _index = 0;
            _individualRuntimeChart = new Chart();
            _cumulativeRuntimeChart = new Chart();
            _runtimeDistributionChart = new Chart();
            _runtimes = new List<TimeSpan>();
            GenerateChart();
        }

        private void GenerateChart()
        {
            var titleFont = new Font(System.Drawing.FontFamily.GenericSansSerif, 12);
            GenerateChart(_individualRuntimeChart, "Runs", "Runtime (Milliseconds)", titleFont, ControlHost);
            GenerateChart(_cumulativeRuntimeChart, "Runs", "Cumulative Runtime (Milliseconds)", titleFont, CumulativeGraphControlHost);
            GenerateChart(_runtimeDistributionChart, "Runtime (Milliseconds)", "Frequency", titleFont, RuntimeDistributionFormsHost);
        }

        private static void GenerateChart(Chart chartReference, string xTitle, string yTitle, Font titleFont, WindowsFormsHost host)
        {
            chartReference.ChartAreas.Add("chtArea");
            host.Child = chartReference;
            chartReference.ChartAreas[0].AxisX.Title = xTitle;
            chartReference.ChartAreas[0].AxisX.TitleFont = titleFont;
            chartReference.ChartAreas[0].AxisY.Title = yTitle;
            chartReference.ChartAreas[0].AxisY.TitleFont = titleFont;
            chartReference.BackColor = Color.White;
            chartReference.BorderSkin.SkinStyle = BorderSkinStyle.None;
        }

        private void DataLabels_OnChecked(object sender, RoutedEventArgs e)
        {
            foreach (var serie in _individualRuntimeChart.Series)
            {
                serie.IsValueShownAsLabel = ShowValues.IsChecked ?? false;
            }
        }

        private int _index;
        public void AddDataToChart(string request, List<TimeSpan> runtimes, List<TimeSpan> aggregateRuntimes)
        {
            if (_individualRuntimeChart.Legends.FindByName(request) == null)
                _individualRuntimeChart.Legends.Add(request);

            _individualRuntimeChart.Series.Add(request);
            _individualRuntimeChart.Series[_index].ChartType = SeriesChartType.Line;
            var milliseconds = runtimes.Select(x => x.TotalMilliseconds).ToList();
            var trialNum = 0;
            var trials = milliseconds.Select(item => ++trialNum).ToList();
            //_runtimes.Clear();
            _runtimes.AddRange(runtimes);
            _individualRuntimeChart.Series[_index].Points.DataBindXY(trials, "Runs", milliseconds, "Runtime (Milliseconds)");
            _individualRuntimeChart.Legends[_index].LegendStyle = LegendStyle.Table;
            _individualRuntimeChart.Legends[_index].TableStyle = LegendTableStyle.Tall;
            _individualRuntimeChart.Legends[_index].Docking = Docking.Bottom;
            _individualRuntimeChart.ChartAreas[0].AxisY.Minimum = aggregateRuntimes.Min().TotalMilliseconds;
            _individualRuntimeChart.ChartAreas[0].AxisY.Maximum = aggregateRuntimes.Max().TotalMilliseconds;
            _individualRuntimeChart.ChartAreas[0].AxisX.Minimum = trials.Min();
            _individualRuntimeChart.ChartAreas[0].AxisX.Maximum = trials.Max();

            _cumulativeRuntimeChart.Legends.Add(request);
            _cumulativeRuntimeChart.Series.Add(request);
            _cumulativeRuntimeChart.Series[_index].ChartType = SeriesChartType.Line;
            var cumulativeMilliseconds = new List<double> { 0 };
            cumulativeMilliseconds.AddRange(GetAggregate(runtimes).ToList());
            trialNum = 0;
            trials = cumulativeMilliseconds.Select(item => trialNum++).ToList();
            _cumulativeRuntimeChart.Series[_index].Points.DataBindXY(trials, "Runs", cumulativeMilliseconds, "Runtime (Milliseconds)");
            _cumulativeRuntimeChart.Legends[_index].LegendStyle = LegendStyle.Table;
            _cumulativeRuntimeChart.Legends[_index].TableStyle = LegendTableStyle.Tall;
            _cumulativeRuntimeChart.Legends[_index].Docking = Docking.Bottom;
            _cumulativeRuntimeChart.ChartAreas[0].AxisY.Minimum = 0;
            _cumulativeRuntimeChart.ChartAreas[0].AxisY.Maximum = cumulativeMilliseconds.Max();
            _cumulativeRuntimeChart.ChartAreas[0].AxisX.Minimum = 0;
            _cumulativeRuntimeChart.ChartAreas[0].AxisX.Maximum = trials.Max();

            if (_runtimeDistributionChart.Series.Count == 0)
                _runtimeDistributionChart.Series.Add("Distribution");
            _runtimeDistributionChart.Series[0].ChartType = SeriesChartType.Column;
            var ssvm = new StatSummaryViewModel("Distribution", _runtimes);
            _runtimeDistributionChart.ChartAreas[0].AxisX.Minimum = ssvm.Mean - 3 * ssvm.StdDev;
            _runtimeDistributionChart.ChartAreas[0].AxisX.Maximum = ssvm.Mean + 3 * ssvm.StdDev;
            var xValues = new List<double>();
            var yValues = new List<int>();
            for (var x = _runtimeDistributionChart.ChartAreas[0].AxisX.Minimum + (0.3 * ssvm.StdDev);
                x <= _runtimeDistributionChart.ChartAreas[0].AxisX.Maximum;
                x += (0.3 * ssvm.StdDev))
            {
                xValues.Add(x);
                yValues.Add(_runtimes.Count(y => y.TotalMilliseconds <= x && y.TotalMilliseconds > x - (0.3 * ssvm.StdDev)));
            }
            _runtimeDistributionChart.ChartAreas[0].AxisY.Minimum = 0;
            _runtimeDistributionChart.ChartAreas[0].AxisY.Maximum = yValues.Max();
            _runtimeDistributionChart.Series[0].Points.DataBindXY(xValues, "Runtime (Milliseconds)", yValues, "Frequency");
            _index++;
        }

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
            var cumulativeMilliseconds = new List<double> { 0 };
            cumulativeMilliseconds.AddRange(GetAggregate(runtimes).ToList());
            trialNum = 0;
            trials = cumulativeMilliseconds.Select(item => trialNum++).ToList();
            _cumulativeRuntimeChart.Series[_index].Points.DataBindXY(trials, "Runs", cumulativeMilliseconds, "Runtime (Milliseconds)");
            _cumulativeRuntimeChart.Legends[_index].LegendStyle = LegendStyle.Table;
            _cumulativeRuntimeChart.Legends[_index].TableStyle = LegendTableStyle.Tall;
            _cumulativeRuntimeChart.Legends[_index].Docking = Docking.Bottom;
            _cumulativeRuntimeChart.ChartAreas[_index].AxisY.Minimum = 0;
            _cumulativeRuntimeChart.ChartAreas[0].AxisY.Maximum = Double.IsNaN(_cumulativeRuntimeChart.ChartAreas[0].AxisY.Maximum) ?
                cumulativeMilliseconds.Max() : Math.Max(cumulativeMilliseconds.Max(), _cumulativeRuntimeChart.ChartAreas[_index].AxisY.Maximum);
            _cumulativeRuntimeChart.ChartAreas[_index].AxisX.Minimum = 0;
            _cumulativeRuntimeChart.ChartAreas[_index].AxisX.Maximum = trials.Max();

            _index++;
        }

        private IEnumerable<double> GetAggregate(List<TimeSpan> runtimes)
        {
            var aggregates = new List<double>();
            for (var i = 1; i <= runtimes.Count; i++)
            {
                aggregates.Add(runtimes.Take(i).Sum(x => x.TotalMilliseconds));
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
