namespace DHI.Services.GIS.Maps
{
    using System.Collections.Generic;
    using Spatial;

    public class MapGraphicElement
    {
        public MapGraphicElement()
        {
            NodeIds = new List<string>();
        }

        public string Id { get; set; }

        public List<string> NodeIds { get; set; }

        public BoundingBox GoogleBoundingBox { get; set; }
        public BoundingBox LonLatBoundingBox { get; set; }
    }
}
