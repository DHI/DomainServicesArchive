namespace DHI.Services.Tables
{
    using System;
    using System.Collections.Generic;

    public class ColumnList : List<Column>
    {
        public new void Add(Column column)
        {
            if (Contains(column))
            {
                throw new ArgumentException(
                    $"Duplicate column names. There is already a column with the name '{column.Name}'.");
            }

            base.Add(column);
        }
    }
}