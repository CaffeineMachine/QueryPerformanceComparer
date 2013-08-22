using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using QuerySessionSummaryControl;

namespace QueryPerformanceComparer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int sessions = 0;
        private QuerySessionControl _currentSession;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Quit_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CreateSession_OnClick(object sender, RoutedEventArgs e)
        {
            var querySessionControl = new QuerySessionControl();
            _currentSession = querySessionControl;
            querySessionControl.Header = string.Format("Session{0}", ++sessions);
            tabs.Items.Add(querySessionControl);
            querySessionControl.IsSelected = true;
        }

        private void PrintSession_OnClick(object sender, RoutedEventArgs e)
        {
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(_currentSession, "Print Performance Session Report");
            }
        }
    }
}
