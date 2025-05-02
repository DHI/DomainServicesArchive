namespace DHI.Services.Meshes
{
    using System.Collections.Generic;

    public class ItemEqualityComparer : IEqualityComparer<Edge>
    {
        public bool Equals(Edge edge1, Edge edge2)
        {
            return edge1.Key == edge2.Key;
        }

        public int GetHashCode(Edge obj)
        {
            return obj.Key.GetHashCode();
        }
    }
}
