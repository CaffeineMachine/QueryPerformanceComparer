using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Forms.DataVisualization.Charting;
using Microsoft.Win32;
using QuerySessionSummaryLib;
using WebClientPerfLib;

namespace QuerySessionSummaryControl
{
    /// <summary>
    /// Interaction logic for TimeDurationView.xaml
    /// </summary>
    public partial class TimeDurationView
    {
        private IEnumerable<double> _resultRuntimes;
        private string _csvForm;
        private Chart _cumulativeRuntimeChart;
        public TimeDurationView()
        {
            InitializeComponent();
            DataContext = new TimeDurationViewModel();
            _resultRuntimes = new List<double>();
            _cumulativeRuntimeChart = new Chart();
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

        private void LoadQueries_OnClick(object sender, RoutedEventArgs e)
        {
            var timeDurationViewModel = DataContext as TimeDurationViewModel;
            if (timeDurationViewModel == null)
                DataContext = new TimeDurationViewModel();
            (DataContext as TimeDurationViewModel).Queries.Clear();
            var dialog = new OpenFileDialog { InitialDirectory = AppDomain.CurrentDomain.BaseDirectory };
            if (dialog.ShowDialog() != true) return;
            using (var fs = File.OpenRead(dialog.FileName))
            {
                using (TextReader reader = new StreamReader(fs))
                {
                    while (reader.Peek() >= 0)
                        (DataContext as TimeDurationViewModel).Queries.Add(reader.ReadLine());
                }
            }
        }

        private void LoadUrls_OnClick(object sender, RoutedEventArgs e)
        {
            var durationViewModel = DataContext as TimeDurationViewModel;
            if (durationViewModel == null)
                DataContext = new TimeDurationViewModel();
            var dialog = new OpenFileDialog { InitialDirectory = AppDomain.CurrentDomain.BaseDirectory };
            if (dialog.ShowDialog() != true) return;

            using (var fs = File.OpenRead(dialog.FileName))
            {
                using (TextReader reader = new StreamReader(fs))
                {
                    while (reader.Peek() >= 0)
                    {
                        (DataContext as TimeDurationViewModel).Urls.Add(reader.ReadLine());
                    }
                }
            }

            //var vms = (from url in (DataContext as TimeDurationViewModel).Urls where (DataContext as TimeDurationViewModel).Summaries.All(x => x.Request != url) select new StatSummaryViewModel(url, new List<TimeSpan>())).ToList();
            var vms = (DataContext as TimeDurationViewModel).Urls.ToList();
            foreach (var item in vms)
                (DataContext as TimeDurationViewModel).Summaries.Add(new StatSummaryViewModel(item, new List<TimeSpan>()));

            var index = 0;
            _cumulativeRuntimeChart.Series.Clear();
            _cumulativeRuntimeChart.Legends.Clear();
            foreach (var url in durationViewModel.Urls)
            {
                _cumulativeRuntimeChart.Legends.Add(url);
                _cumulativeRuntimeChart.Series.Add(url);
                _cumulativeRuntimeChart.Series[index].ChartType = SeriesChartType.Line;
                _cumulativeRuntimeChart.Legends[index].LegendStyle = LegendStyle.Table;
                _cumulativeRuntimeChart.Legends[index].TableStyle = LegendTableStyle.Tall;
                _cumulativeRuntimeChart.Legends[index].Docking = Docking.Bottom;
                index++;
            }
        }

        private void RunTests_OnClick(object sender, RoutedEventArgs e)
        {
            _csvForm = string.Empty;
            var csvBuilder = new StringBuilder();
            var durationViewModel = DataContext as TimeDurationViewModel;
            if (durationViewModel == null)
            {
                MessageBox.Show("ViewModel null, Aborting tests");
                return;
            }

            var queries = durationViewModel.Queries;
            var urls = durationViewModel.Urls;
            if (!queries.Any())
            {
                MessageBox.Show("No queries loaded. Aborting tests");
                return;
            }

            double time;
            if (!Double.TryParse(TestTime.Text, out time))
            {
                return;
            }

            int index = 0;
            foreach (var url in urls)
            {
                csvBuilder.Append(url + ",");
            }
            csvBuilder.AppendLine("Query,");

            

            var wrapper = new WebClientPerfWrapper();
            var sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < time * 60 * 1000)
            {
                foreach (var query in queries)
                {
                    if (sw.ElapsedMilliseconds >= time * 60 * 1000)
                        break;
                    if (urls.Any())
                    {
                        foreach (var url in urls)
                        {
                            if (sw.ElapsedMilliseconds >= time * 60 * 1000)
                                break;
                            var request = string.Format("{0}?{1}", url, query);
                            var responseTime = wrapper.RunPerformanceRequest(request);
                            var match = durationViewModel.Summaries.First(x => x.Request == url);
                            durationViewModel.Summaries.First(x => x.Request == url).Runtimes.Add(responseTime);
                            csvBuilder.Append(string.Format("{0},", responseTime.TotalMilliseconds));
                        }
                        csvBuilder.AppendLine(string.Format("{0},", query));
                    }
                    else
                    {
                        MessageBox.Show("No urls loaded. Aborting tests");
                        return;
                    }
                }
            }
            sw.Stop();
            _resultRuntimes = wrapper.GetResultRuntimes();
            _csvForm = csvBuilder.ToString();
            DataTable dt = new DataTable();
            string[] tableData = _csvForm.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var col = from cl in tableData[0].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                      select new DataColumn(cl);
            dt.Columns.AddRange(col.ToArray());
            (from st in tableData.Skip(1) 
            select dt.Rows.Add(st.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))).ToList();
            gridView.ItemsSource = dt.DefaultView;
            gridView.AutoGenerateColumns = true;
            index = 0;
            var max = durationViewModel.Summaries.Select(x => GetAggregate(x.Runtimes.ToList()).Max()).Max();
            var trialMax = durationViewModel.Summaries.Select(x => x.Runtimes.Count).Max();
            foreach (var url in urls)
            {
                var timespans = durationViewModel.Summaries.First(x => x.Request == url).Runtimes.ToList();
                var cumulativeMiliseconds = GetAggregate(timespans).ToList();
                int trialNum = 0;
                var trials = timespans.Select(item => ++trialNum).ToList();
                _cumulativeRuntimeChart.Series[index].Points.DataBindXY(trials, "Runs", cumulativeMiliseconds,
                                                                        "Runtime (Milliseconds)");
                _cumulativeRuntimeChart.ChartAreas[0].AxisY.Minimum = 0;
                _cumulativeRuntimeChart.ChartAreas[0].AxisX.Minimum = 0;
                _cumulativeRuntimeChart.ChartAreas[0].AxisY.Maximum = max;
                _cumulativeRuntimeChart.ChartAreas[0].AxisX.Maximum = trialMax;
                index++;
            }
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

        private void SerializeResults_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog { InitialDirectory = AppDomain.CurrentDomain.BaseDirectory };
            if (dialog.ShowDialog() != true) return;
            using (var fs = File.Create(dialog.FileName))
            {
                var serializer =
                    new DataContractSerializer(typeof(List<double>));
                serializer.WriteObject(fs, _resultRuntimes);
            }
        }

        private void SaveAsCSV_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog { InitialDirectory = AppDomain.CurrentDomain.BaseDirectory, Filter = "CSV (Comma delimited) |*.csv", DefaultExt = ".csv"};
            if (dialog.ShowDialog() != true) return;
            using (var fs = File.Create(dialog.FileName))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(_csvForm);
                }
            }
        }

        private void CumulativeDataLabels_OnChecked(object sender, RoutedEventArgs e)
        {
            foreach (var serie in _cumulativeRuntimeChart.Series)
            {
                serie.IsValueShownAsLabel = ShowCumulativeValues.IsChecked ?? false;
            }
        }
    }
}
