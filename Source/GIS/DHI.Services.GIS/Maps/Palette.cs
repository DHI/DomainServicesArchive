namespace DHI.Services.GIS.Maps
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using SkiaSharp;
    using Spatial;

    public class Palette : Dictionary<double, MapStyleBand>
    {
        public Palette(string code, int numberOfDecimals = 1, PaletteType type = PaletteType.LowerThresholdValues)
        {
            Guard.Against.NullOrEmpty(code, nameof(code));
            Code = code;
            Type = type;
            try
            {
                var palette = new Dictionary<double, MapStyleBand>();
                var firstBreaks = code.Dissemble('|');
                var numberFormat = "{0:0.";
                for (var i = 0; i < numberOfDecimals; i++)
                {
                    numberFormat += "0";
                }

                numberFormat += "}";
                for (var i = 0; i < firstBreaks.Length; i++)
                {
                    var secondBreaks = firstBreaks[i].Dissemble(':');
                    var hexColors = secondBreaks[1].Dissemble(',');
                    var values = new List<double>();
                    var valueCode = secondBreaks[0];
                    if (valueCode.Contains("^"))
                    {
                        var rangeValueCodeBreaks = valueCode.Dissemble('^');
                        var tempValue = rangeValueCodeBreaks[0].ToDouble();
                        for (var c = 0; c < hexColors.Length; c++)
                        {
                            values.Add(tempValue);
                            tempValue += rangeValueCodeBreaks[1].ToDouble();
                        }
                    }
                    else if (valueCode.Contains("~"))
                    {
                        var rangeValueCodeBreaks = valueCode.Dissemble('~');
                        var startValue = rangeValueCodeBreaks[0].ToDouble();
                        var endValue = rangeValueCodeBreaks[1].ToDouble();
                        var increment = (endValue - startValue) / (hexColors.Length - 1);
                        var tempValue = startValue;
                        for (var c = 0; c < hexColors.Length; c++)
                        {
                            values.Add(tempValue);
                            tempValue += increment;
                        }
                    }
                    else
                    {
                        values.Add(valueCode.ToDouble());
                    }

                    for (var j = 0; j < values.Count; j++)
                    {
                        var value = values[j];
                        var color = hexColors[j].ToColor();
                        var text = string.Format(CultureInfo.InvariantCulture, numberFormat, Math.Round(value, numberOfDecimals));
                        var band = new MapStyleBand
                        {
                            BandColor = color,
                            BandValue = value,
                            BandText = text
                        };
                        palette.Add(value, band);
                    }
                }

                // Set lower- and upper band values
                var allValues = palette.Keys.ToList();
                allValues.Sort();
                for (var i = 0; i < allValues.Count; i++)
                {
                    var value = allValues[i];
                    Add(value, palette[value]);
                    if (i > 0)
                    {
                        var previousValue = allValues[i - 1];
                        this[previousValue].UpperBandValue = value;
                        this[value].LowerBandValue = this[previousValue].BandValue;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"The code '{code}' does not define a valid palette. For examples of valid formats see https://developerdoc-mike-platform-prod.eu.mike-cloud.com/domain_services/web-api-documentation/#map-style-representation", e);
            }
        }

        public PaletteType Type { get; }

        public string Code { get; }

        public IEnumerable<double> ThresholdValues => Keys.ToArray();

        public SKColor GetColor(double value)
        {
            switch (Type)
            {
                case PaletteType.LowerThresholdValues:
                    if (value < ThresholdValues.First())
                    {
                        return SKColors.Transparent;
                    }

                    if (value >= ThresholdValues.Last())
                    {
                        return Values.Last().BandColor;
                    }

                    return this.Single(kvp => kvp.Value.BandValue <= value && kvp.Value.UpperBandValue > value).Value.BandColor;
                case PaletteType.UpperThresholdValues:
                    if (value > ThresholdValues.Last())
                    {
                        return SKColors.Transparent;
                    }

                    if (value <= ThresholdValues.First())
                    {
                        return Values.First().BandColor;
                    }

                    return this.Single(kvp => kvp.Value.LowerBandValue < value && kvp.Value.BandValue >= value).Value.BandColor;
                default:
                    throw new NotSupportedException($"Palette type '{Type}' is not supported.");
            }
        }

        public SKBitmap ToBitmapHorizontal(int width, int height)
        {
            var bitmap = new SKBitmap(width, height);
            using var graphic = new SKCanvas(bitmap);
            var bandCount = Count;
            var bandValues = Keys.ToList();
            var bandWidth = Convert.ToInt32(Math.Floor(Convert.ToDouble(width / bandCount)));
            var widthRemainder = width % bandCount;
            using var font = new SKFont(SKTypeface.FromFamilyName("Arial"), size: 11);
            using var whiteBrush = new SKPaint()
            {
                Color = SKColors.White
            };
            graphic.DrawRect(0, 0, width, height, whiteBrush);
            var totalX = 0;
            var previousBandWidth = 0;
            for (var i = 0; i < bandCount; i++)
            {
                var currentBandWidth = widthRemainder > 0 ? bandWidth + 1 : bandWidth;
                widthRemainder--;
                var bandTopLeftX = totalX + previousBandWidth;
                totalX = bandTopLeftX;
                var bandValue = bandValues[i];
                var band = this[bandValue];
                using var bandBrush = new SKPaint()
                {
                    Color = band.BandColor
                };
                using var textBrush = new SKPaint()
                {
                    Color = GetTextColorForBackground(band.BandColor),
                    TextSize = font.Size,
                };
                if (band.BandColor == SKColors.Transparent)
                {
                    using var hatchBrush = new SKPaint()
                    {
                        PathEffect = SKPathEffect.Create2DLine(1, SKMatrix.Concat(SKMatrix.CreateScale(6, 6), SKMatrix.CreateRotationDegrees(-45))),
                        Color = SKColor.Parse("#eeeeee"),
                    };
                    bandBrush.Color = SKColor.Parse("#eeeeee");
                    textBrush.Color = SKColors.Black;
                    textBrush.TextSize = font.Size;

                    using var hatchPath = new SKPath();
                    hatchPath.AddRect(SKRectI.Create(bandTopLeftX, 0, currentBandWidth, height));

                    graphic.DrawRect(bandTopLeftX, 0, currentBandWidth, height, hatchBrush);
                }
                else
                {
                    graphic.DrawRect(bandTopLeftX, 0, currentBandWidth, height, bandBrush);
                }
                
                SKPoint textPoint;
                switch (Type)
                {
                    case PaletteType.LowerThresholdValues:
                        textPoint = new SKPoint(bandTopLeftX + 4, 2 + font.Size);
                        break;
                    case PaletteType.UpperThresholdValues:
                        var textWidth = textBrush.MeasureText(band.BandText);
                        textPoint = new SKPoint(bandTopLeftX + currentBandWidth - textWidth - 4, 2 + font.Size);
                        break;
                    default:
                        throw new NotSupportedException($"Palette type '{Type}' is not supported.");
                }
                graphic.DrawText(band.BandText, textPoint.X, textPoint.Y, font, textBrush);
                previousBandWidth = currentBandWidth;
            }
            return bitmap;
        }

        public SKBitmap ToBitmapVertical(int width, int height)
        {
            var bitmap = new SKBitmap(width, height);
            using var graphic = new SKCanvas(bitmap);
            var bandCount = Count;
            var bandValues = Keys.ToList();
            var bandHeight = Convert.ToInt32(Math.Floor(Convert.ToDouble(height / bandCount)));
            var heightRemainder = height % bandCount;
            using var font = new SKFont(SKTypeface.FromFamilyName("Arial"), size: 11);
            using var whiteBrush = new SKPaint()
            {
                Color = SKColors.White
            };
            graphic.DrawRect(0, 0, width, height, whiteBrush);
            var totalY = height;
            for (var i = bandCount - 1; i >= 0; i--)
            {
                var bandValue = bandValues[i];
                var band = this[bandValue];
                using var bandBrush = new SKPaint()
                {
                    Color = band.BandColor
                };
                using var textBrush = new SKPaint()
                {
                    Color = GetTextColorForBackground(band.BandColor),
                    TextSize = 8,
                };
                var currentBandHeight = heightRemainder > 0 ? bandHeight + 1 : bandHeight;
                heightRemainder--;
                var bandTopLeftY = totalY - currentBandHeight;
                totalY = bandTopLeftY;
                if (band.BandColor == SKColors.Transparent)
                {
                    using var hatchBrush = new SKPaint()
                    {
                        PathEffect = SKPathEffect.Create2DLine(1, SKMatrix.Concat(SKMatrix.CreateScale(6, 6), SKMatrix.CreateRotationDegrees(-45))),
                        Color = SKColor.Parse("#eeeeee"),
                    };
                    using var hatchPath = new SKPath();
                    hatchPath.AddRect(SKRectI.Create(0, bandTopLeftY, width, currentBandHeight));

                    bandBrush.Color = SKColor.Parse("#eeeeee");
                    textBrush.Color = SKColors.Black;

                    graphic.DrawRect(0, bandTopLeftY, width, currentBandHeight, hatchBrush);
                }
                else
                {
                    graphic.DrawRect(0, bandTopLeftY, width, currentBandHeight, bandBrush);
                }
                

                SKPoint textPoint;
                switch (Type)
                {
                    case PaletteType.LowerThresholdValues:
                        textPoint = new SKPoint(2, bandTopLeftY + 2 + font.Size);
                        break;
                    case PaletteType.UpperThresholdValues:
                        var textHeight = textBrush.TextSize;
                        textPoint = new SKPoint(2, bandTopLeftY + currentBandHeight - textHeight - 2);
                        break;
                    default:
                        throw new NotSupportedException($"Palette type '{Type}' is not supported.");
                }

                graphic.DrawText(band.BandText, textPoint.X, textPoint.Y, font, textBrush);
            }

            return bitmap;
        }

        private static double GetGreyValue(SKColor color)
        {
            return color.Red * 0.299 + color.Green * 0.587 + color.Blue * 0.114;
        }

        private static SKColor GetTextColorForBackground(SKColor color)
        {
            var backgroundGrey = GetGreyValue(color);
            if (backgroundGrey > 128)
            {
                return SKColors.Black;
            }

            return SKColors.White;
        }
    }

    public enum PaletteType
    {
        LowerThresholdValues,
        UpperThresholdValues
    }
}