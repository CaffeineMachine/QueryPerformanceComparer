using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace WebClientPerfLib
{
    public class WebClientPerfWrapper
    {
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
            }
            return elapsed;
        }
    }
}
