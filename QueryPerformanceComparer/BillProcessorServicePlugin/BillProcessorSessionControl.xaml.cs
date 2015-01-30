using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using QuerySessionSummaryLib;
using WebClientPerfLib;

namespace BillProcessorServicePlugin
{
    /// <summary>
    /// Interaction logic for BillProcessorSessionControl.xaml
    /// </summary>
    public partial class BillProcessorSessionControl : UserControl
    {
        private List<string> _requests;

        public BillProcessorSessionControl()
        {
            InitializeComponent();
            ReportSummary.GenerateChart();
            _requests = new List<string>();
        }

        private async void RunTests_OnClick(object sender, RoutedEventArgs e)
        {
            var testCaseGenerator = new BillProcessorServiceTestCaseGenerator();
            testCaseGenerator.CreateTestUrls(@"C:\TestData\83RBillSummary.csv");

            var wrapper = new WebClientPerfWrapper();
            var results = new List<Tuple<TimeSpan, string>>();
            int numTries = 30;
            foreach (var testCase in testCaseGenerator.TestUrls)
            {
                var request = testCase;
                if (string.IsNullOrEmpty(request)) continue;

                if (request.Contains("HB12-I")) continue;
                
                var tasks = Enumerable.Range(0, numTries).Select(x => wrapper.RunPerformanceRequestTask(request));
                var newResults = await Task.WhenAll(tasks);
                results.AddRange(newResults);

                results.AddRange(newResults);

                if (!_requests.Contains(request))
                {
                    ReportSummary.AddDataToChart(request, newResults.Select(x => x.Item1).ToList(), results.Select(x => x.Item1).ToList());
                    ReportSummary.StatSummary.ViewModel = new StatSummaryViewModel(request, results.Select(x => x.Item1).ToList());
                    ReportSummary.IndividualRuntime.ViewModel = new IndividualRuntimeViewModel(results.Select(x => x.Item1).ToList());
                    _requests.Add(request);
                }
                else
                {
                    ReportSummary.MergeDataToChart(request, newResults.Select(x => x.Item1).ToList(), results.Select(x => x.Item1).ToList());
                    var newModelData = ReportSummary.StatSummary.ViewModel.Runtimes;
                    foreach (var item in results.Select(x => x.Item1).ToList())
                        newModelData.Add(item);
                    ReportSummary.StatSummary.ViewModel = new StatSummaryViewModel(request, newModelData);
                    foreach (var item in results.Select(x => x.Item1).ToList())
                        ReportSummary.IndividualRuntime.ViewModel.Runtimes.Add(item.TotalMilliseconds);
                }
            }
        }

        private void SerializeResults_OnClick(object sender, RoutedEventArgs e)
        {
        }
    }
}
