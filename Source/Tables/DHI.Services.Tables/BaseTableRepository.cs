namespace DHI.Services.Tables
{
    using System.Collections.Generic;
    using System.Security.Claims;

    public abstract class BaseTableRepository : BaseDiscreteRepository<Table, string>, ITableRepository
    {
        public IEnumerable<Column> GetColumns(string id, ClaimsPrincipal user = null)
        {
            var maybe = Get(id, user);
            return maybe.HasValue ? maybe.Value.Columns.ToArray() : new Column[0];
        }

        public IEnumerable<Column> GetKeyColumns(string id, ClaimsPrincipal user = null)
        {
            var maybe = Get(id, user);
            return maybe.HasValue ? maybe.Value.KeyColumns : new Column[0];
        }

        public bool ContainsColumn(string id, string columnName, ClaimsPrincipal user = null)
        {
            var maybe = Get(id, user);
            return maybe.HasValue && maybe.Value.ContainsColumn(columnName);
        }

        public abstract object[,] GetData(string id, IEnumerable<QueryCondition> filter = null, ClaimsPrincipal user = null);

        public abstract void Add(Table entity, ClaimsPrincipal user = null);

        public abstract void Remove(string id, ClaimsPrincipal user = null);

        public abstract void Update(Table entity, ClaimsPrincipal user = null);
    }
}