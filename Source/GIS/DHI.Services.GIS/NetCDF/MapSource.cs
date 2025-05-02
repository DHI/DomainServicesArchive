namespace DHI.Services.GIS.NetCDF
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Maps;
    using Spatial;
    using SkiaSharp;

    public class MapSource : BaseMapSource
    {
        private const int _cacheSize = 5;
        private readonly List<string> _cacheQueue = new();
        private readonly List<string> _cacheNcFileQueue = new();
        private readonly Dictionary<string, List<DateTime>> _cacheNcFileTimestamps = new();
        private readonly Dictionary<string, List<double>> _cacheNcFileDataArray = new();
        private readonly Dictionary<string, Dictionary<string, double>> _cacheElementData = new();
        private readonly Dictionary<string, Dictionary<string, double>> _cacheNodeData = new();
        private readonly Dictionary<string, MapGraphicElement> _elements = new();
        private readonly Dictionary<string, MapGraphicNode> _nodes = new();
        private readonly string _filePath;
        private readonly Parameters _parameters;
        private readonly DynamicFileSource _fileSource;
        private readonly object _syncRoot = new();
        private bool _metadataLoaded;
        private BoundingBox _bboxLonLat, _bboxGoogle;
        private List<double> _lats, _lons;

        public MapSource(string filePath, Parameters parameters)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _fileSource = new DynamicFileSource(filePath);
            _parameters = parameters;
        }

        public Layer GetLayerInfo()
        {
            var boundingBox = new BoundingBox(0, 0, 0, 0);
            var coordinateSystem = "Unknown";
            if (!_metadataLoaded)
            {
                var file = _filePath;
                if (File.Exists(file))
                {
                    _GetMetaData(file);
                }
                else
                {
                    file = _parameters.GetParameter("DefaultFile", "");
                    if (File.Exists(file))
                    {
                        _GetMetaData(file);
                    }
                }
            }

            if (_metadataLoaded)
            {
                boundingBox = _bboxLonLat;
                coordinateSystem = "EPSG:4326";
            }

            var layer = new Layer("NetCDF", "NetCDF") {BoundingBox = boundingBox, CoordinateSystem = coordinateSystem};
            return layer;
        }

        public override SKBitmap GetMap(MapStyle style, string crs, BoundingBox boundingBox, int width, int height, string filePath, DateTime? dateTime, string item, Parameters parameters)
        {
            var timestamp = dateTime ?? DateTime.MinValue;
            var varName = parameters.GetParameter("variable", "");
            var palette = style.GetPalette();
            var map = new SKBitmap(width, height);
            using var graphic = new SKCanvas(map);
            var isGoogle = crs.IsGoogle();
            var googleBbox = isGoogle ? boundingBox : boundingBox.LonLatToGoogle();
            var lonLatBbox = isGoogle ? boundingBox.GoogleToLonLat() : boundingBox;
            var origin = new Position(googleBbox.Xmin, googleBbox.Ymin);
            var dx = googleBbox.Width / width;
            var dy = googleBbox.Height / height;
            var foundElementIds = new List<string>();
            var file = _fileSource.GetFile(timestamp, parameters);
            if (File.Exists(file))
            {
                var fileInfo = new FileInfo(file);
                var fileLastWriteId = fileInfo.LastWriteTime.ToIdStandard();
                if (!_metadataLoaded)
                {
                    _GetMetaData(file);
                }
                var cacheId = varName + "_" + timestamp.ToIdStandard() + "_" + fileLastWriteId;
                if (googleBbox.Intersects(_bboxGoogle))
                {
                    var hasData = true;
                    if (!_cacheQueue.Contains(cacheId))
                    {
                        hasData = _GetData(file, varName, timestamp, fileLastWriteId, cacheId);
                    }
                    if (hasData)
                    {
                        var elementIds = _elements.Keys;
                        foreach (var elementId in elementIds)
                        {
                            var element = _elements[elementId];
                            if (googleBbox.Intersects(element.GoogleBoundingBox))
                            {
                                foundElementIds.Add(element.Id);
                            }
                        }
                        if (foundElementIds.Count > 0)
                        {
                            var contour = parameters.GetParameter("contour", true);
                            var nodeValues = _cacheNodeData[cacheId];
                            var elementValues = _cacheElementData[cacheId];
                            var contourLine = parameters.GetParameter("contour_line", false);
                            var contourLineColor = SKColor.Parse("#" + parameters.GetParameter("contour_line_color", "000000"));
                            MapGraphic.PaintMap(graphic, origin, dx, dy, height, palette,
                                contour, contourLine, contourLineColor, foundElementIds, _elements, _nodes, nodeValues, elementValues);
                        }
                    }
                }
            }
            return map;
        }

        public override SortedSet<DateTime> GetDateTimes(string id)
        {
            lock (_syncRoot)
            {
                var ncFile = new NcFile();
                ncFile.Load(_filePath);
                return new SortedSet<DateTime>(ncFile.GetTimestamps());
            }
        }

        private void _GetMetaData(string file)
        {
            lock (_syncRoot)
            {
                if (_metadataLoaded)
                {
                    return;
                }
                var ncFile = new NcFile();
                ncFile.Load(file);
                _lats = ncFile.GetVariableData<double>("lat");
                _lons = ncFile.GetVariableData<double>("lon");
                _bboxLonLat = new BoundingBox(_lons[0], _lats[0], _lons[_lons.Count - 1], _lats[_lats.Count - 1]);
                _bboxGoogle = _bboxLonLat.LonLatToGoogle();
                NcFile.GetElementAndNode(_lons, _lats, _elements, _nodes);
                _metadataLoaded = true;
            }
        }
        
        private bool _GetData(string file, string varName, DateTime timestamp, string fileLastWrite, string cacheId)
        {
            lock (_syncRoot)
            {
                if (_cacheQueue.Contains(cacheId))
                {
                    return true;
                }                
                var ncFileCacheId = file + "_" + fileLastWrite;
                if (!_cacheNcFileQueue.Contains(ncFileCacheId))
                {
                    var ncFile = new NcFile();
                    ncFile.Load(file);
                    _cacheNcFileQueue.Add(ncFileCacheId);
                    _cacheNcFileTimestamps.Add(ncFileCacheId, ncFile.GetTimestamps());
                    _cacheNcFileDataArray.Add(ncFileCacheId, ncFile.GetVariableData<double>(varName));
                    if (_cacheSize == _cacheNcFileQueue.Count)
                    {
                        var oldest = _cacheNcFileQueue[0];
                        _cacheNcFileQueue.RemoveAt(0);
                        _cacheNcFileTimestamps.Remove(oldest);
                        _cacheNcFileDataArray.Remove(oldest);
                    }
                }
                var timestamps = _cacheNcFileTimestamps[ncFileCacheId];
                var dataArray = _cacheNcFileDataArray[ncFileCacheId];
                var timestep = -1;
                if (timestamp == DateTime.MinValue)
                {
                    timestep = 0;
                }
                else
                {
                    for (var t = 0; t < timestamps.Count; t++)
                    {
                        if (timestamps[t] == timestamp)
                        {
                            timestep = t;
                            break;
                        }
                    }
                }
                if (timestep == -1)
                {
                    return false;
                }
                var nodeData = new Dictionary<string, double>();
                var nodeDataCount = new Dictionary<string, int>();
                var elementData = new Dictionary<string, double>();
                for (var i = 0; i < _lons.Count; i++)
                {
                    var lon = _lons[i];
                    for (var j = 0; j < _lats.Count; j++)
                    {
                        var lat = _lats[j];
                        var index = (i + (_lons.Count * j)) + (timestep * _lons.Count * _lats.Count);
                        var data = dataArray[index];
                        if (!double.IsNaN(data))
                        {
                            var elementId = NcFile.GetGridId(lon, lat);
                            var elementVertices = NcFile.GetElementVertices(_lons, _lats, lon, lat, i, j);

                            for (var n = 0; n < elementVertices.Count; n++)
                            {
                                var nodeId = NcFile.GetGridId(elementVertices[n].X, elementVertices[n].Y);
                                if (!nodeData.ContainsKey(nodeId))
                                {
                                    nodeData.Add(nodeId, 0);
                                    nodeDataCount.Add(nodeId, 0);
                                }
                                nodeData[nodeId] += data;
                                nodeDataCount[nodeId]++;
                            }
                            elementData.Add(elementId, data);
                        }
                    }
                }
                var nodeKeys = nodeDataCount.Keys;
                foreach (var nodeId in nodeKeys)
                {
                    nodeData[nodeId] = nodeData[nodeId] / nodeDataCount[nodeId];
                }

                if (_cacheQueue.Count == _cacheSize)
                {
                    var oldest = _cacheQueue[0];
                    _cacheQueue.RemoveAt(0);
                    _cacheNodeData.Remove(oldest);
                    _cacheElementData.Remove(oldest);
                }
                _cacheQueue.Add(cacheId);
                _cacheNodeData.Add(cacheId, nodeData);
                _cacheElementData.Add(cacheId, elementData);
                return true;
            }
        }
    }
}
