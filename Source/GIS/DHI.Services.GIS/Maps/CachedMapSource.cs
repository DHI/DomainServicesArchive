namespace DHI.Services.GIS.Maps
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Threading.Tasks;
    using Spatial;
    using SkiaSharp;

    /// <summary>
    ///     Map Source decorator supporting caching.
    /// </summary>
    /// <seealso cref="IMapSource" />
    public class CachedMapSource : BaseMapSource
    {
        private readonly MemoryCache _cache;
        private readonly int _cachedImageWidth;
        private readonly CacheItemPolicy _cacheItemPolicy;
        private readonly IMapSource _mapSource;
        private readonly int _numberOfCachedZoomLevels;
        private readonly ConcurrentDictionary<string, ZoomLevels> _zoomLevelsDictionary;
        private readonly ConcurrentDictionary<string, DateTime> _lastModifiedDictionary;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CachedMapSource" /> class.
        /// </summary>
        /// <param name="mapSource">The original map source to decorate.</param>
        /// <param name="parameters">The parameters.</param>
        public CachedMapSource(IMapSource mapSource, Parameters parameters)
        {
            _mapSource = mapSource ?? throw new ArgumentNullException(nameof(mapSource));
            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            _cachedImageWidth = parameters.GetParameter("CachedImageWidth", 1024);
            _numberOfCachedZoomLevels = parameters.GetParameter("NumberOfCachedZoomLevels", 5);
            _zoomLevelsDictionary = new ConcurrentDictionary<string, ZoomLevels>();
            _lastModifiedDictionary = new ConcurrentDictionary<string, DateTime>();
            _cache = MemoryCache.Default;
            _cacheItemPolicy = new CacheItemPolicy {SlidingExpiration = TimeSpan.FromMinutes(parameters.GetParameter("CacheExpirationInMinutes", 20))};
        }

        /// <inheritdoc />
        public override SKBitmap GetMap(MapStyle style, string crs, BoundingBox boundingBox, int width, int height, string filePath, DateTime? dateTime, string item, Parameters parameters)
        {
            if (!crs.IsGoogle())
            {
                throw new Exception($"Coordinate system '{crs}' is not supported. Only the Google Maps coordinate system (EPSG:3857) is supported.");
            }

            var tileImages = _GetTileImages(filePath, item, dateTime, boundingBox, style, crs, parameters).ToArray();
            var envelope = tileImages.GetEnvelope();
            if (envelope is null)
            {
                return new SKBitmap(width, height);
            }

            using var stitchedBitmap = Images.Stitch(tileImages);
            var envelopeBoundingBox = (BoundingBox)envelope;
            return Images.ComposeMap(stitchedBitmap, envelopeBoundingBox, boundingBox, new SKSizeI(width, height));
        }

        /// <inheritdoc />
        public override SortedSet<DateTime> GetDateTimes(string id)
        {
            return _mapSource.GetDateTimes(id);
        }

        private IEnumerable<TileImage> _GetTileImages(string filePath, string item, DateTime? datetime, BoundingBox boundingBox, MapStyle style, string crs, Parameters parameters)
        {
            var tileImages = new List<TileImage>();
            _ClearCacheIfFileIsModified(filePath);
            var cachedImageHeight = (int)(_cachedImageWidth * boundingBox.Height / boundingBox.Width);
            var zoomLevels = _GetZoomLevels(filePath, boundingBox);
            var (level, tiles) = zoomLevels.GetTiles(boundingBox);

            Parallel.ForEach(tiles, tile =>
            {
                var cacheKey = $"map-{filePath}-{item}-{datetime}-{level}-{tile}".Replace(':', '-').Replace('/', '-').Replace('\\', '-').Replace('.', '-');
                if (_cache[cacheKey] is TileImage tileImage)
                {
                    tileImages.Add(tileImage);
                }
                else
                {
                    tileImage = _cache[cacheKey] as TileImage;
                    if (tileImage != null)
                    {
                        tileImages.Add(tileImage);
                    }
                    else
                    {
                        lock (_cache)
                        {
                            var image = _mapSource.GetMap(style, crs, tile.BoundingBox, _cachedImageWidth, cachedImageHeight, filePath, datetime, item, parameters);
                            tileImage = new TileImage(image, tile);
                            _cache.Add(cacheKey, tileImage, _cacheItemPolicy);
                            tileImages.Add(tileImage);
                        }
                    }
                }
            });

            return tileImages;
        }

        private void _ClearCacheIfFileIsModified(string filepath)
        {
            var fileInfo = new FileInfo(filepath);
            if (_lastModifiedDictionary.TryGetValue(filepath, out var lastModified))
            {
                if (lastModified == fileInfo.LastWriteTime)
                {
                    return;
                }

                var cacheKeyStart = $"map-{filepath}-".Replace(':', '-').Replace('/', '-').Replace('\\', '-').Replace('.', '-');
                var keys = _cache.Where(kvp => kvp.Key.StartsWith(cacheKeyStart)).Select(kvp => kvp.Key).ToList();
                foreach (var key in keys)
                {
                    _cache.Remove(key);
                }

                _lastModifiedDictionary.TryUpdate(filepath, fileInfo.LastWriteTime, lastModified);
            }
            else
            {
                _lastModifiedDictionary.TryAdd(filepath, fileInfo.LastWriteTime);
            }
        }

        private ZoomLevels _GetZoomLevels(string filepath, BoundingBox boundingBox)
        {
            if (_zoomLevelsDictionary.TryGetValue(filepath, out var zoomLevel))
            {
                return zoomLevel;
            }

            zoomLevel = new ZoomLevels(boundingBox, _numberOfCachedZoomLevels);
            _zoomLevelsDictionary.TryAdd(filepath, zoomLevel);
            return zoomLevel;
        }
    }
}
