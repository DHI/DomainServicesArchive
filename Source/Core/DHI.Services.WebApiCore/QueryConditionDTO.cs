namespace DHI.Services.WebApiCore
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    /// <summary>
    ///     Data transfer object for a query condition resource representation.
    /// </summary>
    public class QueryConditionDTO
    {
        /// <summary>
        ///     Gets or sets the item.
        /// </summary>
        [Required]
        public string Item { get; set; }

        /// <summary>
        ///     Gets or sets the query operator.
        /// </summary>
        [Required]
        public string QueryOperator { get; set; }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        ///     Gets or sets the collection of values.
        /// </summary>
        public IEnumerable<string> Values { get; set; }

        /// <summary>
        ///     Converts the DTO to a QueryCondition object.
        /// </summary>
        /// <returns>QueryCondition.</returns>
        public QueryCondition ToQueryCondition()
        {
            if (!Enum.TryParse(QueryOperator, out QueryOperator queryOperator))
            {
                throw new ArgumentException($"Could not parse query operator '{QueryOperator}'", nameof(QueryOperator));
            }

            if (queryOperator == DHI.Services.QueryOperator.Any)
            {
                if (Values is null || !Values.Any())
                {
                    throw new ArgumentException("The 'Any' query operator requires an array of string values to be set in 'Values'.", nameof(Values));
                }

                return new QueryCondition(Item, queryOperator, Values.Select(value => value.ToObject()).ToArray());
            }

            if (Value is null)
            {
                throw new ArgumentException("No 'Value' was defined.", nameof(Value));
            }

            return new QueryCondition(Item, queryOperator, Value.ToObject());
        }
    }
}