using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Ninject;
using QueryPerformance.Interfaces;

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
            LoadAllBinDirectoryAssemblies();
            if (File.Exists("TypeMappings.xml"))
                kernel.Load("TypeMappings.xml");
            var plugins = kernel.GetAll<IPlugin>().ToList();
            PluginsListView.ItemsSource = plugins;
        }

        private void Quit_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LoadAllBinDirectoryAssemblies()
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
