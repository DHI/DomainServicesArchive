namespace DHI.Services.Rasters.Radar.ESRIASCII
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    /// <summary>
    ///     ESRI ASCII image reader
    /// </summary>
    internal class AsciiImageReader
    {
        private double _rowCount = double.NaN;
        private double _columnCount = double.NaN;
        private float _cellSize = float.NaN;
        private readonly List<float> _values = new List<float>();
        private readonly Stream _stream;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AsciiImageReader"/> class.
        /// </summary>
        /// <param name="stream">The file stream.</param>
        internal AsciiImageReader(Stream stream)
        {
            _stream = stream;
            _ReadStream();
        }

        internal int RowCount => Convert.ToInt32(_rowCount);
        internal int ColumnCount => Convert.ToInt32(_columnCount);
        internal float CellSize => _cellSize;
        internal IList<float> Values => _values;

        private void _ReadStream()
        {
            using var reader = new StreamReader(_stream);
            _columnCount = _ReadHeaderDouble(reader.ReadLine());
            _rowCount = _ReadHeaderDouble(reader.ReadLine());
            _ReadHeaderDouble(reader.ReadLine());
            _ReadHeaderDouble(reader.ReadLine());
            _cellSize = _ReadHeaderFloat(reader.ReadLine());
            var noData = _ReadHeaderFloat(reader.ReadLine());

            while (!reader.EndOfStream)
            {
                _values.AddRange(_ReadValues(reader.ReadLine(), noData));
            }
        }

        private static IEnumerable<float> _ReadValues(string line, float noData)
        {
            var values = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x, CultureInfo.InvariantCulture));
            return values.Select(x => x == noData ? Funcs.NoData : x);
        }

        private static double _ReadHeaderDouble(string line)
        {
            return double.Parse(line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)[1], CultureInfo.InvariantCulture);
        }

        private static float _ReadHeaderFloat(string line)
        {
            return float.Parse(line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)[1], CultureInfo.InvariantCulture);
        }
    }
}