namespace DHI.Services.GIS.NetCDF
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public static class NcUtility
    {
        public static string ToBinary(this byte b)
        {
            return Convert.ToString(b, 2).PadLeft(8, '0');
        }

        public static string ToOctal(this byte b)
        {
            return Convert.ToString(b, 8).PadLeft(3, '0');
        }

        public static string ToHex(this byte b)
        {
            return Convert.ToString(b, 16).PadLeft(2, '0');
        }

        public static string ToDebug(this byte b)
        {
            return Convert.ToChar(b) + " - " + b.ToHex() + " - " + b.ToOctal() + " - " + b.ToBinary();
        }

        public static byte ToByte(this char c)
        {
            return Convert.ToByte(c);
        }

        public static int ToInt(this byte[] ba)
        {
            int bv = ba[ba.Length - 1];
            for (var i = 0; i < ba.Length - 1; i++)
            {
                bv = (ba[i] << (8 * (ba.Length - i - 1))) | bv;
            }
            return bv;
        }

        public static int GetPaddingSize(int length)
        {
            var paddingSize = 4 - (length % 4);
            if (paddingSize == 4)
            {
                paddingSize = 0;
            }
            return paddingSize;
        }

        public static int ReadIntNc(this BinaryReader br, int numByte = 4)
        {
            return br.ReadBytes(numByte).ToInt();
        }

        public static double ReadDecNc(this BinaryReader br, int numByte = 8)
        {
            var bytes = br.ReadBytes(numByte);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes); // Convert big endian to little endian
            }
            if (numByte == 8)
            {
                return BitConverter.ToDouble(bytes, 0);
            }
            else
            {
                return BitConverter.ToSingle(bytes, 0);
            }
        }

        public static string ReadStringByLengthNc(this BinaryReader br, int length)
        {
            if (length > 0)
            {
                var bytes = br.ReadBytes(length);
                var paddingSize = GetPaddingSize(length);
                br.ReadBytes(paddingSize); //get rid of the padding
                return Encoding.UTF8.GetString(bytes);
            }
            else
            {
                return null;
            }
        }

        public static string ReadStringNc(this BinaryReader br)
        {
            var length = br.ReadIntNc();
            return br.ReadStringByLengthNc(length);
        }

        public static NcAttribute ReadAttrNc(this BinaryReader br)
        {
            var attr = new NcAttribute
            {
                Name = br.ReadStringNc(),
                Type = br.ReadIntNc()
            };

            if (attr.Type == NcFile.NC_CHAR)
            {
                attr.Value = br.ReadStringNc();
            }
            else
            {
                var length = br.ReadIntNc();
                if (length == 1)
                {
                    attr.Value = br.ReadValNc(attr.Type);
                }
                else
                {
                    var valueList = new List<object>();
                    for (var i = 0; i < length; i++)
                    {
                        valueList.Add(br.ReadValNc(attr.Type));
                    }
                    attr.Value = valueList;
                }
            }
            return attr;
        }

        public static object ReadValNc(this BinaryReader br, int type)
        {
            object val = null;
            switch (type)
            {
                case NcFile.NC_BYTE:
                    val = br.ReadByte();
                    break;
                case NcFile.NC_CHAR:
                    val = Convert.ToChar(br.ReadByte());
                    break;
                case NcFile.NC_SHORT:
                    val = br.ReadIntNc(2);
                    break;
                case NcFile.NC_INT:
                    val = br.ReadIntNc();
                    break;
                case NcFile.NC_FLOAT:
                    val = br.ReadDecNc(4);
                    break;
                case NcFile.NC_DOUBLE:
                    val = br.ReadDecNc();
                    break;
            }
            return val;
        }

        public static NcVariable ReadVariableNc(this BinaryReader br, List<NcDimension> allDimensions)
        {
            var ncVar = new NcVariable
            {
                Name = br.ReadStringNc(),
                Rank = br.ReadIntNc()
            };

            for (var i = 0; i < ncVar.Rank; i++)
            {
                ncVar.Dimensions.Add(allDimensions[br.ReadIntNc()]);
            }

            ncVar.Attributes = br.ReadAttrListNc();
            ncVar.Type = br.ReadIntNc();
            ncVar.Vsize = br.ReadIntNc();
            ncVar.Begin = br.ReadIntNc();
            return ncVar;
        }

        public static Dictionary<string, NcAttribute> ReadAttrListNc(this BinaryReader br)
        {
            var dict = new Dictionary<string, NcAttribute>();
            var ncAttrList = br.ReadIntNc();
            if (ncAttrList == NcFile.NC_ATTRIBUTE)
            {
                var length = br.ReadIntNc();
                for (var i = 0; i < length; i++)
                {
                    var attr = br.ReadAttrNc();
                    dict.Add(attr.Name, attr);
                }
            }
            else
            {
                br.ReadIntNc(); //absent = zero zero
            }
            return dict;
        }

        public static void WriteAbsentNc(this BinaryWriter bw)
        {
            bw.WriteIntNc(0);
            bw.WriteIntNc(0);
        }

        public static void WriteIntNc(this BinaryWriter bw, int n)
        {
            if (BitConverter.IsLittleEndian)
            {
                for (int i = 3; i >= 0; i--)
                {
                    int shift = i * 8; //bits to shift
                    byte b = (byte)(n >> shift);
                    bw.Write(b);
                }
            }
            else
            {
                bw.Write(n);
            }
        }

        public static void WriteDecNc(this BinaryWriter bw, object d)
        {
            var bytes = d is double ? BitConverter.GetBytes((double)d) : BitConverter.GetBytes((float)d);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes); // Convert big endian to little endian
            }
            bw.Write(bytes);
        }

        public static void WriteStringByLengthNc(this BinaryWriter bw, string s, int length)
        {
            if (length > 0)
            {
                var bytes = Encoding.UTF8.GetBytes(s);
                bw.Write(bytes);
                var paddingSize = GetPaddingSize(length);
                var paddingBytes = new byte[paddingSize];
                bw.Write(paddingBytes);
            }
        }

        public static void WriteStringNc(this BinaryWriter bw, string s)
        {
            var length = s.Length;
            bw.WriteIntNc(length);
            bw.WriteStringByLengthNc(s, length);
        }

        public static void WriteAttrNc(this BinaryWriter bw, NcAttribute attr)
        {
            bw.WriteStringNc(attr.Name);
            bw.WriteIntNc(attr.Type);
            if (attr.Type == NcFile.NC_CHAR)
            {
                bw.WriteStringNc((string)attr.Value);
            }
            else
            {
                bw.WriteIntNc(1); //assume single value
                bw.WriteValNc(attr.Type, attr.Value);
            }        
        }

        public static void WriteValNc(this BinaryWriter bw, int type, object val)
        {
            switch (type)
            {
                case NcFile.NC_BYTE:                    
                    bw.Write(Convert.ToByte(val));
                    break;
                case NcFile.NC_CHAR:
                    bw.Write(Convert.ToByte(val));
                    break;
                case NcFile.NC_SHORT:                    
                    bw.Write(Convert.ToInt16(val));
                    break;
                case NcFile.NC_INT:
                    bw.WriteIntNc(Convert.ToInt32(val));
                    break;
                case NcFile.NC_FLOAT:
                    bw.WriteDecNc(Convert.ToSingle(val));
                    break;
                case NcFile.NC_DOUBLE:
                    bw.WriteDecNc((double)val);
                    break;
            }
        }

        public static void WriteVariableNc(this BinaryWriter bw, NcVariable ncVar)
        {
            bw.WriteStringNc(ncVar.Name);
            bw.WriteIntNc(ncVar.Rank);
            for (var i = 0; i < ncVar.Rank; i++)
            {
                bw.WriteIntNc(ncVar.Dimensions[i].Id);
            }
            bw.WriteAttrListNc(ncVar.Attributes);
            bw.WriteIntNc(ncVar.Type);
            bw.WriteIntNc(ncVar.Vsize);
            bw.WriteIntNc(ncVar.Begin);
        }

        public static void WriteAttrListNc(this BinaryWriter bw, Dictionary<string, NcAttribute> attrDict)
        {
            var numAttr = attrDict.Count;
            if (numAttr == 0)
            {
                bw.WriteAbsentNc();
            }
            else
            {
                bw.WriteIntNc(NcFile.NC_ATTRIBUTE);
                bw.WriteIntNc(numAttr);
                foreach (var pair in attrDict) {
                    bw.WriteAttrNc(pair.Value);
                }
            }
        }

        public static int GetTypeSizeNc(this int type)
        {
            if (type == NcFile.NC_CHAR || type == NcFile.NC_BYTE)
            {
                return 1;
            }
            else if (type == NcFile.NC_SHORT)
            {
                return 2;
            }
            else if (type == NcFile.NC_FLOAT || type == NcFile.NC_INT)
            {
                return 4;
            }
            else if (type == NcFile.NC_DOUBLE)
            {
                return 8;
            }
            else
            {
                throw new Exception("NcFile type '" + type + "' is not supported");
            }
        }

    }
}
