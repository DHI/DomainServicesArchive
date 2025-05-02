namespace SignalRClient
{
    using System;
    using System.Threading.Tasks;
    using DHI.Services;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.AspNetCore.SignalR.Client;

    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine("Use https? (y/n)");
            var protocol = Console.ReadLine() == "y" ? "https" : "http";
            Console.WriteLine("Enter hub port number...");
            var portNumber = Console.ReadLine();
            var url = $"{protocol}://localhost:{portNumber}/notificationhub";
            var connection = new HubConnectionBuilder()
                .WithUrl(url, options => { options.AccessTokenProvider = GetAccessToken; })
                .WithAutomaticReconnect()
                .Build();

            // Jobs
            connection.On<Parameters>("JobAdded", parameters =>
                Console.WriteLine($"Job with ID '{parameters["id"]}' was added by '{parameters.GetParameter("userName", "N/A")}'."));

            connection.On<Parameters>("JobUpdated", parameters =>
                Console.WriteLine($"Job with ID '{parameters["id"]}' was updated by '{parameters.GetParameter("userName", "N/A")}'. Current job: {parameters["data"]}'."));

            // Time Series
            connection.On<Parameters>("TimeSeriesValuesSet", parameters =>
            {
                var id = parameters["id"];
                var userName = parameters.GetParameter("userName", "N/A");
                Console.WriteLine($"The values of time series with ID '{id}' was modified by '{userName}'.");
            });

            connection.On<Parameters>("TimeSeriesAdded", parameters =>
                Console.WriteLine($"Time series with ID '{parameters["id"]}' was added by '{parameters.GetParameter("userName", "N/A")}'."));

            connection.On<Parameters>("TimeSeriesUpdated", parameters =>
                Console.WriteLine($"Time series with ID '{parameters["id"]}' was updated by '{parameters.GetParameter("userName", "N/A")}'."));

            connection.On<Parameters>("TimeSeriesDeleted", parameters =>
                Console.WriteLine($"Time series with ID '{parameters["id"]}' was deleted by '{parameters.GetParameter("userName", "N/A")}'."));

            // Json Documents
            connection.On<Parameters>("JoinedGroup", parameters =>
                Console.WriteLine($"Connection '{parameters["connectionId"]}' (user '{parameters["userName"]}') joined group '{parameters["groupName"]}'. "));

            connection.On<Parameters>("JsonDocumentAdded", parameters =>
                Console.WriteLine($"JSON Document with ID '{parameters["id"]}' was added by '{parameters.GetParameter("userName", "N/A")}'."));

            connection.On<Parameters>("JsonDocumentUpdated", parameters =>
                Console.WriteLine($"JSON Document with ID '{parameters["id"]}' was updated by '{parameters.GetParameter("userName", "N/A")}'."));

            connection.On<Parameters>("JsonDocumentDeleted", parameters =>
                Console.WriteLine($"JSON Document with ID '{parameters["id"]}' was deleted by '{parameters.GetParameter("userName", "N/A")}'."));

            try
            {
                connection.StartAsync().GetAwaiter().GetResult();
                var queryConditions1 = new[]
                {
                    new { Item = "foo", QueryOperator = "Equal", Value = "bar" },
                };

                connection.InvokeAsync("AddJsonDocumentFilter", "fake", queryConditions1).GetAwaiter().GetResult();

                var queryConditions2 = new[]
                {
                    new { Item = "Data", QueryOperator = "Equal", Value = "{ \"message\": \"Hello World\" }" }
                };

                connection.InvokeAsync("AddJsonDocumentFilter", "fake", queryConditions2).GetAwaiter().GetResult();

                var queryConditions3 = new[]
                {
                    new { Item = "Priority", QueryOperator = "GreaterThan", Value = "1"  }
                };

                connection.InvokeAsync("AddJobFilter", "wf-jobs", queryConditions3).GetAwaiter().GetResult();
            }
            catch (HubException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                Environment.Exit(0);
            }
            
            Console.WriteLine($"Listening on {url}. Press any key to quit...");
            Console.ReadKey();
        }

        private static Task<string> GetAccessToken()
        {
            //return Task.FromResult(Environment.GetEnvironmentVariable("JwtSignalRHubDemo"));
            return Task.FromResult(Environment.GetEnvironmentVariable("JwtSignalRHubNew"));
            //return Task.FromResult(Environment.GetEnvironmentVariable("JwtSignalRHub"));
        }
    }
}