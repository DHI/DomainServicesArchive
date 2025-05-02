namespace DHI.Services.GIS.Maps
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Spatial;
    using SkiaSharp;

    public abstract class BaseMapSource : IMapSource
    {
        /// <inheritdoc />
        public abstract SKBitmap GetMap(MapStyle style, string crs, BoundingBox boundingBox, int width, int height, string sourceId, DateTime? dateTime, string item, Parameters parameters);

        /// <inheritdoc />
        public virtual SortedDictionary<DateTime, SKBitmap> GetMaps(MapStyle style, BoundingBox boundingBox, SKSizeI size, Dictionary<DateTime, string> timeSteps, string item, Parameters parameters)
        {
            var images = new ConcurrentBag<(DateTime, SKBitmap)>();
            Parallel.ForEach(Partitioner.Create(timeSteps, EnumerablePartitionerOptions.NoBuffering), timeStep =>
            {
                var map = GetMap(style, "EPSG:3857", boundingBox, size.Width, size.Height, timeStep.Value, timeStep.Key, item, parameters);
                images.Add((timeStep.Key, map));
            });

            return new SortedDictionary<DateTime, SKBitmap>(images.ToDictionary(k => k.Item1, v => v.Item2));
        }

        /// <inheritdoc />
        public abstract SortedSet<DateTime> GetDateTimes(string id);

        /// <inheritdoc />
        public SortedSet<DateTime> GetDateTimes(string id, DateRange dateRange)
        {
            var dateTimes = GetDateTimes(id).Where(d => d >= dateRange.From && d <= dateRange.To);
            return new SortedSet<DateTime>(dateTimes);
        }
    }
}