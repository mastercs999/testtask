using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbfTests
{
    internal class OutputRow
    {
        internal DateTime Timestamp { get; set; }
        internal List<double?> Values { get; set; } = new List<double?>();
        /// <summary>
        /// shall be 1-n directory names where a 128.dbf files was found. Order must be identical to <see cref="Values"/> list order
        /// </summary>
        internal static List<string> Headers { get; set; } = new List<string>();

        internal string AsTextLine() => $"{Timestamp}\t{string.Join("\t", Values)}";
    }
}
