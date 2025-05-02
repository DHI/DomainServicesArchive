namespace DHI.Services.Meshes
{
    using System;

    public class Mesh
    {
        public Mesh(double[] xIn, double[] yIn, float[] zIn, int[] code, int[][] elementTable, string projection)
        {
            X = xIn;
            Y = yIn;
            Z = zIn;
            Code = code;
            ElementTable = elementTable;
            Projection = projection;
        }

        public double[] X { get; }
        public double[] Y { get; }
        public float[] Z { get; }
        public int[] Code { get; }
        public int[][] ElementTable { get; }
        public string Projection { get; }
        public int[][]? NodeTable { get; private set; }

        public double[] CalculateNodeValues(double[] xe, double[] ye, float[] elemData)
        {
            var nNodes = X.Length;
            var nodeData = new double[nNodes];

            var elemDataD = new double[elemData.Length];
            for (var i = 0; i < elemDataD.Length; i++)
            {
                elemDataD[i] = elemData[i];
            }

            for (var i = 0; i < nNodes; i++)
            {
                nodeData[i] = MzCalculateNodeVlauePL(NodeTable[i], xe, ye, elemDataD, X[i], Y[i]);
            }

            return nodeData;
        }

        public void GenerateNodeTable() // only works for tri meshes
        {
            var nElemts = ElementTable.Length;
            var nNodes = 0;
            for (var i = 0; i < nElemts; i++)
            {
                for (var j = 0; j < ElementTable[i].Length; j++)
                {
                    if (ElementTable[i][j] > nNodes)
                    {
                        nNodes = ElementTable[i][j];
                    }
                }
            }

            var indI = new int[nElemts * 3];
            var indJ = new int[nElemts * 3];
            var indK = new int[nElemts * 3];
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < nElemts; j++)
                {
                    indI[i * nElemts + j] = j;
                    indJ[i * nElemts + j] = ElementTable[j][i] - 1;
                    indK[i * nElemts + j] = i;
                }
            }

            NodeTable = new int[nNodes][];
            var count = new int[nNodes];
            for (var i = 0; i < nNodes; i++)
            {
                NodeTable[i] = new int[0];
            }

            for (var i = 0; i < indI.Length; i++)
            {
                count[indJ[i]] += 1;
                var tmp = new int[NodeTable[indJ[i]].Length];
                if (NodeTable[indJ[i]].Length < count[indJ[i]] + 1)
                {
                    for (var j = 0; j < tmp.Length; j++)
                    {
                        tmp[j] = NodeTable[indJ[i]][j];
                    }

                    NodeTable[indJ[i]] = new int[count[indJ[i]]];
                    for (var j = 0; j < tmp.Length; j++)
                    {
                        NodeTable[indJ[i]][j] = tmp[j];
                    }
                }

                NodeTable[indJ[i]][count[indJ[i]] - 1] = indI[i];
            }
        }

        private static double MzCalculateNodeVlauePL(int[] node2Elmt, double[] xe, double[] ye, double[] elemData, double xn, double yn)
        {
            var nElements = node2Elmt.Length;

            var rx = 0.0;
            var ry = 0.0;
            var ixx = 0.0;
            var iyy = 0.0;
            var ixy = 0.0;
            for (var i = 0; i < nElements; i++)
            {
                var id = node2Elmt[i];
                var dx = xe[id] - xn;
                var dy = ye[id] - yn;
                rx += dx;
                ry += dy;
                ixx += dx * dx;
                iyy += dy * dy;
                ixy += dx * dy;
            }

            var lamda = ixx * iyy - ixy * ixy;

            var omegaSum = 0.0;
            var nodeValue = 0.0;
            if (Math.Abs(lamda) > 1e-10 * (ixx * ixx))
            {
                var lamdaX = (ixy * ry - iyy * rx) / lamda;
                var lamdaY = (ixy * rx - ixx * ry) / lamda;

                for (var i = 0; i < nElements; i++)
                {
                    var id = node2Elmt[i];
                    var omega = 1.0 + lamdaX * (xe[id] - xn) + lamdaY * (ye[id] - yn);
                    if (omega < 0)
                    {
                        omega = 0;
                    }
                    else if (omega > 2)
                    {
                        omega = 2;
                    }

                    omegaSum += omega;
                    nodeValue += omega * elemData[id];
                }

                if (Math.Abs(omegaSum) > 1e-10)
                {
                    nodeValue /= omegaSum;
                }
                else
                {
                    omegaSum = 0.0;
                }
            }

            if (Math.Abs(omegaSum) < 1e-30)
            {
                nodeValue = 0.0;

                for (var i = 0; i < nElements; i++)
                {
                    var id = node2Elmt[i];

                    var dx = xe[id] - xn;
                    var dy = ye[id] - yn;

                    var omega = 1.0 / Math.Sqrt(dx * dx + dy * dy);
                    omegaSum += omega;
                    nodeValue += omega * elemData[id];
                }

                if (Math.Abs(Math.Abs(omegaSum)) < 1e-30)
                {
                    nodeValue = 0.0;
                }
                else
                {
                    nodeValue /= omegaSum;
                }
            }

            return nodeValue;
        }

    }
}
