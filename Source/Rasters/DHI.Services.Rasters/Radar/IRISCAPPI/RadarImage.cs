namespace DHI.Services.Rasters.Radar.IRISCAPPI
{
    using System;
    using System.Drawing;
    using System.IO;
    using Radar;

    /// <summary>
    ///     Radar image supporting IRIS CAPPI format.
    /// </summary>
    public class RadarImage : BaseRadarImage
    {
        public new static RadarImage CreateNew(string filePath)
        {
            var radarImage = new RadarImage { Name = Path.GetFileName(filePath) };
            radarImage.FromFile(filePath);
            return radarImage;
        }

        public new static RadarImage CreateNew(Stream stream)
        {
            var radarImage = new RadarImage();
            radarImage.FromStream(stream);
            return radarImage;
        }

        public override void FromStream(Stream stream)
        {
            var binaryReader = new BinaryReader(stream);

            IRISDataStructures.IRISStructureHeader.FromBinaryReaderBlock(binaryReader);
            var productConfiguration = IRISDataStructures.IRISProductConfiguration.FromBinaryReaderBlock(binaryReader);
            var productEnd = IRISDataStructures.IRISProductEnd.FromBinaryReaderBlock(binaryReader);

            // -----------------------------------------------------
            // Get image and data size information
            // Allocate temporary storage structure
            // -----------------------------------------------------
            var gridSizeX = productConfiguration.Xdirection_arr_size;
            var gridSizeY = productConfiguration.Ydirection_arr_size;

            //Implements only to support dbZ format which is Reflectivity 
            if (productConfiguration.data_type_generated == 2)
            {
                PixelValueType = PixelValueType.Reflectivity;
            }
            else if (productConfiguration.data_type_generated == 13)
            {
                PixelValueType = PixelValueType.Intensity;
            }
            else
            {
                throw new NotImplementedException("Cappi support only implemented for DBZ format, the current format is of type " + productConfiguration.data_type_generated);
            }

            var numbPixel = gridSizeX*gridSizeY;
            var i = 0;
            var gridX = 1;
            var gridY = gridSizeY;

            var gridDataB = new float[gridSizeX, gridSizeY];

            while ((i < numbPixel))
            {
                i++;
                switch (productConfiguration.data_type_generated)
                {
                    case 2:
                        // Byte
                        // Note: For Reflectivity Data Format (Data Type = DB_DBZ = 2),
                        // see section 3.3.5 for conversion of values 1..254 to [dBZ] range -31.5 .. +95.0
                        // The byte --> dBZ conversion parameters are written to the P00 file header
                        // In addition it has been decided (by DHI) to convert byte value 0 (zero) to 255
                        // According to IRIS documentation '0' means "No data available".
                        // The value '255' is considered as "Area not scanned".
                        // This will anyway produce zero rainfall.     2012-05-15/DHI,sksh.

                        var readB = IRISDataStructures.IRISReadB.FromBinaryReaderBlock(binaryReader);
                        var b = readB.data;
                        gridDataB[gridX - 1, gridY - 1] = b;
                        break;
                    case 13:
                        // Word
                        // Note: For Rainfall Rate Format (Data Type = DB_RAINRATE2 = 13),
                        // see section 3.3.24 for conversion of values to [mm/hr]

                        var readW = IRISDataStructures.IRISReadW.FromBinaryReaderBlock(binaryReader);
                        var w = readW.data;
                        gridDataB[gridX - 1, gridY - 1] = w;
                        break;
                    default:
                        throw new NotImplementedException($"{productConfiguration.data_type_generated} is not supported");
                }

                gridX++;
                if (gridX > gridSizeX)
                {
                    gridX = 1;
                    gridY -= 1;
                }
            }
            binaryReader.Close();

            //Translating IRIS Cappi Data to DHI Radardata
            double convSlope;
            double convOrd;
            double convOffset;

            string dataQuantity;
            var observation = IRISDataStructures.IRISYMDsTimeToDateTime(productConfiguration.time_of_ingest_sweep);
            var gridDXm = (int)Math.Round(0.01*productConfiguration.Xscale_cmperpix);
            // cm to m
            var latitudeOfRadar = IRISDataStructures.FpDegFromBin4Angle(productEnd.latitude_of_radar);
            var longitudeOfRadar = IRISDataStructures.FpDegFromBin4Angle(productEnd.longitude_of_radar);

            GeoCenter = new PointF((float)latitudeOfRadar, (float)longitudeOfRadar);

            if (productConfiguration.data_type_generated == 2)
            {
                // Store_slope
                convSlope = 0.50;

                // Store_ord
                convOrd = 0.00;

                // Store_offset
                convOffset = 64.00;

                dataQuantity = "dBZ";
            }
            else if (productConfiguration.data_type_generated == 13)
            {
                // Store_slope
                convSlope = 1.0;

                // Store_ord
                convOrd = 0.00;

                // Store_offset
                convOffset = 0.00;

                // Store_quant
                dataQuantity = "mm/hr";
            }
            else
            {
                // Store_slope
                convSlope = 1.0;

                // Store_ord
                convOrd = 0.00;

                // Store_offset
                convOffset = 0.00;

                // Store_quant
                dataQuantity = "";
            }

            Id = observation;
            Type = RadarImageType.Observation;

            PixelValueUnit = dataQuantity;
            TimeOfForecastOffset = 0;
            PixelSize = new Size(gridDXm, gridDXm);
            Size = new Size(gridSizeX, gridSizeY);
            float fVal2;

            switch (productConfiguration.data_type_generated)
            {
                case 2:
                {
                    for (var y = 0; y < Size.Height; y++)
                    {
                        for (var x = 0; x < Size.Width; x++)
                        {
                            fVal2 = gridDataB[x, y];
                            float value;
                            if (fVal2 == 255)
                                value = -9999;
                            else
                                value = (fVal2 - (float)convOffset)*(float)convSlope + (float)convOrd;
                            Values.Add(value);
                        }
                    }
                    break;
                }
                case 13:
                {
                    for (var y = 0; y < Size.Height; y++)
                    {
                        for (var x = 0; x < Size.Width; x++)
                        {
                            var nVal = (ushort)gridDataB[x, y];
                            var nExp = (ushort)(nVal >> 12);
                            var nMantissa = (ushort)(nVal - (nExp << 12));
                            if (nVal == 65535)
                                fVal2 = -9999;
                            else if (nExp == 0)
                                fVal2 = (float)(nMantissa/1000.0);
                            else
                                fVal2 = (float)(((nMantissa + 4096) << (nExp - 1))/1000.0);
                            Values.Add(fVal2);
                        }
                    }
                    break;
                }
            }
        }
    }
}