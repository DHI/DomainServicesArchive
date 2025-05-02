namespace DHI.Services.Samples.Spreadsheet
{
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using Provider.OpenXML;
    using Spreadsheets;

    public partial class Form1 : Form
    {
        private readonly DataTable _dataTable;

        public Form1()
        {
            InitializeComponent();
            var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var rootFolder = Path.Combine(binFolder, @"..\..");

            // Wire up the spreadsheet service
            var spreadsheetRepository = new SpreadsheetRepository(rootFolder);
            var spreadsheetService = new SpreadsheetService(spreadsheetRepository);

            // Get all data in sheet2
            var data = spreadsheetService.GetUsedRange("sample.xlsx", "sheet2");

            // Convert to data table and bind to data grid view
            _dataTable = data.ToDataTable();
            dataGridView1.DataSource = _dataTable;
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            // Using the SaveAsXlsx DataSet extension method to save data set as MS Excel file.
            var dataSet = new DataSet();
            dataSet.Tables.Add(_dataTable.Copy());
            var dialog = new SaveFileDialog { AddExtension = true, DefaultExt = "xlsx", Filter = "Excel Worksheet (*.xlsx)|*.xlsx" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(dialog.FileName))
                {
                    dataSet.SaveAsXlsx(dialog.FileName, false);   
                }
            }
        }
    }
}