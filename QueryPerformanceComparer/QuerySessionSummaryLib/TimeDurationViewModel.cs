using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using QuerySessionSummaryLib.Annotations;

namespace QuerySessionSummaryLib
{
    [DataContract]
    public class TimeDurationViewModel : INotifyPropertyChanged
    {
        public TimeDurationViewModel()
        {
            Urls = new ObservableCollection<string>();
            Queries = new ObservableCollection<string>();
        }

        [DataMember]
        public ObservableCollection<string> Urls { get; set; }
        [DataMember]
        public ObservableCollection<string> Queries { get; set; } 

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
