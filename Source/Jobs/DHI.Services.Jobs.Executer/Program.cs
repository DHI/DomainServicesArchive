namespace DHI.Services.Jobs.Executer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

internal class Program
{
    private static ILogger _logger;

    private static int Main(string[] args)
    {
        _logger = ConfigureLogger();
        if (!TryGetArguments(args, out var taskId, out var connectionId, out var username, out var password, out var url,
                out var urlAuth, out var hostGroup, out var tag, out var priority, out var parameters, out var responseFile))
        {
            return 1;
        }

        var client = new HttpClient();
        var retryPolicy = new HttpFailureRetryPolicy(_logger);
        var token = TryGetBearerToken(client, retryPolicy, urlAuth!, username!, password!);
        if (token is null)
        {
            return 1;
        }

        try
        {
            var jobDto = new JobDto(taskId!)
            {
                HostGroup = hostGroup,
                Priority = priority,
                Tag = tag,
                Parameters = parameters
            };

            _logger.LogInformation("Adding job on: {Url}api/jobs/{ConnectionId}", url, connectionId);
            retryPolicy = new HttpFailureRetryPolicy(_logger);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = retryPolicy.ExecuteAsync(() => client.PostAsync($"{url}api/jobs/{connectionId}", GetStringContent(jobDto))).GetAwaiter().GetResult();

            if (responseFile != string.Empty)
            {
                var content = response.Content.ReadAsStringAsync();
                content.Wait();
                string jsonResponse = content.Result;
                File.WriteAllText(responseFile, jsonResponse);
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogCritical("Failed adding job on: {Url}api/jobs/{ConnectionId}", url, connectionId);
                return 1;
            }

            _logger.LogInformation("Successfully added job on: {Url}api/jobs/{ConnectionId}", url, connectionId);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed adding job.");
            return 1;
        }

        return 0;
    }

    public static StringContent GetStringContent(object obj)
    {
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var body = JsonSerializer.Serialize(obj, options);
        return new StringContent(body, Encoding.UTF8, "application/json");
    }

    private static void ShowHelp()
    {
        var exeName = AppDomain.CurrentDomain.FriendlyName;
        Console.WriteLine("Usage:");
        Console.WriteLine();
        Console.WriteLine($"{exeName} -username <user> -password <password> -url <url> -connectionId <connectionId> -taskId <taskId>");
        Console.WriteLine();
        Console.WriteLine("Optional arguments:");
        Console.WriteLine();
        Console.WriteLine("-urlAuth (use if an authorization server is configured on a separate url.)");
        Console.WriteLine("-parametersFileName (the parametersFileName is a serialized dictionary e.g. {\"MyKey1\": \"MyValue1\", \"MyKey2\": \"MyValue2\"})");
        Console.WriteLine("-parameters (parameters are semicolon (';') separated pairs of <parameter>:<value>. If parameters are given, parametersFileName is ignored.)");
        Console.WriteLine("-hostGroup (if host groups are defined, this will force execution on one of the hosts in the given group.)");
        Console.WriteLine("-priority (priority is an integer. A low number, e.g. 1, is equivalent to a high priority.) ");
        Console.WriteLine("-tag (a tag that is added to the job execution)");
        Console.WriteLine("-responseFileName (the responseFileName define path to store the json response of the job execution)");
        Console.WriteLine();
        Console.WriteLine("Example:");
        Console.WriteLine();
        Console.WriteLine($"{exeName} -username %user% -password %password% -url https://localhost:17510/ -connectionId wf-job -taskId myWorkflow ");
    }

    private static string? GetBasePath()
    {
        using var processModule = Process.GetCurrentProcess().MainModule;
        return Path.GetDirectoryName(processModule?.FileName);
    }

    private static ILogger ConfigureLogger()
    {
        ILogger logger;
        IConfiguration configuration = new ConfigurationBuilder().SetBasePath(GetBasePath()).AddJsonFile("appsettings.json", false, true).Build();
        var connectionString = configuration.GetValue("LoggerConnectionString", "[Path]DHI.Services.Jobs.Executer.log");
        connectionString = connectionString.Replace("[Path]", Path.GetDirectoryName(Uri.UnescapeDataString(new Uri(Assembly.GetExecutingAssembly().Location).AbsolutePath)) + @"\");
        var loggerType = configuration.GetValue("LoggerType", "DHI.Services.Logging.SimpleLogger, DHI.Services");
        try
        {
            logger = (ILogger)Activator.CreateInstance(Type.GetType(loggerType)!, connectionString)!;
        }
        catch
        {
            Console.WriteLine($"Cannot create logger of type {loggerType}. Defaulting to {nameof(SimpleLogger)}");
            logger = new SimpleLogger(connectionString);
        }

        return logger;
    }

    private static string? TryGetBearerToken(HttpClient client, HttpFailureRetryPolicy retryPolicy, string urlAuth, string username, string password)
    {
        _logger.LogInformation("Fetching bearer token from {UrlAuth}.", urlAuth);
        var response = retryPolicy.ExecuteAsync(async () => await client.PostAsync($"{urlAuth}api/tokens", GetStringContent(new { Id = username, Password = password }))).GetAwaiter().GetResult();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogCritical($"Could not fetch bearer token from {urlAuth}.");
            return null;
        }

        var json = response.Content.ReadAsStringAsync().Result;

        try
        {
            var token = JsonDocument.Parse(json).RootElement.GetProperty("accessToken").GetProperty("token").GetString();
            _logger.LogInformation("Successfully fetched bearer token.");
            return token;
        }
        catch // swallow to not leak sensitive info
        {
            _logger.LogError("Could not parse bearer token from response.");
            return null;
        }
    }

    private static bool TryGetArguments(
        string[] args,
        out string? taskId,
        out string? connectionId,
        out string? username,
        out string? password,
        out string? url,
        out string? urlAuth,
        out string? hostGroup,
        out string? tag,
        out int? priority,
        out Dictionary<string, object>? taskParameters,
        out string responseFile)
    {
        taskId = connectionId = username = password = url = urlAuth = hostGroup = tag = null;
        priority = null;
        taskParameters = null;
        responseFile = string.Empty;

        var parser = new CommandLineParser(args);

        if ((taskId = parser["taskId"]) is null)
        {
            ShowHelp();
            return false;
        }

        if ((connectionId = parser["connectionId"]) == null)
        {
            Console.WriteLine("Please specify argument connectionId.");
            ShowHelp();
            return false;
        }

        var variablesOverrideJson = File.Exists("VariablesOverride.json") ? File.ReadAllText("VariablesOverride.json") : "{}";
        var variablesOverride = JsonSerializer.Deserialize<Dictionary<string, object>>(variablesOverrideJson, new JsonSerializerOptions()) ?? new Dictionary<string, object>();

        username = parser["userName"];
        if (variablesOverride.ContainsKey("AuthUserName"))
        {
            username = variablesOverride["AuthUserName"].ToString();
        }

        if (username == null)
        {
            Console.WriteLine("Please specify argument username.");
            ShowHelp();
            return false;
        }

        password = parser["password"];
        if (variablesOverride.ContainsKey("AuthPassword"))
        {
            password = variablesOverride["AuthPassword"].ToString();
        }

        if (password == null)
        {
            Console.WriteLine("Please specify argument password.");
            ShowHelp();
            return false;
        }

        if ((url = parser["url"]) == null)
        {
            Console.WriteLine("Please specify argument url.");
            ShowHelp();
            return false;
        }

        url = url.TrimEnd('/') + "/";

        if (parser["urlAuth"] != null)
        {
            urlAuth = parser["urlAuth"]!.TrimEnd('/') + "/";
        }

        if (variablesOverride.ContainsKey("UrlAuth"))
        {
            urlAuth = variablesOverride["UrlAuth"].ToString()!.TrimEnd('/') + "/";
        }

        hostGroup = parser["hostGroup"];

        if (int.TryParse(parser["priority"], out var p))
        {
            priority = p;
        }

        var parametersFileName = parser["parametersFileName"];
        taskParameters = null;
        if (!string.IsNullOrEmpty(parametersFileName) && File.Exists(parametersFileName))
        {
            var parametersJson = File.ReadAllText(parametersFileName);
            taskParameters = JsonSerializer.Deserialize<Dictionary<string, object>>(parametersJson, new JsonSerializerOptions());
        }
        else
        {
            throw new Exception("Missing parametersFile: " + parametersFileName);
        }

        var parameters = parser["parameters"];
        if (!string.IsNullOrEmpty(parameters))
        {
            taskParameters = parameters.Split(';').ToDictionary<string, string, object>(keyValue => keyValue.Split(':')[0], keyValue => keyValue.Split(':')[1]);
        }

        tag = parser["tag"];

        var responseFilePath = parser["responseFileName"];

        if (!string.IsNullOrEmpty(responseFilePath))
        {
            responseFile = responseFilePath;
        }

        return true;
    }
}