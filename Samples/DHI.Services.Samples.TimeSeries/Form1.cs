namespace DHI.Services.Samples.TimeSeries
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using DHI.Services.TimeSeries;
    using Provider.DIMS;
    using Provider.MIKECore;

    public partial class Form1 : Form
    {
        private readonly DiscreteTimeSeriesService _timeSeriesServiceDfs0;
        private readonly DiscreteTimeSeriesService _timeSeriesServiceDims;

        public Form1()
        {
            InitializeComponent();

            // Dfs0
            var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(binFolder, @"..\..\TaarbaekRev_Spectral.dfs0");
            var dfs0TimeSeriesRepository = new Dfs0TimeSeriesRepository(filePath);
            _timeSeriesServiceDfs0 = new DiscreteTimeSeriesService(dfs0TimeSeriesRepository);
            comboBox1.DataSource = _timeSeriesServiceDfs0.GetIds();

            // DIMS.CORE
            const string Token = "dimstest2016.dhi.dk;{BB729FC3-0135-11E5-BFFA-24FD526B9663};11818178 Brede Å;Master;AEorHVne42mrRUi;ADwdjCq2wyG8DkuRauN4Sqr1wMvJ3mWTPup39litCHE/dEOjiJIK89AQ3qEY/EkG2UGCtkCl+GC+aA22AX4bRH3WthyQ7jrldXrp3fFJ5+RqKHK;1453633290";
            var dimsTimeSeriesRepository = new TimeSeriesRepository(Token);
            _timeSeriesServiceDims = new DiscreteTimeSeriesService(dimsTimeSeriesRepository);
            comboBox2.DataSource = _timeSeriesServiceDims.GetIds();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridView1.DataSource = _timeSeriesServiceDfs0.GetValues((string)comboBox1.SelectedItem).ToSortedSet().ToList();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            var from = new DateTime(1990, 1, 1);
            dataGridView2.DataSource = _timeSeriesServiceDims.GetValues((string)comboBox2.SelectedItem, from).ToSortedSet().ToList();
        }
    }
}