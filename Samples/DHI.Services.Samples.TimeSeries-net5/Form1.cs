namespace DHI.Services.Samples.TimeSeries_net5
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using Provider.MIKECore;
    using TimeSeries;

    public partial class Form1 : Form
    {
        private readonly DiscreteTimeSeriesService _timeSeriesService;

        public Form1()
        {
            InitializeComponent();

            // Dfs0
            var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(binFolder, @"..\..\..\..\TaarbaekRev_Spectral.dfs0");
            var dfs0TimeSeriesRepository = new Dfs0TimeSeriesRepository(filePath);
            _timeSeriesService = new DiscreteTimeSeriesService(dfs0TimeSeriesRepository);
            comboBox1.DataSource = _timeSeriesService.GetIds().ToList();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridView1.DataSource = _timeSeriesService.GetValues((string)comboBox1.SelectedItem).ToSortedSet().ToList();
        }
    }
}