namespace DHI.Services.Rasters.Radar
{
    using System;

    public class Gauge : BaseEntity<string>
    {
        public Gauge(Pixel location, int distanceOfIncluence = 28, double weightFactor = 1)
        {
            Location = location;
            DistanceOfInfluence = distanceOfIncluence;
            WeightFactor = weightFactor;
        }

        public Pixel Location { get; }

        public int DistanceOfInfluence { get; }

        public double WeightFactor { set; get; }

        public double GetWeight(Pixel pixel)
        {
            var distance = Math.Sqrt(Math.Pow(pixel.Col - Location.Col, 2) + Math.Pow(pixel.Row - Location.Row, 2));
            return distance <= DistanceOfInfluence ? (1 - (1 - WeightFactor)*(1 - Math.Sin(distance/DistanceOfInfluence*Math.PI/2))) : 1;
        }
    }
}