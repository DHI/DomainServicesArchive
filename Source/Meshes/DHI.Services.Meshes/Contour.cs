namespace DHI.Services.Meshes
{
    using System.Collections.Generic;
    using NetTopologySuite.Geometries;

    public class Contour
    {
        public Contour(List<double> x, List<double> y, double value)
        {
            X = new double[x.Count];
            Y = new double[x.Count];
            Value = value;

            for (var i = 0; i < x.Count; i++)
            {
                X[i] = x[i];
                Y[i] = y[i];
            }
        }

        public Contour(Coordinate[] coordinates, double value)
        {
            X = new double[coordinates.Length];
            Y = new double[coordinates.Length];
            Value = value;

            for (var i = 0; i < coordinates.Length; i++)
            {
                X[i] = coordinates[i].X;
                Y[i] = coordinates[i].Y;
            }
        }

        public double[] X { get; set; }

        public double[] Y { get; set; }

        public double Value { get; set; }
    }
}
