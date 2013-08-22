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

        public void GenerateChart(IEnumerable<TimeSpan> runtimes)
        {
            _chart.ChartAreas.Add("chtArea");
            controlHost.Child = _chart;
            _chart.ChartAreas[0].AxisX.Title = "Runs";
            _chart.ChartAreas[0].AxisX.TitleFont = new Font(System.Drawing.FontFamily.GenericSansSerif, 12);
            _chart.ChartAreas[0].AxisY.Title = "Runtime";
            _chart.ChartAreas[0].AxisY.TitleFont = new Font(System.Drawing.FontFamily.GenericSansSerif, 12);
            _chart.Legends.Add("Query Runtime");
            _chart.Series.Add("Query Runtime");
            _chart.Series[0].ChartType = SeriesChartType.Line;
            var milliseconds = runtimes.Select(x => x.TotalMilliseconds).ToList();
            int trialNum = 0;
            var trials = milliseconds.Select(item => ++trialNum).ToList();
            _chart.Series[0].Points.DataBindXY(trials, "Runs", milliseconds, "Runtime");
            _chart.BorderSkin.SkinStyle = BorderSkinStyle.Emboss;
            _chart.BorderlineColor = Color.Black;
            _chart.BorderlineWidth = 3;
            _chart.Legends[0].LegendStyle = LegendStyle.Table;
            _chart.Legends[0].TableStyle = LegendTableStyle.Tall;
            _chart.Legends[0].Docking = Docking.Right;
            _chart.BackColor = Color.White;
            _chart.ChartAreas[0].AxisY.Minimum = runtimes.Min().TotalMilliseconds;
            _chart.ChartAreas[0].AxisY.Maximum = runtimes.Max().TotalMilliseconds;
        }

        private void DataLabels_OnChecked(object sender, RoutedEventArgs e)
        {
            foreach (var serie in _chart.Series)
            {
                serie.IsValueShownAsLabel = showValues.IsChecked ?? false;
            }
        }
    }
}
