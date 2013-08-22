using System.ComponentModel;
using QuerySessionSummaryLib.Annotations;

namespace QuerySessionSummaryLib
{
    public class ReportSummaryViewModel : INotifyPropertyChanged
    {
        private StatSummaryViewModel _statSummaryViewModel;
        public StatSummaryViewModel StatSummaryViewModel
        {
            get { return _statSummaryViewModel; }
            set
            {
                if (_statSummaryViewModel != value)
                {
                    _statSummaryViewModel = value;
                    OnPropertyChanged("StatSummaryViewModel");
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
