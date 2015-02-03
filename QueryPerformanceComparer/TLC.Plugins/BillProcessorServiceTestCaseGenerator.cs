using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using QueryPerformance.Interfaces;

namespace TLC.Plugins
{
    /// <summary>
    /// This class is used for generating the URL's of target files based on an input file.
    /// </summary>
    public class BillProcessorServiceTestCaseGenerator : IThreadedPerformanceSessionTestUrlGenerator
    {
        public BillProcessorServiceTestCaseGenerator()
        {
            Properties = new List<string>();
            TestUrls = new List<string>();
        }

        public readonly string BaseUrl = @"http://dev-tss/api/billProcessor";
        public List<string> TestUrls { get; set; }
        public List<string> Properties { get; set; }

        private void CreateTestUrls(string testFileParameters)
        {
            Properties.Clear();
            using (var sr = new StreamReader(testFileParameters))
            {
                var readLine = sr.ReadLine();
                if (readLine != null)
                {
                    var properties = readLine.Split(new string[] { ",", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var property in properties)
                    {
                        Properties.Add(property);
                    }
                }

                while (sr.Peek() >= 0)
                {
                    var line = sr.ReadLine();
                    var lineProperties = line.Split(new string[] { ",", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    if (lineProperties.Length == Properties.Count)
                    {
                        var constraint = new BillProcessorTestConstraint();
                        int j = 0;
                        foreach (var property in Properties)
                        {
                            if (Regex.IsMatch(property, "Repository", RegexOptions.IgnoreCase))
                            {
                                constraint.Repository = lineProperties[j];
                            }
                            else if (Regex.IsMatch(property, "Count"))
                            {
                                constraint.Count = Int32.Parse(lineProperties[j]);
                            }
                            else
                            {
                                constraint.BillType = lineProperties[j];
                            }
                            j++;
                        }

                        for (int i = 1; i < constraint.Count; i++)
                        {
                            var newUrl = BaseUrl + "?";
                            newUrl = string.Format("{0}{1}&{2}", newUrl, "repository=" + constraint.Repository.Replace(" ", "%20"),
                                "docName=" + constraint.BillType + i + "-I");
                            TestUrls.Add(newUrl);
                        }
                    }
                }
            }
        }

        public List<string> GenerateTests(string inputFile = "")
        {
            var testUrls = new List<string>();
            var dialog = new OpenFileDialog();
            var result = dialog.ShowDialog();
            if (result == true)
            {
                testUrls.Clear();
                var fileNames = dialog.FileNames;
                foreach (var filename in fileNames)
                {
                    var testCaseGenerator = new BillProcessorServiceTestCaseGenerator();
                    testCaseGenerator.CreateTestUrls(filename);
                    testUrls.AddRange(testCaseGenerator.TestUrls);
                }
            }
            return testUrls.Distinct().ToList();
        }

        public string DisplayName
        {
            get { return "Bill Processor Service Url Generator"; }
        }
    }

    public class BillProcessorTestConstraint
    {
        public int Count { get; set; }
        public string Repository { get; set; }
        public string BillType { get; set; }
    }
}
