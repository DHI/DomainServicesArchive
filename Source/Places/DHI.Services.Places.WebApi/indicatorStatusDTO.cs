namespace DHI.Services.Places.WebApi
{
    using SkiaSharp;

    public class IndicatorStatusDTO : IndicatorDTO
    {
        public IndicatorStatusDTO(Indicator indicator, SKColor status) : base(indicator)
        {
            Status = status;
        }

        public SKColor Status { get; }
    }
}