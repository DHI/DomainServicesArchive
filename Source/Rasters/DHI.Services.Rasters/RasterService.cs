namespace DHI.Services.Rasters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    /// <summary>
    ///     Raster service.
    /// </summary>
    /// <typeparam name="TRaster">The type of the raster.</typeparam>
    public class RasterService<TRaster> : BaseService<TRaster, DateTime> where TRaster : IRaster
    {
        protected readonly TimeSpan MaxTimeSpan;
        private readonly IRasterRepository<TRaster> _repository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RasterService{TRaster}" /> class.
        /// </summary>
        /// <param name="repository">The raster repository.</param>
        /// <param name="maxTimeSpan">The maximum time span for retrieving batches of rasters.</param>
        public RasterService(IRasterRepository<TRaster> repository, TimeSpan maxTimeSpan)
            : base(repository)
        {
            _repository = repository;
            MaxTimeSpan = maxTimeSpan;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RasterService{TRaster}" /> class.
        /// </summary>
        /// <param name="repository">The raster repository.</param>
        public RasterService(IRasterRepository<TRaster> repository)
            : this(repository, TimeSpan.FromDays(1))
        {
        }

        /// <summary>
        ///     Gets the date/time of the first raster.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The first date/time.</returns>
        public DateTime FirstDateTime(ClaimsPrincipal user = null)
        {
            return _repository.FirstDateTime(user);
        }

        /// <summary>
        ///     Gets the last raster.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The last raster.</returns>
        public TRaster Last(ClaimsPrincipal user = null)
        {
            return _repository.Last(user);
        }

        /// <summary>
        ///     Gets the date/time of the last raster.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The last date/time.</returns>
        public DateTime LastDateTime(ClaimsPrincipal user = null)
        {
            return _repository.LastDateTime(user);
        }

        /// <summary>
        /// Gets the date/times between from and to.
        /// </summary>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <param name="user">The user.</param>
        /// <returns>IEnumerable&lt;DateTime&gt;.</returns>
        /// <value>A list of the date times between from and to.</value>
        public IEnumerable<DateTime> GetDateTimes(DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            return _repository.GetDateTimes(from, to, user);
        }

        /// <summary>
        /// Gets the first date time after each date time in a list of date times.
        /// </summary>
        /// <param name="dateTimes">The date/times list.</param>
        /// <param name="user">The user.</param>
        /// <returns>The first date time after each date time.</returns>
        public IEnumerable<DateTime> GetDateTimesFirstAfter(IEnumerable<DateTime> dateTimes, ClaimsPrincipal user = null)
        {
            return _repository.GetDateTimesFirstAfter(dateTimes, user);
        }

        /// <summary>
        /// Gets the last date time before each date time in a list of date times.
        /// </summary>
        /// <param name="dateTimes">The date/times list.</param>
        /// <param name="user">The user.</param>
        /// <returns>The last date time before each date time</returns>
        public IEnumerable<DateTime> GetDateTimesLastBefore(IEnumerable<DateTime> dateTimes, ClaimsPrincipal user = null)
        {
            return _repository.GetDateTimesLastBefore(dateTimes, user);
        }

        /// <summary>
        ///     Gets the compatible repository types at the path of the executing assembly.
        /// </summary>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes()
        {
            return Service.GetProviderTypes<IRasterRepository<TRaster>>();
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path)
        {
            return Service.GetProviderTypes<IRasterRepository<TRaster>>(path);
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers. If path is null, the path of the executing assembly is used.</param>
        /// <param name="searchPattern">File name search pattern. Can contain a combination of valid literal path and wildcard (*and ?) characters.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path, string searchPattern)
        {
            return Service.GetProviderTypes<IRasterRepository<TRaster>>(path, searchPattern);
        }

        /// <summary>
        /// Determines whether a raster exists at the specified date time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if a raster exists at the specified date time, <c>false</c> otherwise.</returns>
        public bool Exists(DateTime dateTime, ClaimsPrincipal user = null)
        {
            return _repository.Contains(dateTime, user);
        }

        /// <summary>
        /// Gets the raster at the specified date time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="raster">The raster.</param>
        /// <param name="user">The user.</param>
        /// <returns>True if the raster was found, false otherwise.</returns>
        public override bool TryGet(DateTime dateTime, out TRaster raster, ClaimsPrincipal user = null)
        {
            if (_repository.Contains(dateTime, user))
            {
                raster = _repository.Get(dateTime, user).Value;
                return true;
            }

            raster = default;
            return false;
        }

        /// <summary>
        /// Gets the raster at the specified date time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="user">The user.</param>
        /// <returns>The raster.</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public override TRaster Get(DateTime dateTime, ClaimsPrincipal user = null)
        {
            if (_repository.Contains(dateTime, user))
            {
                return base.Get(dateTime, user);
            }

            throw new KeyNotFoundException($"Raster with id '{dateTime}' was not found.");
        }

        /// <summary>
        /// Gets a dictionary (time series) of rasters within a specified time interval.
        /// </summary>
        /// <param name="from">Time interval start.</param>
        /// <param name="to">Time interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>A dictionary (time series) of rasters.</returns>
        public SortedDictionary<DateTime, TRaster> Get(DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            if (from >= to)
            {
                throw new ArgumentException($"FROM date/time '{from}' must be less than TO date/time '{to}'");
            }

            var timeSpan = to - from;
            if (timeSpan > MaxTimeSpan)
            {
                throw new ArgumentException(
                    $"The requested time span of rasters {timeSpan} is too large. The maximum allowed time span is {MaxTimeSpan}.");
            }

            return new SortedDictionary<DateTime, TRaster>(_repository.Get(from, to, user));
        }

        /// <summary>
        /// Gets the first raster after the specified date/time.
        /// </summary>
        /// <param name="dateTime">The date/time.</param>
        /// <param name="user">The user.</param>
        /// <returns>A raster.</returns>
        public TRaster GetFirstAfter(DateTime dateTime, ClaimsPrincipal user = null)
        {
            if (dateTime >= _repository.LastDateTime(user))
            {
                throw new ArgumentException($"No rasters after '{dateTime}'.", nameof(dateTime));
            }

            return _repository.GetFirstAfter(dateTime, user);
        }

        /// <summary>
        /// Gets the first raster after the specified date/time for a list of date times.
        /// </summary>
        /// <param name="dateTimes">The date/times list.</param>
        /// <param name="user">The user.</param>
        /// <returns>The rasters for the list of date times.</returns>
        public IEnumerable<TRaster> GetFirstAfter(IEnumerable<DateTime> dateTimes, ClaimsPrincipal user = null)
        {
            var dateTimeList = dateTimes as IList<DateTime> ?? dateTimes.ToList();
            if (dateTimeList.Any(dateTime => dateTime >= _repository.LastDateTime(user)))
            {
                throw new ArgumentException($"No rasters after '{_repository.LastDateTime(user)}'.", nameof(dateTimes));
            }

            return _repository.GetFirstAfter(dateTimeList, user);
        }

        /// <summary>
        /// Gets the last raster before the specified date/time.
        /// </summary>
        /// <param name="dateTime">The date/time.</param>
        /// <param name="user">The user.</param>
        /// <returns>A raster.</returns>
        public TRaster GetLastBefore(DateTime dateTime, ClaimsPrincipal user = null)
        {
            if (dateTime <= _repository.FirstDateTime(user))
            {
                throw new ArgumentException($"No rasters before '{dateTime}'.", nameof(dateTime));
            }

            return _repository.GetLastBefore(dateTime, user);
        }

        /// <summary>
        /// Gets the last raster before the specified date/time for a list of date times.
        /// </summary>
        /// <param name="dateTimes">The date/times list.</param>
        /// <param name="user">The user.</param>
        /// <returns>The rasters for the list of date times.</returns>
        public IEnumerable<TRaster> GetLastBefore(IEnumerable<DateTime> dateTimes, ClaimsPrincipal user = null)
        {
            var dateTimeList = dateTimes as IList<DateTime> ?? dateTimes.ToList();
            if (dateTimeList.Any(dateTime => dateTime <= _repository.FirstDateTime(user)))
            {
                throw new ArgumentException($"No rasters before '{_repository.LastDateTime(user)}'.", nameof(dateTimes));
            }

            return _repository.GetLastBefore(dateTimeList, user);
        }
    }
}