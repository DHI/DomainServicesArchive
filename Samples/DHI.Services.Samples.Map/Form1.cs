namespace DHI.Services.Samples.Map
{
    using System.IO;
    using System.Reflection;
    using System.Windows.Forms;
    using GIS.Maps;
    using Provider.MIKECore;
    using Spatial;

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Wire up the map style service
            var mapStyleRepository = new MapStyleRepository(Path.Combine(binFolder, "styles.json"));
            var mapStyleService = new MapStyleService(mapStyleRepository);

            // Create a map style (if not already created)
            var mapStyle = new MapStyle("MyStyle", "My Style") {StyleCode = "0^10:#800080,#5500AB,#2A00D5,#0000FF,#0038E1,#006FC3,#00A6A6,#00C46E,#00E237,#00FF00,#55FF00,#AAFF00,#FFFF00,#FFAA00,#FF5500,#FF0000"};
            if (!mapStyleService.Exists("MyStyle"))
            {
                mapStyleService.Add(mapStyle);
            }

            // Wire up the map service
            var mapSource = new Dfs2MapSource(Path.Combine(binFolder, @"..\..\R20141001.dfs2"), new Parameters());
            var mapService = new MapService(mapSource, mapStyleService);

            // Retrieve and display map image
            var image = mapService.GetMap("MyStyle", "EPSG:3857", BoundingBox.Parse("11584184.510675031,78271.51696402066,11623320.26915704,117407.27544603013"), 256, 256, "", null, "1", new Parameters());
            pictureBox1.Image = image;

            // Display legend
            pictureBox2.Image = mapStyle.ToBitmapHorizontal(pictureBox2.Width, pictureBox2.Height);
        }
    }
}