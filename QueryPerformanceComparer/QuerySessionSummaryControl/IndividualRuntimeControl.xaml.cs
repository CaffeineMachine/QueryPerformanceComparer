using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using QuerySessionSummaryLib;
using QuerySessionSummaryLib.Annotations;

namespace QuerySessionSummaryControl
{
    /// <summary>
    /// Interaction logic for IndividualRuntimeControl.xaml
    /// </summary>
    public partial class IndividualRuntimeControl : UserControl, INotifyPropertyChanged
    {
        private IndividualRuntimeViewModel _viewModel;
        public IndividualRuntimeControl()
        {
            InitializeComponent();
            ViewModel = new IndividualRuntimeViewModel(new List<TimeSpan>());
        }

        public IndividualRuntimeViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                if (_viewModel != value)
                {
                    _viewModel = value;
                    this.DataContext = ViewModel;
                    OnPropertyChanged("ViewModel");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
