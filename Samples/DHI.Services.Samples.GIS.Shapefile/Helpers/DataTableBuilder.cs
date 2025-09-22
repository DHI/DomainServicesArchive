using System.Data;

namespace DHI.Services.Samples.GIS.Shapefile.Helpers
{
    public static class DataTableBuilder
    {
        public static DataTable Create(IEnumerable<string> columnNames)
        {
            var dt = new DataTable("Features");
            foreach (var name in columnNames)
                dt.Columns.Add(name);
            return dt;
        }
    }
}
