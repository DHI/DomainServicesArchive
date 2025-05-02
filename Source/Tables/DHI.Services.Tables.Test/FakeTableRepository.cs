namespace DHI.Services.Tables.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Tables;

    internal class FakeTableRepository : BaseTableRepository
    {
        private readonly Dictionary<string, Table> _tableList = new Dictionary<string, Table>();

        public FakeTableRepository(IEnumerable<Table> tableList)
        {
            foreach (var table in tableList)
            {
                _tableList.Add(table.Id, table);
            }
        }

        public override void Add(Table table, ClaimsPrincipal user = null)
        {
            _tableList[table.Id] = table;
        }

        public override void Remove(string id, ClaimsPrincipal user = null)
        {
            _tableList.Remove(id);
        }

        public override void Update(Table table, ClaimsPrincipal user = null)
        {
            _tableList[table.Id] = table;
        }

        public override IEnumerable<Table> GetAll(ClaimsPrincipal user = null)
        {
            return _tableList.Values.ToList();
        }

        public override object[,] GetData(string id, IEnumerable<QueryCondition> filter = null, ClaimsPrincipal user = null)
        {
            throw new System.NotImplementedException();
        }
    }
}