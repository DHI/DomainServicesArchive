namespace DHI.Services.TimeSeries.Text
{
    using System.Collections.Generic;

    public class TimeSeriesTextConfiguration
    {
        public string RootFilePath { get; set; }

        public bool? DateTimeAsUnixTime { get; set; }

        public string DateTimeFormat { get; set; }

        public int? DateTimeColumn { get; set; }

        public int? DateColumn { get; set; }

        public string DateFormat { get; set; }

        public int? TimeColumn { get; set; }

        public int? YearColumn { get; set; }

        public int? MonthColumn { get; set; }

        public int? DayColumn { get; set; }

        public int? HourColumn { get; set; }

        public int? MinuteColumn { get; set; }

        public int? SecondColumn { get; set; }

        public string TimeFormat { get; set; }

        public string TimezoneFrom { get; set; }

        public string TimezoneTo { get; set; }

        public string ValueDelimiter { get; set; } = ",";

        public string DecimalDelimiter { get; set; }

        public char? TrimCharacter { get; set; }

        public int? HeaderLineNumber { get; set; }

        public int DataLineNumber { get; set; } = 2;

        public bool? SkipIfCannotParse { get; set; } = false;

        public bool? NullIfCannotParse { get; set; } = false;

        public Dictionary<string, string> Replace { get; set; } = new Dictionary<string, string>();

        public string ValueRegExFilter { get; set; }
        
        public string ValueRegExFilterExclude { get; set; }

        public string ResampleTimeSpan { get; set; }

        public List<TimeSeriesTextColumns> TimeSeriesColumns { get; set; } = new List<TimeSeriesTextColumns>();

        /// <summary>
        /// If set, the empty cell value in the file will be filled with this value.
        /// Example: replace an empty cell value with "NaN" or "-9999" as deleted value
        /// </summary>
        /// <remarks>
        /// <para>
        /// For cell value which is in incorrect format, it would be handled by <see cref="SkipIfCannotParse"/> or <see cref="NullIfCannotParse"/> or throw exception directly.
        /// For cell value which is empty, it may be considered different as value with incorrect format, and could be filled with this value.
        /// </para> 
        /// <para>
        /// Limitation: This configuration applies only to a single data type (e.g., string, numeric, or datetime).
        /// </para>
        /// </remarks>
        public string FillEmptyValueWith { get; set; }
    }
}