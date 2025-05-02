namespace DHI.Services.Samples.Radar
{
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using Rasters.Radar;
    using Rasters.Zones;

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // Wire up the zone service
            var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var zoneRepository = new ZoneRepository(Path.Combine(binFolder, @"..\..\zones.json"));
            var zoneService = new ZoneService(zoneRepository);

            // draw zone as bitmap and display zone name
            var zone = zoneService.GetAll().First();
            pictureBox1.Image = zone.ToBitmap();
            label2.Text = zone.Name;

            // Read a P00 radar image from file and convert the values to rain intensity
            var image = Rasters.Radar.X00.RadarImage.CreateNew(Path.Combine(binFolder, @"..\..\AROS1245.p00")).ToIntensity();

            // Display a bitmap using the default intensity color gradient
            var colorGradient = ColorGradientType.IntensityDefault.ColorGradient;
            pictureBox2.Image = image.ToBitmap(colorGradient);
            pictureBoxPalette.Image = colorGradient.ToBitmap(pictureBoxPalette.Height, pictureBoxPalette.Width);
            labelMin.Text = colorGradient.ThresholdValues.First().ToString(CultureInfo.CurrentCulture);
            labelMax.Text = colorGradient.ThresholdValues.Last().ToString(CultureInfo.CurrentCulture);

#warning DIMS database with Aarhus radar images is currently not up and running
            // TODO: Set up file-based repository of DMI-data. Speak with AKF.
            //// Wire up the radar image service
            //const string token = "dcradargw;{15C2A90C-C3DA-11E2-B891-083E8EE1ED34};11093400 LAWR-Radar;Master;AEorHVne42mrRUi;ADwdjCq2wyG8DfDtossxfwKdcL5G73FLVGkU2XSrXnXcc9N8DFKhPVSUZCiYazIpDZzaQqn4oz/AyufHDt/ED4xlSQel+m;494699537";
            //const string timeSeriesName = "AROS";
            //var repository = new RadarImageRepository<RadarImage>(token, timeSeriesName);
            //var radarImageService = new RadarImageService<RadarImage>(repository);

            //// Calculate the accumulated depth in the zone within the last 24 hours
            //var to = radarImageService.LastDateTime;
            //var @from = to.AddHours(-24);
            //var depth = radarImageService.GetDepth(zone, @from, to);
            //label3.Text = "Accumulated rain within last 24 hours in zone: " + depth.ToString("##.00", CultureInfo.CurrentCulture) + " mm";

            //// Get intensities in unit micrometer/second in the zone within the last 24 hours
            //var conversionCoefficients = ConversionCoefficients.Default;
            //conversionCoefficients.RainIntensityUnit = RainIntensityUnit.MicroMetersPerSecond;
            //var intensities = radarImageService.GetIntensities(zone, from, to, conversionCoefficients);

        }
    }
}