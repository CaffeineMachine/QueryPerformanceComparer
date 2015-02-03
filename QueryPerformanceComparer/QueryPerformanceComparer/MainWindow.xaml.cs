using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Ninject;
using QueryPerformance.Interfaces;
using QueryPerformance.Utilities;

namespace QueryPerformanceComparer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            IKernel kernel = new StandardKernel();
            LibraryLoader.LoadAllBinDirectoryAssemblies();
            if (File.Exists("TypeMappings.xml"))
                kernel.Load("TypeMappings.xml");
            var plugins = kernel.GetAll<IPlugin>().ToList();
            PluginsListView.ItemsSource = plugins;
        }

        private void Quit_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ReturnToMenu_OnClick(object sender, RoutedEventArgs e)
        {
            GridPanel.Children.Clear();
            GridPanel.Children.Add(StartMenu);
        }

        private void PrintItem_OnClick(object sender, RoutedEventArgs e)
        {
            // reset current transform (in case it is scaled or rotated)
            LayoutTransform = null;

            var size = new Size(ActualWidth, ActualHeight);
            Measure(size);
            Arrange(new Rect(size));

            // Create a render bitmap and push the surface to it
            UpdateLayout();
            new PrintDialog().PrintVisual(this, "Report");
        }

        private void RunModule_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(PluginsListView.SelectedItem is IPlugin))
            {
                MessageBox.Show("Error with plugin. Aborting.");
                return;
            }

            var plugin = (IPlugin) PluginsListView.SelectedItem;
            GridPanel.Children.Clear();
            GridPanel.Children.Add(plugin.Control);
        }
    }
}
