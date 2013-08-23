using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Ninject;

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

        private void PrintSession_OnClick(object sender, RoutedEventArgs e)
        {
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                //printDialog.PrintVisual(_currentSession, "Print Performance Session Report");
            }
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
    }
}
