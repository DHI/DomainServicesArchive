namespace DHI.Services.Rasters.Radar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Zones;

    /// <summary>
    /// Class RadarImageService.
    /// </summary>
    /// <typeparam name="TImage">The type of the radar image.</typeparam>
    /// <seealso cref="DHI.Services.Rasters.RasterService{TImage}" />
    public class RadarImageService<TImage> : RasterService<TImage> where TImage : IRadarImage
    {
        /// <summary>
        /// The repository
        /// </summary>
        private readonly IRasterRepository<TImage> _repository;

        /// <summary>
        /// The maximum analysis time span
        /// </summary>
        private readonly TimeSpan _maxAnalysisTimeSpan;

        /// <summary>
        /// Initializes a new instance of the <see cref="RadarImageService{TImage}" /> class.
        /// </summary>
        /// <param name="repository">The raster image repository.</param>
        public RadarImageService(IRasterRepository<TImage> repository) : base(repository)
        {
            _repository = repository;
            _maxAnalysisTimeSpan = TimeSpan.FromDays(30);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RadarImageService{TImage}" /> class.
        /// </summary>
        /// <param name="repository">The raster image repository.</param>
        /// <param name="maxAnalysisTimeSpan">The maximum time span for images analysis.</param>
        /// <param name="maxTimeSpan">The maximum time span for retrieving batches of images.</param>
        public RadarImageService(IRasterRepository<TImage> repository, TimeSpan maxAnalysisTimeSpan, TimeSpan maxTimeSpan) : base(repository, maxTimeSpan)
        {
            _repository = repository;
            _maxAnalysisTimeSpan = maxAnalysisTimeSpan;
        }

        /// <summary>
        /// Gets an array of available radar image types.
        /// </summary>
        /// <param name="path">The path to look for radar image types.</param>
        public static Type[] GetImageTypes(string path = null)
        {
            return Service.GetProviderTypes<IRadarImage>(path);
        }

        /// <summary>
        /// Gets the average rainfall intensity (using default conversion coefficients) for the given zone within the specified
        /// time interval.
        /// </summary>
        /// <param name="zone">The zone.</param>
        /// <param name="from">Time interval start.</param>
        /// <param name="to">Time interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>The average rainfall intensity.</returns>
        public double GetAverageIntensity(Zone zone, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            return GetAverageIntensity(zone, from, to, ConversionCoefficients.Default, user);
        }

        /// <summary>
        /// Gets the average rainfall intensity (using default conversion coefficients) for the given zone within the specified
        /// time span.
        /// </summary>
        /// <param name="zone">The zone.</param>
        /// <param name="timeSpan">The Time span.</param>
        /// <param name="user">The user.</param>
        /// <returns>The average rainfall intensity.</returns>
        public double GetAverageIntensity(Zone zone, TimeSpan timeSpan, ClaimsPrincipal user = null)
        {
            var to = LastDateTime(user);
            var from = to.Add(-timeSpan);
            return GetAverageIntensity(zone, from, to);
        }

        /// <summary>
        /// Gets the average rainfall intensity (using the specified conversion coefficients) for the given zone within the
        /// specified time interval.
        /// </summary>
        /// <param name="zone">The zone.</param>
        /// <param name="from">Time interval start.</param>
        /// <param name="to">Time interval end.</param>
        /// <param name="conversionCoefficients">The reflectivity-to-intensity conversion coefficients.</param>
        /// <param name="user">The user.</param>
        /// <returns>The average rainfall intensity.</returns>
        public double GetAverageIntensity(Zone zone, DateTime from, DateTime to, ConversionCoefficients conversionCoefficients, ClaimsPrincipal user = null)
        {
            return GetIntensities(zone, from, to, conversionCoefficients, user).Values.Average();
        }

        /// <summary>
        /// Gets the average rainfall intensity (using the specified conversion coefficients) for the given zone within the
        /// specified time span.
        /// </summary>
        /// <param name="zone">The zone.</param>
        /// <param name="timeSpan">The time span.</param>
        /// <param name="conversionCoefficients">The reflectivity-to-intensity conversion coefficients.</param>
        /// <param name="user">The user.</param>
        /// <returns>The average rainfall intensity.</returns>
        public double GetAverageIntensity(Zone zone, TimeSpan timeSpan, ConversionCoefficients conversionCoefficients, ClaimsPrincipal user = null)
        {
            var to = LastDateTime(user);
            var from = to.Add(-timeSpan);
            return GetAverageIntensity(zone, from, to, conversionCoefficients, user);
        }

        /// <summary>
        /// Gets the depth (mm) for the given zone within the specified time interval.
        /// </summary>
        /// <param name="zone">The zone.</param>
        /// <param name="from">Time interval start.</param>
        /// <param name="to">Time interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>The depth.</returns>
        /// <exception cref="Exception"></exception>
        public double GetDepth(Zone zone, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            var intensities = GetIntensities(zone, from, to, user);
            if (intensities.Count <= 1)
            {
                throw new Exception($"Less than 2 images is found in the specified interval from '{from}' to '{to}' ");
            }

            var depth = 0d;
            for (var i = 1; i < intensities.Count; i++)
            {
                var intervalLength = intensities.ElementAt(i).Key - intensities.ElementAt(i - 1).Key;
                var meanIntensity = (intensities.ElementAt(i).Value + intensities.ElementAt(i - 1).Value) / 2;
                depth += intervalLength.TotalHours * meanIntensity;
            }

            return depth;
        }

        /// <summary>
        /// Gets the depth (mm) for the given zone within the specified time span.
        /// </summary>
        /// <param name="zone">The zone.</param>
        /// <param name="timeSpan">The time span.</param>
        /// <param name="user">The user.</param>
        /// <returns>The depth.</returns>
        public double GetDepth(Zone zone, TimeSpan timeSpan, ClaimsPrincipal user = null)
        {
            var to = LastDateTime(user);
            var from = to.Add(-timeSpan);
            return GetDepth(zone, from, to);
        }

        /// <summary>
        /// Gets a dictionary (time series) of rainfall intensities (using default conversion coefficients) for the given zone
        /// within the specified time interval.
        /// </summary>
        /// <param name="zone">The zone.</param>
        /// <param name="from">Time interval start.</param>
        /// <param name="to">Time interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>A dictionary (time series) of rainfall intensities.</returns>
        public SortedDictionary<DateTime, double> GetIntensities(Zone zone, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            return GetIntensities(zone, from, to, ConversionCoefficients.Default, user);
        }

        /// <summary>
        /// Gets a dictionary (time series) of rainfall intensities (using the specified conversion coefficients) for the given
        /// zone within the specified time interval.
        /// </summary>
        /// <param name="zone">The zone.</param>
        /// <param name="from">Time interval start.</param>
        /// <param name="to">Time interval end.</param>
        /// <param name="conversionCoefficients">The reflectivity-to-intensity conversion coefficients.</param>
        /// <param name="user">The user.</param>
        /// <returns>A dictionary (time series) of rainfall intensities.</returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        public SortedDictionary<DateTime, double> GetIntensities(Zone zone, DateTime from, DateTime to, ConversionCoefficients conversionCoefficients, ClaimsPrincipal user = null)
        {
            if (from >= to)
            {
                throw new ArgumentException($"FROM date/time '{from}' must be less than TO date/time '{to}'");
            }

            var timeSpan = to - from;
            if (timeSpan > _maxAnalysisTimeSpan)
            {
                throw new ArgumentException(
                    $"The requested time span for image analysis: {timeSpan} is exceeding the maximum time span of {_maxAnalysisTimeSpan}. The maximum analysis time span can be set through constructor injection in the RadarImagesService.");
            }

            var intensities = new SortedDictionary<DateTime, double>();
            if (timeSpan < MaxTimeSpan)
            {
                var images = Get(from, to, user);
                foreach (var image in images)
                {
                    intensities.Add(image.Key, image.Value.GetIntensity(zone, conversionCoefficients));
                }
            }
            else // split into smaller intervals
            {
                var start = from;
                var end = from + MaxTimeSpan;
                while (end < to)
                {
                    var images = Get(start, end, user);
                    foreach (var image in images)
                    {
                        intensities.Add(image.Key, image.Value.GetIntensity(zone, conversionCoefficients));
                    }

                    start = end.AddSeconds(1);
                    end = start + MaxTimeSpan;
                }

                // Final interval
                end = to;
                var lastImages = Get(start, end, user);
                foreach (var image in lastImages)
                {
                    intensities.Add(image.Key, image.Value.GetIntensity(zone, conversionCoefficients));
                }
            }

            return intensities;
        }

        /// <summary>
        /// Gets the rainfall intensity (using default conversion coefficients) for the given zone at the specified date/time.
        /// </summary>
        /// <param name="zone">The zone.</param>
        /// <param name="dateTime">The date/time.</param>
        /// <param name="user">The user.</param>
        /// <returns>The intensity.</returns>
        public double GetIntensity(Zone zone, DateTime dateTime, ClaimsPrincipal user = null)
        {
            return GetIntensity(zone, dateTime, ConversionCoefficients.Default, user);
        }

        /// <summary>
        /// Gets the rainfall intensity (using the specified conversion coefficients) for the given zone at the specified
        /// date/time.
        /// </summary>
        /// <param name="zone">The zone.</param>
        /// <param name="dateTime">The date/time.</param>
        /// <param name="conversionCoefficients">The reflectivity-to-intensity conversion coefficients.</param>
        /// <param name="user">The user.</param>
        /// <returns>The intensity.</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public double GetIntensity(Zone zone, DateTime dateTime, ConversionCoefficients conversionCoefficients, ClaimsPrincipal user = null)
        {
            if (!_repository.Contains(dateTime, user))
            {
                throw new KeyNotFoundException($"Image with date/time {dateTime} was not found");
            }

            if (TryGet(dateTime, out var image, user))
            {
                return image.GetIntensity(zone, conversionCoefficients);
            }

            throw new KeyNotFoundException($"Image with date/time {dateTime} was not found");
        }

        /// <summary>
        /// Gets the maximum rainfall intensity (using default conversion coefficients) for the given zone within the specified time interval.
        /// </summary>
        /// <param name="zone">The zone.</param>
        /// <param name="from">Time interval start.</param>
        /// <param name="to">Time interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>The maximum rainfall intensity.</returns>
        public double GetMaxIntensity(Zone zone, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            return GetMaxIntensity(zone, from, to, ConversionCoefficients.Default, user);
        }

        /// <summary>
        /// Gets the maximum rainfall intensity (using the specified conversion coefficients) for the given zone within the specified time interval.
        /// </summary>
        /// <param name="zone">The zone.</param>
        /// <param name="from">Time interval start.</param>
        /// <param name="to">Time interval end.</param>
        /// <param name="conversionCoefficients">The reflectivity-to-intensity conversion coefficients.</param>
        /// <param name="user">The user.</param>
        /// <returns>The maximum rainfall intensity.</returns>
        public double GetMaxIntensity(Zone zone, DateTime from, DateTime to, ConversionCoefficients conversionCoefficients, ClaimsPrincipal user = null)
        {
            return GetIntensities(zone, from, to, conversionCoefficients, user).Values.Max();
        }

        /// <summary>
        /// Gets the minimum rainfall intensity (using default conversion coefficients) for the given zone within the specified
        /// time interval.
        /// </summary>
        /// <param name="zone">The zone.</param>
        /// <param name="from">Time interval start.</param>
        /// <param name="to">Time interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>The minimum rainfall intensity.</returns>
        public double GetMinIntensity(Zone zone, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            return GetMinIntensity(zone, from, to, ConversionCoefficients.Default, user);
        }

        /// <summary>
        /// Gets the minimum rainfall intensity (using the specified conversion coefficients) for the given zone within the
        /// specified time interval.
        /// </summary>
        /// <param name="zone">The zone.</param>
        /// <param name="from">Time interval start.</param>
        /// <param name="to">Time interval end.</param>
        /// <param name="conversionCoefficients">The reflectivity-to-intensity conversion coefficients.</param>
        /// <param name="user">The user.</param>
        /// <returns>The minimum rainfall intensity.</returns>
        public double GetMinIntensity(Zone zone, DateTime from, DateTime to, ConversionCoefficients conversionCoefficients, ClaimsPrincipal user = null)
        {
            return GetIntensities(zone, from, to, conversionCoefficients, user).Values.Min();
        }

        /// <summary>
        /// Gets the bias corrected image.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="biasCorrectionService">The bias correction service.</param>
        /// <param name="user">The user.</param>
        /// <returns>TImage.</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="ArgumentNullException">biasCorrectionService</exception>
        public TImage GetCorrected(DateTime dateTime, BiasCorrectionService biasCorrectionService, ClaimsPrincipal user = null)
        {
            if (!_repository.Contains(dateTime))
            {
                throw new KeyNotFoundException($"Image with id '{dateTime}' was not found.");
            }

            if (biasCorrectionService == null)
            {
                throw new ArgumentNullException(nameof(biasCorrectionService));
            }

            if (!TryGet(dateTime, out var image, user))
            {
                throw new KeyNotFoundException($"Image with id '{dateTime}' was not found.");
            }

            var correctionMatrix = biasCorrectionService.GetLastBefore(dateTime);
            if (!correctionMatrix.HasValue)
            {
                return image;
            }

            image.Correct(correctionMatrix.Value);
            return image;
        }
    }
}