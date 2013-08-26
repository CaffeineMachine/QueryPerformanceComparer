using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using QuerySessionSummaryLib;
using WebClientPerfLib;

namespace QuerySessionSummaryControl
{
    /// <summary>
    /// Interaction logic for QuerySessionControl.xaml
    /// </summary>
    public partial class QuerySessionControl : UserControl
    {
        public QuerySessionControl()
        {
            InitializeComponent();
            this.ReportSummary.GenerateChart();
        }

        private List<string> _requests = new List<string>();
        private void RunTests_OnClick(object sender, RoutedEventArgs e)
        {
            var wrapper = new WebClientPerfWrapper();
            var runtimes = new List<TimeSpan>();
            int numTries;
            var request = string.Format("{0}?{1}", urlPath.Text, query.Text);
            if (!Int32.TryParse(tries.Text, out numTries))
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
                this.ReportSummary.AddDataToChart(request, runtimes);
                this.ReportSummary.statSummary.ViewModel = new StatSummaryViewModel(request, runtimes);
                this.ReportSummary.individualRuntime.ViewModel = new IndividualRuntimeViewModel(runtimes);
                _requests.Add(request);
            }
            else
            {
                this.ReportSummary.MergeDataToChart(request, runtimes);
                var newModelData = ReportSummary.statSummary.ViewModel.Runtimes;
                newModelData.AddRange(runtimes);
                this.ReportSummary.statSummary.ViewModel = new StatSummaryViewModel(request, newModelData);
                foreach (var item in runtimes)
                    this.ReportSummary.individualRuntime.ViewModel.Runtimes.Add(item.TotalMilliseconds);
            }

        }

        private void SerializeResults_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var fs = File.Create(dialog.FileName))
                    {
                        var serializer =
                            new DataContractSerializer(typeof(StatSummaryViewModel));
                        serializer.WriteObject(fs, ReportSummary.statSummary.ViewModel);
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }
    }
}
