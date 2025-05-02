namespace DHI.Services.TimeSeries.Text
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <inheritdoc />
    public class TimeSeriesRepository : BaseTimeSeriesRepository<string, double>
    {
        private readonly string _jsonFilePath;

        /// <inheritdoc />
        public TimeSeriesRepository(string jsonFilePath)
        {
            _jsonFilePath = jsonFilePath ?? throw new ArgumentNullException(nameof(jsonFilePath));

            if (!File.Exists(jsonFilePath))
            {
                throw new ArgumentException($"Folder '{jsonFilePath}' does not exist");
            }
        }

        public override Maybe<TimeSeries<string, double>> Get(string id, ClaimsPrincipal user = null)
        {
            return new TimeSeries<string, double>(id, id).ToMaybe();
        }

        /// <inheritdoc />
        public override Maybe<ITimeSeriesData<double>> GetValues(string id, ClaimsPrincipal user = null)
        {
            return ReadTimeSeries(id).ToMaybe();
        }

        private static List<TimeSeriesTextColumns> GetColumns(TimeSeriesText timeSeriesText, List<string> lines)
        {
            if (timeSeriesText.Configuration.TimeSeriesColumns != null && timeSeriesText.Configuration.TimeSeriesColumns.Count > 0)
            {
                if (timeSeriesText.Configuration.TimeSeriesColumns.FirstOrDefault(r => r.Id == timeSeriesText.Id) == null)
                {
                    throw new ArgumentException($"The item {timeSeriesText.Id} does not exist");
                }

                return timeSeriesText.Configuration.TimeSeriesColumns;
            }

            if (!timeSeriesText.Configuration.HeaderLineNumber.HasValue)
            {
                throw new Exception("You must specify a HeaderLineNumber or specify the TimeSeriesColumns");
            }

            var cells = lines[timeSeriesText.Configuration.HeaderLineNumber.Value - 1].Split(new[] {timeSeriesText.Configuration.ValueDelimiter}, timeSeriesText.Configuration.ValueDelimiter == "  " ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None).ToList();
            if (timeSeriesText.Configuration.TrimCharacter.HasValue)
            {
                cells = cells.Select(r => r.Trim().TrimStart(timeSeriesText.Configuration.TrimCharacter.Value).TrimEnd(timeSeriesText.Configuration.TrimCharacter.Value)).ToList();
            }

            return cells.Select((t, i) => new TimeSeriesTextColumns
            {
                Column = i + 1,
                Id = t
            }).ToList();
        }

        private ITimeSeriesData<double> ReadTimeSeries(string id)
        {
            var timeSeries = new TimeSeriesText(id, _jsonFilePath);

            var text = File.ReadAllText(timeSeries.FilePath, Encoding.Default);
            foreach (var replace in timeSeries.Configuration.Replace)
            {
                text = text.Replace(replace.Key, timeSeries.Configuration.Replace[replace.Key]);
            }
            var lines = text.Split(new [] { "\n", "\r\n" }, StringSplitOptions.None).ToList();

            var columns = GetColumns(timeSeries, lines);

            int index;
            if (columns.Any(r => r.Id == timeSeries.Id))
            {
                index = columns.First(r => r.Id == timeSeries.Id).Column - 1;
            }
            else
            {
                if (timeSeries.Configuration.SkipIfCannotParse.HasValue && timeSeries.Configuration.SkipIfCannotParse.Value)
                {
                    return new TimeSeriesData<double>();
                }
                throw new Exception($"It is not possible to identify the column number for {timeSeries.Id} in the columns {string.Join(", ", columns.Select(r => "'" + r.Id + "'"))}");
            }

            TimeZoneInfo timezoneFrom = null;
            TimeZoneInfo timezoneTo = null;
            if (!string.IsNullOrEmpty(timeSeries.Configuration.TimezoneFrom) && !string.IsNullOrEmpty(timeSeries.Configuration.TimezoneTo))
            {
                try
                {
                    timezoneFrom = TimeZoneInfo.FindSystemTimeZoneById(timeSeries.Configuration.TimezoneFrom);
                }
                catch
                {
                    throw new Exception($"Cannot look up time zone {timeSeries.Configuration.TimezoneFrom}. Options are {string.Join(", ", TimeZoneInfo.GetSystemTimeZones().Select(r => "'" + r.Id + "'"))}");
                }

                try
                {
                    timezoneTo = TimeZoneInfo.FindSystemTimeZoneById(timeSeries.Configuration.TimezoneTo);
                }
                catch
                {
                    throw new Exception($"Cannot look up time zone {timeSeries.Configuration.TimezoneTo}. Options are {string.Join(", ", TimeZoneInfo.GetSystemTimeZones().Select(r => "'" + r.Id + "'"))}");
                }
            }

            var values = new List<KeyValuePair<DateTime, double?>>();
            for (var i = timeSeries.Configuration.DataLineNumber - 1; i < lines.Count; i++)
            {
                try
                {
                    if (lines[i].Trim() == string.Empty)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(timeSeries.Configuration.ValueRegExFilter))
                    {
                        if (!Regex.Match(lines[i], timeSeries.Configuration.ValueRegExFilter, RegexOptions.None).Success)
                        {
                            continue;
                        }
                    }

                    if (!string.IsNullOrEmpty(timeSeries.Configuration.ValueRegExFilterExclude))
                    {
                        if (Regex.Match(lines[i], timeSeries.Configuration.ValueRegExFilterExclude, RegexOptions.None).Success)
                        {
                            continue;
                        }
                    }
                    
                    var cells = lines[i].Split(new[] {timeSeries.Configuration.ValueDelimiter}, timeSeries.Configuration.ValueDelimiter == "  " ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None).ToList();
                    if (timeSeries.Configuration.TrimCharacter.HasValue)
                    {
                        cells = cells.Select(r => r.Trim().TrimStart(timeSeries.Configuration.TrimCharacter.Value).TrimEnd(timeSeries.Configuration.TrimCharacter.Value)).ToList();
                    }

                    var valueString = string.Empty;
                    if (index < cells.Count)
                    {
                        valueString = cells[index];
                    }
                    // handle case where value is empty and FillEmptyValueWith is set
                    if(!string.IsNullOrEmpty(timeSeries.Configuration.FillEmptyValueWith) && string.IsNullOrEmpty(valueString))
                    {
                        valueString = timeSeries.Configuration.FillEmptyValueWith;
                    }

                    if (!string.IsNullOrEmpty(timeSeries.Configuration.DecimalDelimiter))
                    {
                        valueString = valueString.Replace(timeSeries.Configuration.DecimalDelimiter, ".");
                    }

                    double? value;
                    if (double.TryParse(valueString, NumberStyles.Any, CultureInfo.InvariantCulture, out var valueParsed))
                    {
                        value = valueParsed;
                    }
                    else
                    {
                        if (timeSeries.Configuration.SkipIfCannotParse.HasValue && timeSeries.Configuration.SkipIfCannotParse.Value)
                        {
                            continue;
                        }

                        if (timeSeries.Configuration.NullIfCannotParse.HasValue && timeSeries.Configuration.NullIfCannotParse.Value)
                        {
                            value = null;
                        }
                        else
                        {
                            throw new Exception($"Cannot parse {valueString} into a number");
                        }
                    }

                    DateTime dateTime;
                    if (timeSeries.Configuration.DateColumn.HasValue && !string.IsNullOrEmpty(timeSeries.Configuration.TimeFormat) && !string.IsNullOrEmpty(timeSeries.Configuration.TimeFormat))
                    {
                        if (!DateTime.TryParseExact(cells[timeSeries.Configuration.DateColumn.Value - 1], timeSeries.Configuration.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var date))
                        {
                            if (timeSeries.Configuration.SkipIfCannotParse is true)
                            {
                                continue;
                            }
                            throw new Exception($"Cannot parse {cells[timeSeries.Configuration.DateColumn.Value - 1]} into date using {timeSeries.Configuration.DateFormat}. The format defined gives {DateTime.Now.ToString(timeSeries.Configuration.DateFormat)}");
                        }

                        if (!DateTime.TryParseExact(cells[timeSeries.Configuration.TimeColumn.Value - 1], timeSeries.Configuration.TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var time))
                        {
                            if (timeSeries.Configuration.SkipIfCannotParse is true)
                            {
                                continue;
                            }
                            throw new Exception($"Cannot parse {cells[timeSeries.Configuration.TimeColumn.Value - 1]} into time using {timeSeries.Configuration.TimeFormat}. The format defined gives {DateTime.Now.ToString(timeSeries.Configuration.TimeFormat)}");
                        }

                        dateTime = DateTime.FromOADate(date.ToOADate() + time.ToOADate() - Math.Truncate(time.ToOADate()));
                    }
                    else if (timeSeries.Configuration.DateTimeColumn.HasValue && !string.IsNullOrEmpty(timeSeries.Configuration.DateTimeFormat))
                    {
                        if (!DateTime.TryParseExact(cells[timeSeries.Configuration.DateTimeColumn.Value - 1], timeSeries.Configuration.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dateTime))
                        {
                            if (!DateTime.TryParseExact(cells[timeSeries.Configuration.DateTimeColumn.Value - 1] + " 00:00:00", timeSeries.Configuration.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dateTime))
                            {
                                if (timeSeries.Configuration.SkipIfCannotParse is true)
                                {
                                    continue;
                                }
                                throw new Exception($"Cannot parse {cells[timeSeries.Configuration.DateTimeColumn.Value - 1]} into date time using {timeSeries.Configuration.DateTimeFormat}. The format defined gives {DateTime.Now.ToString(timeSeries.Configuration.DateTimeFormat)}");
                            }
                        }
                    }
                    else if (timeSeries.Configuration.YearColumn.HasValue)
                    {
                        if (!int.TryParse(cells[timeSeries.Configuration.YearColumn.Value - 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var year))
                        {
                            throw new Exception($"Cannot parse {cells[timeSeries.Configuration.YearColumn.Value - 1]} into a year");
                        }

                        var month = 1;
                        if (timeSeries.Configuration.MonthColumn.HasValue && !int.TryParse(cells[timeSeries.Configuration.MonthColumn.Value - 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out month))
                        {
                            throw new Exception($"Cannot parse {cells[timeSeries.Configuration.MonthColumn.Value - 1]} into a month");
                        }

                        var day = 1;
                        if (timeSeries.Configuration.DayColumn.HasValue && !int.TryParse(cells[timeSeries.Configuration.DayColumn.Value - 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out day))
                        {
                            throw new Exception($"Cannot parse {cells[timeSeries.Configuration.DayColumn.Value - 1]} into a day");
                        }

                        var hour = 0;
                        if (timeSeries.Configuration.HourColumn.HasValue && !int.TryParse(cells[timeSeries.Configuration.HourColumn.Value - 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out hour))
                        {
                            throw new Exception($"Cannot parse {cells[timeSeries.Configuration.HourColumn.Value - 1]} into a hour");
                        }

                        var minute = 0;
                        if (timeSeries.Configuration.MinuteColumn.HasValue && !int.TryParse(cells[timeSeries.Configuration.MinuteColumn.Value - 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out minute))
                        {
                            throw new Exception($"Cannot parse {cells[timeSeries.Configuration.MinuteColumn.Value - 1]} into a minute");
                        }

                        var second = 0;
                        if (timeSeries.Configuration.SecondColumn.HasValue && !int.TryParse(cells[timeSeries.Configuration.SecondColumn.Value - 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out second))
                        {
                            throw new Exception($"Cannot parse {cells[timeSeries.Configuration.SecondColumn.Value - 1]} into a second");
                        }

                        dateTime = new DateTime(year, month, day, hour, minute, second);
                    }
                    else if (timeSeries.Configuration.DateTimeAsUnixTime.HasValue && timeSeries.Configuration.DateTimeAsUnixTime.Value)
                    {
                        if (cells[timeSeries.Configuration.DateTimeColumn.Value - 1].Length == 10) 
                        {
                            dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Convert.ToInt64(cells[timeSeries.Configuration.DateTimeColumn.Value - 1]));
                        }
                        else if (cells[timeSeries.Configuration.DateTimeColumn.Value - 1].Length == 13)
                        {
                            dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Convert.ToInt64(cells[timeSeries.Configuration.DateTimeColumn.Value - 1]));
                        }
                        else
                        {
                            if (timeSeries.Configuration.SkipIfCannotParse.HasValue && timeSeries.Configuration.SkipIfCannotParse.Value)
                            {
                                continue;
                            }
                            throw new Exception($"Cannot parse {cells[timeSeries.Configuration.DateTimeColumn.Value - 1]} into a DateTime");
                        }
                    }
                    else
                    {
                        throw new Exception("You must specify either DateTimeColumn. DateTimeFormat or DateColumn, DateFormat, TimeColumn, TimeFormat");
                    }

                    if (timezoneFrom != null && timezoneTo != null)
                    {
                        dateTime = TimeZoneInfo.ConvertTime(dateTime, timezoneFrom, timezoneTo);
                    }

                    // Remove any if existing
                    values.RemoveAll(r => r.Key == dateTime);

                    values.Add(new KeyValuePair<DateTime, double?>(dateTime, value));
                }
                catch (Exception exception)
                {
                    throw new Exception("Cannot parse line " + lines[i] + ". Exception: " + exception.Message);
                }
            }

            values = values.OrderBy(r => r.Key).ToList();
            var result = new TimeSeriesData<double>(values.Select(dateTime => dateTime.Key).ToList(), values.Select(value => value.Value).ToList());

            if (!string.IsNullOrEmpty(timeSeries.Configuration.ResampleTimeSpan))
            {
                if (TimeSpan.TryParse(timeSeries.Configuration.ResampleTimeSpan, CultureInfo.InvariantCulture, out var timeSpan))
                {
                    if (timeSpan > result.TimeSpan())
                    {
                        result = new TimeSeriesData<double>();
                    }
                    else
                    {
                        result = result.ResampleNiceTimesteps(timeSpan);
                    }                    
                }
                else
                {
                    throw new Exception($"Cannot parse {timeSeries.Configuration.ResampleTimeSpan} as time span");
                }
            }

            return result;
        }
    }
}