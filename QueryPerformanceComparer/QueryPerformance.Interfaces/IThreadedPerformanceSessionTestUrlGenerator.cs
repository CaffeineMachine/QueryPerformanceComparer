using System.Collections.Generic;

namespace QueryPerformance.Interfaces
{
    public interface IThreadedPerformanceSessionTestUrlGenerator
    {
        List<string> TestUrls { get; }
        List<string> GenerateTests(string inputFile = "");
        string DisplayName { get; }
    }
}
