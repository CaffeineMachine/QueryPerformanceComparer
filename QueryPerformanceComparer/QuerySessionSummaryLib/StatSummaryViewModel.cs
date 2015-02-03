using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using QuerySessionSummaryLib.Annotations;

namespace QuerySessionSummaryLib
{
    [DataContract]
    public class StatSummaryViewModel : INotifyPropertyChanged
    {
        public StatSummaryViewModel()
            : this(string.Empty, new List<TimeSpan>())
        {
        }

        public StatSummaryViewModel(string request, IEnumerable<TimeSpan> runtimes)
        {
            Runtimes = new ObservableCollection<TimeSpan>(runtimes);
            Requests = !string.IsNullOrEmpty(request) ? new ObservableCollection<string>(new List<string> { request }) : new ObservableCollection<string>();
            Runtimes.CollectionChanged += RuntimesOnCollectionChanged;
            Requests.CollectionChanged += RequestsOnCollectionChanged;
            RecalculateProperties(runtimes);
        }

        private void RequestsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RecalculateProperties(Runtimes);
            OnPropertyChanged("Requests");
        }

        private void RuntimesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RecalculateProperties(Runtimes);
            OnPropertyChanged("Runtimes");
        }

        public void RecalculateProperties(IEnumerable<TimeSpan> runtimes)
        {
            if (runtimes == null || !runtimes.Any()) return;

            Minimum = Runtimes.Count > 0 ? Runtimes.Min().TotalMilliseconds : 0;
            Maximum = Runtimes.Count > 0 ? Runtimes.Max().TotalMilliseconds : 0;
            Mean = Runtimes.Count > 0 ? Runtimes.Average(x => x.TotalMilliseconds) : 0;
            Median = Runtimes.Count > 0 ? Runtimes[runtimes.Count() / 2].TotalMilliseconds : 0;
            TotalRuntime = Runtimes.Count > 0 ? Runtimes.Select(x => x.TotalMilliseconds).Sum() : 0;
            CalculateStdDev();
        }

        private void CalculateStdDev()
        {
            var varianceDifferences = new List<double>();
            foreach (var runtime in Runtimes)
            {
                var variance = runtime.TotalMilliseconds - Mean;
                var varianceSquared = Math.Pow(variance, 2);
                varianceDifferences.Add(varianceSquared);
            }
            StdDev = Math.Sqrt(varianceDifferences.Sum() / Runtimes.Count);
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

        [DataMember]
        public ObservableCollection<string> Requests
        {
            get;
            set;
        }

        private double _stdDev;

        [DataMember]
        public double StdDev
        {
            get { return _stdDev; }
            set
            {
                if (_stdDev != value)
                {
                    _stdDev = value;
                    OnPropertyChanged("StdDev");
                }
            }
        }

        [DataMember]
        public ObservableCollection<TimeSpan> Runtimes { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            RecalculateProperties(Runtimes);
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
