using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace WebClientPerfLib.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        [Test]
        public void Test1()
        {
            var wrapper = new WebClientPerfWrapper();
            wrapper.RunPerformanceRequest("http://www.google.com/search?q=c%23&ie=utf-8&oe=utf-8&aq=t&rls=org.mozilla:en-US:official&client=firefox-a");
        }

        [Test]
        public void Test2()
        {
            //var wrapper = new WebClientPerfWrapper();
            //var runtimes = new List<TimeSpan>();
            //for (int i = 0; i < 30; i++)
            //{
            //    runtimes.Add(wrapper.RunPerformanceRequest("http://www.google.com/search?q=c%23&ie=utf-8&oe=utf-8&aq=t&rls=org.mozilla:en-US:official&client=firefox-a"));
            //}

            //Console.WriteLine("Minimum Time:" + runtimes.Min());
            //Console.WriteLine("Maximum Time:" + runtimes.Max());
            //Console.WriteLine("Mean Time:" + runtimes.Average(x => x.TotalSeconds));
            //runtimes.Sort();
            //Console.WriteLine("Median Time:" + runtimes[15]);
        }
    }
}
