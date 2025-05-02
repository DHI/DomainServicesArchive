namespace DHI.Services.Places.WebApi
{
    using GIS.Maps;
    using System.ComponentModel.DataAnnotations;
    using TimeSeries;

    public class IndicatorDTO
    {
        public IndicatorDTO()
        {
        }

        public IndicatorDTO(Indicator indicator)
        {
            DataSource = indicator.DataSource;
            StyleCode = indicator.StyleCode;
            TimeInterval = new TimeIntervalDTO(indicator.TimeInterval);
            PaletteType = indicator.PaletteType;
            Quantile = indicator.Quantile;
            if (indicator.AggregationType != null)
            {
                AggregationType = indicator.AggregationType.DisplayName;
            }
        }

        [Required]
        public DataSource DataSource { get; set; }

        [Required]
        public string StyleCode { get; set; }

        public TimeIntervalDTO TimeInterval { get; set; }

        public string AggregationType { get; set; }

        public PaletteType PaletteType { get; set; }

        public double? Quantile { get; set; }

        public Indicator ToIndicator()
        {
            AggregationType aggregationType = null;
            if (AggregationType != null)
            {
                aggregationType = Enumeration.FromDisplayName<AggregationType>(AggregationType);
            }
            
            var indicator = new Indicator(DataSource, StyleCode, TimeInterval.ToTimeInterval(), aggregationType, Quantile, PaletteType);
            return indicator;
        }
    }
}