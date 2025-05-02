namespace DHI.Services.WebApiCore
{
    using System.Collections.Generic;

    /// <summary>
    ///     Data transfer object for a query resource representation.
    /// </summary>
    public class QueryDTO<T> : List<QueryConditionDTO>
    {
        /// <summary>
        ///     Converts the DTO to a Query object.
        /// </summary>
        /// <returns>Query&lt;T&gt;.</returns>
        public Query<T> ToQuery()
        {
            var conditions = new List<QueryCondition>();
            foreach (var queryCondition in this)
            {
                conditions.Add(queryCondition.ToQueryCondition());
            }

            return new Query<T>(conditions);
        }
    }

    /// <summary>
    ///     Data transfer object for a query resource representation.
    /// </summary>
    public class QueryDTO : List<QueryConditionDTO>
    {
        /// <summary>
        ///     Converts the DTO to a list of query conditions.
        /// </summary>
        /// <returns>IEnumerable&lt;QueryCondition&gt;.</returns>
        public IEnumerable<QueryCondition> ToQueryConditions()
        {
            var conditions = new List<QueryCondition>();
            foreach (var queryCondition in this)
            {
                conditions.Add(queryCondition.ToQueryCondition());
            }

            return conditions;
        }
    }
}