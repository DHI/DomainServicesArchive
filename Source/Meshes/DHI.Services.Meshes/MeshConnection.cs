namespace DHI.Services.Meshes
{
    using System.Collections.Generic;
    using System.Linq;

    public class MeshConnection
    {
        public MeshConnection(Mesh mesh)
        {
            NumberOfElements = mesh.ElementTable.Length;
            Edges = new List<Edge>();
            AddEdges(NumberOfElements, mesh, 0, 1);
            AddEdges(NumberOfElements, mesh, 1, 2);
            AddEdges(NumberOfElements, mesh, 2, 0);

            SetEdgeIndexes(Edges);
            var edgesO = Edges.OrderBy(edge => edge.NodeFrom).ThenBy(edge => edge.NodeTo).ToList();
            var edgesD = edgesO.Distinct(new ItemEqualityComparer()).ToList();
            var distinctEdgesMap = MapDistinctEdges(edgesO);

            UniqueEdgesInElement = new int[NumberOfElements, 3];
            for (var i = 0; i < NumberOfElements; i++)
            {
                UniqueEdgesInElement[i, 0] = distinctEdgesMap[i];
                UniqueEdgesInElement[i, 1] = distinctEdgesMap[i + NumberOfElements];
                UniqueEdgesInElement[i, 2] = distinctEdgesMap[i + 2 * NumberOfElements];
            }

            Edges = edgesD;

            var nEdges = Edges.Count;
            EdgeToElement = new int[nEdges, 2];
            for (var i = 0; i < nEdges; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    EdgeToElement[i, j] = -1;
                }
            }

            var ndx = new int[nEdges];
            for (var i = 0; i < NumberOfElements; i++)
            {
                var e1 = UniqueEdgesInElement[i, 0];
                var e2 = UniqueEdgesInElement[i, 1];
                var e3 = UniqueEdgesInElement[i, 2];

                EdgeToElement[e1, ndx[e1]] = i;
                ndx[e1] = ndx[e1] + 1;

                EdgeToElement[e2, ndx[e2]] = i;
                ndx[e2] = ndx[e2] + 1;

                EdgeToElement[e3, ndx[e3]] = i;
                ndx[e3] = ndx[e3] + 1;
            }
        }

        public List<Edge> Edges { get; }

        public int[,] UniqueEdgesInElement { get; }

        public int[,] EdgeToElement { get; }

        private int NumberOfElements { get; }

        public List<int> GetBoundaryElements()
        {
            var boundaryElements = new List<int>();
            for (var i = 0; i < EdgeToElement.GetLength(0); i++)
            {
                if (EdgeToElement[i, 0] == -1)
                {
                    boundaryElements.Add(EdgeToElement[i, 1]);
                }
                else if (EdgeToElement[i, 1] == -1)
                {
                    boundaryElements.Add(EdgeToElement[i, 0]);
                }
            }

            boundaryElements = boundaryElements.Distinct().ToList();

            return boundaryElements;
        }

        private int[] MapDistinctEdges(List<Edge> edgesO)
        {
            var distinctEdgesMap = new int[Edges.Count];

            var groupsSort = new int[edgesO.Count];
            var ind = new int[edgesO.Count];
            groupsSort[0] = 1;
            ind[0] = 0;
            for (var i = 0; i < edgesO.Count - 1; i++)
            {
                if (edgesO[i].NodeFrom != edgesO[i + 1].NodeFrom || edgesO[i].NodeTo != edgesO[i + 1].NodeTo)
                {
                    groupsSort[i + 1] = 1;
                }
                else
                {
                    groupsSort[i + 1] = 0;
                }

                ind[i + 1] = ind[i] + groupsSort[i + 1];
            }

            for (var i = 0; i < edgesO.Count; i++)
            {
                distinctEdgesMap[edgesO[i].Index] = ind[i];
            }

            return distinctEdgesMap;
        }

        private static void SetEdgeIndexes(List<Edge> edges)
        {
            for (var i = 0; i < edges.Count; i++)
            {
                edges[i].SetIndex(i);
            }
        }

        private void AddEdges(int nElem, Mesh mesh, int ind1, int ind2)
        {
            for (var i = 0; i < nElem; i++)
            {
                Edge edge;
                if (mesh.ElementTable[i][ind1] < mesh.ElementTable[i][ind2])
                {
                    edge = new Edge(mesh.ElementTable[i][ind1] - 1, mesh.ElementTable[i][ind2] - 1);
                }
                else
                {
                    edge = new Edge(mesh.ElementTable[i][ind2] - 1, mesh.ElementTable[i][ind1] - 1);
                }

                Edges.Add(edge);
            }
        }
    }
}
