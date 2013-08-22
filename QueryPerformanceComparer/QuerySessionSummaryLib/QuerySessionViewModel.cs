using System.ComponentModel;
using QuerySessionSummaryLib.Annotations;

namespace QuerySessionSummaryLib
{
    public class QuerySessionViewModel : INotifyPropertyChanged
    {
        public QuerySessionViewModel()
        {
            _viewModel = new ReportSummaryViewModel();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private ReportSummaryViewModel _viewModel;
        public ReportSummaryViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                if (_viewModel != value)
                {
                    _viewModel = value;
                    OnPropertyChanged("ViewModel");
                }
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
