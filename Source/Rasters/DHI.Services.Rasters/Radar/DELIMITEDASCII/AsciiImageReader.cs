namespace DHI.Services.Rasters.Radar.DELIMITEDASCII
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    /// <summary>
    ///     Ascii image reader
    /// </summary>
    internal class AsciiImageReader
    {
        private double _rowCount = double.NaN;
        private double _columnCount = double.NaN;
        private readonly List<float> _values = new List<float>();
        private readonly Stream _stream;

        /// <summary>
        ///     Initializes a new instance of the core <see cref="AsciiImageReader"/>
        /// </summary>
        /// <param name="stream"></param>
        internal AsciiImageReader(Stream stream)
        {
            _stream = stream;
            _ReadStream();
        }

        internal int RowCount => Convert.ToInt32(_rowCount);
        internal int ColumnCount => Convert.ToInt32(_columnCount);
        internal IList<float> Values => _values;

        private void _ReadStream()
        {
            using var reader = new StreamReader(_stream);
            // first line is either descriptive text or the start of values
            var firstLine = reader.ReadLine();
            var firstWord = firstLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).First().Replace(",", "");
            var delimiter = firstLine.Contains(",") ? ',' : ' '; 

            var rows = new List<List<float>>();

            if (double.TryParse(firstWord, out _))
            {
                rows.Add(_ReadValues(firstLine, delimiter));
            }

            while (!reader.EndOfStream)
            {
                rows.Add(_ReadValues(reader.ReadLine(), delimiter));
            }

            rows = rows.Where(x => x.Count > 0).ToList();
            _columnCount = rows[0].Count();
            _rowCount = rows.Count();
            rows.Reverse();

            foreach (var row in rows)
            {
                _values.AddRange(row);
            }
        }

        private static List<float> _ReadValues(string line, char delimiter)
        {
            return line.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x, CultureInfo.InvariantCulture)).ToList();
        }
    }
}
