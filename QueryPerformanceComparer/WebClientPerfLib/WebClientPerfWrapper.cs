using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace WebClientPerfLib
{
    public class WebClientPerfWrapper
    {
        List<double> runtimes = new List<double>();
        public TimeSpan RunPerformanceRequest(string uri)
        {
            TimeSpan elapsed;
            var req = WebRequest.Create(uri);
            string responseDoc;
            //req.Proxy = new WebProxy("127.0.0.1", 8888);
            req.Proxy = null;
            var sw = new Stopwatch();
            sw.Start();
            using (var res = req.GetResponse())
            {
                using (var s = res.GetResponseStream())
                using (var sr = new StreamReader(s))
                    responseDoc = sr.ReadToEnd();
                elapsed = sw.Elapsed;
                runtimes.Add(elapsed.TotalMilliseconds);
                sw.Reset();
            }
            return elapsed;
        }

        public void OutputTimesToLog()
        {
            var dialog = new SaveFileDialog();
            dialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (var s = new FileStream(dialog.FileName, FileMode.Create))
                using (TextWriter writer = new StreamWriter(s))
                {
                    foreach (var item in runtimes)
                    {
                        writer.WriteLine(item);
                    }
                }
            }
        }
    }
}
