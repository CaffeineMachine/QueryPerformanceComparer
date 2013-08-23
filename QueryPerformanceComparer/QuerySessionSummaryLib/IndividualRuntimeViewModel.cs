using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using QuerySessionSummaryLib.Annotations;

namespace QuerySessionSummaryLib
{
    public class IndividualRuntimeViewModel : INotifyPropertyChanged
    {
        public IndividualRuntimeViewModel(IEnumerable<TimeSpan> runtimes)
        {
            Runtimes = new ObservableCollection<double>(runtimes.Select(x => x.TotalMilliseconds).ToList());
        }

        public ObservableCollection<double> Runtimes { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
