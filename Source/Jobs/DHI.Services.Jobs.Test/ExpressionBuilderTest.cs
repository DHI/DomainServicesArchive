namespace DHI.Services.Jobs.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Jobs;
    using Xunit;

    public class ExpressionBuilderTest
    {
        [Fact]
        public void FilterNullThrows()
        {
            Assert.Throws<ArgumentNullException>(() => ExpressionBuilder.Build<Job>(null));
        }

        [Fact]
        public void FilterEmptyThrows()
        {
            Assert.Throws<ArgumentException>(() => ExpressionBuilder.Build<Job>(new List<QueryCondition>()));
        }

        [Fact]
        public void NonExistingPropertyThrows()
        {
            var filter = new List<QueryCondition>() { new QueryCondition("NonExistingProperty", QueryOperator.Equal, "value") };

            Assert.Throws<ArgumentException>(() => ExpressionBuilder.Build<Job>(filter));
        }

        [Fact]
        public void IllegalValueTypeThrows()
        {
            var filter = new List<QueryCondition>() { new QueryCondition("TaskID", QueryOperator.Equal, 99) };

            Assert.Throws<InvalidOperationException>(() => ExpressionBuilder.Build<Job>(filter));
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
        public void NotSupportedOperatorThrows(QueryOperator queryOperator)
        {
            var filter = new List<QueryCondition>() { new QueryCondition("TaskID", queryOperator, "MyTask") };

            Assert.Throws<NotImplementedException>(() => ExpressionBuilder.Build<Job>(filter));
        }

        [Fact]
        public void BuildIsOk()
        {
            var filter = new List<QueryCondition>()
            {
                new QueryCondition("TaskID", QueryOperator.Equal, "MyTask"),
                new QueryCondition("Requested", QueryOperator.GreaterThan, DateTime.Now.AddDays(-1))
            };

            var expression = ExpressionBuilder.Build<Job>(filter);

            Assert.StartsWith("t => ((t.TaskId == \"MyTask\") AndAlso (t.Requested > ", expression.ToString());
            Assert.Equal(typeof(bool), expression.ReturnType);
        }
    }
}