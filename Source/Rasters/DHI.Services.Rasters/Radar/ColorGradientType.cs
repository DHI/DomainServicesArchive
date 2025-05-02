namespace DHI.Services.Rasters.Radar
{
    using System.Collections.Generic;
    using System.Drawing;

    public abstract class ColorGradientType : Enumeration
    {
        public static readonly ColorGradientType IntensityDefault = new RadarColorGradientIntensityDefault();
        public static readonly ColorGradientType IntensityLightYellowToRed = new RadarColorGradientIntensityLightYellowToRed();
        public static readonly ColorGradientType IntensityLightYellowToRedLogarithmic = new RadarColorGradientIntensityLightYellowToRedLogarithmic();
        public static readonly ColorGradientType ReflectivityDefault = new RadarColorGradientReflectivityDefault();
        public static readonly ColorGradientType ReflectivityLightYellowToRed = new RadarColorGradientReflectivityLightYellowToRed();

        protected ColorGradientType(int value, string displayName)
            : base(value, displayName)
        {
        }

        public abstract ColorGradient ColorGradient { get; }

        private class RadarColorGradientIntensityDefault : ColorGradientType
        {
            public RadarColorGradientIntensityDefault()
                : base(0, "IntensityDefault")
            {
            }

            public override ColorGradient ColorGradient
            {
                get
                {
                    var pointColors = new SortedDictionary<double, Color> { { 1, Color.Blue }, { 70, Color.Cyan }, { 140, Color.Yellow }, { 210, Color.Red } };
                    return new ColorGradient(pointColors);
                }
            }
        }

        private class RadarColorGradientIntensityLightYellowToRed : ColorGradientType
        {
            public RadarColorGradientIntensityLightYellowToRed()
                : base(1, "IntensityLightYellowToRed")
            {
            }

            public override ColorGradient ColorGradient
            {
                get
                {
                    var pointColors = new SortedDictionary<double, Color> { { 1, Color.FromArgb(255, 240, 200) }, { 100, Color.FromArgb(246, 137, 0) }, { 200, Color.FromArgb(200, 0, 0) } };
                    return new ColorGradient(pointColors);
                }
            }
        }

        private class RadarColorGradientIntensityLightYellowToRedLogarithmic : ColorGradientType
        {
            public RadarColorGradientIntensityLightYellowToRedLogarithmic()
                : base(2, "IntensityLightYellowToRedLogarithmic")
            {
            }

            public override ColorGradient ColorGradient
            {
                get
                {
                    var pointColors = new SortedDictionary<double, Color> { { 1, Color.FromArgb(255, 240, 200) }, { 20, Color.FromArgb(246, 137, 0) }, { 200, Color.FromArgb(200, 0, 0) } };
                    return new ColorGradient(pointColors, true);
                }
            }
        }

        private class RadarColorGradientReflectivityDefault : ColorGradientType
        {
            public RadarColorGradientReflectivityDefault()
                : base(3, "ReflectivityDefault")
            {
            }

            public override ColorGradient ColorGradient
            {
                get
                {
                    var pointColors = new SortedDictionary<double, Color> { { 10, Color.Blue }, { 30, Color.Cyan }, { 50, Color.Yellow }, { 65, Color.Red } };
                    return new ColorGradient(pointColors);
                }
            }
        }

        private class RadarColorGradientReflectivityLightYellowToRed : ColorGradientType
        {
            public RadarColorGradientReflectivityLightYellowToRed()
                : base(4, "ReflectivityLightYellowToRed")
            {
            }

            public override ColorGradient ColorGradient
            {
                get
                {
                    var pointColors = new SortedDictionary<double, Color> { { 10, Color.FromArgb(255, 240, 200) }, { 40, Color.FromArgb(246, 137, 0) }, { 65, Color.FromArgb(200, 0, 0) } };
                    return new ColorGradient(pointColors);
                }
            }
        }
    }
}