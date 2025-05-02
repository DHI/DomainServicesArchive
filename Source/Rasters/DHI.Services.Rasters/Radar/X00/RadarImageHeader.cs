using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DHI.Services.Rasters.Test")]

namespace DHI.Services.Rasters.Radar.X00
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///     X00 radar image header.
    /// </summary>
    internal class RadarImageHeader
    {
        private readonly byte[] _bytes;
        private byte[] _dateTime = new byte[14]; // Int, YYYYMMDDHHmmSS
        private byte[] _eastUppb = new byte[3]; // Int, Number of elements on x-axis, aka max index in east direction
        private byte[] _geoCoordE = new byte[7]; // Float, Geographical EEE.mmm coordinate long of center of picture
        private byte[] _geoCoordN = new byte[7]; // Float, Geographical NNN.mmm coordinate lat  of center of picture
        private byte[] _heightUppb = new byte[3];
        private byte[] _imageType = new byte[1]; // Char, Nature of image O=Observed, P=Predicted
        private byte[] _mmPredict = new byte[3]; // Int, Time in minutes to add to "time of observation" to get "time of prediction", zero if img_type=O
        private byte[] _northUppb = new byte[3]; // Int, Number of elements on y-axis, aka max index in north direction
        private byte[] _pixelSize = new byte[4]; // Int, Xxxx => X=Km xxx=meter
        private byte[] _radarName = new byte[20]; // Char, Radar name
        private byte[] _signal = new byte[1]; // Char, X to mark a radar image
        private byte[] _spare = new byte[1];
        private byte[] _storeOffset = new byte[6]; // Float, ?
        private byte[] _storeOrd = new byte[6]; // Float, ?
        private byte[] _storeQuantity = new byte[8]; // String, Physical quantity right padded with blanks
        private byte[] _storeSlope = new byte[4];
        private byte[] _totalBytes = new byte[3]; // Int,
        private byte[] _trailerOffset = new byte[3]; // Int,
        private byte[] _trailerSize = new byte[2]; // Int,

        /// <summary>
        ///     Initializes a new instance of the <see cref="RadarImageHeader" /> class.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        public RadarImageHeader(byte[] bytes)
        {
            _bytes = bytes;
            _ReadBytes();
            _Convert();
        }

        /// <summary>
        ///     Gets the total length of header in bytes.
        /// </summary>
        /// <value>The length of the header in bytes.</value>
        public static int ByteLength => 99;

        /// <summary>
        ///     Gets or sets the date and time.
        /// </summary>
        /// <value>The date and time of the observation.</value>
        public DateTime DateTime { get; set; }

        /// <summary>
        ///     Gets or sets the number of pixels in east direction.
        /// </summary>
        /// <value>The count of elements on x-axis. Number of pixels in East direction.</value>
        public int EastUppb { get; set; }

        /// <summary>
        ///     Gets or sets the longitude.
        /// </summary>
        /// <value>The geographical coordinate (EEE.mmm). Longitude of center of image.</value>
        public double GeoCoordE { get; set; }

        /// <summary>
        ///     Gets or sets the latitude.
        /// </summary>
        /// <value>The geographical coordinate (NNN.mmm). Latitude of center of image.</value>
        public double GeoCoordN { get; set; }

        /// <summary>
        ///     Gets or sets the max number of elements in height direction.
        /// </summary>
        /// <value>The max number of elements in height direction.</value>
        public int HeightUppb { get; set; }

        /// <summary>
        ///     Gets or sets the image type.
        /// </summary>
        /// <value>The image type. 'O' for observed, 'P' for predicted (aka. forecasted).</value>
        public string ImageType { get; set; }

        /// <summary>
        ///     Gets or sets the offset (in minutes) to time of prediction.
        /// </summary>
        /// <value>The time in minutes to add to the time of observation to get the time of a prediction/forecast image.</value>
        public int MmPredict { get; set; }

        /// <summary>
        ///     Gets or sets the number of pixels in north direction.
        /// </summary>
        /// <value>The count of elements on y-axis. Number of pixels in North direction.</value>
        public int NorthUppb { get; set; }

        /// <summary>
        ///     Gets or sets the pixel size.
        /// </summary>
        /// <value>The side length of pixels in meters.</value>
        public int PixelSize { get; set; }

        /// <summary>
        ///     Gets or sets the radar name.
        /// </summary>
        /// <value>The name of the radar.</value>
        public string RadarName { get; set; }

        /// <summary>
        ///     Gets or sets the signal type.
        /// </summary>
        /// <value>
        ///     The signal type. 'X' means standard radar data (a byte image). 'D' means data are floating point values (a
        ///     Double image). The parameter 'StoreQuantity' should always specify the unit of the values.
        /// </value>
        public string Signal { get; set; }

        /// <summary>
        ///     Gets or sets the spare.
        /// </summary>
        /// <value>The usage is not specified in format.</value>
        public byte Spare { get; set; }

        /// <summary>
        ///     Gets or sets the offset.
        /// </summary>
        /// <value>The Offset. Conversion constant used for linear conversion of image byte values to measured values.</value>
        public double StoreOffset { get; set; }

        /// <summary>
        ///     Gets or sets the storeOrd.
        /// </summary>
        /// <value>The Usage is not specified in format.</value>
        public double StoreOrd { get; set; }

        /// <summary>
        ///     Gets or sets the measured quantity.
        /// </summary>
        /// <value>The measured quantity.</value>
        public string StoreQuantity { get; set; }

        /// <summary>
        ///     Gets or sets the slope.
        /// </summary>
        /// <value>The Slope. Conversion constant used for linear conversion of image byte values to measured values.</value>
        public double StoreSlope { get; set; }

        /// <summary>
        ///     Gets or sets the total number of bytes.
        /// </summary>
        /// <value>The total number of bytes.</value>
        public int TotalBytes { get; set; }

        /// <summary>
        ///     Gets or sets the trailer_offset.
        /// </summary>
        /// <value>The position of the trailer bytes.</value>
        public int TrailerOffset { get; set; }

        /// <summary>
        ///     Gets or sets the trailer_size.
        /// </summary>
        /// <value>The size of the trailer.</value>
        public int TrailerSize { get; set; }

        private void _Convert()
        {
            var cultureInfoUS = CultureInfo.CreateSpecificCulture("en-us");

            Signal = Encoding.ASCII.GetString(_signal);
            Array.Reverse(_totalBytes); // Turn little endian into big endian ;)
            TotalBytes = BitConverter.ToUInt16(_totalBytes, 0);
            Array.Reverse(_trailerOffset); // Turn little endian into big endian ;)
            TrailerOffset = BitConverter.ToUInt16(_trailerOffset, 0);
            Array.Reverse(_trailerSize); // Turn little endian into big endian ;)
            TrailerSize = BitConverter.ToUInt16(_trailerSize, 0);
            ImageType = Encoding.ASCII.GetString(_imageType);
            MmPredict = Convert.ToInt16(Encoding.ASCII.GetString(_mmPredict));
            PixelSize = Convert.ToInt16(Encoding.ASCII.GetString(_pixelSize));
            DateTime = _ConvertToDateTime(Encoding.ASCII.GetString(_dateTime));
            RadarName = Encoding.ASCII.GetString(_radarName).Replace("\0", " ").Trim();
            EastUppb = Convert.ToInt16(Encoding.ASCII.GetString(_eastUppb));
            NorthUppb = Convert.ToInt16(Encoding.ASCII.GetString(_northUppb));
            HeightUppb = Convert.ToInt16(Encoding.ASCII.GetString(_heightUppb));
            StoreSlope = Convert.ToDouble(Encoding.ASCII.GetString(_storeSlope), cultureInfoUS);
            StoreOrd = Convert.ToDouble(Encoding.ASCII.GetString(_storeOrd), cultureInfoUS);
            StoreOffset = Convert.ToDouble(Encoding.ASCII.GetString(_storeOffset), cultureInfoUS);
            StoreQuantity = Encoding.ASCII.GetString(_storeQuantity).Trim();
            GeoCoordE = Convert.ToDouble(Encoding.ASCII.GetString(_geoCoordE), cultureInfoUS);
            GeoCoordN = Convert.ToDouble(Encoding.ASCII.GetString(_geoCoordN), cultureInfoUS);
            Spare = _spare[0];
        }

        private DateTime _ConvertToDateTime(string dateTimeString)
        {
            var stringBuilder = new StringBuilder();
            foreach (char t in dateTimeString)
            {
                if (char.IsNumber(t))
                {
                    stringBuilder.Append(t);
                }
                else
                {
                    stringBuilder.Append("0");
                }
            }

            dateTimeString = stringBuilder.ToString();

            var dateTime = new DateTime(
                Convert.ToInt32(dateTimeString.Substring(0, 4), 10),
                Convert.ToInt32(dateTimeString.Substring(4, 2), 10),
                Convert.ToInt32(dateTimeString.Substring(6, 2), 10),
                Convert.ToInt32(dateTimeString.Substring(8, 2), 10),
                Convert.ToInt32(dateTimeString.Substring(10, 2), 10),
                Convert.ToInt32(dateTimeString.Substring(12, 2), 10));
            return dateTime;
        }

        private void _ReadBytes()
        {
            _signal = _bytes.Take(_signal.Length).ToArray();
            _totalBytes = _bytes.Skip(1).Take(_totalBytes.Length).ToArray();
            _trailerOffset = _bytes.Skip(4).Take(_trailerOffset.Length).ToArray();
            _trailerSize = _bytes.Skip(7).Take(_trailerSize.Length).ToArray();

            _imageType = _bytes.Skip(9).Take(_imageType.Length).ToArray();
            _mmPredict = _bytes.Skip(10).Take(_mmPredict.Length).ToArray();
            _pixelSize = _bytes.Skip(13).Take(_pixelSize.Length).ToArray();
            _dateTime = _bytes.Skip(17).Take(_dateTime.Length).ToArray();

            _radarName = _bytes.Skip(31).Take(_radarName.Length).ToArray();
            _eastUppb = _bytes.Skip(51).Take(_eastUppb.Length).ToArray();
            _northUppb = _bytes.Skip(54).Take(_northUppb.Length).ToArray();
            _heightUppb = _bytes.Skip(57).Take(_heightUppb.Length).ToArray();

            _storeSlope = _bytes.Skip(60).Take(_storeSlope.Length).ToArray();
            _storeOrd = _bytes.Skip(64).Take(_storeOrd.Length).ToArray();
            _storeOffset = _bytes.Skip(70).Take(_storeOffset.Length).ToArray();
            _storeQuantity = _bytes.Skip(76).Take(_storeQuantity.Length).ToArray();

            _geoCoordE = _bytes.Skip(84).Take(_geoCoordE.Length).ToArray();
            _geoCoordN = _bytes.Skip(91).Take(_geoCoordN.Length).ToArray();
            _spare = _bytes.Skip(98).Take(_spare.Length).ToArray();
        }
    }
}