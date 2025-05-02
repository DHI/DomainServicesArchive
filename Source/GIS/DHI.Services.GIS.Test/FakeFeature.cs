namespace DHI.Services.GIS.Test
{
    using System.Collections.Generic;
    using Spatial;

    internal class FakeFeature : IFeature
    {
        public FakeFeature(string id, IGeometry geometry)
        {
            AttributeValues = new Dictionary<string, object> {{"id", id}};
            Geometry = geometry;
        }

        public IList<IAssociation> Associations { get; } = new List<IAssociation>();

        public Dictionary<string, object> AttributeValues { get; }

        public IGeometry Geometry { get; set; }
    }
}