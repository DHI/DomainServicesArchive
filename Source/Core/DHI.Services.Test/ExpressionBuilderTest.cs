namespace DHI.Services.Test
{
    using System;
    using System.Collections.Generic;
    using Xunit;
    using Accounts;

    public class ExpressionBuilderTest
    {
        [Fact]
        public void FilterNullThrows()
        {
            Assert.Throws<ArgumentNullException>(() => ExpressionBuilder.Build<Account>(null));
        }

        [Fact]
        public void FilterEmptyThrows()
        {
            Assert.Throws<ArgumentException>(() => ExpressionBuilder.Build<Account>(new List<QueryCondition>()));
        }

        [Fact]
        public void NonExistingPropertyThrows()
        {
            var filter = new List<QueryCondition> { new QueryCondition("NonExistingProperty", QueryOperator.Equal, "value") };

            Assert.Throws<ArgumentException>(() => ExpressionBuilder.Build<Account>(filter));
        }

        [Fact]
        public void IllegalValueTypeThrows()
        {
            var filter = new List<QueryCondition> { new QueryCondition("Email", QueryOperator.Equal, 99) };

            Assert.Throws<InvalidOperationException>(() => ExpressionBuilder.Build<Account>(filter));
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
            var filter = new List<QueryCondition> { new QueryCondition("Name", queryOperator, "John Doe") };

            Assert.Throws<NotImplementedException>(() => ExpressionBuilder.Build<Account>(filter));
        }

        [Fact]
        public void BuildIsOk()
        {
            var filter = new List<QueryCondition>
            {
                new QueryCondition("Id", QueryOperator.Equal, "john.doe"),
                new QueryCondition("Activated", QueryOperator.Equal, true)
            };

            var expression = ExpressionBuilder.Build<Account>(filter);

            Assert.Equal(typeof(Func<Account, bool>), expression.Type);
        }

        [Fact]
        public void BuildAnyForListIsOk()
        {
            var filter = new List<QueryCondition>
            {
                new QueryCondition("Id", QueryOperator.Any, new List<int>{ 1, 2, 3 })
            };

            var expression = ExpressionBuilder.Build<TestEntity<int>>(filter);

            Assert.Equal(typeof(Func<TestEntity<int>, bool>), expression.Type);
        }

        [Fact]
        public void InvokeAnyForListIsOk()
        {
            var filter = new List<QueryCondition>
            {
                new QueryCondition("Id", QueryOperator.Any, new List<int>{ 1, 2, 3 })
            };

            var expression = ExpressionBuilder.Build<TestEntity<int>>(filter);

            Assert.True(expression.Compile().Invoke(new TestEntity<int> { Id = 1 }));
        }

        [Fact]
        public void InvokeAnyNotFoundForListIsOk()
        {
            var filter = new List<QueryCondition>
            {
                new QueryCondition("Id", QueryOperator.Any, new List<int>{ 1, 2, 3 })
            };

            var expression = ExpressionBuilder.Build<TestEntity<int>>(filter);

            Assert.True(expression.Compile().Invoke(new TestEntity<int> { Id = 1 }));
        }

        [Fact]
        public void BuildAnyForArrayIsOk()
        {
            var filter = new List<QueryCondition>
            {
                new QueryCondition("Id", QueryOperator.Any, new int[]{ 1, 2, 3 })
            };

            var expression = ExpressionBuilder.Build<TestEntity<int>>(filter);

            Assert.Equal(typeof(Func<TestEntity<int>, bool>), expression.Type);
        }

        [Fact]
        public void InvokeAnyForArrayIsOk()
        {
            var filter = new List<QueryCondition>
            {
                new QueryCondition("Id", QueryOperator.Any, new int[]{ 1, 2, 3 })
            };

            var expression = ExpressionBuilder.Build<TestEntity<int>>(filter);

            Assert.True(expression.Compile().Invoke(new TestEntity<int> { Id = 1 }));
        }

        [Fact]
        public void InvokeNotFoundAnyForArrayIsOk()
        {
            var filter = new List<QueryCondition>
            {
                new QueryCondition("Id", QueryOperator.Any, new int[]{ 1, 2, 3 })
            };

            var expression = ExpressionBuilder.Build<TestEntity<int>>(filter);

            Assert.False(expression.Compile().Invoke(new TestEntity<int> { Id = 4 }));
        }

        private class TestEntity<T> 
        {
            public T Id { get; set; }
        }
    }
}