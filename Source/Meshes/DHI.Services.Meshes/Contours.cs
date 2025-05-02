namespace DHI.Services.Meshes
{
    using System.Collections.Generic;
    using System.Linq;
    using NetTopologySuite;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Utilities;
    using NetTopologySuite.Operation.Polygonize;

    public class Contours
    {
        private readonly double _value;

        public Contours(List<double> x, List<double> y, double value)
        {
            _value = value;
            var contour = new Contour(x, y, value);
            ContourList = new List<Contour>();
            FixContour(contour);
        }

        public List<Contour> ContourList { get; }

        private void FixContour(Contour contour)
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory();
            var nPoints = contour.X.Length;

            var coordinates = new Coordinate[nPoints];
            for (var j = 0; j < nPoints; j++)
            {
                coordinates[j] = new Coordinate(contour.X[j], contour.Y[j]);
            }

            coordinates = RemoveDuplicateCoordinates(coordinates);

            if (coordinates.Length >= 4 && coordinates[0].Equals(coordinates[coordinates.Length - 1]))
            {
                var geometry = geometryFactory.CreatePolygon(coordinates);
                if (!geometry.IsSimple)
                {
                    var geometryV = Validate(geometry);
                    if (geometryV.GetType() == typeof(MultiPolygon))
                    {
                        foreach (var geom in ((GeometryCollection)geometryV).Geometries)
                        {
                            AddContours((Polygon)geom);
                        }
                    }
                    else
                    {
                        AddContours((Polygon)geometryV);
                    }
                }
                else
                {
                    AddContours(geometry);
                }
            }
        }

        private void AddContours(Polygon poly)
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory();
            if (poly.ExteriorRing.Coordinates.Length > 2)
            {
                var coordinates = MakePolygonCCW(poly.ExteriorRing.Coordinates);
                var simpleGeom = geometryFactory.CreatePolygon(coordinates);
                ContourList.Add(new Contour(simpleGeom.ExteriorRing.Coordinates, _value));
            }

            foreach (var ring in poly.InteriorRings)
            {
                if (ring.Coordinates.Length > 2)
                {
                    var coordinates = MakePolygonCCW(ring.Coordinates);
                    var simpleGeom = geometryFactory.CreatePolygon(coordinates);
                    ContourList.Add(new Contour(simpleGeom.ExteriorRing.Coordinates, _value));
                }
            }
        }

        private static Coordinate[] RemoveDuplicateCoordinates(Coordinate[] coordinates)
        {
            var removeInds = new List<int>();

            for (var i = 1; i < coordinates.Length; i++)
            {
                if (coordinates[i].Equals(coordinates[i - 1]))
                {
                    removeInds.Add(i);
                }
            }

            var coordinatesL = coordinates.ToList();
            foreach (var indice in removeInds.OrderByDescending(v => v))
            {
                coordinatesL.RemoveAt(indice);
            }

            return coordinatesL.ToArray();
        }

        private static Coordinate[] MakePolygonCCW(Coordinate[] coordinates)
        {
            var sum = 0.0;
            for (var i = 0; i < coordinates.Length - 1; i++)
            {
                sum += (coordinates[i + 1].X - coordinates[i].X) * (coordinates[i + 1].Y + coordinates[i].Y);
            }

            if (sum <= 0)
            {
                return coordinates;
            }

            var coordinatesR = new Coordinate[coordinates.Length];
            for (var i = 0; i < coordinates.Length; i++)
            {
                coordinatesR[i] = coordinates[coordinates.Length - 1 - i];
            }

            return coordinatesR;
        }

        private static Geometry Validate(Geometry geom)
        {
            if (geom.GetType() == typeof(Polygon))
            {
                if (geom.IsValid)
                {
                    geom.Normalize(); // validate does not pick up rings in the wrong order - this will fix that
                    return geom; // If the polygon is valid just return it
                }

                var polygonizer = new Polygonizer();
                AddPolygon((Polygon)geom, polygonizer);
                return ToPolygonGeometry2(polygonizer.GetPolygons());
            }

            if (geom.GetType() == typeof(MultiPolygon))
            {
                if (geom.IsValid)
                {
                    geom.Normalize(); // validate does not pick up rings in the wrong order - this will fix that
                    return geom; // If the multipolygon is valid just return it
                }

                var polygonizer = new Polygonizer();
                for (var n = geom.NumGeometries; n-- > 0;)
                {
                    AddPolygon((Polygon)geom.GetGeometryN(n), polygonizer);
                }

                return ToPolygonGeometry2(polygonizer.GetPolygons());
            }

            return geom; // In my case, I only care about polygon / multipolygon geometries
        }

        private static void AddPolygon(Polygon polygon, Polygonizer polygonizer)
        {
            AddLineString(polygon.ExteriorRing, polygonizer);
            for (var n = polygon.NumInteriorRings; n-- > 0;)
            {
                AddLineString(polygon.GetInteriorRingN(n), polygonizer);
            }
        }

        private static void AddLineString(LineString lineString, Polygonizer polygonizer)
        {
            if (lineString.GetType() == typeof(LinearRing))
            {
                // LinearRings are treated differently to line strings : we need a LineString NOT a LinearRing
                lineString = lineString.Factory.CreateLineString(lineString.CoordinateSequence);
            }

            // unioning the linestring with the point makes any self intersections explicit.
            var point = lineString.Factory.CreatePoint(lineString.GetCoordinateN(0));
            var toAdd = lineString.Union(point);

            //Add result to polygonizer
            polygonizer.Add(toAdd);
        }

        private static Geometry? ToPolygonGeometry2(ICollection<Geometry> polygons)
        {
            var polygonsA = polygons.ToArray();
            switch (polygonsA.Length)
            {
                case 0:
                    return null; // No valid polygons!
                case 1:
                    return polygonsA[0]; // single polygon - no need to wrap
                default:
                    var combiner = new GeometryCombiner(polygons);
                    var combined = combiner.Combine();
                    return combined;
            }
        }
    }
}
