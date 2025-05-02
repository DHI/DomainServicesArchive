namespace DHI.Services.Rasters.Radar
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;

    /// <summary>
    ///     Radar image repository base class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseRadarImageRepository<T> : IRasterRepository<T> where T : IRadarImage
    {
        private const string DateTemplate = "{datetimeFormat}";
        private const string DateCounterTemplateHour = "$$$";
        private const string DateCounterTemplateDay = "###";
        private readonly string _datePattern;
        private readonly string _fileExtension;
        private readonly string _filePattern;
        private readonly string _folderPath;

        /// <summary>
        ///     Initializes a new instance of the RadarImageRepository class.
        /// </summary>
        /// <param name="connectionString">
        ///     Connection string with folder, filename template, and date time format eg:
        ///     C:\data\images;Radar33{datetimeFormat}.ascii;yyyyMMddHHmmss
        /// </param>
        protected BaseRadarImageRepository(string connectionString)
            : this(
                RadarConnection.Parse(connectionString).FolderPath,
                RadarConnection.Parse(connectionString).FilePattern,
                RadarConnection.Parse(connectionString).DateTimeFormat
            )
        {
        }

        /// <summary>
        ///     Initializes a new instance of the RadarImageRepository class.
        /// </summary>
        /// <param name="folderPath">Folder path containing radar images eg: C:\data\images</param>
        /// <param name="filePattern">File name pattern of the radar files eg: Radar33{datetimeFormat}.ascii</param>
        /// <param name="dateTimeFormat">
        ///     Datetime format in the radar file names eg: yyyyMMddHHmmss or yyyyMMddHH_$$$ where $$$
        ///     indicates hours from start, or yyyyMMddHH_### where ### indicates days from the start.
        /// </param>
        protected BaseRadarImageRepository(string folderPath, string filePattern, string dateTimeFormat)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new ArgumentException($"Folder: {folderPath} does not exist");
            }

            var directoryInfo = new DirectoryInfo(folderPath);
            var fileExtension = filePattern.Split('.').Last();

            if (directoryInfo.GetFiles($"*.{fileExtension}").Length == 0)
            {
                throw new ArgumentException($"Folder: {folderPath} contains no matching radar files");
            }

            _folderPath = folderPath;
            _fileExtension = fileExtension;
            _filePattern = filePattern;
            _datePattern = dateTimeFormat;
        }

        /// <summary>
        ///     Determines whether the repository contains an image at the specified date time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if the repository contains an image at the specified date time; otherwise, <c>false</c>.</returns>
        public bool Contains(DateTime dateTime, ClaimsPrincipal user = null)
        {
            return _GetDateTimes().Any(x => x.Key == dateTime);
        }

        /// <summary>
        ///     Gets the date/time of the first image.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The date/time of the first image.</returns>
        public DateTime FirstDateTime(ClaimsPrincipal user = null)
        {
            return _GetDateTimes().First().Key;
        }

        /// <summary>
        ///     Gets the images within the specified time interval.
        /// </summary>
        /// <param name="from">Time interval start.</param>
        /// <param name="to">Time interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>A Dictionary of images.</returns>
        public Dictionary<DateTime, T> Get(DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            var dictionary = new Dictionary<DateTime, T>();
            var datetimes = GetDateTimes(from, to);

            datetimes.ToList().ForEach(x => { dictionary.Add(x, _GetImage(x)); });

            return dictionary;
        }

        /// <summary>
        ///     Gets the image at the specified date/time.
        /// </summary>
        /// <param name="dateTime">The date/time.</param>
        /// <param name="user">The user.</param>
        /// <returns>Maybe&lt;TEntity&gt;.</returns>
        public Maybe<T> Get(DateTime dateTime, ClaimsPrincipal user = null)
        {
            var image = _GetImage(dateTime);
            return image == null || image.Equals(default(T)) ? Maybe.Empty<T>() : image.ToMaybe();
        }

        /// <summary>
        ///     Gets the date/times between from and to
        /// </summary>
        /// <param name="from">Time start.</param>
        /// <param name="to">Time end.</param>
        /// <param name="user">The user.</param>
        /// <returns>An enumerable of date times.</returns>
        public IEnumerable<DateTime> GetDateTimes(DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            return _GetDateTimes().Where(x => (x.Key >= from) & (x.Key <= to)).Select(y => y.Key);
        }

        /// <summary>
        ///     Gets the first date time after each date time in a list of date times.
        /// </summary>
        /// <param name="dateTimes">The date/times list.</param>
        /// <param name="user">The user.</param>
        /// <returns>The first date time after each date time.</returns>
        public IEnumerable<DateTime> GetDateTimesFirstAfter(IEnumerable<DateTime> dateTimes, ClaimsPrincipal user = null)
        {
            return dateTimes.Select(_GetFirstAfter).Where(y => y.HasValue).Select(z => z.Value);
        }

        /// <summary>
        ///     Gets the last date time before each date time in a list of date times.
        /// </summary>
        /// <param name="dateTimes">The date/times list.</param>
        /// <param name="user">The user.</param>
        /// <returns>The last date time before each date time</returns>
        public IEnumerable<DateTime> GetDateTimesLastBefore(IEnumerable<DateTime> dateTimes, ClaimsPrincipal user = null)
        {
            return dateTimes.Select(_GetLastBefore).Where(y => y.HasValue).Select(z => z.Value);
        }

        /// <summary>
        ///     Gets the first image after the specified date/time.
        /// </summary>
        /// <param name="dateTime">The date/time.</param>
        /// <param name="user">The user.</param>
        /// <returns>A raster.</returns>
        public T GetFirstAfter(DateTime dateTime, ClaimsPrincipal user = null)
        {
            return _GetFirstAfter(dateTime).HasValue ? _GetImage(_GetFirstAfter(dateTime).Value) : default;
        }

        /// <summary>
        ///     Gets the first image after the specified date/time for a list of date times.
        /// </summary>
        /// <param name="dateTimes">The date/times list.</param>
        /// <param name="user">The user.</param>
        /// <returns>A a list of images.</returns>
        public IEnumerable<T> GetFirstAfter(IEnumerable<DateTime> dateTimes, ClaimsPrincipal user = null)
        {
            return GetDateTimesFirstAfter(dateTimes).Select(_GetImage);
        }

        /// <summary>
        ///     Gets the last image before the specified date/time.
        /// </summary>
        /// <param name="dateTime">The date/time.</param>
        /// <param name="user">The user.</param>
        /// <returns>An image.</returns>
        public T GetLastBefore(DateTime dateTime, ClaimsPrincipal user = null)
        {
            return _GetLastBefore(dateTime).HasValue ? _GetImage(_GetLastBefore(dateTime).Value) : default;
        }

        /// <summary>
        ///     Gets the last image before the specified date/time for a list of date times.
        /// </summary>
        /// <param name="dateTimes">The date/times list.</param>
        /// <param name="user">The user.</param>
        /// <returns>A a list of images.</returns>
        public IEnumerable<T> GetLastBefore(IEnumerable<DateTime> dateTimes, ClaimsPrincipal user = null)
        {
            return GetDateTimesLastBefore(dateTimes).Select(_GetImage);
        }

        /// <summary>
        ///     Gets the last image.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The last image.</returns>
        public T Last(ClaimsPrincipal user = null)
        {
            return _GetImage(LastDateTime());
        }

        /// <summary>
        ///     Gets the date/time of the last image.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The date/time of the last image.</returns>
        public DateTime LastDateTime(ClaimsPrincipal user = null)
        {
            return _GetDateTimes().Last().Key;
        }

        private T _GetImage(DateTime dateTime)
        {
            var filesDictionary = _GetDateTimes();
            var thisFileQuery = filesDictionary.Where(x => x.Key == dateTime);

            if (!thisFileQuery.Any())
            {
                return default;
            }

            var image = (T)Activator.CreateInstance(typeof(T));
            var filePath = Path.Combine(_folderPath, thisFileQuery.First().Value);
            try
            {
                image.FromFile(filePath);
            }
            catch
            {
                return default;
            }

            return image;
        }

        private Dictionary<DateTime, string> _GetDateTimes()
        {
            var directoryInfo = new DirectoryInfo(_folderPath);
            var baseFileName = _filePattern.Split('{')[0]; // isolate the base part of the filename (i.e. the part without the datetime)
            var allFiles = directoryInfo.GetFiles($"{baseFileName}*.{_fileExtension}");

            var dateList = allFiles.Select(x =>
            {
                var filename = x.Name; // eg:  Radar33201803191452hello
                var dateStartPosition = _filePattern.IndexOf(DateTemplate, StringComparison.Ordinal);

                var dateString = filename.Substring(dateStartPosition, _datePattern.Length);

                // if the _datePattern contains $$$ or ### then we cannot simply parse
                if (_datePattern.Contains(DateCounterTemplateHour) || _datePattern.Contains(DateCounterTemplateDay))
                {
                    var template = _datePattern.Contains(DateCounterTemplateHour) ? DateCounterTemplateHour : DateCounterTemplateDay;
                    var datePart = dateString.Substring(0, _datePattern.IndexOf(template, StringComparison.Ordinal));
                    var counterPart = dateString.Substring(_datePattern.IndexOf(template, StringComparison.Ordinal), template.Length);

                    var updatedDatePattern = _datePattern.Replace(template, "");
                    var baseDate = DateTime.ParseExact(datePart, updatedDatePattern, CultureInfo.InvariantCulture);
                    var counter = int.Parse(counterPart);

                    var multiplier = _datePattern.Contains(DateCounterTemplateHour) ? 1 : 24;
                    return new KeyValuePair<DateTime, string>(baseDate.AddHours(counter * multiplier), x.FullName);
                }

                return new KeyValuePair<DateTime, string>(DateTime.ParseExact(dateString, _datePattern, CultureInfo.InvariantCulture), x.FullName);
            });

            return dateList.OrderBy(y => y.Key).ToDictionary(x => x.Key, x => x.Value);
        }

        private DateTime? _GetLastBefore(DateTime dateTime)
        {
            var datetimes = _GetDateTimes().ToList();
            var indexFirstAfter = -1;

            if (datetimes.Any(x => x.Key > dateTime))
            {
                var firstAfter = datetimes.First(x => x.Key > dateTime);
                indexFirstAfter = datetimes.IndexOf(firstAfter);
            }

            if (dateTime >= datetimes.Last().Key)
            {
                indexFirstAfter = datetimes.Count - 1;
            }

            if (indexFirstAfter > 0)
            {
                return datetimes[indexFirstAfter - 1].Key;
            }

            return null;
        }

        private DateTime? _GetFirstAfter(DateTime dateTime)
        {
            var datetimes = _GetDateTimes().ToList();

            if (datetimes.Any(x => x.Key > dateTime))
            {
                return datetimes.First(x => x.Key > dateTime).Key;
            }

            return null;
        }
    }
}