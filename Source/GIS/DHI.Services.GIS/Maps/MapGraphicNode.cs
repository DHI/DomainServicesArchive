namespace DHI.Services.GIS.Maps
{
    using Spatial;

    public class MapGraphicNode
    {
        public string Id { get; set; }

        public Position LonLat { get; set; }

        public Position Google { get; set; }
    }
}
