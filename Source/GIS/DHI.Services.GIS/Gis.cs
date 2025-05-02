namespace DHI.Services.GIS
{
    using System.Collections.Generic;
    using System.Linq;
    using Spatial;

    public static class Gis
    {
        public static bool Contains(List<Position> ring, Position position)
        {
            var vertexCount = ring.Count;
            int i, j = vertexCount - 1;
            var odd = false;
            var x = position.X;
            var y = position.Y;

            var polyX = ring.Select(r => r.X).ToArray();
            var polyY = ring.Select(r => r.Y).ToArray();
            for (i = 0; i < vertexCount; i++)
            {
                if (polyY[i] < y && polyY[j] >= y
                    || polyY[j] < y && polyY[i] >= y)
                {
                    if (polyX[i] + (y - polyY[i]) / (polyY[j] - polyY[i]) * (polyX[j] - polyX[i]) < x)
                    {
                        odd = !odd;
                    }
                }

                j = i;
            }

            return odd;
        }
    }
}