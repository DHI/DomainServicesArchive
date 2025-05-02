namespace DHI.Services.GIS.Maps
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Spatial;
    using SkiaSharp;

    public abstract class BaseGroupedMapSource : BaseDiscreteRepository<Layer, string>, IGroupedMapSource
    {
        /// <inheritdoc />
        public abstract IEnumerable<Layer> GetByGroup(string group, ClaimsPrincipal user = null);

        /// <inheritdoc />
        public abstract bool ContainsGroup(string group, ClaimsPrincipal user = null);

        /// <inheritdoc />
        public virtual IEnumerable<string> GetFullNames(string group, ClaimsPrincipal user = null)
        {
            return GetByGroup(group, user).Select(layer => layer.FullName).ToArray();
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> GetFullNames(ClaimsPrincipal user = null)
        {
            return GetAll(user).Select(layer => layer.FullName).ToArray();
        }

        /// <inheritdoc />
        public abstract SKBitmap GetMap(MapStyle style, string crs, BoundingBox boundingBox, int width, int height, string sourceId, DateTime? dateTime, string item, Parameters parameters);

        /// <inheritdoc />
        public SortedDictionary<DateTime, SKBitmap> GetMaps(MapStyle style, BoundingBox boundingBox, SKSizeI size, Dictionary<DateTime, string> timeSteps, string item, Parameters parameters)
        {
            var images = new SortedDictionary<DateTime, SKBitmap>();
            Parallel.ForEach(Partitioner.Create(timeSteps, EnumerablePartitionerOptions.NoBuffering), timeStep =>
            {
                var map = GetMap(style, "EPSG:3857", boundingBox, size.Width, size.Height, timeStep.Value, timeStep.Key, item, parameters);
                images.Add(timeStep.Key, map);
            });

            return images;
        }

        /// <inheritdoc />
        public abstract SortedSet<DateTime> GetDateTimes(string id);

        /// <inheritdoc />
        public SortedSet<DateTime> GetDateTimes(string id, DateRange dateRange)
        {
            return (SortedSet<DateTime>)GetDateTimes(id).Where(d => d >= dateRange.From && d <= dateRange.To);
        }

        /// <inheritdoc />
        public abstract (Maybe<Stream>, string fileType, string fileName) GetStream(string id, ClaimsPrincipal user = null);
    }
}