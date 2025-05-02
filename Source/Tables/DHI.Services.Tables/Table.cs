namespace DHI.Services.Tables
{
    using System.Collections.Generic;
    using System.Linq;

    public class Table : BaseEntity<string>
    {
        public Table(string id)
            : base(id)
        {
            Columns = new ColumnList();
            Data = new List<List<object>>();
        }

        public ColumnList Columns { get; }

        /// <summary>
        ///     Inner list is the row data
        /// </summary>
        public List<List<object>> Data { get; }

        public IEnumerable<Column> KeyColumns
        {
            get { return Columns.Where(col => col.IsKey).ToArray(); }
        }

        public bool ContainsColumn(string name)
        {
            return Columns.Any(col => col.Name == name);
        }

        public bool ShouldSerializeData()
        {
            return Data.Any();
        }

        public bool ShouldSerializeKeyColumns()
        {
            return KeyColumns.Any();
        }
    }
}