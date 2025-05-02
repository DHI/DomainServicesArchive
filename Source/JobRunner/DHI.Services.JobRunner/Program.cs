namespace DHI.Services.JobRunner
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.ServiceProcess;
    using System.Threading.Tasks;
    using Jobs;
    using Jobs.Workflows;
    using Logging;
    using Properties;
    using Scalars;

    internal static class Program
    {
        private static ILogger _logger;

        private static void Main()
        {
            if (Settings.Default.Debug)
            {
                Debugger.Launch();
            }

            var loggerConnectionString = Settings.Default.LoggerConnectionString.Resolve();
            _logger = (ILogger)Activator.CreateInstance(Type.GetType(Settings.Default.LoggerType), loggerConnectionString);

            GroupedScalarService<string, int> scalarService = null;
            if (Settings.Default.EnableScalarService)
            {
                var scalarRepositoryConnectionString = Settings.Default.ScalarRepositoryConnectionString.Resolve();
                var scalarRepository = (IGroupedScalarRepository<string, int>)Activator.CreateInstance(Type.GetType(Settings.Default.ScalarRepositoryType), scalarRepositoryConnectionString);
                scalarService = Settings.Default.ScalarServiceLogging ? new GroupedScalarService<string, int>(scalarRepository, _logger) : new GroupedScalarService<string, int>(scalarRepository);
            }

            try
            {
                var connectionsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "worker-connections.json");
                if (!File.Exists(connectionsPath))
                {
                    throw new FileNotFoundException($"File not found. '{connectionsPath}'");
                }
                var connections = new ConnectionService(new ConnectionRepository(connectionsPath));
                var jobWorkerConnections = connections.GetAll().Where(c => c.GetType().IsAssignableFrom(typeof (JobWorkerConnection)));
                var jobWorkers = new List<JobWorker<Workflow, string>>();
                var jobServices = new Dictionary<string, JobService<Workflow, string>>();
                foreach (var connection in jobWorkerConnections)
                {
                    var jobWorkerConnection = (JobWorkerConnection)connection;
                    jobServices.Add(jobWorkerConnection.Id, jobWorkerConnection.CreateJobService());
                    JobWorker<Workflow, string> jobWorker;
                    if (Settings.Default.VerboseLogging)
                    {
                        jobWorker = (JobWorker<Workflow, string>)jobWorkerConnection.Create(_logger);
                    }
                    else
                    {
                        jobWorker = (JobWorker<Workflow, string>)jobWorkerConnection.Create();
                    }

                    jobWorker.Executing += JobWorker_Executing;
                    jobWorker.Executed += JobWorker_Executed;
                    jobWorker.Cancelling += JobWorker_Cancelling;
                    jobWorker.Cancelled += JobWorker_Cancelled;
                    jobWorker.Interrupted += JobWorker_Interrupted;
                    jobWorkers.Add(jobWorker);
                    Task.Run(() => jobWorker.Clean());
                }

                var servicesToRun = new ServiceBase[]
                {
                    Settings.Default.EnableScalarService ? new JobExecutionService(jobWorkers, _logger, scalarService, jobServices) : new JobExecutionService(jobWorkers, _logger)
                };

                ServiceBase.Run(servicesToRun);
            }
            catch (Exception ex)
            {
                _logger.Log(new LogEntry(LogLevel.Critical, ex.ToString(), "Job Runner Service"));
                throw;
            }
        }

        private static void JobWorker_Executed(object sender, EventArgs<Tuple<Guid, JobStatus, string, string>> e)
        {
            _logger.Log(new LogEntry(e.Item.Item2 == JobStatus.Error ? LogLevel.Error : LogLevel.Information, $"Job worker '{((JobWorker<Workflow, string>)sender).Id}' executed task '{e.Item.Item3}' with job ID '{e.Item.Item1}'. Status '{e.Item.Item2}'." + (e.Item.Item2 == JobStatus.Error ? $" Message: {e.Item.Item4}" : string.Empty), "Job Runner Service"));
        }

        private static void JobWorker_Executing(object sender, EventArgs<Job<Guid, string>> e)
        {
            _logger.Log(new LogEntry(LogLevel.Information, $"Job worker '{((JobWorker<Workflow, string>)sender).Id}' executing task '{e.Item.TaskId}' on host '{e.Item.HostId}' with job ID '{e.Item.Id} for account '{e.Item.AccountId}'...", "Job Runner Service"));
        }

        private static void JobWorker_Cancelled(object sender, EventArgs<Tuple<Guid, string>> e)
        {
            _logger.Log(new LogEntry(LogLevel.Information, $"Job worker '{((JobWorker<Workflow, string>)sender).Id}' cancelled job '{e.Item.Item1}'." + (e.Item.Item2 != null ? $" Message: {e.Item.Item2}" : string.Empty), "Job Runner Service"));
        }

        private static void JobWorker_Cancelling(object sender, EventArgs<Job<Guid, string>> e)
        {
            _logger.Log(new LogEntry(LogLevel.Information, $"Job worker '{((JobWorker<Workflow, string>)sender).Id}' cancelling job '{e.Item.Id} on host '{e.Item.HostId}'...", "Job Runner Service"));
        }
        private static void JobWorker_Interrupted(object sender, EventArgs<Guid> e)
        {
            _logger.Log(new LogEntry(LogLevel.Warning, $"Job worker '{((JobWorker<Workflow, string>)sender).Id}' interrupted while executing job ID '{e.Item}'", "Job Runner Service"));
        }
    }
}