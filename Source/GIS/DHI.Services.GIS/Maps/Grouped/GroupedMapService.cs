namespace DHI.Services.GIS.Maps
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Claims;
    using Spatial;
    using SkiaSharp;

    public class GroupedMapService : BaseGroupedDiscreteService<Layer, string>, IGroupedMapService
    {
        private readonly MapService _mapService;
        private readonly IGroupedMapSource _groupMapSource;

        public GroupedMapService(IGroupedMapSource groupedMapSource, MapStyleService mapStyleService = null)
            : base(groupedMapSource)
        {
            _mapService = new MapService(groupedMapSource, mapStyleService);
            _groupMapSource = groupedMapSource;
        }

        /// <inheritdoc />
        public SKBitmap GetMap(string style, string crs, BoundingBox boundingBox, int width, int height, string sourceId, DateTime? dateTime, string item, Parameters parameters)
        {
            return _mapService.GetMap(style, crs, boundingBox, width, height, sourceId, dateTime, item, parameters);
        }

        /// <inheritdoc />
        public SortedDictionary<DateTime, SKBitmap> GetMaps(string style, BoundingBox boundingBox, SKSizeI size, Dictionary<DateTime, string> timeSteps, string item, Parameters parameters)
        {
            return _mapService.GetMaps(style, boundingBox, size, timeSteps, item, parameters);
        }

        /// <inheritdoc />
        public SortedSet<DateTime> GetDateTimes(string id)
        {
            return _mapService.GetDateTimes(id);
        }

        /// <inheritdoc />
        public SortedSet<DateTime> GetDateTimes(string id, DateRange dateRange)
        {
            return _mapService.GetDateTimes(id, dateRange);
        }

        /// <inheritdoc />
        public (Stream, string fileType, string fileName) GetStream(string id, ClaimsPrincipal user = null)
        {
            var (maybe, fileType, fileName) = _groupMapSource.GetStream(id, user);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"Feature collection with id '{id}' was not found.");
            }
            return (maybe.Value, fileType, fileName);
        }

        public static Type[] GetMapSourceTypes(string path = null)
        {
            return Service.GetProviderTypes<IGroupedMapSource>(path);
        }

        public static Type[] GetMapStyleRepositoryTypes(string path = null)
        {
            return Service.GetProviderTypes<IMapStyleRepository>(path);
        }
    }
}