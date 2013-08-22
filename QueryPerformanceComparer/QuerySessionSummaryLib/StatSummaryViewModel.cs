using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QuerySessionSummaryLib.Annotations;

namespace QuerySessionSummaryLib
{
    public class StatSummaryViewModel : INotifyPropertyChanged
    {
        private List<TimeSpan> _runtimes;
        public StatSummaryViewModel() : this(new List<TimeSpan> {new TimeSpan(0)})
        {
        }

        public StatSummaryViewModel(IEnumerable<TimeSpan> runtimes)
        {
            _runtimes = runtimes.ToList();
            _runtimes.Sort();
            Minimum = _runtimes.Min().TotalMilliseconds;
            Maximum = _runtimes.Max().TotalMilliseconds;
            Mean = _runtimes.Average(x => x.TotalMilliseconds);
            Median = _runtimes[runtimes.Count()/2].TotalMilliseconds;
            TotalRuntime = _runtimes.Select(x => x.TotalMilliseconds).Sum();
        }

        private double _minimum;
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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
