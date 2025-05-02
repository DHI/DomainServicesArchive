namespace DHI.Services.Jobs.Automations.Triggers;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Npgsql;
using TriggerParametersExport;

[Serializable]
public class SqlTrigger : BaseTrigger, ISqlTriggerParameters
{
    private readonly IDbConnection _connection;

    public SqlTrigger(string connectionString, DbmsType dbmsType, string[] queries, string id, string description) : base(id, description)
    {
        ConnectionString = connectionString;
        DbmsType = dbmsType;
        Queries = queries;
    }

    internal SqlTrigger(IDbConnection connection, string id, string description) : base(id, description)
    {
        _connection = connection;
    }

    public string[] Queries { get; set; } = Array.Empty<string>();
    public string ConnectionString { get; set; }
    public DbmsType DbmsType { get; set; }

    public override AutomationResult Execute(ILogger logger, IDictionary<string, string> parameters = null)
    {
        if (!Queries.Any())
        {
            logger.LogWarning("No queries to execute");
            return AutomationResult.NotMet();
        }

        using IDbConnection connection = _connection ?? DbmsType switch
        {
            DbmsType.Postgres => new NpgsqlConnection(ConnectionString),
            DbmsType.AzureSql or DbmsType.SqlServer => new SqlConnection(ConnectionString),
            DbmsType.SqLite => new SqliteConnection(ConnectionString),
            DbmsType.MySQL => new MySqlConnection(ConnectionString),
            DbmsType.Undefined or _ => throw new ArgumentOutOfRangeException(nameof(DbmsType), DbmsType, "DbmsType not supported, currently only Postgres, AzureSql and SqlServer are supported")
        };

        connection.Open();

        var automationParameters = parameters?.ToDictionary(k => k.Key, v => (object)v.Value)
                                   ?? new Dictionary<string, object>();

        logger.LogDebug("Executing 1st query=\"{Query}\" with parameters=\"{Parameters}\"", Queries[0], JsonSerializer.Serialize(parameters));

        var firstQueryResults = connection.Query(Queries[0], automationParameters).Select(rw => rw as IDictionary<string, object>).ToList();
        if (firstQueryResults.Count == 0)
        {
            logger.LogWarning("First SQL Statement returned no rows");
            return AutomationResult.NotMet();
        }

        logger.LogInformation("First SQL Statement returned rows {Count}", firstQueryResults.Count);

        if (Queries.Length == 1)
        {
            logger.LogInformation("Only one SQL Statement, returning first row");
            var result = firstQueryResults.First().ToDictionary(k => k.Key, v => v.Value.ToString());
            return AutomationResult.Met(result);
        }

        var requestQueue = new Stack<(int QueryIndex, IDictionary<string, object> Paramerters)>();
        for (int rowIndex = firstQueryResults.Count - 1; rowIndex >= 0; rowIndex--)
        {
            var row = firstQueryResults[rowIndex];
            requestQueue.Push((1, row));
        }

        Dictionary<string, string> finalQueryResults = null;

        while (requestQueue.Count > 0)
        {
            var (queryIndex, rowParameters) = requestQueue.Pop();
            var result = ExecuteQuery(connection, Queries[queryIndex], rowParameters, automationParameters, logger);
            if (result is null || result.Count == 0)
            {
                continue;
            }

            if (queryIndex == Queries.Length - 1)
            {
                finalQueryResults = result.First().ToDictionary(k => k.Key, v => v.Value.ToString());

                logger.LogInformation("Final query met");

                break;
            }

            for (var rowIndex = result.Count - 1; rowIndex >= 0; rowIndex--)
            {
                var row = result[rowIndex];
                requestQueue.Push((queryIndex + 1, row));
            }
        }

        return finalQueryResults is not null && finalQueryResults.Count > 0
            ? AutomationResult.Met(finalQueryResults)
            : AutomationResult.NotMet();
    }

    private List<IDictionary<string, object>> ExecuteQuery(IDbConnection connection, string query, IDictionary<string, object> previousQueryResultRow, IDictionary<string, object> automationParameters, ILogger logger)
    {
        var results = new List<IDictionary<string, object>>();
        var parameters = previousQueryResultRow;
        foreach (var parameter in automationParameters)
        {
            parameters[parameter.Key] = parameter.Value;
        }


        logger.LogDebug("Executing query=\"{Query}\" with parameters=\"{Parameters}\"", query, JsonSerializer.Serialize(parameters));
        var result = connection.Query(query, parameters).Select(rw => rw as IDictionary<string, object>).ToList();

        logger.LogDebug("Query resulted in rows=\"{Result}\"", JsonSerializer.Serialize(result));

        results.AddRange(result);

        return results;
    }
}
