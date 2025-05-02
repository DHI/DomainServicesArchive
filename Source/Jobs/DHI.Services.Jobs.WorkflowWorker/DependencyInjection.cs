using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DHI.Services.Jobs.WorkflowWorker
{
    /// <summary>
    /// Utility static class for registering assembly types for dependancy injection.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Register assembly classes in passed service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        public static void Adds(IServiceCollection serviceCollection, ILogger logger)
        {
            serviceCollection.AddSingleton<IUserIdProvider, MachineNameUserIdProvider>();
            serviceCollection.AddSingleton<AvailableCache>();
            serviceCollection.AddSingleton<ReportCache>();
            serviceCollection.AddSingleton<SignalRWorkflowWorker>();
            serviceCollection.AddSingleton<ISignalRHostCollection>(c => logger == null ? new SignalRHostRepository() : new SignalRHostRepository(logger));
            serviceCollection.AddSingleton<IHostRepository, SignalRHostRepository>();
        }
    }
}
