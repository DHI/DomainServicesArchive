using BaseWebApi;
using DHI.Chart.Map;
using DHI.Services;
using DHI.Services.Accounts;
using DHI.Services.Authentication;
using DHI.Services.Authentication.PasswordHistory;
using DHI.Services.Authorization;
using DHI.Services.Documents;
using DHI.Services.Filters;
using DHI.Services.GIS;
using DHI.Services.GIS.Maps;
using DHI.Services.Jobs;
using DHI.Services.Jobs.Automations;
using DHI.Services.Jobs.Workflows;
using DHI.Services.JsonDocuments;
using DHI.Services.Logging.WebApi;
using DHI.Services.Mails;
using DHI.Services.Meshes;
using DHI.Services.Models;
using DHI.Services.Notifications;
using DHI.Services.Places;
using DHI.Services.Provider.MCLite;
using DHI.Services.Provider.MIKECore;
using DHI.Services.Provider.PostgreSQL;
using DHI.Services.Rasters.Radar;
using DHI.Services.Rasters.Radar.DELIMITEDASCII;
using DHI.Services.Rasters.Zones;
using DHI.Services.Scalars;
using DHI.Services.Spreadsheets;
using DHI.Services.TimeSeries;
using DHI.Services.TimeSteps;
using DHI.Services.WebApiCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Serilog;
using Serilog.Extensions.Logging;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.ComponentModel.Composition.Hosting;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var configuration = builder.Configuration;

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Configure JWT Bearer authentication
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Validate the token issuer
            ValidateAudience = true, // Validate the token audience
            ValidateLifetime = true, // Validate the token's expiration
            ValidateIssuerSigningKey = true, // Validate the signing key of the token
            ValidIssuer = configuration["Tokens:Issuer"], // Set the expected issuer
            ValidAudience = configuration["Tokens:Audience"], // Set the expected audience
            IssuerSigningKey = RSA.BuildSigningKey(configuration["Tokens:PublicRSAKey"].Resolve()) // Set the signing key
        };
    });

// Authorization
builder.Services.AddAuthorization(options =>
{
    // Define authorization policies
    options.AddPolicy("AdministratorsOnly", policy => policy.RequireClaim(ClaimTypes.GroupSid, "Administrators"));
    options.AddPolicy("EditorsOnly", policy => policy.RequireClaim(ClaimTypes.GroupSid, "Editors"));
});

// API versioning
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version", "version", "ver"),
        new HeaderApiVersionReader("api-version"));
});

// MVC
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services
    .AddCors(options =>
    {
        var permittedOrigins = configuration.GetSection("PermittedOrigins")?.GetChildren().Select(c => c.Value).ToArray();

        options.AddPolicy(name: MyAllowSpecificOrigins,
            policy =>
            {
                policy.WithOrigins(permittedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
            });
    })
    .AddResponseCompression(options => { options.EnableForHttps = true; })
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization options
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition
            = BaseWebApi.SerializerOptionsDefault.Options.DefaultIgnoreCondition;
        options.JsonSerializerOptions.PropertyNamingPolicy
            = BaseWebApi.SerializerOptionsDefault.Options.PropertyNamingPolicy;
        options.JsonSerializerOptions
               .AddConverters(DHI.Services.Security.WebApi.SerializerOptionsDefault.Options.Converters);
        options.JsonSerializerOptions
               .AddConverters(BaseWebApi.SerializerOptionsDefault.Options.Converters);
        options.JsonSerializerOptions
               .AddConverters(DHI.Services.Connections.WebApi.SerializerOptionsDefault.Options.Converters);
        options.JsonSerializerOptions
               .AddConverters(DHI.Services.Spreadsheets.WebApi.SerializerOptionsDefault.Options.Converters);
        options.JsonSerializerOptions
               .AddConverters(DHI.Services.TimeSeries.WebApi.SerializerOptionsDefault.Options.Converters);
        options.JsonSerializerOptions
               .AddConverters(DHI.Services.Places.WebApi.SerializerOptionsDefault.Options.Converters);
        options.JsonSerializerOptions
               .AddConverters(DHI.Services.GIS.WebApi.SerializerOptionsDefault.Options.Converters);
        options.JsonSerializerOptions
               .AddConverters(DHI.Services.Meshes.WebApi.SerializerOptionsDefault.Options.Converters);
        options.JsonSerializerOptions
               .AddConverters(DHI.Services.Rasters.WebApi.SerializerOptionsDefault.Options.Converters);
        options.JsonSerializerOptions
               .AddConverters(DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options.Converters);
        options.JsonSerializerOptions
               .AddConverters(DHI.Services.Models.WebApi.SerializerOptionsDefault.Options.Converters);
#warning Depending on which Web API packages you install in this project, you need to register domain-specific JSON converters for these packages
    });

// HSTS
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.MaxAge = TimeSpan.FromDays(configuration.GetValue<double>("AppConfiguration:HstsMaxAgeInDays"));
});

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(configuration["Swagger:SpecificationName"], new OpenApiInfo
    {
        Title = configuration["Swagger:DocumentTitle"],
        Version = "1",
        Description = File.ReadAllText(configuration["Swagger:DocumentDescription"].Resolve())
    });

    options.EnableAnnotations();
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.WebApi.xml"));
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.Logging.WebApi.xml"));
#warning Depending on which Web API packages you install in this project, you need to register the XML-files from these packages for descriptions in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter the word 'Bearer' followed by a space and the JWT.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// SignalR
#region Enable if using SignalR capabilities
//builder.Services.AddSignalR();
#endregion

// Set the data directory (App_Data folder) for DHI Domain Services
var contentRootPath = builder.Configuration.GetValue("AppConfiguration:ContentRootPath", builder.Environment.ContentRootPath);
AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(contentRootPath, "App_Data")); // Set the data directory for the application

var seriLogger = new LoggerConfiguration()
    .WriteTo.File("[AppData]log.txt".Resolve(), outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message:lj}{NewLine}{Exception}") // Configure Serilog to log to a file with a specific format
    .WriteTo.Console() // Also log to the console
    .CreateLogger(); // Create the logger

var logger = new SerilogLoggerFactory(seriLogger)
   .CreateLogger<Program>(); // Create a logger instance using Serilog

// Configure the necessary services for constructor injection into controllers of DHI Domain Services Web APIs
#warning In production code, you should use replace the JSON-file based repostiories with for example the PostgreSQL repositories

// Pwned passwords
builder.Services
    .AddPwnedPasswordHttpClient(minimumFrequencyToConsiderPwned: 1) // Add a service to check for pwned passwords
    .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.RetryAsync(3)) // Retry failed HTTP requests up to 3 times
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(2))); // Set a timeout policy of 2 seconds

// Password Policy
builder.Services.AddScoped(_ => new PasswordPolicy
{
    // Define password policy
    RequiredLength = 6, // Set minimum password length
    RequiredUniqueChars = 0, // Set minimum number of unique characters
    MinimumDigit = 1, // Set minimum number of digits
    MinimumNonAlphanumeric = 0, // Set minimum number of non-alphanumeric characters
    RequireNonAlphanumeric = false, // Do not require non-alphanumeric characters
    RequireLowercase = false, // Do not require lowercase letters
    RequireUppercase = false, // Do not require uppercase letters
    RequireDigit = true // Require digits in the password
});

// Password History Policy
builder.Services.AddScoped(_ => new PasswordExpirationPolicy
{
    // Define password expiration and history policy
    PreviousPasswordsReUseLimit = 3, // Set limit for reusing old passwords
    PasswordExpiryDurationInDays = 5 // Set password expiration duration
});

// Login Attempt Policy
builder.Services.AddScoped(_ => new LoginAttemptPolicy
{
    // Define login attempt policy
    MaxNumberOfLoginAttempts = 2, // Set maximum number of login attempts before lockout
    ResetInterval = TimeSpan.FromMinutes(1), // Set interval after which login attempts reset
    LockedPeriod = TimeSpan.FromDays(10) // Set account lockout period
});

var connectionString = builder.Configuration.GetConnectionString("Postgres-Security") ?? builder.Configuration["Postgres-Security:ConnectionString"];
string logConnectionString = builder.Configuration["Postgres-Security:LogConnectionString"];
var container = GetTriggerCatalog(configuration);
builder.Services.AddSerilog(seriLogger);
builder.Services.AddScoped<Microsoft.Extensions.Logging.ILogger>(provider => new SimpleLogger("[AppData]log.json".Resolve()));
builder.Services.AddScoped<INotificationRepository>(_ => new DHI.Services.Provider.PostgreSQL.NotificationRepository(logConnectionString));
builder.Services.AddScoped<IAccountRepository>(_ => new DHI.Services.Provider.PostgreSQL.AccountRepository(connectionString, null, DHI.Services.Security.WebApi.SerializerOptionsDefault.Options.Converters, _.GetRequiredService<LoginAttemptPolicy>()));
builder.Services.AddScoped<IMailTemplateRepository>(_ => new MailTemplateRepository("mail-templates.json", DHI.Services.Security.WebApi.SerializerOptionsDefault.Options));
builder.Services.AddScoped<IUserGroupRepository>(_ => new DHI.Services.Provider.PostgreSQL.UserGroupRepository(connectionString, null));
builder.Services.AddScoped<IRefreshTokenRepository>(_ => new DHI.Services.Provider.PostgreSQL.RefreshTokenRepository(connectionString, null));
builder.Services.AddScoped<IAuthenticationProvider>(_ => new DHI.Services.Provider.PostgreSQL.AccountRepository(connectionString, null, DHI.Services.Security.WebApi.SerializerOptionsDefault.Options.Converters, _.GetRequiredService<LoginAttemptPolicy>()));
builder.Services.AddScoped<IPasswordHistoryRepository>(_ => new DHI.Services.Provider.PostgreSQL.PasswordHistoryRepository(connectionString, null, DHI.Services.Security.WebApi.SerializerOptionsDefault.Options.Converters));
builder.Services.AddScoped(_ => new DHI.Services.Connections.WebApi.ConnectionTypeService(AppContext.BaseDirectory));
builder.Services.AddScoped<IMapStyleRepository>(_ => new DHI.Services.GIS.WebApi.MapStyleRepository("styles.json", DHI.Services.GIS.WebApi.SerializerOptionsDefault.Options));
builder.Services.AddScoped<IZoneRepository>(_ => new DHI.Services.Rasters.WebApi.ZoneRepository("zones.json"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<DHI.Services.Provider.DS.IAccessTokenProvider, HttpContextAccessTokenProvider>();
builder.Services.AddSingleton<IFilterRepository>(new DHI.Services.Filters.FilterRepository("[AppData]signalr-filters.json".Resolve()));
builder.Services.AddScoped<IHostRepository>(_ => new DHI.Services.Jobs.WebApi.GroupedHostRepository("grouped_hosts.json"));
builder.Services.AddScoped<IAutomationRepository>(_ => new DHI.Services.Jobs.WebApi.AutomationRepository("automations.json"));
builder.Services.AddScoped<IScalarRepository<string, int>>(_ => new DHI.Services.Scalars.ScalarRepository("[AppData]scalars.json".Resolve()));
builder.Services.AddSingleton<IJobRepository<Guid, string>>(sp =>
{
    var jobRepo1 = new DHI.Services.Jobs.WebApi.JobRepository("[AppData]jobs.json".Resolve());
    var jobRepo2 = new DHI.Services.Jobs.WebApi.JobRepository("[AppData]jobs2.json".Resolve());

    return new ReadOnlyCompositeJobRepository(new[] { jobRepo1, jobRepo2 });
});
builder.Services.AddScoped<Microsoft.Extensions.Logging.ILogger>(_ => new SimpleLogger("[AppData]log.log".Resolve()));
builder.Services.AddSingleton<ITriggerRepository>(new TriggerRepository(container));
builder.Services.AddSignalR(hubOptions => { hubOptions.EnableDetailedErrors = true; });

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<IAccountRepository>().Count();
    scope.ServiceProvider.GetRequiredService<IUserGroupRepository>().Count();
    scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>().Count();
    scope.ServiceProvider.GetRequiredService<IPasswordHistoryRepository>().Count();
}
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var specificationName = configuration["Swagger:SpecificationName"];
        options.SwaggerEndpoint($"../swagger/{specificationName}/swagger.json", configuration["Swagger:DocumentName"]);
        options.DocExpansion(DocExpansion.None);
        options.DefaultModelsExpandDepth(-1);
    });
}
else
{
    app.UseHsts();
}

app.UseExceptionHandling();
//app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    #region Enable if using SignalR capabilities
    //endpoints.MapHub<NotificationHub>("/notificationhub");
    #endregion
});

#region Install Web APIs and providers and configure more services here.
// https://developerdoc-mike-platform-prod.eu.mike-cloud.com/domain_services/how-to/#configuration-in-code
var lazyCreation = builder.Configuration.GetValue("AppConfiguration:LazyCreation", true);
Services.Configure(new DHI.Services.Connections.WebApi.ConnectionRepository("connections.json", DHI.Services.Connections.WebApi.SerializerOptionsDefault.Options), lazyCreation);

var scalarDS = configuration["DS-Scalars:ConnectionString"];
var jsonDocumentsPostgres = configuration["Postgres-JsonDocuments:ConnectionString"];
var sqlitePath = Path.Combine("[AppData]".Resolve(), "MCSQLiteTest.sqlite");
var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
var tokenProvider = new HttpContextAccessTokenProvider(httpContextAccessor);
var tsRepo = new DHI.Services.TimeSeries.CSV.TimeSeriesRepository("[AppData]csv".Resolve());
var csvLogger = app.Services.GetRequiredService<ILogger<GroupedDiscreteTimeSeriesService>>();
var tsService = new GroupedDiscreteTimeSeriesService(tsRepo, csvLogger);
var timeSeriesList = new List<TimeSeries> { new TimeSeries("myTimeSeries", "My Time Series") };
var repository = new InMemoryTimeSeriesRepository<string, double>(timeSeriesList);
var service = new UpdatableTimeSeriesService(repository);
var daylightService = new CoreTimeSeriesService(new DHI.Services.TimeSeries.Daylight.TimeSeriesRepository());
var sqliteConn = $"database=[AppData]MCSQLiteTest.sqlite".Resolve() + ";dbflavour=SQLite";
var dfs2File = Path.Combine("[AppData]".Resolve(), "dfs2", "R20141001.dfs2");

ServiceLocator.Register(new DiscreteTimeSeriesService<string, double>(new DHI.Services.TimeSeries.CSV.TimeSeriesRepository("[AppData]".Resolve())), "csv");
ServiceLocator.Register(new GisService<string>(new DHI.Services.Provider.ShapeFile.FeatureRepository("[AppData]shp".Resolve())), "shp");

Dictionary<string, IDiscreteTimeSeriesService<string, double>> timeSeriesServices = new Dictionary<string, IDiscreteTimeSeriesService<string, double>>();
timeSeriesServices.Add("csv", Services.Get<IDiscreteTimeSeriesService<string, double>>("csv"));

Dictionary<string, IScalarService<string, int>> scalarServices = new Dictionary<string, IScalarService<string, int>>();
var gisService = Services.Get<IGisService<string>>("shp");
var JsonDocumentService = new JsonDocumentService(new JsonDocumentRepositorySecured(jsonDocumentsPostgres));

var meshRepository = new DfsuMeshRepository(new FileSource("[AppData]\\dfsu".Resolve()));
var meshService = new GroupedMeshService(meshRepository);

ServiceLocator.Register(new DHI.Services.Scalars.GroupedScalarService<string, int>(new DHI.Services.Scalars.ScalarRepository("[AppData]scalars.json".Resolve()), new SimpleLogger("[AppData]scalars.log".Resolve())), "json-scalars");
ServiceLocator.Register(new DHI.Services.Scalars.GroupedScalarService<string, int>(new DHI.Services.Provider.DS.ScalarRepository($"{scalarDS}/api/scalars/json-scalars", tokenProvider, 3, null), new SimpleLogger("[AppData]scalarsDS.log".Resolve())), "ds-scalars");
ServiceLocator.Register(new SpreadsheetService(new DHI.Services.Provider.OpenXML.SpreadsheetRepository("[AppData]".Resolve())), "xlsx");
ServiceLocator.Register(new TimeStepService<string, object>(new Dfs2TimeStepServer("[AppData]R20141001.dfs2".Resolve())), "dfs2");
ServiceLocator.Register(new LogService(new ClefLogRepository("[AppData]".Resolve())), "json-logger");
ServiceLocator.Register(new DocumentService<string>(new DocumentRepository("database=mc2014.2")),"mc-doc");
ServiceLocator.Register(new GroupedDocumentService<string>(new DocumentRepository($"database={sqlitePath};dbflavour=SQLite")), "mclite");
ServiceLocator.Register(new GroupedDiscreteTimeSeriesService<string, double>(new DHI.Services.TimeSeries.CSV.TimeSeriesRepository("[AppData]csv".Resolve())), "grouped-csv");
ServiceLocator.Register(new CoreTimeSeriesService(new DHI.Services.TimeSeries.Daylight.TimeSeriesRepository()),"daylight");
ServiceLocator.Register(tsService, "csv2");
ServiceLocator.Register(service, "myTsConnection");
ServiceLocator.Register(daylightService, "daylight-ts");
ServiceLocator.Register(JsonDocumentService, "json-documents");
ServiceLocator.Register(new PlaceService(new DHI.Services.Places.PlaceRepository("[AppData]places.json".Resolve()),timeSeriesServices, scalarServices, gisService), "json");

ServiceLocator.Register(new GroupedGisService(new DHI.Services.Provider.MCLite.FeatureRepository("database=mc2014.2")),"mc-ws1");
ServiceLocator.Register(new GroupedMapService(new GroupedMapSource(sqliteConn),new MapStyleService(new DHI.Services.GIS.WebApi.MapStyleRepository("[AppData]styles.json".Resolve(), DHI.Services.GIS.WebApi.SerializerOptionsDefault.Options))),"mc-groupedmap");

ServiceLocator.Register(new GisService(new DHI.Services.Provider.ShapeFile.FeatureRepository("[AppData]".Resolve())),"shape");

ServiceLocator.Register(new MapService(new Dfs2MapSource(dfs2File),new MapStyleService(new DHI.Services.GIS.WebApi.MapStyleRepository(Path.Combine("[AppData]".Resolve(), "styles.json"), DHI.Services.GIS.WebApi.SerializerOptionsDefault.Options))),"dfs2-map");

ServiceLocator.Register(new MapService(new DfsuMapSource("[AppData]".Resolve()),new MapStyleService(new DHI.Services.GIS.WebApi.MapStyleRepository(Path.Combine("[AppData]".Resolve(), "styles.json"), DHI.Services.GIS.WebApi.SerializerOptionsDefault.Options))),"dfsu-map");

ServiceLocator.Register(new GroupedUpdatableGisService(new DHI.Services.Provider.MCLite.FeatureRepository(sqliteConn)),"gis-mclite");

ServiceLocator.Register(new GroupedMapService(new GroupedMapSource(sqliteConn),new MapStyleService(new DHI.Services.GIS.WebApi.MapStyleRepository(Path.Combine("[AppData]".Resolve(), "styles.json"), DHI.Services.GIS.WebApi.SerializerOptionsDefault.Options))),"groupedmap-mclite");

ServiceLocator.Register(meshService, "dfsu");

ServiceLocator.Register(new RadarImageService<AsciiImage>(new DelimitedAsciiRepository("[AppData]RadarImages;PM_{datetimeFormat}.txt;yyyyMMddHH_$$$".Resolve())),"ascii");

ServiceLocator.Register(new JobService<Workflow, string>(new DHI.Services.Jobs.WebApi.JobRepository("jobs.json"), new TaskService<Workflow, string>(new DHI.Services.Jobs.Workflows.WorkflowRepository("[AppData]workflows.json", DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options.Converters)),null), "wf-jobs");

ServiceLocator.Register(new TaskService<Workflow, string>(new DHI.Services.Jobs.Workflows.WorkflowRepository("[AppData]workflows.json")),"wf-tasks");

ServiceLocator.Register(new DHI.Services.Jobs.Scenarios.ScenarioService(new DHI.Services.Jobs.Scenarios.ScenarioRepository("[AppData]scenarios.json".Resolve(), DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options.Converters),new DHI.Services.Jobs.WebApi.JobRepository("jobs.json")),"json-scenarios");

var notificationRepositoryPath = "[AppData]notifications.json".Resolve();
var notificationRepository = new DHI.Services.Notifications.WebApi.NotificationRepository(notificationRepositoryPath);
ServiceLocator.Register(new NotificationService(notificationRepository), "json-notifications");

var workflowRepository = new DHI.Services.Jobs.Workflows.CodeWorkflowRepository("[AppData]workflows2.json".Resolve());
var workflowService = new CodeWorkflowService(workflowRepository);
ServiceLocator.Register(workflowService, "wf-tasks2");

var jobRepository = new DHI.Services.Jobs.WebApi.JobRepository("jobs2.json");
var jobService = new JobService<CodeWorkflow, string>(jobRepository, workflowService);
ServiceLocator.Register(jobService, "wf-jobs2");

ServiceLocator.Register(new DiscreteTimeSeriesService<string, double>(new DHI.Services.Provider.MIKE1D.ResultFileTimeSeriesRepository("[AppData]Exam6Base.res1d".Resolve())), "exam-6-base");

var fixedStartDate = new DateTime(2015, 1, 1);
var fixedEndDate = new DateTime(2015, 1, 31);
ServiceLocator.Register(new TimeSeriesService<string, double>(new DHI.Services.Provider.USGS.TimeSeriesRepository($"http://waterservices.usgs.gov/nwis/dv?startDT={fixedStartDate:yyyy-MM-dd}&endDt={fixedEndDate:yyyy-MM-dd}")), "timeseries-usgs");

var modelService = new ModelDataReaderService(new ModelDataReaderRepository("[AppData]models.json".Resolve(), DHI.Services.Models.WebApi.SerializerOptionsDefault.Options.Converters));
ServiceLocator.Register(modelService, "json-models");
var worker = new FakeScenarioWorker();
var scenarioService = new DHI.Services.Models.ScenarioService(new ScenarioRepositoryWithFakeFactory("[AppData]models-scenarios.json".Resolve(), DHI.Services.Models.WebApi.SerializerOptionsDefault.Options.Converters), modelService, worker);
ServiceLocator.Register(scenarioService, "json-models-scenarios");

ServiceLocator.Register(new GroupedDocumentService<string>(new DHI.Services.Provider.MIKECloud.GroupedDocumentRepository(new Guid("2e647926-272a-4fd6-b0b4-4acbf2667511"), new Guid("47ba74c0-f4d2-4bc5-bfc4-a71bf4c5594c"), DHI.Platform.SDK.Configuration.PlatformEnvironment.Test)), "document-mc");
#endregion

app.Run();

// Check

static CompositionContainer GetTriggerCatalog(IConfiguration configuration)
{
    var catalog = new AggregateCatalog();
    catalog.Catalogs.Add(new AssemblyCatalog(typeof(BaseTrigger).Assembly));

    var catalogDirectory = configuration.GetConnectionString("TriggerCatalogDirectory");
    if (!string.IsNullOrEmpty(catalogDirectory))
    {
        catalog.Catalogs.Add(new DirectoryCatalog(catalogDirectory));
    }

    return new CompositionContainer(catalog);
}

public class HttpContextAccessTokenProvider : DHI.Services.Provider.DS.IAccessTokenProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextAccessTokenProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<string> GetAccessToken()
    {
        var context = _httpContextAccessor.HttpContext;

        if (context != null && context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var token = authHeader.ToString();

            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring("Bearer ".Length).Trim();
            }

            return Task.FromResult(token);
        }

        return Task.FromResult(string.Empty);
    }
}