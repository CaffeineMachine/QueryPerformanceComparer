using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ninject;
using QuerySessionSummaryControl;

namespace QueryPerformanceComparer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IKernel _kernel;
        public MainWindow()
        {
            InitializeComponent();
            _kernel = new StandardKernel();
            LoadAllBinDirectoryAssemblies();
            if (File.Exists("TypeMappings.xml"))
                _kernel.Load("TypeMappings.xml");
        }

        private void Quit_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public void LoadAllBinDirectoryAssemblies()
        {
            var binPath = AppDomain.CurrentDomain.BaseDirectory; // note: don't use CurrentEntryAssembly or anything like that.

            foreach (var dll in Directory.GetFiles(binPath, "*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    Assembly.LoadFile(dll);
                }
                catch (FileLoadException)
                { } // The Assembly has already been loaded.
                catch (BadImageFormatException)
                { } // If a BadImageFormatException exception is thrown, the file is not an assembly.

            } // foreach dll
        }

        private void DurationTest_Click(object sender, RoutedEventArgs e)
        {
            GridPanel.Children.Clear();
            GridPanel.ColumnDefinitions.Clear();
            var timeDurationView = new TimeDurationView();
            GridPanel.Children.Add(timeDurationView);
        }

        private void QuantityTest_Click(object sender, RoutedEventArgs e)
        {
            GridPanel.Children.Clear();
            var sessionControl = new QuerySessionControl();
            GridPanel.Children.Add(sessionControl);
        }

        private void ReturnToMenu_OnClick(object sender, RoutedEventArgs e)
        {
            GridPanel.Children.Clear();
            GridPanel.Children.Add(StartMenu);
        }

        private void PrintItem_OnClick(object sender, RoutedEventArgs e)
        {
            Transform transform = this.LayoutTransform;
            // reset current transform (in case it is scaled or rotated)
            this.LayoutTransform = null;

            Size size = new Size(this.ActualWidth, this.ActualHeight);
            this.Measure(size);
            this.Arrange(new Rect(size));

            // Create a render bitmap and push the surface to it
            this.UpdateLayout();
            new PrintDialog().PrintVisual(this, "Report");
        }
    }
}
