namespace DHI.Services.GIS.Maps
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json.Serialization;
    using Spatial;
    using SkiaSharp;

    [Serializable]
    public class MapStyle : BaseNamedEntity<string>
    {
        private Dictionary<double, MapStyleBand> _palette;
        private int _numberOfDecimals = 1;

        [JsonConstructor]
        public MapStyle(string id, string name)
            : base(id, name)
        {
        }

        public MapStyle(string id, string name, string styleCode, int numberOfDecimals = 1)
            : this(id, name)
        {
            StyleCode = styleCode;
            _numberOfDecimals = numberOfDecimals;
            GetPaletteFromCode(styleCode, numberOfDecimals);
        }

        public string StyleCode { get; set; }

        public string StyleFile { get; set; }

        public static Dictionary<double, MapStyleBand> GetPaletteFromCode(string code, int numberOfDecimal)
        {
            return new Palette(code, numberOfDecimal);
        }

        public static Dictionary<double, MapStyleBand> GetPaletteFromFile(string file)
        {
            var palette = new Dictionary<double, MapStyleBand>();
            var sortedPalette = new Dictionary<double, MapStyleBand>();
            var lines = File.ReadAllLines(file);
            for (var i = 1; i < lines.Length; i++)
            {
                var details = lines[i].Dissemble(',');
                var lowerValue = details[0].Trim().ToDouble();
                var upperValue = details[1].Trim().ToDouble();
                var color = details[2].Trim().ToColor();
                var text = details[3].Trim();
                var band = new MapStyleBand
                {
                    BandColor = color,
                    BandValue = lowerValue,
                    LowerBandValue = lowerValue,
                    UpperBandValue = upperValue,
                    BandText = text
                };
                palette.Add(lowerValue, band);
            }

            var allValues = palette.Keys.ToList();
            allValues.Sort();
            for (var i = 0; i < allValues.Count; i++)
            {
                var value = allValues[i];
                sortedPalette.Add(value, palette[value]);
            }
            return sortedPalette;
        }

        public Dictionary<double, MapStyleBand> GetPalette()
        {
            if (_palette != null)
            {
                return _palette;
            }

            if (!string.IsNullOrWhiteSpace(StyleCode))
            {
                _palette = GetPaletteFromCode(StyleCode, _numberOfDecimals);
            }
            else if (!string.IsNullOrWhiteSpace(StyleFile))
            {
                _palette = GetPaletteFromFile(StyleFile);
            }

            return _palette;
        }

        [JsonIgnore]
        public IEnumerable<double> ThresholdValues => GetPalette().Keys.ToArray();

        public SKColor GetColor(double value)
        {
            return ((Palette)GetPalette()).GetColor(value);
        }

        public void SetNumberOfDecimal(int numberOfDecimal)
        {
            _numberOfDecimals = numberOfDecimal;
        }

        public SKBitmap ToBitmapHorizontal(int width, int height)
        {
            return ((Palette)GetPalette()).ToBitmapHorizontal(width, height);
        }

        public SKBitmap ToBitmapVertical(int width, int height)
        {
            return ((Palette)GetPalette()).ToBitmapVertical(width, height);
        }
    }
}