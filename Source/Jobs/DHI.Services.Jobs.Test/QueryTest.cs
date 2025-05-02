namespace DHI.Services.Jobs.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Jobs;
    using Xunit;

    public class QueryTest
    {
        [Fact]
        public void ToExpressionForEmptyConditionsThrows()
        {
            var query = new Query<FakeEntity>();
            Assert.Throws<ArgumentException>(() => query.ToExpression());
        }

        [Fact]
        public void ToExpressionForNonExistingPropertyThrows()
        {
            var conditions = new List<QueryCondition> { new QueryCondition("NonExistingProperty", QueryOperator.Equal, "value") };
            var query = new Query<FakeEntity>(conditions);

            Assert.Throws<ArgumentException>(() => query.ToExpression());
        }

        [Fact]
        public void ToExpressionForIllegalValueTypeThrows()
        {
            var conditions = new List<QueryCondition> { new QueryCondition("TaskID", QueryOperator.Equal, 99) };
            var query = new Query<Job>(conditions);

            Assert.Throws<InvalidOperationException>(() => query.ToExpression());
        }

        [Theory]
        [InlineData(QueryOperator.Contains)]
        [InlineData(QueryOperator.Intersects)]
        [InlineData(QueryOperator.Like)]
        [InlineData(QueryOperator.NotLike)]
        [InlineData(QueryOperator.SpatiallyContains)]
        [InlineData(QueryOperator.SpatiallyIntersects)]
        [InlineData(QueryOperator.SpatiallyWithin)]
        [InlineData(QueryOperator.SpatiallyWithinDistance)]
        public void ToExpressionForNotSupportedOperatorThrows(QueryOperator queryOperator)
        {
            var conditions = new List<QueryCondition> { new QueryCondition("TaskID", queryOperator, "MyTask") };
            var query = new Query<Job>(conditions);

            Assert.Throws<NotImplementedException>(() => query.ToExpression());
        }

        [Fact]
        public void ToExpressionIsOk()
        {
            var conditions = new List<QueryCondition>
            {
                new QueryCondition("TaskID", QueryOperator.Equal, "MyTask"),
                new QueryCondition("Requested", QueryOperator.GreaterThan, DateTime.Now.AddDays(-1))
            };

            var query = new Query<Job>(conditions);
            var expression = query.ToExpression();

            Assert.StartsWith("t => ((t.TaskId == \"MyTask\") AndAlso (t.Requested > ", expression.ToString());
            Assert.Equal(typeof(bool), expression.ReturnType);
        }

        [Fact]
        public void GetEnumeratorIsOk()
        {
            var query = new Query<Job>
            {
                new QueryCondition("TaskID", QueryOperator.Equal, "MyTask"),
                new QueryCondition("Requested", QueryOperator.GreaterThan, DateTime.Now.AddDays(-1))
            };

            Assert.Equal(2, query.Count());
        }

        [Fact]
        public void ToStringIsOk()
        {
            var conditions = new List<QueryCondition>
            {
                new QueryCondition("TaskID", QueryOperator.Equal, "MyTask"),
                new QueryCondition("Requested", QueryOperator.GreaterThan, DateTime.Now.AddDays(-1))
            };

            var query = new Query<Job>(conditions);

            Assert.IsType<string>(query.ToString());
        }
    }
}