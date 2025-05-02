namespace DHI.Services.Jobs.Test
{
    using System;
    using System.Linq;
    using Xunit;

    public class BuildQueryTest
    {
        [Fact]
        public void IsOk()
        {
            var query = JobService<FakeTask<string>, string>.BuildQuery(status: JobStatus.Completed, since: DateTime.Now.AddDays(-1));
            Assert.Equal(2, query.Count());
            var condition = query.ElementAt(0);
            Assert.Equal("Requested", condition.Item);
            Assert.Equal(QueryOperator.GreaterThanOrEqual, condition.QueryOperator);
            Assert.IsType<DateTime>(condition.Value);
            condition = query.ElementAt(1);
            Assert.Equal("Status", condition.Item);
            Assert.Equal(QueryOperator.Equal, condition.QueryOperator);
            Assert.Equal(JobStatus.Completed, condition.Value);
        }
    }
}