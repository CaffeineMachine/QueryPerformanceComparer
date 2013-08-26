using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using QuerySessionSummaryLib;
using WebClientPerfLib;

namespace QuerySessionSummaryControl
{
    /// <summary>
    /// Interaction logic for TimeDurationView.xaml
    /// </summary>
    public partial class TimeDurationView : UserControl
    {
        public TimeDurationView()
        {
            InitializeComponent();
            this.DataContext = new TimeDurationViewModel();
        }

        private void LoadQueries_OnClick(object sender, RoutedEventArgs e)
        {
            (this.DataContext as TimeDurationViewModel).Queries.Clear();
            var dialog = new OpenFileDialog();
            dialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var fs = File.OpenRead(dialog.FileName))
                    {
                        using (TextReader reader = new StreamReader(fs))
                        {
                            while (reader.Peek() >= 0)
                                (this.DataContext as TimeDurationViewModel).Queries.Add(reader.ReadLine());
                        }
                    }

                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        private void LoadUrls_OnClick(object sender, RoutedEventArgs e)
        {
            (this.DataContext as TimeDurationViewModel).Urls.Clear();
            var dialog = new OpenFileDialog();
            dialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var fs = File.OpenRead(dialog.FileName))
                    {
                        using (TextReader reader = new StreamReader(fs))
                        {
                            while (reader.Peek() >= 0)
                                (this.DataContext as TimeDurationViewModel).Urls.Add(reader.ReadLine());
                        }
                    }

                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        private void RunTests_OnClick(object sender, RoutedEventArgs e)
        {
            var queries = (this.DataContext as TimeDurationViewModel).Queries;
            var urls = (this.DataContext as TimeDurationViewModel).Urls;
            if (!queries.Any())
            {
                MessageBox.Show("No queries loaded. Aborting tests");
                return;
            }

            double time = 0;
            if (!Double.TryParse(testTime.Text, out time))
            {
                return;
            }
            var wrapper = new WebClientPerfWrapper();
            var sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < time * 60 * 1000)
            {
                foreach (var query in queries)
                {
                    if (sw.ElapsedMilliseconds >= time * 60 * 1000)
                        break;
                    string request = string.Empty;
                    if (urls.Any())
                    {
                        foreach (var url in urls)
                        {
                            if (sw.ElapsedMilliseconds >= time * 60 * 1000)
                                break;
                            request = string.Format("{0}?{1}", url, query);
                            wrapper.RunPerformanceRequest(request);
                        }
                    }
                    else
                    {
                        MessageBox.Show("No urls loaded. Aborting tests");
                        return;
                    }
                }

            }
            sw.Stop();
            wrapper.OutputTimesToLog();
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
                            new DataContractSerializer(typeof(TimeDurationViewModel));
                        serializer.WriteObject(fs, (this.DataContext) as TimeDurationViewModel);
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
