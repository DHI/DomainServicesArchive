namespace DHI.Services.GIS.NetCDF
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class NcVariable
    {
        bool _dataLoaded;
        readonly List<object> _data;
        public string Name { get; set; }
        public int Rank { get; set; }
        public int Type { get; set; }
        public int Vsize { get; set; }
        public int Begin { get; set; }
        public List<NcDimension> Dimensions { get; set; }
        public Dictionary<string, NcAttribute> Attributes { get; set; }
        public double ScaleFactor => Attributes.ContainsKey("scale_factor") ? Convert.ToDouble(Attributes["scale_factor"].Value) : 1;

        public double FillValue => Attributes.ContainsKey("_FillValue") ? Convert.ToDouble(Attributes["_FillValue"].Value) : double.NaN;

        public int SlabDataCount
        {
            get
            {
                var start = 0;
                if (IsRecord)
                {
                    start = 1;
                }
                var count = 1;
                for (var i = start; i < Dimensions.Count; i++)
                {
                    count *= Dimensions[i].Length;
                }
                return count;
            }
        }

        public bool IsRecord => Dimensions.Count > 0 && Dimensions[0].IsRecord;

        public bool IsGeo2D => Dimensions.Count == 3 && Dimensions[0].Name == "time" && Dimensions[1].Name == "lat" && Dimensions[2].Name == "lon";

        public int ValByteCount => Type.GetTypeSizeNc();

        public int SlabByteCount => SlabDataCount * ValByteCount;

        public NcVariable()
        {
            Dimensions = new List<NcDimension>();
            Attributes = new Dictionary<string, NcAttribute>();
            _dataLoaded = false;
            _data = new List<object>();
        }

        public List<object> LoadData(BinaryReader br, int recSlabByteCount)
        {
            if (!_dataLoaded)
            {
                br.BaseStream.Seek(Begin, SeekOrigin.Begin);                
                if (IsRecord)
                {
                    var byteSkip = recSlabByteCount - SlabByteCount;
                    if (recSlabByteCount > SlabByteCount)
                    {
                        byteSkip += NcUtility.GetPaddingSize(SlabByteCount);
                    }
                    while (br.BaseStream.Position < br.BaseStream.Length)
                    {
                        for (var i = 0; i < SlabDataCount; i++)
                        {
                            var objVal = br.ReadValNc(Type);
                            if (Type == NcFile.NC_CHAR)
                            {
                                _data.Add(objVal.ToString());
                            }
                            else
                            {
                                var rawVal = Convert.ToDouble(objVal);
                                var val = ScaleFactor * rawVal;
                                if (FillValue == rawVal)
                                {
                                    val = double.NaN;
                                }
                                _data.Add(val);
                            }                                               
                        }                        
                        br.BaseStream.Seek(br.BaseStream.Position + byteSkip, SeekOrigin.Begin);
                    }
                }
                else
                {
                    for (var i = 0; i < SlabDataCount; i++)
                    {
                        var objVal = br.ReadValNc(Type);
                        if (Type == NcFile.NC_CHAR)
                        {
                            _data.Add(objVal.ToString());
                        }
                        else
                        {
                            var rawVal = Convert.ToDouble(objVal);
                            var val = ScaleFactor * rawVal;
                            if (FillValue == rawVal)
                            {
                                val = double.NaN;
                            }
                            _data.Add(val);
                        }
                    }
                }
                _dataLoaded = true;
            }
            return _data;
        }

        public void WriteDataAtSetPosition(BinaryWriter bw, List<object> data, bool noPadding = false)
        {   
            for (var i = 0; i < data.Count; i++)
            {   
                if (Type == NcFile.NC_CHAR)
                {
                    bw.WriteValNc(Type, data[i]);
                }
                else
                {
                    var val = Convert.ToDouble(data[i]);
                    if (double.IsNaN(val))
                    {
                        val = FillValue;
                    }
                    else
                    {
                        val = val / ScaleFactor;
                    }                    
                    bw.WriteValNc(Type, val);
                }     
            }
            if (!noPadding)
            {
                var paddingSize = NcUtility.GetPaddingSize(SlabByteCount);
                var paddingBytes = new byte[paddingSize];
                bw.Write(paddingBytes);
            }
        }
        
        public int CalculateVsize()
        {
            var paddingSize = NcUtility.GetPaddingSize(SlabByteCount);
            return SlabByteCount + paddingSize;
        }
    }
}
