using DHI.Services.Jobs;
using DHI.Services.Jobs.Automations;
using DHI.Services.Jobs.Workflows;
using DHI.Services.Provider.DS;
using DHI.Services.Scalars;
using JobAutomator.Caching;
using JobAutomator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace JobAutomator;

/// <summary>
/// Factory Method Interface
/// </summary>
internal interface IDependencyFactory
{
    internal AccessTokenProvider TokenProvider { get; }
    internal JobService<CodeWorkflow, string> GetJobService(string jobsConnectionString);
    internal AutomationService GetAutomationService(string automationConnectionString, string scalarConnectionString, string jobsConnectionString);
    internal ScalarService<int> GetScalarService(string postgresConnectionString);
    internal IJobServiceFactory GetJobServiceFactory();

}

/// <summary>
/// Builds non-local dependencies
/// </summary>
public class DependencyFactory : IDependencyFactory
{
    private readonly ILogger _logger;
    private readonly AccessTokenProvider _tokenProvider;
    private readonly IConfiguration _configuration;

    public DependencyFactory(IConfiguration configuration, ILogger logger)
    {
        _logger = logger;
        _configuration = configuration;
        var authModel = new AuthenticationConfiguration();
        configuration.GetSection("Authentication").Bind(authModel);

        _tokenProvider = new AccessTokenProvider(authModel.Url, authModel.UserName, authModel.Password, logger);
    }

    public AccessTokenProvider TokenProvider => _tokenProvider;

    public JobService<CodeWorkflow, string> GetJobService(string jobsConnectionString)
    {
        var taskRepository = new DHI.Services.Provider.DS.CodeWorkflowRepository($"{jobsConnectionString}/api/tasks/wf-tasks", _tokenProvider, 3, _logger);
        var taskService = new CodeWorkflowService(taskRepository);

        var jobRepository = new DHI.Services.Provider.DS.JobRepository($"{jobsConnectionString}/api/jobs/wf-jobs-Minion", _tokenProvider, 3, _logger);
        return new JobService<CodeWorkflow, string>(jobRepository, taskService);
    }

    public AutomationService GetAutomationService(string automationConnectionString, string scalarConnectionString, string jobsConnectionString)
    {
        var dsRepo = new DHI.Services.Provider.DS.AutomationRepository($"{automationConnectionString}/api/automations", _tokenProvider, 3, _logger);

        var cacheCfg = new CacheSettings();
        _configuration.GetSection("Cache").Bind(cacheCfg);

        var versionEndpoint = _configuration["AutomationApi:VersionEndpoint"] ?? $"{automationConnectionString}/api/automations/version";

        var cachedRepo = new CachingAutomationRepository(dsRepo, cacheCfg, _tokenProvider, versionEndpoint, _logger);

        var scalarRepo = new DHI.Services.Provider.DS.ScalarRepository($"{scalarConnectionString}/api/scalars/wf-scalars", _tokenProvider, 3, _logger);
        var jobRepo = new DHI.Services.Provider.DS.JobRepository($"{jobsConnectionString}/api/jobs/wf-jobs-Minion", _tokenProvider, 3, _logger);

        return new AutomationService(cachedRepo, scalarRepo, jobRepo, _logger);
    }

    public ScalarService<int> GetScalarService(string scalarConnectionString)
    {
        var scalarRepository = new DHI.Services.Provider.DS.ScalarRepository($"{scalarConnectionString}/api/scalars/wf-scalars", _tokenProvider, 3, _logger);
        return new DHI.Services.Provider.PostgreSQL.ScalarService(scalarRepository, _logger);
    }

    public IJobServiceFactory GetJobServiceFactory()
    {
        var jobServiceMap = new Dictionary<string, JobService<CodeWorkflow, string>>();

        var jobVariants = new[] { "wf-jobs-Minion", "wf-jobs-Titan" };

        foreach (var variant in jobVariants)
        {
            var baseUrl = $"{_configuration.GetConnectionString("Jobs")}/api/jobs/{variant}";
            var jobRepo = new DHI.Services.Provider.DS.JobRepository(baseUrl, _tokenProvider, 3, _logger);

            var taskUrl = $"{_configuration.GetConnectionString("Jobs")}/api/tasks/wf-tasks";
            var taskRepo = new DHI.Services.Provider.DS.CodeWorkflowRepository(taskUrl, _tokenProvider, 3, _logger);
            var taskService = new CodeWorkflowService(taskRepo);

            var jobService = new JobService<CodeWorkflow, string>(jobRepo, taskService);

            jobServiceMap[variant.ToLowerInvariant()] = jobService;
        }

        return new JobServiceFactory(jobServiceMap, _logger);
    }

}

public class AccessTokenProvider : IAccessTokenProvider
{
    private readonly string _baseUrl;
    private readonly string _username;
    private readonly string _password;
    private readonly ILogger _logger;
    private string _refreshToken;

    private string _cachedToken;
    private DateTime _expiration;

    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public AccessTokenProvider(string baseUrl, string username, string password, ILogger logger = null)
    {
        _baseUrl = baseUrl?.TrimEnd('/');
        _username = username ?? throw new ArgumentNullException(nameof(username));
        _password = password ?? throw new ArgumentNullException(nameof(password));
        _logger = logger;
    }

    public async Task<string> GetAccessToken()
    {
        return await GetAccessTokenInternal(forceRefresh: false);
    }

    public async Task<string> GetAccessTokenInternal(bool forceRefresh)
    {
        if (!forceRefresh && !string.IsNullOrEmpty(_cachedToken) && _expiration > DateTime.UtcNow.AddMinutes(1))
            return _cachedToken;

        await _refreshLock.WaitAsync();
        try
        {
            if (!forceRefresh && !string.IsNullOrEmpty(_cachedToken) && _expiration > DateTime.UtcNow.AddMinutes(1))
                return _cachedToken;

            var accountRepo = new AccountRepository($"{_baseUrl}/api/accounts", new NullAccessTokenProvider(), 5, _logger);

            TokenResponse tokenResponse;

            if (!string.IsNullOrEmpty(_refreshToken))
            {
                try
                {
                    tokenResponse = await accountRepo.RefreshTokenAsync(_refreshToken);
                    _logger?.LogInformation("Refreshed access token via refresh token.");
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Refresh token failed. Falling back to username/password login.");
                    tokenResponse = await accountRepo.CreateTokenAsync(_username, _password);
                }
            }
            else
            {
                tokenResponse = await accountRepo.CreateTokenAsync(_username, _password);
            }

            _cachedToken = tokenResponse?.AccessToken?.Token;
            _refreshToken = tokenResponse?.RefreshToken?.Token;
            _expiration = tokenResponse?.AccessToken?.Expiration ?? DateTime.UtcNow.AddMinutes(5);

            if (string.IsNullOrEmpty(_cachedToken))
                throw new InvalidOperationException("Failed to retrieve access token.");

            _logger?.LogInformation("Token refreshed. Expires at {Expiration}.", _expiration);
            return _cachedToken;
        }
        finally
        {
            _refreshLock.Release();
        }
    }
}

public class NullAccessTokenProvider : IAccessTokenProvider
{
    public Task<string> GetAccessToken() => Task.FromResult(string.Empty);
}
