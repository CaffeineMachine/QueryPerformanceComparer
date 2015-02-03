using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
        private Chart _cumulativeRuntimeChart, _individualRuntimeChart;

        public TimeDurationView()
        {
            InitializeComponent();
            DataContext = new TimeDurationViewModel();
            _resultRuntimes = new List<double>();
            _cumulativeRuntimeChart = new Chart();
            _cumulativeRuntimeChart.ChartAreas.Add("chtArea");
            CumulativeGraphControlHost.Child = _cumulativeRuntimeChart;
            _cumulativeRuntimeChart.ChartAreas[0].AxisX.Title = "Runs";
            _cumulativeRuntimeChart.ChartAreas[0].AxisX.TitleFont = new Font(
                System.Drawing.FontFamily.GenericSansSerif, 12);
            _cumulativeRuntimeChart.ChartAreas[0].AxisY.Title = "Runtime (Milliseconds)";
            _cumulativeRuntimeChart.ChartAreas[0].AxisY.TitleFont = new Font(
                System.Drawing.FontFamily.GenericSansSerif, 12);
            _cumulativeRuntimeChart.BackColor = Color.White;
            _cumulativeRuntimeChart.BorderSkin.SkinStyle = BorderSkinStyle.None;

            _individualRuntimeChart = new Chart();
            _individualRuntimeChart.ChartAreas.Add("chtArea");
            IndividualGraphControlHost.Child = _individualRuntimeChart;
            _individualRuntimeChart.ChartAreas[0].AxisX.Title = "Runs";
            _individualRuntimeChart.ChartAreas[0].AxisX.TitleFont = new Font(
                System.Drawing.FontFamily.GenericSansSerif, 12);
            _individualRuntimeChart.ChartAreas[0].AxisY.Title = "Runtime (Milliseconds)";
            _individualRuntimeChart.ChartAreas[0].AxisY.TitleFont = new Font(
                System.Drawing.FontFamily.GenericSansSerif, 12);
            _individualRuntimeChart.BackColor = Color.White;
            _individualRuntimeChart.BorderSkin.SkinStyle = BorderSkinStyle.None;
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
                _individualRuntimeChart.Legends.Add(url);
                _individualRuntimeChart.Series.Add(url);
                _individualRuntimeChart.Series[index].ChartType = SeriesChartType.Line;
                _individualRuntimeChart.Legends[index].LegendStyle = LegendStyle.Table;
                _individualRuntimeChart.Legends[index].TableStyle = LegendTableStyle.Tall;
                _individualRuntimeChart.Legends[index].Docking = Docking.Bottom;

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
                csvBuilder.Append(url + ",Result Count,");
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
                            var match = durationViewModel.Summaries.First(x => x.Requests.Contains(url));
                            durationViewModel.Summaries.First(x => x.Requests.Contains(url)).Runtimes.Add(responseTime.Item1);
                            csvBuilder.Append(string.Format("{0},", responseTime.Item1.TotalMilliseconds));
                            var docMatch = Regex.Match(responseTime.Item2, @"Results \d+ through \d+ out of (?'results'\d+) matches");
                            if (Regex.IsMatch(responseTime.Item2, @"Results \d+ through \d+ out of (?'results'\d+) matches"))
                                csvBuilder.Append(string.Format("{0},", Int32.Parse(docMatch.Groups["results"].ToString())));
                            else
                            {
                                csvBuilder.Append("0,");
                            }
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
            //DataTable dt = new DataTable();
            //string[] tableData = _csvForm.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            //var col = from cl in tableData[0].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
            //          select new DataColumn(cl);
            //dt.Columns.AddRange(col.ToArray());
            //(from st in tableData.Skip(1) 
            //select dt.Rows.Add(st.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))).ToList();
            //gridView.ItemsSource = dt.DefaultView;
            //gridView.AutoGenerateColumns = true;
            index = 0;
            var max = durationViewModel.Summaries.Select(x => GetAggregate(x.Runtimes.ToList()).Max()).Max();
            var trialMax = durationViewModel.Summaries.Select(x => x.Runtimes.Count).Max();
            var timespanMax = durationViewModel.Summaries.Select(x => x.Runtimes.Max()).Max().TotalMilliseconds;
            var timespanMin = durationViewModel.Summaries.Select(x => x.Runtimes.Min()).Min().TotalMilliseconds;
            foreach (var url in urls)
            {
                var timespans = durationViewModel.Summaries.First(x => x.Requests.Contains(url)).Runtimes.ToList();
                var cumulativeMiliseconds = GetAggregate(timespans).ToList();
                int trialNum = 0;
                var trials = timespans.Select(item => ++trialNum).ToList();
                _individualRuntimeChart.Series[index].Points.DataBindXY(trials, "Runs",
                                                                        timespans.Select(x => x.TotalMilliseconds)
                                                                                 .ToList(), "Runtime (Milliseconds)");
                _individualRuntimeChart.ChartAreas[0].AxisY.Minimum = timespanMin;
                _individualRuntimeChart.ChartAreas[0].AxisY.Maximum = timespanMax;
                _individualRuntimeChart.ChartAreas[0].AxisX.Minimum = 0;
                _individualRuntimeChart.ChartAreas[0].AxisX.Maximum = trials.Max();

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
            var dialog = new SaveFileDialog
                             {
                                 InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                                 Filter = "CSV (Comma delimited) |*.csv",
                                 DefaultExt = ".csv"
                             };
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

        private void IndividualDataLabels_OnChecked(object sender, RoutedEventArgs e)
        {
            foreach (var serie in _individualRuntimeChart.Series)
            {
                serie.IsValueShownAsLabel = ShowCumulativeValues.IsChecked ?? false;
            }
        }

        private ConcurrentQueue<Tuple<string, TimeSpan, string>> _concurrentRuntimes = new ConcurrentQueue<Tuple<string, TimeSpan, string>>();
        private void LoadTests_OnClick(object sender, RoutedEventArgs e)
        {
            int time = 0, threads = 0;
            bool loadTestTimeParsed = Int32.TryParse(loadTestTime.Text, out time);
            if (!loadTestTimeParsed)
            {
                MessageBox.Show("Error parsing load test time. Stopping tests.");
                return;
            }

            bool loadTestThreadsParsed = Int32.TryParse(loadTestThreads.Text, out threads);
            if (!loadTestThreadsParsed)
            {
                MessageBox.Show("Error parsing load test threads. Stopping tests.");
                return;
            }

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

            Stopwatch sw = Stopwatch.StartNew();
            var wrapper = new WebClientPerfWrapper();
            while (sw.ElapsedMilliseconds < time * 60 * 1000)
            {
                int index = 0;
                foreach (var query in queries)
                {
                    if (urls.Any())
                    {
                        // Build one request for each url.
                        var requests = urls.Select(x => string.Format("{0}?{1}", x, query)).ToList();
                        if (sw.ElapsedMilliseconds >= time * 60 * 1000)
                            break;
                        foreach (var req in requests)
                        {
                            var thread = new Thread(() =>
                                                        {
                                                            var runtime = wrapper.RunPerformanceRequest(
                                                                req);
                                                            _concurrentRuntimes.Enqueue(new Tuple<string, TimeSpan, string>(req, runtime.Item1, runtime.Item2));
                                                        });
                            thread.Start();
                        }

                        index++;
                        if (index % threads == 0)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                    else
                    {
                        MessageBox.Show("No urls loaded. Aborting tests");
                        return;
                    }
                }
            }

            var csvValue = BuildLoadTimesCsv();

            var dialog = new SaveFileDialog
            {
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                Filter = "CSV (Comma delimited) |*.csv",
                DefaultExt = ".csv"
            };
            if (dialog.ShowDialog() != true) return;
            using (var fs = File.Create(dialog.FileName))
            {
                using (var writer = new StreamWriter(fs))
                {
                    writer.Write(csvValue);
                }
            }
        }

        private string BuildLoadTimesCsv()
        {
            var sb = new StringBuilder();
            foreach (var tuple in _concurrentRuntimes)
            {
                var responseDoc = tuple.Item3.ToString();
                var results = 0;
                if (Regex.IsMatch(responseDoc, @"Results \d+ through \d+ out of (?'results'\d+) matches"))
                {
                    var match = Regex.Match(responseDoc, @"Results \d+ through \d+ out of (?'results'\d+) matches");
                    results = Int32.Parse(match.Groups["results"].ToString());
                }
                sb.AppendLine(string.Format("{0},{1},{2},", tuple.Item1, tuple.Item2.TotalMilliseconds, results));
            }

            return sb.ToString();
        }
    }
}