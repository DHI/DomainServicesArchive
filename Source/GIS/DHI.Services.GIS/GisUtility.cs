namespace DHI.Services.GIS
{
    using System.Collections.Generic;
    using System.Linq;
    using Spatial;

    public static class GisUtility
    {
        public static bool GisContains(List<Position> ring, Position position)
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

        public static bool GisIntersects(params List<Position>[] rings)
        {
            for (var i = 0; i < rings.Length; i++)
            {
                var mainRing = rings[i];
                for (var j = 0; j < rings.Length; j++)
                {
                    for (var p = 0; p < mainRing.Count; p++)
                    {
                        var vertex = mainRing[p];
                        if (GisContains(rings[j], vertex))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static string PositionsToString(List<Position> positions)
        {
            var s = "[";
            var connector = "";
            for (var i = 0; i < positions.Count; i++)
            {
                s += connector + positions[i];
                connector = "]";
            }

            return s;
        }
    }
}