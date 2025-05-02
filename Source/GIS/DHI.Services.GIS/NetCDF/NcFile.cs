namespace DHI.Services.GIS.NetCDF
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Maps;
    using Spatial;

    public class NcFile
    {
        public const int CDF1 = 0x43444601;
        public const int NC_DIMENSION = 0x0000000a;
        public const int NC_VARIABLE = 0x0000000b;
        public const int NC_ATTRIBUTE = 0x0000000c;
        public const int NC_BYTE = 0x00000001;
        public const int NC_CHAR = 0x00000002;
        public const int NC_SHORT = 0x00000003;
        public const int NC_INT = 0x00000004;
        public const int NC_FLOAT = 0x00000005;
        public const int NC_DOUBLE = 0x00000006;

        public List<NcDimension> Dimensions;
        public Dictionary<string, NcAttribute> GlobalAttributes;
        public Dictionary<string, NcVariable> Variables;
        public int NumOfRec;

        string _file;

        public NcFile()
        {
            Initialise();
        }

        public List<string> RecVariableNames 
        {
            get
            {
                var list = new List<string>();
                foreach (var pair in Variables)
                {
                    var ncVar = pair.Value;
                    if (ncVar.IsRecord)
                    {
                        list.Add(ncVar.Name);
                    }
                }
                return list;
            }
        }

        public int RecSlabByteCount
        {
            get
            {
                var l = 0;
                var count = 0;
                foreach (var pair in Variables)
                {
                    var ncVar = pair.Value;
                    if (ncVar.IsRecord)
                    {
                        l += ncVar.SlabByteCount;
                        if (count > 0)
                        {
                            l += NcUtility.GetPaddingSize(ncVar.SlabByteCount);
                        }
                        count++;
                    }
                }
                return l;
            }
        }

        public string DefaultVariableName
        {
            get
            {
                foreach (var pair in Variables)
                {
                    var ncVar = pair.Value;
                    if (ncVar.IsGeo2D)
                    {
                        return ncVar.Name;
                    }
                }
                return null;
            }
        }

        public NcDimension DimensionByName(string name)
        {
            var filter = Dimensions.Where(r => r.Name == name);
            if (filter.Any())
            {
                return Dimensions.First(r => r.Name == name);
            }
            else
            {
                throw new Exception("Dimension '" + name + "' does not exist");
            }
        }

        public void Initialise()
        {
            Dimensions = new List<NcDimension>();
            GlobalAttributes = new Dictionary<string, NcAttribute>();
            Variables = new Dictionary<string, NcVariable>();        
            NumOfRec = 0;
        }

        public void Load(string file)
        {
            Initialise();
            _file = file;
            using (BinaryReader br = new BinaryReader(File.Open(_file, FileMode.Open)))
            {
                int length = (int)br.BaseStream.Length;
                var ncMagic = br.ReadIntNc();
                if (ncMagic != CDF1)
                {
                    throw new Exception("Not supported version");
                }
                NumOfRec = br.ReadIntNc();
                var ncDimension = br.ReadIntNc();
                if (ncDimension == 0)
                {
                    throw new Exception("Zero dimension");
                }
                var ncNumDim = br.ReadIntNc();
                for (var i = 0; i < ncNumDim; i++)
                {
                    var dim = new NcDimension
                    {
                        Id = i,
                        Name = br.ReadStringNc(),
                        Length = br.ReadIntNc(4)
                    };
                    Dimensions.Add(dim);
                }
                GlobalAttributes = br.ReadAttrListNc();
                var ncVarList = br.ReadIntNc();
                if (ncVarList == NC_VARIABLE)
                {
                    var ncVarListLength = br.ReadIntNc();
                    for (var i = 0; i < ncVarListLength; i++)
                    {
                        var ncVar = br.ReadVariableNc(Dimensions);
                        Variables.Add(ncVar.Name, ncVar);                                                
                    }
                }
                else
                {
                    br.ReadIntNc(); //absent = zero zero
                }
            }
        }

        public List<T> GetVariableData<T>(string name)
        {
            var varName = name;
            if (string.IsNullOrWhiteSpace(name))
            {
                varName = DefaultVariableName;
            }
            var ncVar = Variables[varName];
            using (BinaryReader br = new BinaryReader(File.Open(_file, FileMode.Open)))
            {
                var data = ncVar.LoadData(br, RecSlabByteCount).Cast<T>().ToList();
                return data;
            }
        }

        public List<DateTime> GetTimestamps()
        {
            var timeVarName = "time";
            var timeVar = Variables[timeVarName];
            var unitDetails = ((string)timeVar.Attributes["units"].Value).Split(' ');
            var timeUnit = unitDetails[0].ToLowerInvariant();
            var refTime = DateTime.Parse(unitDetails[2]);

            var timestamps = new List<DateTime>();
            var timeData = GetVariableData<double>(timeVarName);
            for (var i = 0; i < timeData.Count; i++)
            {
                var timestamp = DateTime.MinValue;
                switch (timeUnit) 
                {
                    case "seconds":
                        timestamp = refTime.AddSeconds(timeData[i]);
                        break;
                    case "days":
                        timestamp = refTime.AddDays(timeData[i]);
                        break;
                    default:
                        throw new Exception("Not supported time unit '" + timeUnit + "'");
                }
                timestamps.Add(timestamp);
            }
            return timestamps;
        }

        public void Create(string file)
        {
            _file = file;
            if (File.Exists(_file))
            {
                File.Delete(_file);
            }
            NumOfRec = 0; //start with 0 records
            using (var bw = new BinaryWriter(File.OpenWrite(file)))
            {
                bw.WriteIntNc(CDF1);
                bw.WriteIntNc(NumOfRec); 
                var numDim = Dimensions.Count;
                if (numDim == 0)
                {
                    throw new Exception("Zero dimension");
                }
                bw.WriteIntNc(NC_DIMENSION);
                bw.WriteIntNc(numDim);
                for (var i = 0; i < numDim; i++)
                {
                    var dim = Dimensions[i];
                    bw.WriteStringNc(dim.Name);
                    bw.WriteIntNc(dim.Length);
                }
                bw.WriteAttrListNc(GlobalAttributes);
                var numVar = Variables.Count;
                if (numVar == 0)
                {
                    bw.WriteAbsentNc();
                }
                else
                {
                    bw.WriteIntNc(NC_VARIABLE);
                    bw.WriteIntNc(numVar);
                    foreach (var pair in Variables)
                    {
                        bw.WriteVariableNc(pair.Value);
                    }
                }
            }
        }

        public void WriteNonRecVariableData<T>(string name, List<T> data)
        {
            var varName = name;
            if (string.IsNullOrWhiteSpace(name))
            {
                varName = DefaultVariableName;
            }
            var ncVar = Variables[varName];
            using (var bw = new BinaryWriter(new FileStream(_file, FileMode.Open)))
            {
                var writeData = data.Cast<object>().ToList();
                bw.Seek(ncVar.Begin, SeekOrigin.Begin);
                ncVar.WriteDataAtSetPosition(bw, writeData);
            }
        }

        public void WriteRecSetData<T>(Dictionary<string, List<T>> setData)
        {
            using (var bw = new BinaryWriter(new FileStream(_file, FileMode.Append)))
            {
                var names = RecVariableNames;
                for (var i = 0; i < names.Count; i++)
                {
                    var varName = names[i];
                    var ncVar = Variables[varName];
                    var data = setData[varName];
                    var writeData = data.Cast<object>().ToList();
                    ncVar.WriteDataAtSetPosition(bw, writeData, names.Count == 1);
                }
            }
            using (var bw = new BinaryWriter(new FileStream(_file, FileMode.Open)))
            {   
                NumOfRec++;
                bw.Seek(4, SeekOrigin.Begin);
                bw.WriteIntNc(NumOfRec);
            }
        }

        public static void GetElementAndNode(List<double> lons, List<double> lats, Dictionary<string, MapGraphicElement> elements, Dictionary<string, MapGraphicNode> nodes)
        {
            for (var i = 0; i < lons.Count; i++)
            {
                var lon = lons[i];
                for (var j = 0; j < lats.Count; j++)
                {
                    var element = new MapGraphicElement();
                    var lat = lats[j];
                    element.Id = GetGridId(lon, lat);
                    var elementVertices = GetElementVertices(lons, lats, lon, lat, i, j);
                    var minVectexGoogle = elementVertices[0].ToGoogle();
                    var maxVectexGoogle = elementVertices[2].ToGoogle();
                    element.GoogleBoundingBox = new BoundingBox(minVectexGoogle.X, minVectexGoogle.Y,
                        maxVectexGoogle.X, maxVectexGoogle.Y);
                    for (var n = 0; n < elementVertices.Count; n++)
                    {
                        var node = new MapGraphicNode {Id = GetGridId(elementVertices[n].X, elementVertices[n].Y)};
                        element.NodeIds.Add(node.Id);
                        if (!nodes.ContainsKey(node.Id))
                        {
                            node.LonLat = elementVertices[n];
                            node.Google = node.LonLat.ToGoogle();
                            nodes.Add(node.Id, node);
                        }
                    }
                    elements.Add(element.Id, element);
                }
            }
        }

        public static string GetGridId(double lon, double lat)
        {
            return lon + "_" + lat;
        }

        public static List<Position> GetElementVertices(List<double> lons, List<double> lats, double lon, double lat, int i, int j)
        {
            var nodeLonMax = i < lons.Count - 1 ?
                        lon + (lons[i + 1] - lon) / 2 : lon + (lon - lons[i - 1]) / 2;
            var nodeLonMin = i > 0 ?
                lon - (lon - lons[i - 1]) / 2 : lon - (lons[i + 1] - lon) / 2;
            var nodeLatMax = j < lats.Count - 1 ?
                lat + (lats[j + 1] - lat) / 2 : lat + (lat - lats[j - 1]) / 2;
            var nodeLatMin = j > 0 ?
                lat - (lat - lats[j - 1]) / 2 : lat - (lats[j + 1] - lat) / 2;
            var elementVertices = new List<Position>
            {
                new Position(nodeLonMin, nodeLatMin),
                new Position(nodeLonMax, nodeLatMin),
                new Position(nodeLonMax, nodeLatMax),
                new Position(nodeLonMin, nodeLatMax)
            };
            return elementVertices;
        }
    }
}
