namespace DHI.Services.Meshes
{
    public class Edge
    {
        public Edge(int nodeFrom, int nodeTo)
        {
            NodeFrom = nodeFrom;
            NodeTo = nodeTo;
            Key = NodeFrom + "_" + NodeTo;
        }

        public string Key { get; }
        public int NodeFrom { get; }
        public int NodeTo { get; }
        public int Index { get; private set; }

        public void SetIndex(int index)
        {
            Index = index;
        }
    }
}
