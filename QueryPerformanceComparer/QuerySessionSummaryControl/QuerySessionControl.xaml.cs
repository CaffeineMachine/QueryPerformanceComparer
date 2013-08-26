using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using Microsoft.Win32;
using QuerySessionSummaryLib;
using WebClientPerfLib;

namespace QuerySessionSummaryControl
{
    /// <summary>
    /// Interaction logic for QuerySessionControl.xaml
    /// </summary>
    public partial class QuerySessionControl
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        private List<string> _requests;
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        public QuerySessionControl()
        {
            InitializeComponent();
            ReportSummary.GenerateChart();
            _requests = new List<string>();
        }

        private void RunTests_OnClick(object sender, RoutedEventArgs e)
        {
            var wrapper = new WebClientPerfWrapper();
            var runtimes = new List<TimeSpan>();
            int numTries;
            var request = string.Format("{0}?{1}", UrlPath.Text, Query.Text);
            if (!Int32.TryParse(Tries.Text, out numTries))
            {
                MessageBox.Show("Number of tries is not an integer.");
                return;
            }
            if (numTries <= 0)
            {
                MessageBox.Show("Number of tries must be greater than 0.");
                return;
            }
            for (var i = 0; i < numTries; i++)
            {
                runtimes.Add(wrapper.RunPerformanceRequest(request));
            }

            if (!_requests.Contains(request))
            {
                ReportSummary.AddDataToChart(request, runtimes);
                ReportSummary.StatSummary.ViewModel = new StatSummaryViewModel(request, runtimes);
                ReportSummary.IndividualRuntime.ViewModel = new IndividualRuntimeViewModel(runtimes);
                _requests.Add(request);
            }
            else
            {
                ReportSummary.MergeDataToChart(request, runtimes);
                var newModelData = ReportSummary.StatSummary.ViewModel.Runtimes;
                newModelData.AddRange(runtimes);
                ReportSummary.StatSummary.ViewModel = new StatSummaryViewModel(request, newModelData);
                foreach (var item in runtimes)
                    ReportSummary.IndividualRuntime.ViewModel.Runtimes.Add(item.TotalMilliseconds);
            }

        }

        private void SerializeResults_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog { InitialDirectory = AppDomain.CurrentDomain.BaseDirectory };
            if (dialog.ShowDialog() == true)
            {
                using (var fs = File.Create(dialog.FileName))
                {
                    var serializer =
                        new DataContractSerializer(typeof(StatSummaryViewModel));
                    serializer.WriteObject(fs, ReportSummary.StatSummary.ViewModel);
                }
            }
        }
    }
}
