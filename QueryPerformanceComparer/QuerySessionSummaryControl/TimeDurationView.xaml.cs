using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
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
        public TimeDurationView()
        {
            InitializeComponent();
            DataContext = new TimeDurationViewModel();
            _resultRuntimes = new List<double>();
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
                            csvBuilder.Append(responseTime.TotalMilliseconds);
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
    }
}
