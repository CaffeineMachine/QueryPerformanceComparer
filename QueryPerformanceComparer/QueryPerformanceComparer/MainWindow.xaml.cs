﻿using System;
using System.IO;
using System.Reflection;
using System.Windows;
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
            gridPanel.Children.Clear();
            gridPanel.ColumnDefinitions.Clear();
            var timeDurationView = new TimeDurationView();
            gridPanel.Children.Add(timeDurationView);
        }

        private void QuantityTest_Click(object sender, RoutedEventArgs e)
        {
            gridPanel.Children.Clear();
            gridPanel.ColumnDefinitions.Clear();
            var sessionControl = new QuerySessionControl();
            gridPanel.Children.Add(sessionControl);
        }
    }
}
