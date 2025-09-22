namespace DHI.Services.WebApiCore.Test
{
    using System;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Xunit;

    public class QueryConditionDTOTest
    {
        [Theory]
        [InlineData("Equals")]
        [InlineData(">")]
        [InlineData("=")]
        public void UnknownQueryOperatorThrows(string queryOperator)
        {
            Action toQueryCondition = () => new QueryConditionDTO
            {
                Item = "datetime",
                QueryOperator = queryOperator,
                Value = "2019"
            }.ToQueryCondition();

            toQueryCondition.Should().Throw<ArgumentException>().WithMessage("Could not parse query operator*");
        }

        [Fact]
        public void MissingValuePropertyThrows()
        {
            Action toQueryCondition = () => new QueryConditionDTO
            {
                Item = "datetime",
                QueryOperator = "GreaterThan",
                Values = new[] { "2019", "2018" }
            }.ToQueryCondition();

            toQueryCondition.Should().Throw<ArgumentException>().WithMessage("No 'Value' was defined.*");
        }

        [Fact]
        public void MissingValuesPropertyForQueryOperatorAnyThrows()
        {
            Action toQueryCondition = () => new QueryConditionDTO
            {
                Item = "datetime",
                QueryOperator = "Any",
                Value = "2019"
            }.ToQueryCondition();

            toQueryCondition.Should().Throw<ArgumentException>()
                .WithMessage("The 'Any' query operator requires an array of string values to be set in 'Values'.*");
        }

        [Fact]
        public void ToQueryConditionIsOk()
        {
            var queryCondition = new QueryConditionDTO
            {
                Item = "datetime",
                QueryOperator = "GreaterThan",
                Value = "2019-1-1"
            }.ToQueryCondition();

            queryCondition.QueryOperator.Should().Be(QueryOperator.GreaterThan);
            queryCondition.Item.Should().Be("datetime");
            queryCondition.Value.Should().Be(new DateTime(2019, 1, 1));

            queryCondition = new QueryConditionDTO
            {
                Item = "LogLevel",
                QueryOperator = "Equal",
                Value = "LogLevel.Warning"
            }.ToQueryCondition();

            queryCondition.QueryOperator.Should().Be(QueryOperator.Equal);
            queryCondition.Item.Should().Be("LogLevel");
            queryCondition.Value.Should().Be(LogLevel.Warning);
        }

        [Fact]
        public void ToQueryConditionForQueryOperatorAnyIsOk()
        {
            var queryCondition = new QueryConditionDTO
            {
                Item = "LogLevel",
                QueryOperator = "Any",
                Values = new[] { "LogLevel.Warning", "LogLevel.Error" }
            }.ToQueryCondition();

            queryCondition.QueryOperator.Should().Be(QueryOperator.Any);
            queryCondition.Item.Should().Be("LogLevel");
            queryCondition.Value.Should().BeOfType<object[]>();
            ((object[])queryCondition.Value).Should().HaveCount(2);
            ((object[])queryCondition.Value).Should().Contain(LogLevel.Warning);
            ((object[])queryCondition.Value).Should().Contain(LogLevel.Error);
        }
    }
}