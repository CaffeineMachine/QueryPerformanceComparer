using System.ComponentModel;
using System.Windows.Controls;
using QuerySessionSummaryLib;
using QuerySessionSummaryLib.Annotations;

namespace QuerySessionSummaryControl
{
    /// <summary>
    /// Interaction logic for StatSummaryControl.xaml
    /// </summary>
    public partial class StatSummaryControl : UserControl, INotifyPropertyChanged
    {
        private StatSummaryViewModel _viewModel;
        public StatSummaryControl() : this(new StatSummaryViewModel())
        {
        }

        public StatSummaryControl(StatSummaryViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
        }

        public StatSummaryViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                if (_viewModel != value)
                {
                    _viewModel = value;
                    DataContext = _viewModel;
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
