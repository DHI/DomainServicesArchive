namespace DHI.Services.TimeSeries.Web
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Web.Http;
    using System.Web.Http.Cors;
    using System.Web.Http.Description;
    using Microsoft.Web.Http;
    using WebApi.OutputCache.V2;

    /// <summary>
    ///     Time Series Analysis API
    /// </summary>
    [EnableCors("*", "*", "*")]
    [RoutePrefix("api/timeseries/{connectionId}")]
    [ControllerExceptionFilter]
    [CacheTimeSeriesOutput(MustRevalidate = true)]
    [AutoInvalidateCacheOutput]
    [ApiVersion("1")]
    public class TimeSeriesAnalysisController : ApiController
    {
        /// <summary>
        ///     Gets the minimum value for the given time series within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series not found</response>
        [Route("{id}/min")]
        [ResponseType(typeof(double?))]
        public IHttpActionResult GetMin(string connectionId, string id, string from = null, string to = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user).Minimum());
            }

            var fromDateTime = from == null ? DateTime.MinValue : Datetime.Parse(from);
            var toDateTime = to == null ? DateTime.MaxValue : Datetime.Parse(to);
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Minimum());
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
        /// <response code="404">Time series not found</response>
        [Route("list/min")]
        [HttpPost]
        [ResponseType(typeof(Dictionary<string, double?>))]
        public IHttpActionResult GetMinList(string connectionId, [FromBody] string[] ids, string from = null, string to = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var dictionary = new Dictionary<string, double?>();
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                foreach (var id in ids)
                {
                    dictionary.Add(id, timeSeriesService.GetValues(FullNameString.FromUrl(id), user:user).Minimum());
                }

                return Ok(dictionary);
            }

            var fromDateTime = from == null ? DateTime.MinValue : Datetime.Parse(from);
            var toDateTime = to == null ? DateTime.MaxValue : Datetime.Parse(to);
            foreach (var id in ids)
            {
                dictionary.Add(id, timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Minimum());
            }

            return Ok(dictionary);
        }

        /// <summary>
        ///     Gets the maximum value for the given time series within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series not found</response>
        [Route("{id}/max")]
        [ResponseType(typeof(double?))]
        public IHttpActionResult GetMax(string connectionId, string id, string from = null, string to = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user:user).Maximum());
            }

            var fromDateTime = from == null ? DateTime.MinValue : Datetime.Parse(from);
            var toDateTime = to == null ? DateTime.MaxValue : Datetime.Parse(to);
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Maximum());
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
        /// <response code="404">Time series not found</response>
        [Route("list/max")]
        [HttpPost]
        [ResponseType(typeof(Dictionary<string, double?>))]
        public IHttpActionResult GetMaxList(string connectionId, [FromBody] string[] ids, string from = null, string to = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var dictionary = new Dictionary<string, double?>();
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                foreach (var id in ids)
                {
                    dictionary.Add(id, timeSeriesService.GetValues(FullNameString.FromUrl(id), user:user).Maximum());
                }

                return Ok(dictionary);
            }

            var fromDateTime = from == null ? DateTime.MinValue : Datetime.Parse(from);
            var toDateTime = to == null ? DateTime.MaxValue : Datetime.Parse(to);
            foreach (var id in ids)
            {
                dictionary.Add(id, timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Maximum());
            }

            return Ok(dictionary);
        }

        /// <summary>
        ///     Gets the sum value for the given time series within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series not found</response>
        [Route("{id}/sum")]
        [ResponseType(typeof(double?))]
        public IHttpActionResult GetSum(string connectionId, string id, string from = null, string to = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user:user).Sum());
            }

            var fromDateTime = from == null ? DateTime.MinValue : Datetime.Parse(from);
            var toDateTime = to == null ? DateTime.MaxValue : Datetime.Parse(to);
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Sum());
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
        /// <response code="404">Time series not found</response>
        [Route("list/sum")]
        [HttpPost]
        [ResponseType(typeof(Dictionary<string, double?>))]
        public IHttpActionResult GetSumList(string connectionId, [FromBody] string[] ids, string from = null, string to = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var dictionary = new Dictionary<string, double?>();
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                foreach (var id in ids)
                {
                    dictionary.Add(id, timeSeriesService.GetValues(FullNameString.FromUrl(id), user:user).Sum());
                }

                return Ok(dictionary);
            }

            var fromDateTime = from == null ? DateTime.MinValue : Datetime.Parse(from);
            var toDateTime = to == null ? DateTime.MaxValue : Datetime.Parse(to);
            foreach (var id in ids)
            {
                dictionary.Add(id, timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Sum());
            }

            return Ok(dictionary);
        }

        /// <summary>
        ///     Gets the average value for the given time series within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series not found</response>
        [Route("{id}/average")]
        [ResponseType(typeof(double?))]
        public IHttpActionResult GetAverage(string connectionId, string id, string from = null, string to = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user:user).Average());
            }

            var fromDateTime = from == null ? DateTime.MinValue : Datetime.Parse(from);
            var toDateTime = to == null ? DateTime.MaxValue : Datetime.Parse(to);
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Average());
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
        /// <response code="404">Time series not found</response>
        [Route("list/average")]
        [HttpPost]
        [ResponseType(typeof(Dictionary<string, double?>))]
        public IHttpActionResult GetAverageList(string connectionId, [FromBody] string[] ids, string from = null, string to = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var dictionary = new Dictionary<string, double?>();
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                foreach (var id in ids)
                {
                    dictionary.Add(id, timeSeriesService.GetValues(FullNameString.FromUrl(id), user:user).Average());
                }

                return Ok(dictionary);
            }

            var fromDateTime = from == null ? DateTime.MinValue : Datetime.Parse(from);
            var toDateTime = to == null ? DateTime.MaxValue : Datetime.Parse(to);
            foreach (var id in ids)
            {
                dictionary.Add(id, timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Average());
            }

            return Ok(dictionary);
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
        /// <param name="id">The identifier.</param>
        /// <param name="window">The window (number of data points).</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series not found</response>
        [Route("{id}/movingaverage")]
        [ResponseType(typeof(TimeSeriesData<double>))]
        public IHttpActionResult GetMovingAverage(string connectionId, string id, int window, string from = null, string to = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user:user).MovingAverage(window));
            }

            var fromDateTime = from == null ? DateTime.MinValue : Datetime.Parse(from);
            var toDateTime = to == null ? DateTime.MaxValue : Datetime.Parse(to);
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).MovingAverage(window));
        }

        /// <summary>
        ///     Gets a reduced (simplified) time series using the Ramer–Douglas–Peucker algorithm of the given time series within
        ///     the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'tolerance' query parameter is optional. The tolerance is a relative tolerance expressed as a percentage of the
        ///     difference between maximum and minimum value within the analysed time interval. If no tolerance is given, the
        ///     default value is 2%.
        ///     The 'minimumcount' query parameter is optional. The minimumcount defines at threshold value for ignoring the
        ///     request to reduce the times series. If, for example, the minimum count is set to 1000, and the number of data
        ///     points within the given time interval is less than 1000, the reduction request will be ignored, and the complete
        ///     time series will be returned. If no minimum count is given, the default value is 3000 data points.
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <param name="minimumCount">The minimum count.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series not found</response>
        [Route("{id}/reduced")]
        [ResponseType(typeof(TimeSeriesData<double>))]
        public IHttpActionResult GetReduced(string connectionId, string id, double tolerance = 2, int minimumCount = 3000, string from = null, string to = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user:user).Reduce(tolerance, minimumCount));
            }

            var fromDateTime = from == null ? DateTime.MinValue : Datetime.Parse(from);
            var toDateTime = to == null ? DateTime.MaxValue : Datetime.Parse(to);
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Reduce(tolerance, minimumCount));
        }

        /// <summary>
        ///     Gets a list of reduced (simplified) time series using the Ramer–Douglas–Peucker algorithm of the given list of time
        ///     series within the given time interval.
        /// </summary>
        /// <remarks>
        ///     The 'tolerance' query parameter is optional. The tolerance is a relative tolerance expressed as a percentage of the
        ///     difference between maximum and minimum value within the analysed time interval. If no tolerance is given, the
        ///     default value is 2%.
        ///     The 'minimumcount' query parameter is optional. The minimumcount defines at threshold value for ignoring the
        ///     request to reduce the times series. If, for example, the minimum count is set to 1000, and the number of data
        ///     points within the given time interval is less than 1000, the reduction request will be ignored, and the complete
        ///     time series will be returned. If no minimum count is given, the default value is 3000 data points.
        ///     The 'from' and 'to' query parameters can be omitted. If no time interval is given, the analysis is performed on the
        ///     entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="ids">The time series IDs.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <param name="minimumCount">The minimum count.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series not found</response>
        [Route("list/reduced")]
        [HttpPost]
        [ResponseType(typeof(Dictionary<string, ITimeSeriesData<double>>))]
        public IHttpActionResult GetReducedList(string connectionId, [FromBody] string[] ids, double tolerance = 2, int minimumCount = 3000, string from = null, string to = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
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

            var fromDateTime = from == null ? DateTime.MinValue : Datetime.Parse(from);
            var toDateTime = to == null ? DateTime.MaxValue : Datetime.Parse(to);

            foreach (var id in ids)
            {
                dictionary.Add(id, timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Reduce(tolerance, minimumCount));
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
        /// <param name="id">The identifier.</param>
        /// <param name="percentage">The percentage.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series not found</response>
        [Route("{id}/percentile/{percentage}")]
        [ResponseType(typeof(double?))]
        public IHttpActionResult GetPercentile(string connectionId, string id, int percentage, string from = null, string to = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user:user).PercentileValue(percentage));
            }

            var fromDateTime = from == null ? DateTime.MinValue : Datetime.Parse(from);
            var toDateTime = to == null ? DateTime.MaxValue : Datetime.Parse(to);
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
        /// <response code="404">Time series not found</response>
        [Route("list/percentile/{percentage}")]
        [HttpPost]
        [ResponseType(typeof(Dictionary<string, double?>))]
        public IHttpActionResult GetPercentileList(string connectionId, [FromBody] string[] ids, int percentage, string from = null, string to = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
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

            var fromDateTime = from == null ? DateTime.MinValue : Datetime.Parse(from);
            var toDateTime = to == null ? DateTime.MaxValue : Datetime.Parse(to);
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
        ///     The timespan query parameter is mandatory. The from and to query parameters are optional. If no time interval is
        ///     given, the analysis is performed on the entire range of time series values.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="timeSpan">The time span.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series not found</response>
        [Route("{id}/resampled")]
        [ResponseType(typeof(TimeSeriesData<double>))]
        public IHttpActionResult GetResampled(string connectionId, string id, TimeSpan timeSpan, string from = null, string to = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            var timeSeries = timeSeriesService.Get(FullNameString.FromUrl(id), user);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user:user).Resample(timeSpan, timeSeries.DataType));
            }

            var fromDateTime = from == null ? DateTime.MinValue : Datetime.Parse(from);
            var toDateTime = to == null ? DateTime.MaxValue : Datetime.Parse(to);
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user).Resample(timeSpan, timeSeries.DataType));
        }
    }
}