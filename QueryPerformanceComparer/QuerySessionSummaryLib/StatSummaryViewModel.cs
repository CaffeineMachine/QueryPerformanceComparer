using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using QuerySessionSummaryLib.Annotations;

namespace QuerySessionSummaryLib
{
    [DataContract]
    public class StatSummaryViewModel : INotifyPropertyChanged
    {
        private List<TimeSpan> _runtimes;
        public StatSummaryViewModel() : this(string.Empty, new List<TimeSpan> {new TimeSpan(0)})
        {
        }

        public StatSummaryViewModel(string request, IEnumerable<TimeSpan> runtimes)
        {
            Request = request;
            RecalculateProperties(runtimes);
        }

        public void RecalculateProperties(IEnumerable<TimeSpan> runtimes)
        {
            var timeSpans = runtimes as IList<TimeSpan> ?? runtimes.ToList();
            _runtimes = timeSpans.ToList();
            _runtimes.Sort();
            Minimum = _runtimes.Min().TotalMilliseconds;
            Maximum = _runtimes.Max().TotalMilliseconds;
            Mean = _runtimes.Average(x => x.TotalMilliseconds);
            Median = _runtimes[timeSpans.Count()/2].TotalMilliseconds;
            TotalRuntime = _runtimes.Select(x => x.TotalMilliseconds).Sum();
        }

        private double _minimum;
        [DataMember]
        public double Minimum
        {
            get { return _minimum; }
            set
            {
                if (_minimum != value)
                {
                    _minimum = value;
                    OnPropertyChanged("Minimum");
                }
            }
        }

        private double _maximum;
        [DataMember]
        public double Maximum
        {
            get { return _maximum; }
            set
            {
                if (_maximum != value)
                {
                    _maximum = value;
                    OnPropertyChanged("Maximum");
                }
            }
        }

        private double _mean;
        [DataMember]
        public double Mean
        {
            get { return _mean; }
            set
            {
                if (_mean != value)
                {
                    _mean = value;
                    OnPropertyChanged("Mean");
                }
            }
        }

        private double _median;
        [DataMember]
        public double Median
        {
            get { return _median; }
            set
            {
                if (_median != value)
                {
                    _median = value;
                    OnPropertyChanged("Median");
                }
            }
        }

        private double _totalRuntime;
        [DataMember]
        public double TotalRuntime
        {
            get { return _totalRuntime; }
            set
            {
                if (_totalRuntime != value)
                {
                    _totalRuntime = value;
                    OnPropertyChanged("TotalRuntime");
                }
            }
        }

        private string _request;
        [DataMember]
        public string Request
        {
            get { return _request; }
            set
            {
                if (_request != value)
                {
                    _request = value;
                    OnPropertyChanged("Request");
                }
            }
        }

        [DataMember]
        public List<TimeSpan> Runtimes
        {
            get { return _runtimes; }
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
