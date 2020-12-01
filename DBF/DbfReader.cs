using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbfTests
{
    internal class DbfReader
    {
        internal class ValueRow
        {
            public double Value { get; }
            public DateTime Timestamp { get; }

            public ValueRow(double value, DateTime timestamp)
            {
                Value = value;
                Timestamp = timestamp;
            }
        }

        private readonly string ColumnAttType = "ATT_TYPE";
        private readonly string ColumnValInt = "VALINT";
        private readonly string ColumnValReal = "VALREAL";
        private readonly string ColumnValBool = "VALBOOL";
        private readonly string ColumnDate = "DATE_NDX";
        private readonly string ColumnTime = "TIME_NDX";

        public IEnumerable<ValueRow> ReadValues(string filePath)
        {
            using (var connection = new OleDbConnection($"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={Path.GetDirectoryName(filePath)};Extended Properties=dBASE IV;User ID=;Password=;"))
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                using (var dataAdapter = new OleDbDataAdapter($"select ATT_TYPE, VALINT, VALREAL, VALBOOL, DATE_NDX, TIME_NDX from {Path.GetFileName(filePath)} where VALID=1 and RELIABLE=1", connection))
                using (var dataset = new DataSet())
                {
                    dataAdapter.Fill(dataset);

                    // Only one table should exist anyway
                    if (dataset.Tables.Count == 1)
                        return dataset.Tables[0].Rows.Cast<DataRow>().Select(GetValueRow);
                    else
                        throw new ApplicationException($"File {filePath} has been ignored. 1 table must exist within the file!");
                }
            }
        }

        private ValueRow GetValueRow(DataRow dataRow)
        {
            var attType = (double)dataRow[ColumnAttType];
            double value = 0;

            switch (attType)
            {
                case 1:
                    value = (double)dataRow[ColumnValInt];
                    break;
                case 2:
                    value = (double)dataRow[ColumnValReal];
                    break;
                case 3:
                    value = (double)dataRow[ColumnValBool];
                    break;
            }

            string date = ((double)dataRow[ColumnDate]).ToString();
            string timestring = $"0000{dataRow[ColumnTime]}";
            string time = timestring.Substring(timestring.Length - 4);
            DateTime timestamp = DateTime.ParseExact($"{date}{time}", "yyyMMddHHmm", CultureInfo.InvariantCulture).AddYears(1900);

            return new ValueRow(value, timestamp);
        }
    }
}
