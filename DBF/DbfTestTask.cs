using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbfTests
{
    [TestClass]
    public class DbfTestTask
    {
        [TestMethod]
        public void TestTask()
        {
            const string RootDir = @".\Data";
            const string RelevantFileName = "128.dbf";

            // Find all files
            string[] filePaths = Directory.GetFiles(RootDir, RelevantFileName, SearchOption.AllDirectories);

            // Now load all the files and create a dictionary for fast lookup
            DbfReader reader = new DbfReader();
            Dictionary<(string filePath, DateTime timestamp), double> idToValue = filePaths
                .SelectMany(x => reader.ReadValues(x).Select(y => (filePath: x, timestamp: y.Timestamp, value: y.Value)))
                .Distinct()
                .ToDictionary(x => (x.filePath, x.timestamp), x => x.value);

            // Transform the files into desired list of output rows
            OutputRow.Headers = filePaths.ToList();
            List<OutputRow> outputs = idToValue.Select(x => x.Key.timestamp).Distinct().OrderBy(x => x).Select(x => new OutputRow()
            {
                Timestamp = x,
                Values = OutputRow.Headers.Select(y => idToValue.TryGetValue((y, x), out double val) ? val : (double?)null).ToList()
            }).ToList();

            // This solution is definitely not the most efficient.
            // Many times, code efficiency goes against code readability.
            // I prefer code readability when performance is not an issue.

            // the following asserts should pass
            Assert.AreEqual(25790, outputs.Count);
            Assert.AreEqual(27, OutputRow.Headers.Count);
            Assert.AreEqual(27, outputs[0].Values.Count);
            Assert.AreEqual(27, outputs[11110].Values.Count);
            Assert.AreEqual(27, outputs[25789].Values.Count);
            Assert.AreEqual(633036852000000000, outputs.Min(o => o.Timestamp).Ticks);
            Assert.AreEqual(634756887000000000, outputs.Max(o => o.Timestamp).Ticks);
            Assert.AreEqual(633036852000000000, outputs[0].Timestamp.Ticks);
            Assert.AreEqual(634756887000000000, outputs.Last().Timestamp.Ticks);

            // write into file that we can compare results later on (you don't have to do something)
            string content = "Time\t" + string.Join("\t", OutputRow.Headers) + Environment.NewLine +
                          string.Join(Environment.NewLine, outputs.Select(o => o.AsTextLine()));
            File.WriteAllText(@".\output.txt", content);
        }
    }
}
