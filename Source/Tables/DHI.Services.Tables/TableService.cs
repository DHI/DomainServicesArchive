namespace DHI.Services.Tables
{
    using System;
    using System.Collections.Generic;

    public class TableService : BaseUpdatableDiscreteService<Table, string>
    {
        private readonly ITableRepository _repository;

        public TableService(ITableRepository repository) : base(repository)
        {
            _repository = repository;
        }

        public static Type[] GetRepositoryTypes(string path = null)
        {
            return Service.GetProviderTypes<ITableRepository>(path);
        }

        public IEnumerable<Column> GetColumns(string id)
        {
            return _repository.GetColumns(id);
        }

        public IEnumerable<Column> GetKeyColumns(string id)
        {
            return _repository.GetKeyColumns(id);
        }

        public bool ContainsColumn(string id, string columnName)
        {
            return _repository.ContainsColumn(id, columnName);
        }

        public object[,] GetData(string id, IEnumerable<QueryCondition> filter = null)
        {
            return _repository.GetData(id, filter);
        }
    }
}