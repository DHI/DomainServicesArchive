namespace DHI.Services.Places
{
    using System;
    using System.Text.Json.Serialization;
    using GIS.Maps;
    using TimeSeries;

    /// <summary>
    ///     Class Indicator.
    /// </summary>
    [Serializable]
    public class Indicator
    {
        [field: NonSerialized]
        private readonly Palette? _palette;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Indicator" /> class.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <param name="styleCode">The style code.</param>
        /// <param name="timeInterval">The time interval for data aggregation. Only relevant if data source type is time series. If not defined, an explicit time period must be defined when calculating the aggregated time series values.</param>
        /// <param name="aggregationType">The aggregation type. Only relevant if data source type is time series.</param>
        /// <param name="quantile">The quantile to extract for status calculation in case of data source type "EnsembleTimeSeries"</param>
        /// <param name="paletteType">The type of the palette, i. e. whether thresholds define lower or upper threshold values</param>
        [JsonConstructor]
        public Indicator(DataSource dataSource, string styleCode, TimeInterval? timeInterval = null, AggregationType? aggregationType = null, double? quantile = null, PaletteType paletteType = PaletteType.LowerThresholdValues)
        {
            if (dataSource.Type == DataSourceType.TimeSeries || dataSource.Type == DataSourceType.EnsembleTimeSeries)
            {
                Guard.Against.Null(aggregationType, nameof(aggregationType));
            }

            Guard.Against.NullOrEmpty(styleCode, nameof(styleCode));
            DataSource = dataSource;
            StyleCode = styleCode;
            PaletteType = paletteType;
            _palette = new Palette(styleCode, 1, paletteType);
            TimeInterval = timeInterval;
            AggregationType = aggregationType;
            Quantile = quantile;
        }

        /// <summary>
        ///     Gets the data source.
        /// </summary>
        /// <value>The data source.</value>
        public DataSource DataSource { get; }

        /// <summary>
        ///     Gets or sets the time interval.
        /// </summary>
        /// <remarks>Only relevant if data source type is time series.</remarks>
        public TimeInterval? TimeInterval { get; }

        /// <summary>
        ///     Gets or sets the type of the aggregation.
        /// </summary>
        /// <remarks>Only relevant if data source type is time series.</remarks>
        public AggregationType? AggregationType { get; }

        /// <summary>
        ///     Gets or sets the quantile.
        /// </summary>
        /// <remarks>Only relevant if data source type is ensemble time series.</remarks>
        public double? Quantile { get; }

        /// <summary>
        ///     Gets the style code.
        /// </summary>
        public string StyleCode { get; }

        /// <summary>
        ///     Gets the palette type.
        /// </summary>
        /// <value>The palette.</value>
        public PaletteType PaletteType { get; }

        /// <summary>
        ///     Gets the palette.
        /// </summary>
        /// <value>The palette.</value>
        public Palette GetPalette()
        {
            return _palette ?? new Palette(StyleCode, 1, PaletteType);
        }
    }
}