namespace DHI.Services.GIS.Maps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SkiaSharp;
    using Spatial;

    public static class MapGraphic
    {
        public static void PaintMap(SKCanvas graphic, Position origin, double dx, double dy, int height,
            Dictionary<double, MapStyleBand> palette, bool contour, bool contourLine,
            SKColor contourLineColor, List<string> foundElementIds,
            Dictionary<string, MapGraphicElement> dataElements, Dictionary<string, MapGraphicNode> dataNodes,
            Dictionary<string, double> nodeValues, Dictionary<string, double> elementValues)
        {
            var bandValues = palette.Keys.ToArray();
            var nodeDrawingPoints = new Dictionary<string, SKPoint>();
            if (contour)
            {
                using var contourLinePen = new SKPaint()
                {
                    Color = contourLineColor
                };
                for (var i = 0; i < foundElementIds.Count; i++)
                {
                    if (!elementValues.ContainsKey(foundElementIds[i]))
                    {
                        continue;
                    }
                    var dataElement = dataElements[foundElementIds[i]];
                    var vertices = new List<SKPoint>();
                    for (var v = 0; v < dataElement.NodeIds.Count; v++)
                    {
                        var nodeId = dataElement.NodeIds[v];
                        if (!nodeDrawingPoints.ContainsKey(nodeId))
                        {
                            var shapeVertex = dataNodes[nodeId].Google;
                            var vertexPoint = shapeVertex.ProjectTo(origin, dx, dy);
                            nodeDrawingPoints.Add(nodeId, vertexPoint.ToDrawingPoint(height));
                        }
                        vertices.Add(nodeDrawingPoints[nodeId]);
                    }

                    var includedBandVertices = new Dictionary<double, List<SKPoint>>();
                    var foundBandValue = double.MinValue;
                    for (var j = 0; j < dataElement.NodeIds.Count; j++)
                    {
                        var nodeIndex = j;
                        var nextNodeIndex = j == dataElement.NodeIds.Count - 1 ? 0 : j + 1;
                        var nodeId = dataElement.NodeIds[nodeIndex];
                        var nextNodeId = dataElement.NodeIds[nextNodeIndex];
                        var vertex = vertices[nodeIndex];
                        var nextVertex = vertices[nextNodeIndex];
                        var nodeVal = nodeValues[nodeId];
                        var nextNodeVal = nodeValues[nextNodeId];
                        for (var z = 0; z < bandValues.Length; z++)
                        {
                            var bandValue = bandValues[z];
                            var color = palette[bandValues[z]].BandColor;
                            if (nodeVal > bandValue && bandValue > foundBandValue)
                            {
                                foundBandValue = bandValue;
                            }

                            if ((bandValue > nodeVal && bandValue < nextNodeVal) || (bandValue > nextNodeVal && bandValue < nodeVal))
                            {
                                var nodeK = Math.Abs(bandValue - nodeVal);
                                var nextNodeK = Math.Abs(bandValue - nextNodeVal);
                                var sumK = nodeK + nextNodeK;
                                var pointX = (nodeK * nextVertex.X + nextNodeK * vertex.X) / sumK;
                                var pointY = (nodeK * nextVertex.Y + nextNodeK * vertex.Y) / sumK;
                                if (!includedBandVertices.ContainsKey(bandValue))
                                {
                                    includedBandVertices.Add(bandValue, new List<SKPoint>());
                                }

                                includedBandVertices[bandValue].Add(new SKPoint
                                {
                                    X = pointX.ToFloat(),
                                    Y = pointY.ToFloat()
                                });
                            }
                        }
                    }

                    var includedBandValues = includedBandVertices.Keys.ToList();
                    includedBandValues.Sort();
                    if (includedBandValues.Count == 0)
                    {
                        using var bandBrush = new SKPaint();
                        if (palette.ContainsKey(foundBandValue))
                        {
                            bandBrush.Color = palette[foundBandValue].BandColor;
                            bandBrush.Style = SKPaintStyle.Fill;
                        }
                        else
                        {
                            bandBrush.Color = SKColors.Transparent;
                            bandBrush.Style = SKPaintStyle.Fill;
                        }
                        using var verticesPath = new SKPath();
                        verticesPath.AddPoly(vertices.ToArray(), true);
                        graphic.DrawPath(verticesPath, bandBrush);
                    }
                    else
                    {
                        var paintVertices = new List<SKPoint>();
                        var bandColor = SKColors.Transparent;
                        var level = includedBandValues[0];
                        if (palette[level].HasLowerBand())
                        {
                            bandColor = palette[palette[level].LowerBandValue].BandColor;
                        }

                        for (var j = 0; j < dataElement.NodeIds.Count; j++)
                        {
                            if (nodeValues[dataElement.NodeIds[j]] <= level)
                            {
                                paintVertices.Add(vertices[j]);
                            }
                        }
                        paintVertices.AddRange(includedBandVertices[level]);
                        PaintBand(graphic, bandColor, paintVertices);
                        for (var z = 0; z < includedBandValues.Count - 1; z++)
                        {
                            level = includedBandValues[z];
                            paintVertices = new List<SKPoint>();
                            bandColor = palette[level].BandColor;
                            paintVertices.AddRange(includedBandVertices[level]);
                            paintVertices.AddRange(includedBandVertices[includedBandValues[z + 1]]);
                            for (var j = 0; j < dataElement.NodeIds.Count; j++)
                            {
                                if (nodeValues[dataElement.NodeIds[j]] > level && nodeValues[dataElement.NodeIds[j]] <= includedBandValues[z + 1])
                                {
                                    paintVertices.Add(vertices[j]);
                                }
                            }
                            PaintBand(graphic, bandColor, paintVertices);
                        }
                        paintVertices = new List<SKPoint>();
                        level = includedBandValues[includedBandValues.Count - 1];
                        bandColor = palette[level].BandColor;
                        for (var j = 0; j < dataElement.NodeIds.Count; j++)
                        {
                            if (nodeValues[dataElement.NodeIds[j]] > level)
                            {
                                paintVertices.Add(vertices[j]);
                            }
                        }
                        paintVertices.AddRange(includedBandVertices[level]);
                        PaintBand(graphic, bandColor, paintVertices);
                    }
                    if (contourLine)
                    {
                        foreach (var pair in includedBandVertices)
                        {
                            if (pair.Value.Count >= 2)
                            {
                                graphic.DrawPoints(SKPointMode.Lines, pair.Value.ToArray(), contourLinePen);
                            }
                        }
                    }
                }
            }
            else
            {
                for (var i = 0; i < foundElementIds.Count; i++)
                {
                    var elementId = foundElementIds[i];
                    if (!elementValues.ContainsKey(foundElementIds[i]))
                    {
                        continue;
                    }
                    var dataElement = dataElements[elementId];
                    var vertices = new List<SKPoint>();
                    for (var v = 0; v < dataElement.NodeIds.Count; v++)
                    {
                        var nodeId = dataElement.NodeIds[v];
                        if (!nodeDrawingPoints.ContainsKey(nodeId))
                        {
                            var shapeVertex = dataNodes[nodeId].Google;
                            var vertexPoint = shapeVertex.ProjectTo(origin, dx, dy);
                            nodeDrawingPoints.Add(nodeId, vertexPoint.ToDrawingPoint(height));
                        }

                        vertices.Add(nodeDrawingPoints[nodeId]);
                    }
                    var color = SKColors.Transparent;
                    for (var z = 0; z < bandValues.Length; z++)
                    {
                        if (elementValues[elementId] >= bandValues[z])
                        {
                            color = palette[bandValues[z]].BandColor;
                        }
                        else
                        {
                            break;
                        }
                    }
                    using var brush = new SKPaint()
                    {
                        Color = color,
                        Style = SKPaintStyle.Fill
                    };
                    using var path = new SKPath();
                    path.AddPoly(vertices.ToArray(), true);
                    graphic.DrawPath(path, brush);
                }
            }
        }

        public static void PaintMapVector(SKCanvas graphic, Position origin, double dx, double dy, int height,
            int vectorEvery, double vectorLowerLimit, double vectorUpperLimit, SKColor vectorColor,
            List<string> selectedElementIds, Dictionary<string, MapGraphicElement> dataElements, Dictionary<string, MapGraphicNode> dataNodes,
            Dictionary<string, double> firstElementValues, Dictionary<string, double> secondElementValues, string dataMode)
        {
            using var pen = new SKPaint()
            {
                Color = vectorColor,
            };
            using var arrowCap = new SKPath();
            arrowCap.AddPoly(new SKPoint[] {
                new SKPoint(4f, -4f),
                new SKPoint(0f, 4f),
                new SKPoint(-4f, -4f),
            });

            for (var i = 0; i < selectedElementIds.Count; i++)
            {
                var elementId = selectedElementIds[i];
                if (!firstElementValues.ContainsKey(selectedElementIds[i]))
                {
                    continue;
                }

                var dataElement = dataElements[elementId];
                var vertices = new List<SKPoint>();
                for (var v = 0; v < dataElement.NodeIds.Count; v++)
                {
                    var nodeId = dataElement.NodeIds[v];
                    var shapeVertex = dataNodes[nodeId].LonLat.ToGoogle();
                    var vertexPoint = shapeVertex.ProjectTo(origin, dx, dy);
                    var vertexPointF = new SKPoint(Convert.ToSingle(vertexPoint.X), Convert.ToSingle(vertexPoint.Y));
                    vertices.Add(vertexPointF);
                }

                var centroid = GetCentroid(vertices);
                var startPosition = new Position(centroid.X, centroid.Y);
                var startPoint = startPosition.ToDrawingPoint(height);

                double vectorStrength;
                double cosA = 0;
                double sinA = 0;
                if (dataMode == "uv")
                {
                    var uStrength = firstElementValues[elementId];
                    var vStrength = secondElementValues[elementId];
                    vectorStrength = Math.Sqrt(Math.Pow(uStrength, 2) + Math.Pow(vStrength, 2));
                    cosA = uStrength / vectorStrength;
                    sinA = vStrength / vectorStrength;
                }
                else if (dataMode == "sd")
                {
                    vectorStrength = firstElementValues[elementId];
                    var direction = secondElementValues[elementId];
                    var angle = 360 - direction + 90;
                    var angleRad = angle * Math.PI / 180;
                    cosA = Math.Cos(angleRad);
                    sinA = Math.Sin(angleRad);
                }
                else
                {
                    continue;
                }
                if (vectorStrength == 0)
                {
                    continue;
                }
                var vectorMinLengthMeter = 0.002;
                var vectorMaxLengthMeter = 0.005;
                var lengthRange = vectorMaxLengthMeter - vectorMinLengthMeter;
                var vectorLengthMeter = vectorMinLengthMeter;
                if (vectorStrength > vectorUpperLimit)
                {
                    vectorLengthMeter = vectorMaxLengthMeter;
                }
                else if (vectorStrength <= vectorUpperLimit && vectorStrength >= vectorLowerLimit)
                {
                    var strengthRange = vectorUpperLimit - vectorLowerLimit;
                    var strengthInRange = vectorStrength - vectorLowerLimit;
                    vectorLengthMeter = vectorMinLengthMeter + (strengthInRange / strengthRange) * lengthRange;
                }
                var endPoint = new SKPoint();
                var xDiffMetre = vectorLengthMeter * cosA;
                var yDiffMetre = vectorLengthMeter * sinA;

                var dpiX = 96.0;
                var dpiY = 96.0;
                var xDiffPx = xDiffMetre * 39.3701 * dpiX;
                var yDiffPx = yDiffMetre * 39.3701 * dpiY;
                var endX = centroid.X + xDiffPx;
                var endY = centroid.Y + yDiffPx;
                var endPosition = new Position(endX, endY);
                endPoint = endPosition.ToDrawingPoint(height);
                graphic.DrawLine(startPoint, endPoint, pen);

                using var arrow = new SKPath(arrowCap);
                var angleRadians = Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X);
                arrow.Transform(SKMatrix.CreateRotation(-(float)Math.PI/2f + (float)angleRadians));
                arrow.Offset(endPoint.X, endPoint.Y);
                graphic.DrawPath(arrow, pen);
            }
        }

        public static SKPoint GetCentroid(List<SKPoint> vertices)
        {
            var vertexCount = vertices.Count;
            float centroidX = 0;
            float centroidY = 0;
            float signedArea = 0;
            float x0 = 0; // Current vertex X
            float y0 = 0; // Current vertex Y
            float x1 = 0; // Next vertex X
            float y1 = 0; // Next vertex Y
            float a = 0;  // Partial signed area
            // For all vertices except last
            int i = 0;
            for (i = 0; i < vertexCount - 1; ++i)
            {
                x0 = vertices[i].X;
                y0 = vertices[i].Y;
                x1 = vertices[i + 1].X;
                y1 = vertices[i + 1].Y;
                a = x0 * y1 - x1 * y0;
                signedArea += a;
                centroidX += (x0 + x1) * a;
                centroidY += (y0 + y1) * a;
            }

            // Do last vertex
            x0 = vertices[i].X;
            y0 = vertices[i].Y;
            x1 = vertices[0].X;
            y1 = vertices[0].Y;
            a = x0 * y1 - x1 * y0;
            signedArea += a;
            centroidX += (x0 + x1) * a;
            centroidY += (y0 + y1) * a;

            signedArea *= 0.5F;
            centroidX /= (6.0F * signedArea);
            centroidY /= (6.0F * signedArea);
            return new SKPoint()
            {
                X = centroidX,
                Y = centroidY
            };
        }

        public static SKBitmap GetCustomBitmap(int width, int height, string message, SKColor backgroundColour)
        {
            var returnImage = new SKBitmap(width, height);
            if (backgroundColour.Alpha != 0)
            {
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        returnImage.SetPixel(x, y, backgroundColour);
                    }
                }
            }

            using var graphics = new SKCanvas(returnImage);
            using var font = new SKFont(SKTypeface.FromFamilyName("Arial"), size: 11);
            using var brush = new SKPaint()
            {
                Color = SKColors.Black,
                FilterQuality = SKFilterQuality.High,
                IsAntialias = true,
            };
            graphics.DrawText(message, 0, 0 + font.Size, font, brush);
            graphics.Dispose();
            return returnImage;
        }

        public static SKBitmap GetErrorBitmap(int width, int height, string error)
        {
            return GetCustomBitmap(width, height, error, new SKColor(255, 114, 86, 50));
        }

        public static SKBitmap GetBlankBitmap(int width, int height)
        {
            return GetCustomBitmap(width, height, "", new SKColor(255, 255, 255, 0));
        }

        public static void PaintBand(SKCanvas graphics, SKColor bandColor, List<SKPoint> bandVertices)
        {
            using var bandBrush = new SKPaint()
            {
                Color = bandColor,
                Style = SKPaintStyle.Fill
            };
            var bandVertexCount = bandVertices.Count;
            

            for (var v1 = 1; v1 < bandVertexCount; v1++)
            {
                for (var v2 = v1; v2 < bandVertexCount; v2++)
                {
                    using var path = new SKPath();
                    path.AddPoly(new[] { bandVertices[0], bandVertices[v1], bandVertices[v2] }, true);
                    graphics.DrawPath(path, bandBrush);
                }
            }
        }

    }
}
