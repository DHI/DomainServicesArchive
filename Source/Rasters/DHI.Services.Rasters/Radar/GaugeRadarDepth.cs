namespace DHI.Services.Rasters.Radar
{
    public readonly struct GaugeRadarDepth
    {
        public GaugeRadarDepth(double gaugeDepth, double radarDepth)
            : this()
        {
            GaugeDepth = gaugeDepth;
            RadarDepth = radarDepth;
        }

        public double GaugeDepth { get; }
        public double RadarDepth { get; }
    }
}