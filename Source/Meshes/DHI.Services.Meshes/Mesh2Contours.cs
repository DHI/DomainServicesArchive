namespace DHI.Services.Meshes
{
    using System.Collections.Generic;
    using System.Linq;

    public class Mesh2Contours
    {
        public Mesh2Contours(Mesh mesh, float[] elementData, double[] nodeData, IEnumerable<double> contourLevels, MeshConnection meshConnection)
        {
            Mesh = mesh;
            MeshConnection = meshConnection;
            NodeData = nodeData;
            ContourLevels = new List<double>(contourLevels);

            var len = elementData.Length;
            ElementData = new double[len];
            for (var i = 0; i < len; i++)
            {
                ElementData[i] = elementData[i];
            }

            Contours = TriContour();
        }

        public Mesh2Contours(Mesh mesh, double[] elementData, double[] nodeData, IEnumerable<double> contourLevels, MeshConnection meshConnection)
        {
            Mesh = mesh;
            MeshConnection = meshConnection;
            ElementData = elementData;
            NodeData = nodeData;
            ContourLevels = new List<double>(contourLevels);

            Contours = TriContour();
        }

        public Mesh2Contours(Mesh mesh, double[] nodeData, IEnumerable<double> contourLevels, MeshConnection meshConnection)
        {
            Mesh = mesh;
            MeshConnection = meshConnection;
            NodeData = nodeData;
            ContourLevels = new List<double>(contourLevels);

            ElementData = CalculateCentroidValues();

            Contours = TriContour();
        }

        public Mesh2Contours(Mesh mesh, double[] nodeData, IEnumerable<double> contourLevels)
        {
            Mesh = mesh;
            MeshConnection = new MeshConnection(Mesh);
            NodeData = nodeData;
            ContourLevels = new List<double>(contourLevels);

            ElementData = CalculateCentroidValues();

            Contours = TriContour();
        }

        private Mesh Mesh { get; }
        private MeshConnection MeshConnection { get; }
        private double[] ElementData { get; }
        private double[] NodeData { get; }
        private List<double> ContourLevels { get; }
        public List<Contour> Contours { get; }

        private List<Contour> TriContour()
        {
            var nElem = Mesh.ElementTable.Length;

            ContourLevels.Sort();
            ContourLevels.Reverse();

            var elemIn = new bool[nElem];
            var elemOld = new bool[nElem];

            var contours = new List<Contour>();

            foreach (var contourLevel in ContourLevels)
            {
                contours.AddRange(GenerateContour(contourLevel, ref elemIn, ref elemOld, nElem));
            }

            return contours;
        }

        private List<Contour> GenerateContour(double contourLevel, ref bool[] elemIn, ref bool[] elemOld, int nElem)
        {
            var elemsI = new List<int>();
            for (var i = 0; i < nElem; i++)
            {
                if (ElementData[i] >= contourLevel && !elemOld[i])
                {
                    elemIn[i] = true;
                    elemsI.Add(i);
                }
            }

            var nElemsI = elemsI.Count;
            var bnd = new List<int>();
            var next = 0;
            for (var i = 0; i < nElemsI; i++)
            {
                var ct = elemsI[i];
                var count = 0;
                for (var j = 0; j < 3; j++)
                {
                    var ce = MeshConnection.UniqueEdgesInElement[ct, j];
                    if (!elemIn[MeshConnection.EdgeToElement[ce, 0]] || MeshConnection.EdgeToElement[ce, 1] >= 0 && !elemIn[MeshConnection.EdgeToElement[ce, 1]])
                    {
                        bnd.Add(ce);
                        next++;
                    }
                    else
                    {
                        count++;
                    }
                }

                if (count == 3)
                {
                    elemOld[ct] = true;
                }
            }

            var numb = next - 1;
            if (numb < 0)
            {
                return new List<Contour>();
            }

            var n = bnd.Count;
            var p1 = new double[2];
            var p2 = new double[2];
            var penew = new double[n, 2];
            for (var i = 0; i < n; i++)
            {
                var t1 = MeshConnection.EdgeToElement[bnd[i], 0];
                var t2 = MeshConnection.EdgeToElement[bnd[i], 1];
                var h1 = ElementData[t1];
                p1[0] = (Mesh.X[Mesh.ElementTable[t1][0] - 1] + Mesh.X[Mesh.ElementTable[t1][1] - 1] + Mesh.X[Mesh.ElementTable[t1][2] - 1]) / 3.0;
                p1[1] = (Mesh.Y[Mesh.ElementTable[t1][0] - 1] + Mesh.Y[Mesh.ElementTable[t1][1] - 1] + Mesh.Y[Mesh.ElementTable[t1][2] - 1]) / 3.0;

                double h2;
                if (t2 >= 0)
                {
                    h2 = ElementData[t2];
                    p2[0] = (Mesh.X[Mesh.ElementTable[t2][0] - 1] + Mesh.X[Mesh.ElementTable[t2][1] - 1] + Mesh.X[Mesh.ElementTable[t2][2] - 1]) / 3.0;
                    p2[1] = (Mesh.Y[Mesh.ElementTable[t2][0] - 1] + Mesh.Y[Mesh.ElementTable[t2][1] - 1] + Mesh.Y[Mesh.ElementTable[t2][2] - 1]) / 3.0;
                }
                else
                {
                    h2 = (NodeData[MeshConnection.Edges[bnd[i]].NodeFrom] + NodeData[MeshConnection.Edges[bnd[i]].NodeTo]) / 2.0;
                    p2[0] = (Mesh.X[MeshConnection.Edges[bnd[i]].NodeFrom] + Mesh.X[MeshConnection.Edges[bnd[i]].NodeTo]) / 2.0;
                    p2[1] = (Mesh.Y[MeshConnection.Edges[bnd[i]].NodeFrom] + Mesh.Y[MeshConnection.Edges[bnd[i]].NodeTo]) / 2.0;
                }

                var r = (contourLevel - h1) / (h2 - h1);
                penew[i, 0] = p1[0] + r * (p2[0] - p1[0]);
                penew[i, 1] = p1[1] + r * (p2[1] - p1[1]);
            }

            var c = new List<Sorter>();
            for (var i = 0; i < bnd.Count; i++)
            {
                c.Add(new Sorter(MeshConnection.Edges[bnd[i]].NodeFrom, i));
                c.Add(new Sorter(MeshConnection.Edges[bnd[i]].NodeTo, i));
            }

            c = c.OrderBy(sorter => sorter.Value).ToList();

            var k = 0;
            next = 0;
            while (k < 2 * numb + 1)
            {
                if (c[k].Value == c[k + 1].Value)
                {
                    c[next].Value = c[k].Index;
                    c[next].Index = c[k + 1].Index;
                    next++;
                    k = k + 2;
                }
                else
                {
                    k++;
                }
            }

            for (var i = c.Count - 1; i >= next; i--)
            {
                c.RemoveAt(i);
            }

            return SortContours(penew, c, contourLevel);
        }

        private List<Contour> SortContours(double[,] penew, List<Sorter> c, double contourLevel)
        {
            var contours = new List<Contour>();
            var n = penew.GetLength(0);
            var ncc = c.Count;

            var ndx = new int[MeshConnection.Edges.Count];
            var n2E = new int[n, 2];
            for (var i = 0; i < n; i++)
            {
                n2E[i, 0] = -1;
                n2E[i, 1] = -1;
            }

            for (var i = 0; i < ncc; i++)
            {
                var n1 = c[i].Value;
                var n2 = c[i].Index;
                n2E[n1, ndx[n1]] = i;
                ndx[n1] = ndx[n1] + 1;
                n2E[n2, ndx[n2]] = i;
                ndx[n2] = ndx[n2] + 1;
            }

            var bndn = new bool[n];
            var bnde = new bool[ncc];
            for (var i = 0; i < n; i++)
            {
                bndn[i] = n2E[i, 1] == -1;
            }

            for (var i = 0; i < ncc; i++)
            {
                bnde[i] = bndn[c[i].Value] || bndn[c[i].Index];
            }

            var ce = 0;
            var start = ce;
            var cn = c[0].Index;
            var flag = new bool[ncc];
            var x = new List<double>();
            var y = new List<double>();
            x.Add(penew[c[ce].Value, 0]);
            y.Add(penew[c[ce].Value, 1]);

            for (var i = 0; i < ncc; i++)
            {
                flag[ce] = true;

                x.Add(penew[cn, 0]);
                y.Add(penew[cn, 1]);

                if (ce == n2E[cn, 0])
                {
                    ce = n2E[cn, 1];
                }
                else
                {
                    ce = n2E[cn, 0];
                }

                if (ce == -1 || ce == start || flag[ce])
                {
                    if (x.Count > 2)
                    {
                        var contoursI = new Contours(x, y, contourLevel);
                        foreach (var con in contoursI.ContourList)
                        {
                            contours.Add(con);
                        }
                    }

                    var finish = true;
                    for (var j = 0; j < ncc; j++)
                    {
                        if (!flag[j])
                        {
                            finish = false;
                            break;
                        }
                    }

                    if (finish)
                    {
                        break;
                    }

                    var edges = new List<int>();
                    for (var j = 0; j < ncc; j++)
                    {
                        if (!flag[j])
                        {
                            edges.Add(j);
                        }
                    }

                    ce = edges[0];
                    foreach (var t in edges)
                    {
                        if (bnde[t])
                        {
                            ce = t;
                            break;
                        }
                    }

                    start = ce;
                    x = new List<double>();
                    y = new List<double>();
                    if (bndn[c[ce].Index])
                    {
                        cn = c[ce].Value;
                        x.Add(penew[c[ce].Index, 0]);
                        y.Add(penew[c[ce].Index, 1]);
                    }
                    else
                    {
                        cn = c[ce].Index;
                        x.Add(penew[c[ce].Value, 0]);
                        y.Add(penew[c[ce].Value, 1]);
                    }
                }
                else
                {
                    if (cn == c[ce].Value)
                    {
                        cn = c[ce].Index;
                    }
                    else
                    {
                        cn = c[ce].Value;
                    }
                }
            }

            return contours;
        }

        private double[] CalculateCentroidValues()
        {
            var nElem = Mesh.ElementTable.Length;
            var nNodes = Mesh.X.Length;

            var ht = new double[nElem];
            var pt = new double[nElem, 2];
            var hnx = new double[nNodes];
            var hny = new double[nNodes];
            var count = new int[nNodes];
            for (var i = 0; i < nElem; i++)
            {
                var n = new[] { Mesh.ElementTable[i][0] - 1, Mesh.ElementTable[i][1] - 1, Mesh.ElementTable[i][2] - 1 };
                var x23 = Mesh.X[n[1]] - Mesh.X[n[2]];
                var y23 = Mesh.Y[n[1]] - Mesh.Y[n[2]];
                var x21 = Mesh.X[n[1]] - Mesh.X[n[0]];
                var y21 = Mesh.Y[n[1]] - Mesh.Y[n[0]];
                var htx = (y23 * NodeData[n[0]] + (y21 - y23) * NodeData[n[1]] - y21 * NodeData[n[2]]) / (x23 * y21 - x21 * y23);
                var hty = (x23 * NodeData[n[0]] + (x21 - x23) * NodeData[n[1]] - x21 * NodeData[n[2]]) / (y23 * x21 - y21 * x23);
                hnx[n[0]] = hnx[n[0]] + htx;
                hny[n[0]] = hny[n[0]] + hty;
                count[n[0]] = count[n[0]] + 1;
                hnx[n[1]] = hnx[n[1]] + htx;
                hny[n[1]] = hny[n[1]] + hty;
                count[n[1]] = count[n[1]] + 1;
                hnx[n[2]] = hnx[n[2]] + htx;
                hny[n[2]] = hny[n[2]] + hty;
                count[n[2]] = count[n[2]] + 1;

                pt[i, 0] = (Mesh.X[n[0]] + Mesh.X[n[1]] + Mesh.X[n[2]]) / 3.0;
                pt[i, 1] = (Mesh.Y[n[0]] + Mesh.Y[n[1]] + Mesh.Y[n[2]]) / 3.0;
            }

            for (var i = 0; i < nNodes; i++)
            {
                hnx[i] = hnx[i] / count[i];
                hny[i] = hny[i] / count[i];
            }

            for (var i = 0; i < nElem; i++)
            {
                var n = new[] { Mesh.ElementTable[i][0] - 1, Mesh.ElementTable[i][1] - 1, Mesh.ElementTable[i][2] - 1 };

                ht[i] = (NodeData[n[0]] + (pt[i, 0] - Mesh.X[n[0]]) * hnx[n[0]] + (pt[i, 1] - Mesh.Y[n[0]]) * hny[n[0]] + NodeData[n[1]] + (pt[i, 0] - Mesh.X[n[1]]) * hnx[n[1]] + (pt[i, 1] - Mesh.Y[n[1]]) * hny[n[1]] + NodeData[n[2]] + (pt[i, 0] - Mesh.X[n[2]]) * hnx[n[2]] + (pt[i, 1] - Mesh.Y[n[2]]) * hny[n[2]]) / 3.0;
            }

            return ht;
        }
    }
}
