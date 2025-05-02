namespace DHI.Services.TimeSeries.WebApi
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using WebApiCore;

    /// <summary>
    ///     Time Series Analysis API
    /// </summary>
    [Produces("application/json")]
    [Route("api/timeseries/{connectionId}")]
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for analyzing time series data.")]
    public class TimeSeriesAnalysisController : ControllerBase
    {
        /// <summary>
        ///     Gets the minimum value for the given time series within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="204">No content. Time series ID or dateTimes not found.</response>
        [HttpGet("{id}/min")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult<double?> GetMin(string connectionId, string id, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ICoreTimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetAggregatedValue(FullNameString.FromUrl(id), AggregationType.Minimum, from, to, user));
        }

        /// <summary>
        ///     Gets the minimum value for the given time series grouped by the given period within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="period">The aggregation period. The supported types are 'Hourly', 'Daily', 'Monthly' and 'Yearly'.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="204">No content. Time series ID or dateTimes not found.</response>
        [HttpGet("{id}/min/period/{period}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult<ITimeSeriesData<double>> GetMinByPeriod(string connectionId, string id, Period period, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).Minimum(period));
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Minimum(period));
        }

        /// <summary>
        ///     Gets a list of minimum values for the given list of time series within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="ids">The time series IDs.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        [HttpPost("list/min")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        public ActionResult<Dictionary<string, double?>> GetMinList(string connectionId, [FromBody] string[] ids, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ICoreTimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetAggregatedValues(ids, AggregationType.Minimum, from, to, user));
        }

        /// <summary>
        ///     Gets the maximum value for the given time series within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="204">No content. Time series ID or dateTimes not found.</response>
        [HttpGet("{id}/max")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult<double?> GetMax(string connectionId, string id, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ICoreTimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetAggregatedValue(FullNameString.FromUrl(id), AggregationType.Maximum, from, to, user));
        }

        /// <summary>
        ///     Gets the maximum value for the given time series grouped by the given period within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="period">The aggregation period. The supported types are 'Hourly', 'Daily', 'Monthly' and 'Yearly'.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="204">No content. Time series ID or dateTimes not found.</response>
        [HttpGet("{id}/max/period/{period}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult<ITimeSeriesData<double>> GetMaxByPeriod(string connectionId, string id, Period period, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).Maximum(period));
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Maximum(period));
        }

        /// <summary>
        ///     Gets a list of maximum values for the given list of time series within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="ids">The time series IDs.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        [HttpPost("list/max")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        public ActionResult<Dictionary<string, double?>> GetMaxList(string connectionId, [FromBody] string[] ids, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ICoreTimeSeriesService<string, double>>(connectionId); 
            return Ok(timeSeriesService.GetAggregatedValues(ids, AggregationType.Maximum, from, to, user));
        }

        /// <summary>
        ///     Gets the sum value for the given time series within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        [HttpGet("{id}/sum")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<double?> GetSum(string connectionId, string id, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ICoreTimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetAggregatedValue(FullNameString.FromUrl(id), AggregationType.Sum, from, to, user));
        }

        /// <summary>
        ///     Gets the sum value for the given time series grouped by the given period within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="period">The aggregation period. The supported types are 'Hourly', 'Daily', 'Monthly' and 'Yearly'.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        [HttpGet("{id}/sum/period/{period}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<ITimeSeriesData<double>> GetSumByPeriod(string connectionId, string id, Period period, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).Sum(period));
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Sum(period));
        }

        /// <summary>
        ///     Gets a list of sum values for the given list of time series within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="ids">The time series IDs.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        [HttpPost("list/sum")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        public ActionResult<Dictionary<string, double?>> GetSumList(string connectionId, [FromBody] string[] ids, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ICoreTimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetAggregatedValues(ids, AggregationType.Sum, from, to, user));
        }

        /// <summary>
        ///     Gets the average value for the given time series within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        [HttpGet("{id}/average")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<double?> GetAverage(string connectionId, string id, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ICoreTimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetAggregatedValue(FullNameString.FromUrl(id), AggregationType.Average, from, to, user));
        }

        /// <summary>
        ///     Gets the average values for the given time series grouped by the given period within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="period">The aggregation period. The supported types are 'Hourly', 'Daily', 'Monthly' and 'Yearly'.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        [HttpGet("{id}/average/period/{period}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<ITimeSeriesData<double>> GetAverageByPeriod(string connectionId, string id, Period period, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).Average(period));
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Average(period));
        }

        /// <summary>
        ///     Gets a list of average values for the given list of time series within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="ids">The time series IDs.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        [HttpPost("list/average")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        public ActionResult<Dictionary<string, double?>> GetAverageList(string connectionId, [FromBody] string[] ids, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ICoreTimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetAggregatedValues(ids, AggregationType.Average, from, to, user));
        }

        /// <summary>
        ///     Gets the moving average over the given window for the given time series within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'window' query parameter is mandatory and is an integer defining the number of data points in the window.
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="window">The window (number of data points).</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        [HttpGet("{id}/movingaverage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<ITimeSeriesData<double>> GetMovingAverage(string connectionId, string id, int window, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user:user).MovingAverage(window));
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).MovingAverage(window));
        }

        /// <summary>
        ///     Gets a reduced (simplified) time series using the Ramer–Douglas–Peucker algorithm of the given time series within
        ///     the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="tolerance">
        ///     A relative tolerance expressed as a percentage of the difference between maximum and minimum value within the analyzed time interval. 
        /// </param>
        /// <param name="minimumCount">
        ///     A threshold value for ignoring the request to reduce the times series.
        ///     If, for example, the minimum count is set to 1000, and the number of data points within the given time interval is less than 1000, the reduction request will be ignored, and the complete time series will be returned.
        /// </param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        [HttpGet("{id}/reduced")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<ITimeSeriesData<double>> GetReduced(string connectionId, string id, double tolerance = 2, int minimumCount = 3000, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user:user).Reduce(tolerance, minimumCount));
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Reduce(tolerance, minimumCount));
        }

        /// <summary>
        ///     Gets a list of reduced (simplified) time series using the Ramer–Douglas–Peucker algorithm of the given list of time
        ///     series within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="ids">The time series IDs.</param>
        /// <param name="tolerance">
        ///     A relative tolerance expressed as a percentage of the difference between maximum and minimum value within the analyzed time interval. 
        /// </param>
        /// <param name="minimumCount">
        ///     A threshold value for ignoring the request to reduce the times series.
        ///     If, for example, the minimum count is set to 1000, and the number of data points within the given time interval is less than 1000, the reduction request will be ignored, and the complete time series will be returned.
        /// </param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        [HttpPost("list/reduced")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        public ActionResult<Dictionary<string, ITimeSeriesData<double>>> GetReducedList(string connectionId, [FromBody] string[] ids, double tolerance = 2, int minimumCount = 3000, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var dictionary = new Dictionary<string, ITimeSeriesData<double>>();
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);

            if (from == null && to == null)
            {
                foreach (var id in ids)
                {
                    dictionary.Add(id, timeSeriesService.GetValues(FullNameString.FromUrl(id), user:user).Reduce(tolerance, minimumCount));
                }

                return Ok(dictionary);
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            foreach (var id in ids)
            {
                dictionary.Add(id, timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Reduce(tolerance, minimumCount));
            }

            return Ok(dictionary);
        }

        /// <summary>
        ///     Gets a list of smoothed time series using a Savitzky-Golay filter.
        /// </summary>
        /// <remarks>
        ///     The Savitzky-Golay filter better preserves the peaks in the input data compared to other filters like moving average.
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series identifier.</param>
        /// <param name="window">The window size (number of points). Must be an odd number.</param>
        /// <param name="order">The polynomial order. Should be between 0 and 5. When order equals 0 or 1, then the method performs a moving average filter.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        [HttpGet("{id}/smoothed")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<ITimeSeriesData<double>> GetSmoothed(string connectionId, string id, int window = 5, int order = 2, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).Smoothing(window, order));
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Smoothing(window, order));
        }

        /// <summary>
        ///     Gets a smoothed time series using a Savitzky-Golay filter.
        /// </summary>
        /// <remarks>
        ///     The Savitzky-Golay filter better preserves the peaks in the input data compared to other filters like moving average.
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="ids">The time series identifiers.</param>
        /// <param name="window">The window size (number of points). Must be an odd number.</param>
        /// <param name="order">The polynomial order. Should be between 0 and 5. When order equals 0 or 1, then the method performs a moving average filter.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        [HttpPost("list/smoothed")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        public ActionResult<Dictionary<string, ITimeSeriesData<double>>> GetSmoothedList(string connectionId, [FromBody] string[] ids, int window = 5, int order = 2, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var dictionary = new Dictionary<string, ITimeSeriesData<double>>();
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);

            if (from == null && to == null)
            {
                foreach (var id in ids)
                {
                    dictionary.Add(id, timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).Smoothing(window, order));
                }

                return Ok(dictionary);
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            foreach (var id in ids)
            {
                dictionary.Add(id, timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Smoothing(window, order));
            }

            return Ok(dictionary);
        }

        /// <summary>
        ///     Gets the value (percentile) below below which a given percentage of values within the given time interval fall.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="percentage">The percentage.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="204">No content. Time series ID or dateTimes not found.</response>
        [HttpGet("{id}/percentile/{percentage}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult<double?> GetPercentile(string connectionId, string id, int percentage, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user:user).PercentileValue(percentage));
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).PercentileValue(percentage));
        }

        /// <summary>
        ///     Gets a list of percentile values for the given list of time series within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="ids">The time series IDs.</param>
        /// <param name="percentage">The percentage.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        [HttpPost("list/percentile/{percentage}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        public ActionResult<Dictionary<string, double?>> GetPercentileList(string connectionId, [FromBody] string[] ids, int percentage, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var dictionary = new Dictionary<string, double?>();
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                foreach (var id in ids)
                {
                    dictionary.Add(id, timeSeriesService.GetValues(FullNameString.FromUrl(id), user:user).PercentileValue(percentage));
                }

                return Ok(dictionary);
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            foreach (var id in ids)
            {
                dictionary.Add(id, timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).PercentileValue(percentage));
            }

            return Ok(dictionary);
        }

        /// <summary>
        ///     Gets a resampled time series within the given time interval with time steps equivalent to the given time span.
        /// </summary>
        /// <remarks>
        ///     The time series values are found using interpolation according to the time series data type.
        ///     If the time series data type is not explicitly defined, the data type 'Instantaneous' is assumed.
        ///     However, if the time series data type is StepAccumulated, the resampled values are found as the sum of values within the given time span.
        ///     The timespan query parameter is mandatory. The from and to query parameters are optional. If no time interval is
        ///     given, the analysis is performed on the entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="timeSpan">The time span.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series ID not found.</response>
        [HttpGet("{id}/resampled")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ITimeSeriesData<double>> GetResampled(string connectionId, string id, TimeSpan timeSpan, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            var timeSeries = timeSeriesService.Get(FullNameString.FromUrl(id), user);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user:user).Resample(timeSpan, timeSeries.DataType));
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Resample(timeSpan, timeSeries.DataType));
        }

        /// <summary>
        ///     Gets the maximum value of a resampled time series within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="timeSpan">The time span.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="204">No content. Time series ID or dateTimes not found.</response>
        [HttpGet("{id}/resampled/max")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<double?> GetMaxResampled(string connectionId, string id, TimeSpan timeSpan, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            var timeSeries = timeSeriesService.Get(FullNameString.FromUrl(id), user);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).Resample(timeSpan, timeSeries.DataType).Maximum());
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Resample(timeSpan, timeSeries.DataType).Maximum());
        }

        /// <summary>
        ///     Gets the minimum value of a resampled time series within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="timeSpan">The time span.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="204">No content. Time series ID or dateTimes not found.</response>
        [HttpGet("{id}/resampled/min")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<double?> GetMinResampled(string connectionId, string id, TimeSpan timeSpan, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            var timeSeries = timeSeriesService.Get(FullNameString.FromUrl(id), user);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).Resample(timeSpan, timeSeries.DataType).Minimum());
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Resample(timeSpan, timeSeries.DataType).Minimum());
        }

        /// <summary>
        ///     Gets the average value of a resampled time series within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="timeSpan">The time span.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="204">No content. Time series ID or dateTimes not found.</response>
        [HttpGet("{id}/resampled/average")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<double?> GetAverageResampled(string connectionId, string id, TimeSpan timeSpan, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            var timeSeries = timeSeriesService.Get(FullNameString.FromUrl(id), user);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).Resample(timeSpan, timeSeries.DataType).Average());
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Resample(timeSpan, timeSeries.DataType).Average());
        }

        /// <summary>
        ///     Gets the standard deviation value of a resampled time series within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="timeSpan">The time span.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="204">No content. Time series ID or dateTimes not found.</response>
        [HttpGet("{id}/resampled/standarddeviation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<double?> GetStandardDeviationResampled(string connectionId, string id, TimeSpan timeSpan, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            var timeSeries = timeSeriesService.Get(FullNameString.FromUrl(id), user);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).Resample(timeSpan, timeSeries.DataType).StandardDeviation());
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Resample(timeSpan, timeSeries.DataType).StandardDeviation());
        }

        /// <summary>
        ///     Gets linear trendline of a resampled time series within the given time interval.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="timeSpan">The time span.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        [HttpGet("{id}/resampled/lineartrendline")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<double?> GetLinearTrendlineResampled(string connectionId, string id, TimeSpan timeSpan, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            var timeSeries = timeSeriesService.Get(FullNameString.FromUrl(id), user);
            (double slope, double offset, ITimeSeriesData<double> trendline) response;
            if (from == null && to == null)
            {
                response = timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).Resample(timeSpan, timeSeries.DataType).LinearTrendline();
            }
            else
            {
                var fromDateTime = from ?? DateTime.MinValue;
                var toDateTime = to ?? DateTime.MaxValue;
                response = timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Resample(timeSpan, timeSeries.DataType).LinearTrendline();
            }

            return Ok(new { response.slope, response.offset, response.trendline });
        }

        /// <summary>
        ///     Gets a resampled time series by the given period within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The time series values are found using interpolation according to the time series data type.
        ///     If the time series data type is not explicitly defined, the data type 'Instantaneous' is assumed.
        ///     However, if the time series data type is StepAccumulated, the resampled values are found as the sum of values within the given period.
        ///     The period query parameter is mandatory. The from and to query parameters are optional. If no time interval is
        ///     given, the analysis is performed on the entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="period">The aggregation period. The supported types are 'Hourly', 'Daily', 'Monthly' and 'Yearly'.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series ID not found.</response>
        [HttpGet("{id}/resampled/period/{period}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ITimeSeriesData<double>> GetResampledByPeriod(string connectionId, string id, Period period, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            var timeSeries = timeSeriesService.Get(FullNameString.FromUrl(id), user);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).Resample(period, timeSeries.DataType));
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Resample(period, timeSeries.DataType));
        }

        /// <summary>
        ///     Gets maximum value of a resampled time series by the given period within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The time series values are found using interpolation according to the time series data type.
        ///     If the time series data type is not explicitly defined, the data type 'Instantaneous' is assumed.
        ///     However, if the time series data type is StepAccumulated, the resampled values are found as the sum of values within the given period.
        ///     The period query parameter is mandatory. The from and to query parameters are optional. If no time interval is
        ///     given, the analysis is performed on the entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="period">The aggregation period. The supported types are 'Hourly', 'Daily', 'Monthly' and 'Yearly'.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series ID not found.</response>
        [HttpGet("{id}/resampled/period/{period}/max")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<double?> GetMaxResampledByPeriod(string connectionId, string id, Period period, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            var timeSeries = timeSeriesService.Get(FullNameString.FromUrl(id), user);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).Resample(period, timeSeries.DataType).Maximum());
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Resample(period, timeSeries.DataType).Maximum());
        }

        /// <summary>
        ///     Gets minimum value of a resampled time series by the given period within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The time series values are found using interpolation according to the time series data type.
        ///     If the time series data type is not explicitly defined, the data type 'Instantaneous' is assumed.
        ///     However, if the time series data type is StepAccumulated, the resampled values are found as the sum of values within the given period.
        ///     The period query parameter is mandatory. The from and to query parameters are optional. If no time interval is
        ///     given, the analysis is performed on the entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="period">The aggregation period. The supported types are 'Hourly', 'Daily', 'Monthly' and 'Yearly'.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series ID not found.</response>
        [HttpGet("{id}/resampled/period/{period}/min")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<double?> GetMinResampledByPeriod(string connectionId, string id, Period period, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            var timeSeries = timeSeriesService.Get(FullNameString.FromUrl(id), user);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).Resample(period, timeSeries.DataType).Minimum());
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Resample(period, timeSeries.DataType).Minimum());
        }

        /// <summary>
        ///     Gets average value of a resampled time series by the given period within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The time series values are found using interpolation according to the time series data type.
        ///     If the time series data type is not explicitly defined, the data type 'Instantaneous' is assumed.
        ///     However, if the time series data type is StepAccumulated, the resampled values are found as the sum of values within the given period.
        ///     The period query parameter is mandatory. The from and to query parameters are optional. If no time interval is
        ///     given, the analysis is performed on the entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="period">The aggregation period. The supported types are 'Hourly', 'Daily', 'Monthly' and 'Yearly'.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series ID not found.</response>
        [HttpGet("{id}/resampled/period/{period}/average")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<double?> GetAverageResampledByPeriod(string connectionId, string id, Period period, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            var timeSeries = timeSeriesService.Get(FullNameString.FromUrl(id), user);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).Resample(period, timeSeries.DataType).Average());
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Resample(period, timeSeries.DataType).Average());
        }

        /// <summary>
        ///     Gets standard deviation value of a resampled time series by the given period within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The time series values are found using interpolation according to the time series data type.
        ///     If the time series data type is not explicitly defined, the data type 'Instantaneous' is assumed.
        ///     However, if the time series data type is StepAccumulated, the resampled values are found as the sum of values within the given period.
        ///     The period query parameter is mandatory. The from and to query parameters are optional. If no time interval is
        ///     given, the analysis is performed on the entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="period">The aggregation period. The supported types are 'Hourly', 'Daily', 'Monthly' and 'Yearly'.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series ID not found.</response>
        [HttpGet("{id}/resampled/period/{period}/standarddeviation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<double?> GetStandardDeviationResampledByPeriod(string connectionId, string id, Period period, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            var timeSeries = timeSeriesService.Get(FullNameString.FromUrl(id), user);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).Resample(period, timeSeries.DataType).StandardDeviation());
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Resample(period, timeSeries.DataType).StandardDeviation());
        }

        /// <summary>
        ///     Gets linear trendline of a resampled time series by the given period within the given time interval.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="period">The aggregation period. The supported types are 'Hourly', 'Daily', 'Monthly' and 'Yearly'.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        [HttpGet("{id}/resampled/period/{period}/lineartrendline")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<double?> GetLinearTrendlineResampledByPeriod(string connectionId, string id, Period period, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            var timeSeries = timeSeriesService.Get(FullNameString.FromUrl(id), user);
            (double slope, double offset, ITimeSeriesData<double> trendline) response;
            if (from == null && to == null)
            {
                response = timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).Resample(period, timeSeries.DataType).LinearTrendline();
            }
            else
            {
                var fromDateTime = from ?? DateTime.MinValue;
                var toDateTime = to ?? DateTime.MaxValue;
                response = timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Resample(period, timeSeries.DataType).LinearTrendline();
            }

            return Ok(new { response.slope, response.offset, response.trendline });
        }

        /// <summary>
        ///     Gets a duration curve of a time series.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="durationInHours">The duration in hours.</param>
        /// <param name="numberOfIntervals">The number of value intervals.</param>
        /// <param name="minNumberOfValues">
        ///     The minimum number of values accepted.
        ///     If time series data does not contain enough values (!= null) an exception is thrown.
        /// </param>
        [HttpGet("{id}/durationcurve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<TimeSeriesData<double>> GetDurationCurve(string connectionId, string id, int durationInHours, int numberOfIntervals = 10, int minNumberOfValues = 100)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user:user).DurationCurve(durationInHours, numberOfIntervals, minNumberOfValues));
        }

        /// <summary>
        ///     Gets a duration curve of a time series with set static probabilities, to measure extreme values
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        [HttpGet("{id}/durationcurve/extreme")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<TimeSeriesData<double>> GetExtremeDurationCurve(string connectionId, string id)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).DurationCurve());
        }

        /// <summary>
        ///     Gets standard deviation of a time series
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        [HttpGet("{id}/standarddeviation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<double?> GetStandardDeviation(string connectionId, string id, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).StandardDeviation());
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).StandardDeviation());
        }

        /// <summary>
        ///     Gets standard deviation of time series grouped by the given period within the given time interval.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="period">The aggregation period. The supported types are 'Hourly', 'Daily', 'Monthly' and 'Yearly'.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        [HttpGet("{id}/standarddeviation/period/{period}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<double?> GetStandardDeviationByPeriod(string connectionId, string id, Period period, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).StandardDeviation(period));
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).StandardDeviation(period));
        }

        /// <summary>
        ///     Gets a list of standard deviation values for the given list of time series within the given time interval.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="ids">List of time series ID</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        [HttpPost("list/standarddeviation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        public ActionResult<Dictionary<string, double?>> GetStandardDeviationList(string connectionId, [FromBody] string[] ids, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var dictionary = new Dictionary<string, double?>();
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                foreach (var id in ids)
                {
                    dictionary.Add(id, timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).StandardDeviation());
                }
                return Ok(dictionary);
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            foreach (var id in ids)
            {
                dictionary.Add(id, timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).StandardDeviation());
            }

            return Ok(dictionary);
        }

        /// <summary>
        ///     Gets linear trendline of a time series
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The time series ID</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        [HttpGet("{id}/lineartrendline")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<double?> GetLinearTrendline(string connectionId, string id, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            (double slope, double offset, ITimeSeriesData<double> trendline) response;
            if (from == null && to == null)
            {
                response = timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).LinearTrendline();
            }
            else
            {
                var fromDateTime = from ?? DateTime.MinValue;
                var toDateTime = to ?? DateTime.MaxValue;
                response = timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).LinearTrendline();
            }

            return Ok(new { response.slope, response.offset, response.trendline });
        }
    }
}