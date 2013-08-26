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
        public TimeSpan RunPerformanceRequest(string uri)
        {
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
                    responseDoc = sr.ReadToEnd();
                elapsed = sw.Elapsed;
                _runtimes.Add(elapsed.TotalMilliseconds);
                sw.Reset();
            }
            return elapsed;
        }

        public IEnumerable<double> GetResultRuntimes()
        {
            return _runtimes;
        }
    }
}
