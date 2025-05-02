namespace DHI.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    ///     Class ExpressionBuilder.
    /// </summary>
    public static class ExpressionBuilder
    {
        /// <summary>
        ///     Builds a LINQ expression from a collection of query conditions.
        /// </summary>
        /// <typeparam name="T">Type of LINQ parameter</typeparam>
        /// <param name="filter">The collection of query conditions.</param>
        /// <returns>Expression&lt;Func&lt;T, System.Boolean&gt;&gt;.</returns>
        /// <exception cref="ArgumentNullException">filter</exception>
        /// <exception cref="ArgumentException">Filter cannot be empty.;filter</exception>
        public static Expression<Func<T, bool>> Build<T>(IEnumerable<QueryCondition> filter)
        {
            Guard.Against.Null(filter, nameof(filter));
            var queryConditions = filter as IList<QueryCondition> ?? filter.ToList();
            if (!queryConditions.Any())
            {
                throw new ArgumentException("Filter cannot be empty.", nameof(filter));
            }

            var parameter = Expression.Parameter(typeof(T), "t");
            Expression expression = null;
            foreach (var condition in queryConditions)
            {
                expression = expression == null ? _ToExpression(parameter, condition) : Expression.AndAlso(expression, _ToExpression(parameter, condition));
            }

            return Expression.Lambda<Func<T, bool>>(expression, parameter);
        }

        private static Expression _ToExpression(ParameterExpression param, QueryCondition condition)
        {
            Expression property = Expression.Property(param, condition.Item);
            Expression value = Expression.Constant(condition.Value);

            //set property value to default if it is nullable
            if (Nullable.GetUnderlyingType(property.Type) != null) 
            {
                property = Expression.Call(property, "GetValueOrDefault", Type.EmptyTypes);
            }

            // cast to enum
            if (condition.Value != null && condition.Value.GetType().IsEnum)
            {
                var enumType = Enum.GetUnderlyingType(condition.Value.GetType());
                property = Expression.Convert(property, enumType);
                value = Expression.Convert(value, enumType);
            }

            return condition.QueryOperator switch
            {
                QueryOperator.GreaterThan => Expression.GreaterThan(property, value),
                QueryOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, value),
                QueryOperator.LessThan => Expression.LessThan(property, value),
                QueryOperator.LessThanOrEqual => Expression.LessThanOrEqual(property, value),
                QueryOperator.Equal => Expression.Equal(property, value),
                QueryOperator.NotEqual => Expression.NotEqual(property, value),
                QueryOperator.Any => GetContainsExpression(property, value),
                _ => throw new NotImplementedException($"Query operator '{condition.QueryOperator}' is not supported."),
            };
        }

        private static Expression GetContainsExpression(Expression property, Expression value)
        {
            // Check if the value is an array or a list
            if (!value.Type.IsCollection())
            {
                throw new NotSupportedException($"The value '{value}' is not a collection.");
            }
            var propertyType = value.Type;
            if (value.Type.IsArray)
            {
                var elementType = propertyType.GetElementType();
                var containsMethod = typeof(Enumerable).GetMethods()
                    .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(elementType);

                var parameter = Expression.Parameter(elementType, "x");
                var equalityExpression = Expression.Equal(parameter, property);
                var lambda = Expression.Lambda(equalityExpression, parameter);

                return Expression.Call(null, containsMethod, value, lambda);
            }
            else
            {
                // Get the method info for Enumerable.Contains
                var containsMethod = value.Type.GetMethods()
                    .First(m => m.Name == "Contains" && m.GetParameters().Length == 1);

                // Create the expression call to Enumerable.Contains
                return Expression.Call(value, containsMethod, property);
            }            
        }
    }
}