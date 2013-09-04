using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace WebClientPerfLib
{
    public class WebClientPerfWrapper
    {
        readonly List<double> _runtimes = new List<double>();
        readonly List<string> _documents = new List<string>(); 
        public Tuple<TimeSpan, string> RunPerformanceRequest(string uri)
        {
            Tuple<TimeSpan, string> result;
            TimeSpan elapsed;
            var req = WebRequest.Create(uri);
            string responseDoc;
            req.Proxy = null;
            var sw = new Stopwatch();
            sw.Start();
            using (var res = req.GetResponse())
            {
                using (var s = res.GetResponseStream())
                using (var sr = new StreamReader(s))
                {
                    responseDoc = sr.ReadToEnd();
                    _documents.Add(responseDoc);
                }
                elapsed = sw.Elapsed;
                _runtimes.Add(elapsed.TotalMilliseconds);
                result = new Tuple<TimeSpan, string>(elapsed, responseDoc);
                sw.Reset();
            }
            return result;
        }

        public IEnumerable<double> GetResultRuntimes()
        {
            return _runtimes;
        }

        public IEnumerable<string> GetResponses()
        {
            return _documents;
        }
    }
}
