namespace DHI.Services.Samples.Tables
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using DHI.Services.Tables;
    using Provider.ODBC;

    public partial class Form1 : Form
    {
        private readonly TableService _tableService;

        public Form1()
        {
            InitializeComponent();

            var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(binFolder, @"..\..\Database1.accdb");
            var connectionString = $"Driver={{Microsoft Access Driver (*.mdb, *.accdb)}};Dbq={filePath};Uid=Admin;Pwd=;";

            // Wire up the table service
            var tableRepository = new TableRepository(connectionString);
            _tableService = new TableService(tableRepository);

            // Populate combo box with table names
            comboBox1.DataSource = _tableService.GetIds().ToList();
        }

        private void comboBox1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            var id = (string)comboBox1.SelectedItem;
            object[,] data;
            if (id == "WaterLevels")
            {
                // Query filtered data
                var filter = new List<QueryCondition> { new QueryCondition("WaterLevel", QueryOperator.GreaterThan, 23.1) };
                data = _tableService.GetData(id, filter);
            }
            else
            {
                // Query all data
                data = _tableService.GetData(id);
            }

            // Convert to data table and bind to data grid view
            dataGridView1.DataSource = data.ToDataTable();
        }
    }
}