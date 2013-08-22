using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using QuerySessionSummaryLib;
using WebClientPerfLib;

namespace QuerySessionSummaryControl
{
    /// <summary>
    /// Interaction logic for QuerySessionControl.xaml
    /// </summary>
    public partial class QuerySessionControl : TabItem
    {
        public QuerySessionControl()
        {
            InitializeComponent();
        }

        private void RunTests_OnClick(object sender, RoutedEventArgs e)
        {
            var wrapper = new WebClientPerfWrapper();
            var runtimes = new List<TimeSpan>();
            int numTries;
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
                runtimes.Add(wrapper.RunPerformanceRequest(string.Format("{0}?{1}", urlPath.Text, query.Text)));
            }

            this.ReportSummary.GenerateChart(runtimes);
            this.ReportSummary.statSummary.ViewModel = new StatSummaryViewModel(runtimes);
        }
    }
}
