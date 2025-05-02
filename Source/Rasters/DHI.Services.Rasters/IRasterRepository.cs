namespace DHI.Services.Rasters
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;

    /// <summary>
    ///     Raster repository abstraction.
    /// </summary>
    /// <typeparam name="TRaster">The raster type.</typeparam>
    public interface IRasterRepository<TRaster> : IRepository<TRaster, DateTime> where TRaster : IRaster
    {
        /// <summary>
        /// Determines whether the repository contains a raster at the specified date time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if the repository contains a raster at the specified date time; otherwise, <c>false</c>.</returns>
        bool Contains(DateTime dateTime, ClaimsPrincipal user = null);

        /// <summary>
        /// Gets the date/time of the first raster.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The date/time of the first raster.</returns>
        DateTime FirstDateTime(ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the last raster.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The last raster.</returns>
        TRaster Last(ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the date/time of the last raster.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The date/time of the last raster.</returns>
        DateTime LastDateTime(ClaimsPrincipal user = null);

        /// <summary>
        /// Gets the date/times between from and to
        /// </summary>
        /// <param name="from">Time start.</param>
        /// <param name="to">Time end.</param>
        /// <param name="user">The user.</param>
        /// <returns>An enumerable of date times.</returns>
        IEnumerable<DateTime> GetDateTimes(DateTime from, DateTime to, ClaimsPrincipal user = null);

        /// <summary>
        /// Gets the first date time after each date time in a list of date times.
        /// </summary>
        /// <param name="dateTimes">The date/times list.</param>
        /// <param name="user">The user.</param>
        /// <returns>The first date time after each date time.</returns>
        IEnumerable<DateTime> GetDateTimesFirstAfter(IEnumerable<DateTime> dateTimes, ClaimsPrincipal user = null);

        /// <summary>
        /// Gets the last date time before each date time in a list of date times.
        /// </summary>
        /// <param name="dateTimes">The date/times list.</param>
        /// <param name="user">The user.</param>
        /// <returns>The last date time before each date time</returns>
        IEnumerable<DateTime> GetDateTimesLastBefore(IEnumerable<DateTime> dateTimes, ClaimsPrincipal user = null);

        /// <summary>
        /// Gets the rasters within the specified time interval.
        /// </summary>
        /// <param name="from">Time interval start.</param>
        /// <param name="to">Time interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>A Dictionary of rasters.</returns>
        Dictionary<DateTime, TRaster> Get(DateTime from, DateTime to, ClaimsPrincipal user = null);

        /// <summary>
        /// Gets the first raster after the specified date/time.
        /// </summary>
        /// <param name="dateTime">The date/time.</param>
        /// <param name="user">The user.</param>
        /// <returns>A raster.</returns>
        TRaster GetFirstAfter(DateTime dateTime, ClaimsPrincipal user = null);

        /// <summary>
        /// Gets the first raster after the specified date/time for a list of date times.
        /// </summary>
        /// <param name="dateTimes">The date/times list.</param>
        /// <param name="user">The user.</param>
        /// <returns>A a list of rasters.</returns>
        IEnumerable<TRaster> GetFirstAfter(IEnumerable<DateTime> dateTimes, ClaimsPrincipal user = null);

        /// <summary>
        /// Gets the last raster before the specified date/time.
        /// </summary>
        /// <param name="dateTime">The date/time.</param>
        /// <param name="user">The user.</param>
        /// <returns>A raster.</returns>
        TRaster GetLastBefore(DateTime dateTime, ClaimsPrincipal user = null);

        /// <summary>
        /// Gets the last raster before the specified date/time for a list of date times.
        /// </summary>
        /// <param name="dateTimes">The date/times list.</param>
        /// <param name="user">The user.</param>
        /// <returns>A a list of rasters.</returns>
        IEnumerable<TRaster> GetLastBefore(IEnumerable<DateTime> dateTimes, ClaimsPrincipal user = null);
    }
}