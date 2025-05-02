namespace DHI.Services.Meshes
{
    public class ElementCentres
    {
        public ElementCentres(double[] x, double[] y, float[] z, int[][] tn)
        {
            var len = tn.GetLength(0);
            Xe = new double[len];
            Ye = new double[len];
            Ze = new float[len];

            for (var i = 0; i < len; i++)
            {
                Xe[i] = 0;
                Ye[i] = 0;
                Ze[i] = 0;
                var len2 = tn[i].Length;
                for (var j = 0; j < len2; j++)
                {
                    Xe[i] += x[tn[i][j] - 1];
                    Ye[i] += y[tn[i][j] - 1];
                    Ze[i] += z[tn[i][j] - 1];
                }

                Xe[i] = Xe[i] / len2;
                Ye[i] = Ye[i] / len2;
                Ze[i] = Ze[i] / len2;
            }
        }

        public double[] Xe { get; }
        public double[] Ye { get; }
        public float[] Ze { get; }
    }

}
