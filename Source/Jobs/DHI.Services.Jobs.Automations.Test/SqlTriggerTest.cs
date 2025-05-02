namespace DHI.Services.Jobs.Automations.Test;

using Logging;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Triggers;
using Xunit;
using Xunit.Abstractions;

public class SqlTriggerTest : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ILogger _logger;

    public SqlTriggerTest(ITestOutputHelper testOutputHelper)
    {
        _logger = new TestLogger(testOutputHelper);
        var dbFile = Path.GetFullPath("../../../Data/bookstore.db");
        _connection = new SqliteConnection($"Data Source={dbFile}");
    }

    [Fact]
    public void SimpleSingleQueryMeetsCriteriaReturnsTrue()
    {
        var trigger = new SqlTrigger(_connection, "id", "description")
        {
            Queries = new[] { "SELECT 1" }
        };

        var result = trigger.Execute(_logger);

        Assert.True(result.IsMet);
        var outputParameter = Assert.Single(result.TaskParameters);
        Assert.Equal("1", outputParameter.Key);
        Assert.Equal("1", outputParameter.Value);
    }

    [Fact]
    public void SimpleMultipleQueryMeetsCriteriaReturnsTrue()
    {
        var trigger = new SqlTrigger(_connection, "id", "description")
        {
            Queries = new[] { "SELECT 1", "SELECT 1" }
        };

        var result = trigger.Execute(_logger);

        Assert.True(result.IsMet);
        var outputParameter = Assert.Single(result.TaskParameters);
        Assert.Equal("1", outputParameter.Key);
        Assert.Equal("1", outputParameter.Value);
    }

    [Fact]
    public void SingleQueryMeetsCriteriaReturnsTrue()
    {
        var trigger = new SqlTrigger(_connection, "id", "description")
        {
            Queries = new[]
            {
                @"select title, filesize, prod_year, price, pub_id
                  from Book
                  WHERE prod_year < 2000
                  ORDER BY filesize desc
                  limit 5;"
            }
        };

        var result = trigger.Execute(_logger);
        Assert.True(result.IsMet);
        Assert.Equal(5, result.TaskParameters.Count);
        Assert.Equal("Message in a Bottle", result.TaskParameters["title"]);
        Assert.Equal("1999", result.TaskParameters["prod_year"]);
        Assert.Equal("9706", result.TaskParameters["filesize"]);
        Assert.Equal("7.5", result.TaskParameters["price"]);
        Assert.Equal("35", result.TaskParameters["pub_id"]);
    }

    [Fact]
    public void MultipleQueryMeetsCriteriaReturnsTrue()
    {
        var trigger = new SqlTrigger(_connection, "id", "description")
        {
            Queries = new[]
            {
                @"select id as pub_id 
                  from Publisher
                  WHERE id == @automation_parameter;",

                @"select title, prod_year, pub_id
                  from Book
                  WHERE prod_year > @always_available AND pub_id == @pub_id
                  ORDER BY filesize desc;",
            }
        };

        var parameters = new Dictionary<string, string>
        {
            ["automation_parameter"] = "43",
            ["always_available"] = "2000"
        };
        var result = trigger.Execute(_logger, parameters);
        Assert.True(result.IsMet);
        Assert.Equal(3, result.TaskParameters.Count);

        Assert.Equal("The Data Warehouse Toolkit: The Complete Guide to Dimensional Modeling", result.TaskParameters["title"]);
        Assert.Equal("2002", result.TaskParameters["prod_year"]);
        Assert.Equal("43", result.TaskParameters["pub_id"]);
    }

    [Fact]
    public void SingleQueryReturnsFalse()
    {
        var trigger = new SqlTrigger(_connection, "id", "description")
        {
            Queries = new[]
            {
                @"select title, filesize, prod_year, price, pub_id
                  from Book
                  WHERE prod_year < 1000;"
            }
        };

        var result = trigger.Execute(_logger);
        Assert.False(result.IsMet);
    }

    [Fact]
    public void MultiQueryReturnsFalseOnFirst()
    {
        var trigger = new SqlTrigger(_connection, "id", "description")
        {
            Queries = new[]
            {
                @"select id as pub_id 
                  from Publisher
                  WHERE id == -1",

                @"select title, prod_year, pub_id
                  from Book
                  ORDER BY filesize desc;",
            }
        };

        var result = trigger.Execute(_logger);
        Assert.False(result.IsMet);
    }

    [Fact]
    public void MultiQueryReturnsFalseOnSecond()
    {
        var trigger = new SqlTrigger(_connection, "id", "description")
        {
            Queries = new[]
            {
                @"select id as pub_id 
                  from Publisher
                  WHERE id == 43",

                @"select title, prod_year, pub_id
                  from Book
                  WHERE prod_year < 1000
                  ORDER BY filesize desc;",
            }
        };

        var result = trigger.Execute(_logger);
        Assert.False(result.IsMet);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}