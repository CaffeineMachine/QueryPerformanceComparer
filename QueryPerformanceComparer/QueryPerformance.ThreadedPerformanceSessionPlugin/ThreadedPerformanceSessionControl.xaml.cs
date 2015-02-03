using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Ninject;
using QueryPerformance.Interfaces;
using QueryPerformance.Utilities;
using QuerySessionSummaryLib;
using WebClientPerfLib;

namespace QueryPerformance.ThreadedPerformanceSessionPlugin
{
    /// <summary>
    /// Interaction logic for ThreadedPerformanceSessionControl.xaml
    /// </summary>
    public partial class ThreadedPerformanceSessionControl
    {
        private List<string> _testUrls;
        public ThreadedPerformanceSessionControl()
        {
            InitializeComponent();
            ReportSummary.GenerateChart();
            _testUrls = new List<string>();
            IKernel kernel = new StandardKernel();
            LibraryLoader.LoadAllBinDirectoryAssemblies();
            if (File.Exists("TypeMappings.xml"))
                kernel.Load("TypeMappings.xml");
            var plugins = kernel.GetAll<IThreadedPerformanceSessionTestUrlGenerator>().ToList();
            UrlGeneratorPlugins.ItemsSource = plugins.ToList();
        }

        private async void RunTests_OnClick(object sender, RoutedEventArgs e)
        {
            var wrapper = new WebClientPerfWrapper();
            var results = new List<Tuple<TimeSpan, string, string>>();
            var numTries = Int32.Parse(TrialsTextBox.Text);
            ReportSummary.StatSummary.ViewModel = new StatSummaryViewModel();
            foreach (var testCase in _testUrls)
            {
                var request = testCase;
                if (string.IsNullOrEmpty(request)) continue;

                var tasks = Enumerable.Range(0, numTries).Select(x => wrapper.RunPerformanceRequestTask(request));
                var newResults = await Task.WhenAll(tasks);
                results.AddRange(newResults);
                ReportSummary.AddDataToChart(request, newResults.Select(x => x.Item1).ToList(), results.Select(x => x.Item1).ToList());
                ReportSummary.StatSummary.ViewModel.Requests.Add(request);
                foreach (var result in results)
                    ReportSummary.StatSummary.ViewModel.Runtimes.Add(result.Item1);
                ReportSummary.IndividualRuntime.ViewModel = new IndividualRuntimeViewModel(results.Select(x => x.Item1).ToList());
            }
        }

        private void SerializeResults_OnClick(object sender, RoutedEventArgs e)
        {

        }

        private void GenerateTests_OnClick(object sender, RoutedEventArgs e)
        {
            _testUrls = (UrlGeneratorPlugins.SelectedItem as IThreadedPerformanceSessionTestUrlGenerator).GenerateTests();
        }
    }
}
