using DHI.Services.TimeSeries;
using System;
using System.Data;

namespace DHI.Services.Samples.TimeSeries.DomainAdapters
{
    public static class TimeSeriesDataAdapter
    {
        /// <summary>Convert ITimeSeriesData&lt;double&gt; to a DataTable with columns (Time, Value).</summary>
        public static DataTable ToDataTable(ITimeSeriesData<double>? data)
        {
            var table = new DataTable();
            table.Columns.Add("Time", typeof(DateTime));
            table.Columns.Add("Value", typeof(double));

            if (data == null || data.DateTimes.Count == 0)
                return table;

            for (int i = 0; i < data.DateTimes.Count; i++)
            {
                var row = table.NewRow();
                row["Time"] = data.DateTimes[i];
                var v = data.Values[i];
                row["Value"] = v.HasValue ? v.Value : (object)DBNull.Value;
                table.Rows.Add(row);
            }

            return table;
        }
    }
}
