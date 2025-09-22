namespace DHI.Services.WebApiCore.Test
{
    using System;
    using FluentAssertions;
    using Microsoft.Extensions.Logging.Abstractions;
    using Xunit;

    public class QueryDTOTest
    {
        [Theory]
        [InlineData("Equals")]
        [InlineData(">")]
        [InlineData("=")]
        public void UnknownQueryOperatorThrows(string queryOperator)
        {
            Action toQuery = () => new QueryDTO<LogEntry<string>>
            {
                new QueryConditionDTO
                {
                    Item = "LogLevel",
                    QueryOperator = queryOperator,
                    Value = "Error"
                }
            }.ToQuery();

            toQuery.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void MissingValuePropertyThrows()
        {
            Action toQuery = () => new QueryDTO<LogEntry<string>>
            {
                new QueryConditionDTO
                {
                    Item = "LogLevel",
                    QueryOperator = "Equal",
                    Values = new[] { "Error", "Warning" }
                }
            }.ToQuery();

            toQuery.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void MissingValuesPropertyForQueryOperatorAnyThrows()
        {
            Action toQuery = () => new QueryDTO<LogEntry<string>>
            {
                new QueryConditionDTO
                {
                    Item = "LogLevel",
                    QueryOperator = "Any",
                    Value = "Error"
                }
            }.ToQuery();

            toQuery.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ToQueryIsOk()
        {
            var query = new QueryDTO<LogEntry<string>>
            {
                new QueryConditionDTO
                {
                    Item = "LogLevel",
                    QueryOperator = "GreaterThanOrEqual",
                    Value = "LogLevel.Warning"
                },
                new QueryConditionDTO
                {
                    Item = "Category",
                    QueryOperator = "Equal",
                    Value = "MySource"
                }
            }.ToQuery();

            query.Should().HaveCount(2);
            query.ToString().Should().Be("LogLevel >= Warning AND Category = MySource");
            query.ToExpression().Type.Should().Be(typeof(Func<LogEntry<string>, bool>));
        }

        [Fact]
        public void ToQueryForQueryOperatorAnyIsOk()
        {
            var query = new QueryDTO<LogEntry<string>>
            {
                new QueryConditionDTO
                {
                    Item = "LogLevel",
                    QueryOperator = "Any",
                    Values = new[] { "LogLevel.Error", "LogLevel.Warning" }
                },
                new QueryConditionDTO
                {
                    Item = "Source",
                    QueryOperator = "Equal",
                    Value = "MySource"
                }
            }.ToQuery();

            query.Should().HaveCount(2);
            query.ToString().Should().Be("LogLevel Any (Error OR Warning) AND Source = MySource");
        }
    }
}