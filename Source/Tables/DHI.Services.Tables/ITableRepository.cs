namespace DHI.Services.Tables
{
    using System.Collections.Generic;
    using System.Security.Claims;

    public interface ITableRepository : IRepository<Table, string>, IDiscreteRepository<Table, string>, IUpdatableRepository<Table, string>
    {
        IEnumerable<Column> GetColumns(string id, ClaimsPrincipal user = null);

        IEnumerable<Column> GetKeyColumns(string id, ClaimsPrincipal user = null);

        bool ContainsColumn(string id, string columnName, ClaimsPrincipal user = null);

        /// <summary>
        /// Inner list is the row data
        /// </summary>
        object[,] GetData(string id, IEnumerable<QueryCondition> filter = null, ClaimsPrincipal user = null);
    }
}