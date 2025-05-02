namespace DHI.Services.Rasters.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.IO;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Radar;
    using Swashbuckle.AspNetCore.Annotations;
    using Zones;

    /// <summary>
    ///     Radar Images API
    /// </summary>
    [Produces("application/json")]
    [Route("api/radarimages/{connectionId}")]
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for radar images analysis.")]
    public class RadarImagesController<TImage> : ControllerBase where TImage : IRadarImage
    {
        private readonly ZoneService _zoneService;

        public RadarImagesController(IZoneRepository zoneRepository)
        {
            _zoneService = new ZoneService(zoneRepository);
        }

        /// <summary>
        ///     Gets a radar image at the given datetime.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="date">The date time.</param>
        [HttpGet("{date:datetime}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<TImage> Get(string connectionId, DateTime date)
        {
            var user = HttpContext.User;
            var radarImageService = Services.Get<RadarImageService<TImage>>(connectionId);
            return Ok(radarImageService.Get(date, user));
        }

        /// <summary>
        ///     Gets the last available radar image.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet("last")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<TImage> GetLast(string connectionId)
        {
            var user = HttpContext.User;
            var radarImageService = Services.Get<RadarImageService<TImage>>(connectionId);
            return Ok(radarImageService.Last(user));
        }

        /// <summary>
        ///     Gets the last available radar image before the give datetime.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="date">The date time.</param>
        [HttpGet("lastbefore/{date:datetime}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<TImage> GetLastBefore(string connectionId, DateTime date)
        {
            var user = HttpContext.User;
            var radarImageService = Services.Get<RadarImageService<TImage>>(connectionId);
            return Ok(radarImageService.GetLastBefore(date, user));
        }

        /// <summary>
        ///     Gets a list of the last available radar images before the given datetimes.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="dateTimes">The date times.</param>
        [HttpPost("list/lastbefore")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<TImage>> GetLastBefore(string connectionId, [FromBody] IEnumerable<DateTime> dateTimes)
        {
            var user = HttpContext.User;
            var radarImageService = Services.Get<RadarImageService<TImage>>(connectionId);
            return Ok(radarImageService.GetLastBefore(dateTimes, user));
        }

        /// <summary>
        ///     Gets the first radar image after the given datetime.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="date">The date time.</param>
        [HttpGet("firstafter/{date:datetime}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<TImage> GetFirstAfter(string connectionId, DateTime date)
        {
            var user = HttpContext.User;
            var radarImageService = Services.Get<RadarImageService<TImage>>(connectionId);
            return Ok(radarImageService.GetFirstAfter(date, user));
        }

        /// <summary>
        ///     Gets a list of the first radar images after the given datetimes.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="dateTimes">The date times.</param>
        [HttpPost("list/firstafter")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<TImage>> GetFirstAfter(string connectionId, [FromBody] IEnumerable<DateTime> dateTimes)
        {
            var user = HttpContext.User;
            var radarImageService = Services.Get<RadarImageService<TImage>>(connectionId);
            return Ok(radarImageService.GetFirstAfter(dateTimes, user));
        }

        /// <summary>
        ///     Gets the datetime for the last available radar image.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet("datetime/last")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<DateTime> GetLastDateTime(string connectionId)
        {
            var user = HttpContext.User;
            var radarImageService = Services.Get<RadarImageService<TImage>>(connectionId);
            return Ok(radarImageService.LastDateTime(user));
        }

        /// <summary>
        ///     Gets the datetime for the first available radar image.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet("datetime/first")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<DateTime> GetFirstDateTime(string connectionId)
        {
            var user = HttpContext.User;
            var radarImageService = Services.Get<RadarImageService<TImage>>(connectionId);
            return Ok(radarImageService.FirstDateTime(user));
        }

        /// <summary>
        ///     Gets a list of datetimes within the given time interval.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        [HttpGet("datetimes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<DateTime>> GetDateTimes(string connectionId, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var radarImageService = Services.Get<RadarImageService<TImage>>(connectionId);
            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(radarImageService.GetDateTimes(fromDateTime, toDateTime, user));
        }

        /// <summary>
        ///     Gets a list of the first datetimes after after the given datetimes.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="dateTimes">The datetimes.</param>
        [HttpPost("datetimes/firstafter")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<DateTime>> GetDateTimesFirstAfter(string connectionId, [FromBody] IEnumerable<DateTime> dateTimes)
        {
            var user = HttpContext.User;
            var radarImageService = Services.Get<RadarImageService<TImage>>(connectionId);
            return Ok(radarImageService.GetDateTimesFirstAfter(dateTimes, user));
        }

        /// <summary>
        ///     Gets a list of the last datetimes before the given datetimes.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="dateTimes">The datetimes.</param>
        [HttpPost("datetimes/lastbefore")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<DateTime>> GetDateTimesLastBefore(string connectionId, [FromBody] IEnumerable<DateTime> dateTimes)
        {
            var user = HttpContext.User;
            var radarImageService = Services.Get<RadarImageService<TImage>>(connectionId);
            return Ok(radarImageService.GetDateTimesLastBefore(dateTimes, user));
        }

        /// <summary>
        ///     Calculates the accumulated rainfall (the depth) in the given zone within the given time interval.
        /// </summary>
        /// <remarks>
        ///     Having defined zones within the radar image boundaries, various statistical information for a zone - such as the
        ///     accumulated rainfall (the depth) or the average rain intensity within a certain time interval - can be calculated
        ///     and retrieved on the fly.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="zoneId">The zone identifier.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        [HttpGet("depth/{zoneId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<double> GetDepthByTimeInterval(string connectionId, string zoneId, DateTime from, DateTime? to = null)
        {
            var user = HttpContext.User;
            var radarImageService = Services.Get<RadarImageService<TImage>>(connectionId);
            var toDateTime = to ?? radarImageService.LastDateTime(user);
            var zone = _zoneService.Get(zoneId, user);
            return Ok(radarImageService.GetDepth(zone, from, toDateTime, user));
        }

        /// <summary>
        ///     Calculates the accumulated rainfall (the depth) in the given zone within the last given number of hours.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="zoneId">The zone identifier.</param>
        /// <param name="hours">The number of hours.</param>
        [HttpGet("depth/{zoneId}/hours/{hours}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<double> GetDepth(string connectionId, string zoneId, int hours)
        {
            var user = HttpContext.User;
            var radarImageService = Services.Get<RadarImageService<TImage>>(connectionId);
            var timespan = new TimeSpan(hours, 0, 0);
            var zone = _zoneService.Get(zoneId, user);
            return Ok(radarImageService.GetDepth(zone, timespan));
        }

        /// <summary>
        ///     Returns a time series of mean intensities (mm/h) in the given zone within the given time interval.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="zoneId">The zone identifier.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        [HttpGet("intensities/{zoneId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<SortedDictionary<DateTime, double>> GetIntensities(string connectionId, string zoneId, DateTime from, DateTime? to = null)
        {
            var user = HttpContext.User;
            var radarImageService = Services.Get<RadarImageService<TImage>>(connectionId);
            var zone = _zoneService.Get(zoneId, user);
            var toDateTime = to ?? radarImageService.LastDateTime(user);
            return Ok(radarImageService.GetIntensities(zone, from, toDateTime, user));
        }

        /// <summary>
        ///     Calculates the maximum intensity (mm/h) in the given zone within the given time interval.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="zoneId">The zone identifier.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        [HttpGet("intensity/max/{zoneId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<double> GetMaxIntensity(string connectionId, string zoneId, DateTime from, DateTime? to = null)
        {
            var user = HttpContext.User;
            var radarImageService = Services.Get<RadarImageService<TImage>>(connectionId);
            var toDateTime = to ?? radarImageService.LastDateTime(user);
            var zone = _zoneService.Get(zoneId, user);
            return Ok(radarImageService.GetMaxIntensity(zone, from, toDateTime, user));
        }

        /// <summary>
        ///     Calculates the average intensity (mm/h) in the given zone within the given time interval.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="zoneId">The zone identifier.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        [HttpGet("intensity/average/{zoneId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<double> GetAverageIntensity(string connectionId, string zoneId, DateTime from, DateTime? to = null)
        {
            var user = HttpContext.User;
            var radarImageService = Services.Get<RadarImageService<TImage>>(connectionId);
            var toDateTime = to ?? radarImageService.LastDateTime(user);
            var zone = _zoneService.Get(zoneId, user);
            return Ok(radarImageService.GetAverageIntensity(zone, from, toDateTime, user));
        }

        /// <summary>
        ///     Calculates the average intensity (mm/h) in the given zone within the last given number of hours.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="zoneId">The zone identifier.</param>
        /// <param name="hours">The number of hours.</param>
        [HttpGet("intensity/average/{zoneId}/hours/{hours}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<double> GetAverageIntensity(string connectionId, string zoneId, int hours)
        {
            var user = HttpContext.User;
            var radarImageService = Services.Get<RadarImageService<TImage>>(connectionId);
            var timespan = new TimeSpan(hours, 0, 0);
            var zone = _zoneService.Get(zoneId, user);
            return Ok(radarImageService.GetAverageIntensity(zone, timespan, user));
        }

        /// <summary>
        ///     Gets a bitmap of the radar image intensity at the given datetime using the given color gradient type.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="date">The date time.</param>
        /// <param name="style">The color gradient style.</param>
        [HttpGet("{date:datetime}/bitmap")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAsBitmap(string connectionId, DateTime date, string style = "IntensityDefault")
        {
            var user = HttpContext.User;
            var colorGradientType = Enumeration.FromDisplayName<ColorGradientType>(style);
            var radarImageService = Services.Get<RadarImageService<TImage>>(connectionId);
            var image = radarImageService.Get(date, user);
            if (image.PixelValueType != PixelValueType.Intensity)
            {
                image = (TImage)image.ToIntensity();
            }

            var bitmap = image.ToBitmap(colorGradientType.ColorGradient);
            var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return File(memoryStream, "image/png");
        }

        /// <summary>
        ///     Gets the map style as a bitmap.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="style">The map style.</param>
        /// <param name="height">The bitmap height in pixels.</param>
        /// <param name="width">The bitmap width in pixels.</param>
        [HttpGet("style/{style}/bitmap")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetStyleAsBitmap(string connectionId, string style, int height = 300, int width = 100)
        {
            var colorGradientType = Enumeration.FromDisplayName<ColorGradientType>(style);
            var bitmap = colorGradientType.ColorGradient.ToBitmap(height, width);
            var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return File(memoryStream, "image/png");
        }

        /// <summary>
        ///     Gets a bitmap of the last radar image intensity using the given color gradient type.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// /// <param name="style">The color gradient style.</param>
        [HttpGet("last/bitmap")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetLastAsBitmap(string connectionId, string style = "IntensityDefault")
        {
            var user = HttpContext.User;

            var colorGradientType = Enumeration.FromDisplayName<ColorGradientType>(style);
            var radarImageService = Services.Get<RadarImageService<TImage>>(connectionId);
            var image = radarImageService.Last(user);
            if (image.PixelValueType != PixelValueType.Intensity)
            {
                image = (TImage)image.ToIntensity();
            }

            var bitmap = image.ToBitmap(colorGradientType.ColorGradient);
            var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return File(memoryStream, "image/png");
        }
    }
}