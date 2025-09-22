using DHI.Services;
using DHI.Services.Converters;
using DHI.Services.Jobs;
using DHI.Services.Jobs.Automations;
using DHI.Services.Jobs.Workflows;
using DHI.Services.Scalars;
using JobAutomator.InMemoryRepository;
using JobAutomator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JobAutomator;
public class LocalDependencyFactory : IDependencyFactory
{
    private readonly IConfiguration _cfg;
    private readonly ILogger _log;
    private readonly AccessTokenProvider _tokenProvider;

    public LocalDependencyFactory(IConfiguration configuration, ILogger logger)
    {
        _cfg = configuration;
        _log = logger;

        TryLoadTriggerPlugins();

        var auth = new AuthenticationConfiguration();
        _cfg.GetSection("Authentication").Bind(auth);
        _tokenProvider = new AccessTokenProvider(auth.Url, auth.UserName, auth.Password, logger);

        try
        {
            var t = typeof(AccessTokenProvider);
            t.GetField("_cachedToken", BindingFlags.NonPublic | BindingFlags.Instance)
             ?.SetValue(_tokenProvider, "local-test-token");
            t.GetField("_expiration", BindingFlags.NonPublic | BindingFlags.Instance)
             ?.SetValue(_tokenProvider, DateTime.UtcNow.AddDays(1));
            t.GetField("_refreshToken", BindingFlags.NonPublic | BindingFlags.Instance)
             ?.SetValue(_tokenProvider, "local-refresh");
        }
        catch (Exception ex)
        {
            _log?.LogDebug(ex, "Could not pre-seed AccessTokenProvider; initial auth may be attempted.");
        }
    }

    AccessTokenProvider IDependencyFactory.TokenProvider => _tokenProvider;

    public JobService<CodeWorkflow, string> GetJobService(string _)
    {
        var taskRepoPath = _cfg["LocalConnectionStrings:TaskRepositoryConnectionString"];
        ICodeWorkflowRepository taskRepo = !string.IsNullOrWhiteSpace(taskRepoPath)
            ? new CodeWorkflowRepository(taskRepoPath)
            : new InMemoryTaskRepository();

        var taskSvc = new CodeWorkflowService(taskRepo);

        var jobsPg = _cfg["LocalConnectionStrings:LocalPostgres:Jobs"];
        if (!string.IsNullOrWhiteSpace(jobsPg))
        {
            var jobRepoPg = new DHI.Services.Provider.PostgreSQL.JobRepository(jobsPg, _log);
            return new JobService<CodeWorkflow, string>(jobRepoPg, taskSvc);
        }

        var jobRepoMem = new InMemoryJobRepository();
        return new JobService<CodeWorkflow, string>(jobRepoMem, taskSvc);
    }

    public AutomationService GetAutomationService(string _, string __, string ___)
    {
        var path = _cfg["LocalConnectionStrings:LocalAutomationsPath"] ?? "C:\\Services\\JobAutomator\\Automations";
        var autoRepo = new DirectoryAutomationRepository(path);

        PatchRepoJsonOptionsToMatchWebApi(autoRepo);

        var scalarsPg = _cfg["LocalConnectionStrings:LocalPostgres:Scalars"];
        IGroupedScalarRepository<string, int> scalarRepo =
            !string.IsNullOrWhiteSpace(scalarsPg)
                ? new DHI.Services.Provider.PostgreSQL.ScalarRepository(scalarsPg, _log)
                : new InMemoryScalarRepository();

        IJobRepository<Guid, string> jobsRepo;
        var jobsPg = _cfg["LocalConnectionStrings:LocalPostgres:Jobs"];
        if (!string.IsNullOrWhiteSpace(jobsPg))
        {
            jobsRepo = new DHI.Services.Provider.PostgreSQL.JobRepository(jobsPg, _log);
        }
        else
        {
            jobsRepo = new InMemoryJobRepository();
        }

        return new AutomationService(autoRepo, scalarRepo, jobsRepo, _log);
    }

    public ScalarService<int> GetScalarService(string _)
    {
        var scalarsPg = _cfg["LocalConnectionStrings:LocalPostgres:Scalars"];
        if (!string.IsNullOrWhiteSpace(scalarsPg))
        {
            var repo = new DHI.Services.Provider.PostgreSQL.ScalarRepository(scalarsPg, _log);
            return new DHI.Services.Provider.PostgreSQL.ScalarService(repo, _log);
        }

        return new DHI.Services.Scalars.ScalarService<int>(new InMemoryScalarRepository(), _log);
    }

    public IJobServiceFactory GetJobServiceFactory()
    {
        var map = new Dictionary<string, JobService<CodeWorkflow, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["wf-jobs-minion"] = GetJobService(""),
            ["wf-jobs-titan"] = GetJobService("")
        };
        return new JobServiceFactory(map, _log);
    }

    private void TryLoadTriggerPlugins()
    {
        try
        {
            var coreAsm = typeof(BaseTrigger).Assembly;
            _log?.LogDebug("Core trigger assembly loaded: {Asm}", coreAsm.FullName);

            TryLoadByShortName("DHI.Services.Jobs.Automations");
            TryLoadByShortName("DHI.Services.Jobs.Automations.Triggers");

            var dirs = new List<string>();

            var cs = _cfg.GetConnectionString("TriggerCatalogDirectory");
            if (!string.IsNullOrWhiteSpace(cs))
                dirs.Add(cs);

            var k1 = _cfg["TriggerCatalogDirectory"];
            if (!string.IsNullOrWhiteSpace(k1))
                dirs.Add(k1);

            var k2 = _cfg["LocalConnectionStrings:TriggerCatalogDirectory"];
            if (!string.IsNullOrWhiteSpace(k2))
                dirs.Add(k2);

            var k3 = _cfg["AppConfiguration:TriggerCatalogDirectory"];
            if (!string.IsNullOrWhiteSpace(k3))
                dirs.Add(k3);

            var contentRoot = _cfg.GetValue<string>("AppConfiguration:ContentRootPath")
                             ?? Directory.GetCurrentDirectory();
            dirs.Add(Path.Combine(contentRoot, "App_Data", "Triggers"));
            dirs.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Triggers"));

            var candidates = dirs
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(Path.GetFullPath)
                .Distinct(StringComparer.OrdinalIgnoreCase);

            var loaded = AppDomain.CurrentDomain.GetAssemblies()
                           .Select(a => a.FullName)
                           .Where(n => n != null)
                           .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var dir in candidates)
            {
                if (!Directory.Exists(dir))
                {
                    _log?.LogTrace("Trigger dir not found: {Dir}", dir);
                    continue;
                }

                foreach (var dll in Directory.EnumerateFiles(dir, "*.dll", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        var name = AssemblyName.GetAssemblyName(dll).FullName;
                        if (loaded.Contains(name))
                            continue;

                        Assembly.LoadFrom(dll);
                        loaded.Add(name);
                        _log?.LogInformation("Loaded trigger plug-in: {Dll}", dll);
                    }
                    catch (Exception ex)
                    {
                        _log?.LogWarning(ex, "Failed to load trigger plug-in: {Dll}", dll);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _log?.LogWarning(ex, "TryLoadTriggerPlugins failed (continuing without external triggers).");
        }

        // local helper
        void TryLoadByShortName(string shortName)
        {
            try
            {
                if (AppDomain.CurrentDomain.GetAssemblies()
                      .Any(a => string.Equals(a.GetName().Name, shortName, StringComparison.OrdinalIgnoreCase)))
                    return;

                Assembly.Load(shortName);
                _log?.LogDebug("Preloaded assembly: {Asm}", shortName);
            }
            catch (Exception ex)
            {
                _log?.LogTrace(ex, "Could not preload assembly {Asm}", shortName);
            }
        }
    }

    private static JsonSerializerOptions BuildWebApiLikeOptions()
    {
        var opts = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            IncludeFields = true,
        };

        opts.Converters.Add(new NullableBoolFlexibleConverter());
        opts.Converters.Add(new BoolFlexibleConverter());

        opts.Converters.Add(new JsonStringEnumConverter());
        opts.Converters.Add(new ObjectToInferredTypeConverter());
        opts.Converters.Add(new DictionaryTypeResolverConverter<string, DHI.Services.Jobs.Host>());
        opts.Converters.Add(new DictionaryTypeResolverConverter<Guid, Job>());
        opts.Converters.Add(new DictionaryTypeResolverConverter<string, Automation<string>>(isNestedDictionary: true));
        opts.Converters.Add(new TriggerConverter());
        opts.Converters.Add(new JsonCollectionItemConverter<ITrigger, TriggerConverter>());

        opts.Converters.Add(new TypeResolverConverter<Parameters>());
        opts.Converters.Add(new EnumerationConverter());
        opts.Converters.Add(new TypeStringConverter());
        opts.Converters.Add(new AutoNumberToStringConverter());
        opts.Converters.Add(new PermissionConverter());
        opts.Converters.Add(new ConnectionDictionaryConverter());
        opts.Converters.Add(new ConnectionConverter());
        opts.Converters.Add(new TypeResolverConverter<ProviderArgument>());
        opts.Converters.Add(new TypeResolverConverter<ProviderType>());
        opts.Converters.Add(new TypeResolverConverter<ConnectionType>());

        return opts;
    }

    private void PatchRepoJsonOptionsToMatchWebApi(object repo)
    {
        try
        {
            static FieldInfo FindField(Type t, string name)
            {
                while (t != null)
                {
                    var f = t.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
                    if (f != null)
                        return f;
                    t = t.BaseType;
                }
                return null;
            }

            var fOpts = FindField(repo.GetType(), "_jsonSerializerOptions");
            if (fOpts == null)
            {
                _log?.LogWarning("Could not find _jsonSerializerOptions on {Type}", repo.GetType().FullName);
                return;
            }

            var webApiLike = BuildWebApiLikeOptions();
            fOpts.SetValue(repo, webApiLike);
            _log?.LogDebug("Patched DirectoryAutomationRepository JSON options to match WebAPI.");
        }
        catch (Exception ex)
        {
            _log?.LogWarning(ex, "Failed to patch DirectoryAutomationRepository JSON options");
        }
    }
}

public sealed class SerializerOptionsDefault
{
    #region ' Thread-Safe Singleton Constructor '

    private static readonly System.Lazy<SerializerOptionsDefault> _lazy = new(() => new SerializerOptionsDefault());

    private static SerializerOptionsDefault instance => _lazy.Value;

    #endregion

    private SerializerOptionsDefault()
    {
        _serializer = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            IncludeFields = true,
        };
        _serializer.AddConverters(_defaultJsonConverters());
    }

    private readonly JsonSerializerOptions _serializer;

    private static readonly Func<JsonConverter[]> _defaultJsonConverters = () =>
    {
        return new JsonConverter[]
        {
                new JsonStringEnumConverter(),
                new ObjectToInferredTypeConverter(),
                new DictionaryTypeResolverConverter<string, DHI.Services.Jobs.Host>(),
                new DictionaryTypeResolverConverter<Guid, Job>(),
                new DictionaryTypeResolverConverter<string, Automation<string>>(isNestedDictionary: true),
                new TriggerConverter(),
                new JsonCollectionItemConverter<ITrigger, TriggerConverter>(),
                new TypeResolverConverter<Parameters>(),
                new EnumerationConverter(),
                new TypeStringConverter(),
                new AutoNumberToStringConverter(),
                new PermissionConverter(),
                new ConnectionDictionaryConverter(),
                new ConnectionConverter(),
                new TypeResolverConverter<ProviderArgument>(),
                new TypeResolverConverter<ProviderType>(),
                new TypeResolverConverter<ConnectionType>()
        };
    };

    public static JsonSerializerOptions Options => instance._serializer;
}
