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
using DHI.Services.Mails;
using DHI.Services.Notifications;
using DHI.Services.Places;
using DHI.Services.Provider.MIKECore;
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
using Serilog;
using Serilog.Extensions.Logging;
using ServicesRegistrationList;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var configuration = builder.Configuration;

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Tokens:Issuer"],
            ValidAudience = configuration["Tokens:Audience"],
            IssuerSigningKey = RSA.BuildSigningKey(configuration["Tokens:PublicRSAKey"].Resolve()),
            ClockSkew = TimeSpan.Zero // Default value is a 5 minutes delay
        };

        #region Enable if using SignalR capabilities
        //options.Events = new JwtBearerEvents
        //{
        //    OnMessageReceived = context =>
        //    {
        //        var accessToken = context.Request.Query["access_token"];
        //        var path = context.HttpContext.Request.Path;
        //        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationhub"))
        //        {
        //            context.Token = accessToken;
        //        }
        //        return Task.CompletedTask;
        //    }
        //};
        #endregion
    });

// Authorization
builder.Services.AddAuthorization(options =>
{
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
builder.Services
    .AddResponseCompression(options => { options.EnableForHttps = true; })
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization options
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition
            = SerializerOptionsDefault.Options.DefaultIgnoreCondition;
        options.JsonSerializerOptions.PropertyNamingPolicy
            = SerializerOptionsDefault.Options.PropertyNamingPolicy;
        options.JsonSerializerOptions
               .AddConverters(DHI.Services.Security.WebApi.SerializerOptionsDefault.Options.Converters);
        options.JsonSerializerOptions
               .AddConverters(ServicesRegistrationList.SerializerOptionsDefault.Options.Converters);
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
builder.Services.AddSignalR();
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

builder.Services.AddSerilog(seriLogger); // Register the Serilog logger with the dependency injection container

#region DHI.Services.Security.WebApi
// json-based persistance
builder.Services.AddScoped<IAccountRepository>(_ => new AccountRepository("accounts.json", SerializerOptionsDefault.Options));
builder.Services.AddScoped<IUserGroupRepository>(_ => new UserGroupRepository("user-groups.json", SerializerOptionsDefault.Options));
builder.Services.AddScoped<IRefreshTokenRepository>(_ => new RefreshTokenRepository("refresh-tokens.json", SerializerOptionsDefault.Options));
builder.Services.AddScoped<IAuthenticationProvider>(_ => new AccountRepository("accounts.json", SerializerOptionsDefault.Options));
builder.Services.AddScoped<IPasswordHistoryRepository>(_ => new PasswordHistoryRepository("passwordhistory.json", SerializerOptionsDefault.Options));
builder.Services.AddScoped<Microsoft.Extensions.Logging.ILogger>(_ => new SimpleLogger("[AppData]log.log".Resolve()));

// postgreSQL-based persistance
var connectionString = builder.Configuration.GetConnectionString("PostgreSQL") ?? builder.Configuration["Security-Repository:PostgreSQL:ConnectionString"];
string logConnectionString = builder.Configuration["Security-Repository:PostgreSQL:ConnectionString"];

builder.Services.AddScoped<INotificationRepository>(_ => new DHI.Services.Provider.PostgreSQL.NotificationRepository(logConnectionString)); // NotificationRepository is the alternative for logging
builder.Services.AddScoped<IAccountRepository>(_ => new DHI.Services.Provider.PostgreSQL.AccountRepository(connectionString, null, SerializerOptionsDefault.Options.Converters, _.GetRequiredService<LoginAttemptPolicy>()));
builder.Services.AddScoped<IUserGroupRepository>(_ => new DHI.Services.Provider.PostgreSQL.UserGroupRepository(connectionString, null));
builder.Services.AddScoped<IRefreshTokenRepository>(_ => new DHI.Services.Provider.PostgreSQL.RefreshTokenRepository(connectionString, null));
builder.Services.AddScoped<IAuthenticationProvider>(_ => new DHI.Services.Provider.PostgreSQL.AccountRepository(connectionString, null, SerializerOptionsDefault.Options.Converters, _.GetRequiredService<LoginAttemptPolicy>()));
builder.Services.AddScoped<IPasswordHistoryRepository>(_ => new DHI.Services.Provider.PostgreSQL.PasswordHistoryRepository(connectionString, null, SerializerOptionsDefault.Options.Converters));

// DS module (calling external API that exposes DHI.Services.Security.WebApi)
var dsConnectionString = builder.Configuration.GetConnectionString("Security-Repository:DS:ConnectionString");
builder.Services.AddScoped<IAccountRepository>(_ => new DHI.Services.Provider.DS.AccountRepository($"{dsConnectionString}/api/accounts", null, 5, logger));

// Still only support json-based persistance
builder.Services.AddScoped<IMailTemplateRepository>(_ => new MailTemplateRepository("mail-templates.json", SerializerOptionsDefault.Options));
#endregion

#region DHI.Services.Documents.WebApi
builder.Services.AddScoped(provider => new ConnectionTypeService(AppContext.BaseDirectory)); // Remove this one if you already have it from other examples
#endregion

#region DHI.Services.GIS.WebApi
builder.Services.AddScoped(provider => new ConnectionTypeService(AppContext.BaseDirectory)); // Remove this one if you already have it from other examples

// If you use json-persistance data
builder.Services.AddScoped<IMapStyleRepository>(_ => new DHI.Services.GIS.WebApi.MapStyleRepository("styles.json", DHI.Services.TimeSeries.WebApi.SerializerOptionsDefault.Options));

// If you use GeoServer provider (Need to run docker GeoServer container)
var baseUrl = "Your Base URL";
var userName = "Your Username";
var password = "Your Password";
builder.Services.AddScoped<IMapStyleRepository>(_ => new DHI.Services.Provider.GeoServer.MapStyleRepository($"BaseUrl={baseUrl};UserName={userName};Password={password}"));

// If you use PostgreSQL provider
builder.Services.AddScoped<IMapStyleRepository>(_ => new DHI.Services.Provider.PostgreSQL.MapStyleRepository("Your ConnectionString", logger));
#endregion

#region DHI.Services.Jobs.WebApi
builder.Services.AddScoped<DHI.Services.Jobs.IHostRepository>(_ => new DHI.Services.Jobs.WebApi.GroupedHostRepository("grouped_hosts.json"));

// json-based persistance
builder.Services.AddScoped<DHI.Services.Jobs.Automations.IAutomationRepository>(_ => new DHI.Services.Jobs.WebApi.AutomationRepository("automations.json"));

// Directory-based persistance
builder.Services.AddScoped<DHI.Services.Jobs.Automations.IAutomationRepository>(_ => new DHI.Services.Jobs.Automations.DirectoryAutomationRepository("C:\\Services\\JobAutomator\\Automations"));

// DS provider
builder.Services.AddScoped<DHI.Services.Jobs.Automations.IAutomationRepository>(_ => new DHI.Services.Provider.DS.AutomationRepository("Your BaseURI", null, 5, logger));

builder.Services.AddScoped<Microsoft.Extensions.Logging.ILogger>(_ => new SimpleLogger("[AppData]log.log".Resolve()));

// Use this if you use json-based persistance for Jobs
builder.Services.AddSingleton<DHI.Services.Jobs.IJobRepository<Guid, string>>(sp =>
{
    var jobRepo1 = new DHI.Services.Jobs.WebApi.JobRepository("[AppData]jobs.json".Resolve());
    var jobRepo2 = new DHI.Services.Jobs.WebApi.JobRepository("[AppData]jobs2.json".Resolve());

    return new ReadOnlyCompositeJobRepository(new[] { jobRepo1, jobRepo2 });
});

// Use this if you use postgreSQL for Jobs
builder.Services.AddSingleton<DHI.Services.Jobs.IJobRepository<Guid, string>>(sp =>
{
    var jobRepo1 = new DHI.Services.Provider.PostgreSQL.JobRepository("Your Connection String", logger);
    var jobRepo2 = new DHI.Services.Provider.PostgreSQL.JobRepository("Your Connection String", logger);

    return new ReadOnlyCompositeJobRepository(new[] { jobRepo1, jobRepo2 });
});

// Use this if you use DS for Jobs
builder.Services.AddSingleton<DHI.Services.Jobs.IJobRepository<Guid, string>>(sp =>
{
    var jobRepo1 = new DHI.Services.Provider.DS.JobRepository("Your BaseURI", null, 5, logger);
    var jobRepo2 = new DHI.Services.Provider.DS.JobRepository("Your BaseURI", null, 5, logger);

    return new ReadOnlyCompositeJobRepository(new[] { jobRepo1, jobRepo2 });
});

// json-based persistance
builder.Services.AddScoped<DHI.Services.Scalars.IScalarRepository<string, int>>(_ => new DHI.Services.Scalars.ScalarRepository("[AppData]scalars.json".Resolve()));

// PostgreSQL persistance
builder.Services.AddScoped<DHI.Services.Scalars.IScalarRepository<string, int>>(_ => new DHI.Services.Provider.PostgreSQL.ScalarRepository("Your Connection String", logger));

// DS provider
builder.Services.AddScoped<DHI.Services.Scalars.IScalarRepository<string, int>>(_ => new DHI.Services.Provider.DS.ScalarRepository("Your BaseURI", null, 5, logger));
builder.Services.AddSingleton<IFilterRepository>(_ => new FilterRepository("[AppData]signalr-filters.json".Resolve()));

var container = GetTriggerCatalog(configuration);
builder.Services.AddSingleton<ITriggerRepository>(new TriggerRepository(container));
# endregion

# region DHI.Services.JsonDocuments.WebApi
builder.Services.AddScoped<Microsoft.Extensions.Logging.ILogger>(provider => new SimpleLogger("[AppData]log.json".Resolve()));
builder.Services.AddSingleton<IFilterRepository>(provider => new FilterRepository("[AppData]signalr-filters.json".Resolve()));
# endregion

# region DHI.Services.Notifications.WebApi
builder.Services.AddScoped(_ => new ConnectionTypeService(AppContext.BaseDirectory));
# endregion

# region DHI.Services.Rasters.WebApi
builder.Services.AddScoped<IZoneRepository>(_ => new DHI.Services.Rasters.WebApi.ZoneRepository("[AppData]zones.json".Resolve()));
# endregion

# region DHI.Services.TimeSeries.WebApi
builder.Services.AddSingleton<IFilterRepository>(new FilterRepository("[AppData]signalr-filters.json".Resolve()));
builder.Services.AddSignalR(hubOptions => { hubOptions.EnableDetailedErrors = true; });
# endregion

// Configure the necessary services for constructor injection into controllers of DHI Domain Services Web APIs
#warning In production code, you should use replace the JSON-file based repostiories with for example the PostgreSQL repositories

var app = builder.Build();
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
app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseRouting();
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
#region DHI.Services.Documents.WebApi
// Using MCLite as a provider, single Service
var sqlitePath = Path.Combine("[AppData]".Resolve(), "MCSQLiteTest1.sqlite"); // Make sure you have MCSQLiteTest.sqlite or change it with your own .sqlite
ServiceLocator.Register(
    new DHI.Services.Documents.DocumentService<string>
    (new DHI.Services.Provider.MCLite.DocumentRepository($"database={sqlitePath};dbflavour=SQLite")),
    "mc-doc");

// Using MCLite as a provider, grouped Service
ServiceLocator.Register(
    new DHI.Services.Documents.GroupedDocumentService<string>
    (new DHI.Services.Provider.MCLite.DocumentRepository($"database={sqlitePath};dbflavour=SQLite")),
    "mclite-doc");

// Using MIKECloud as a provider, single Service (make sure to use your own apiKey and projectId)
ServiceLocator.Register(
    new DHI.Services.Documents.DocumentService<string>
    (new DHI.Services.Provider.MIKECloud.DocumentRepository("apiKey=...;projectId=...;environment=Dev")),
    "mikecloud-doc");

// Using MIKECloud as a provider, grouped Services (make sure to use your own apiKey and projectId)
ServiceLocator.Register(
    new DHI.Services.Documents.GroupedDocumentService<string>
    (new DHI.Services.Provider.MIKECloud.GroupedDocumentRepository("apiKey=...;projectId=...;environment=Dev")),
    "mikecloud-grouped-doc");

// Using LocalDirectory as a provider (Use your own localpath)
ServiceLocator.Register(
    new DHI.Services.Documents.DocumentService<string>
    (new DHI.Services.Documents.FileDocumentRepository("C:\\Services\\Documents")),
    "directory-doc");

// Using LocalDirectory as a provider for GroupedServices (Use your own localpath)
ServiceLocator.Register(
    new DHI.Services.Documents.GroupedDocumentService<string>
    (new DHI.Services.Documents.FileDocumentRepository("C:\\Services\\Documents")),
    "directory-grouped-doc");
#endregion

#region DHI.Services.GIS.WebApi
#region MCLite providers
var sqliteConn = $"database={Path.Combine("[AppData]".Resolve(), "MCSQLiteTest.sqlite")};dbflavour=SQLite";
// Using MCLite as a provider, single Service
ServiceLocator.Register(new DHI.Services.GIS.GisService<string>
    (new DHI.Services.Provider.MCLite.FeatureRepository("database=mc2014.2")),
    "mc-gis");

// Using MCLite as a provider, single updatable Service
ServiceLocator.Register(new DHI.Services.GIS.UpdatableGisService<string, Guid>
    (new DHI.Services.Provider.MCLite.FeatureRepository("database=mc2014.2")),
    "mc-gis");

// Using MCLite as a provider, grouped Service
ServiceLocator.Register(new DHI.Services.GIS.GroupedGisService<string>
    (new DHI.Services.Provider.MCLite.FeatureRepository("database=mc2014.2")),
    "mc-grouped-gis");

// Using MCLite as a provider, grouped updatable Service
ServiceLocator.Register(new DHI.Services.GIS.GroupedUpdatableGisService<string, Guid>
    (new DHI.Services.Provider.MCLite.FeatureRepository("database=mc2014.2")),
    "mc-grouped-updatable-gis");

// Using MCLite as a provider for Map Source and .json-based for MapStyle, grouped Service
ServiceLocator.Register(new DHI.Services.GIS.Maps.GroupedMapService
    (new DHI.Services.Provider.MCLite.GroupedMapSource(sqliteConn),
    new DHI.Services.GIS.Maps.MapStyleService
    (new DHI.Services.GIS.WebApi.MapStyleRepository("[AppData]styles.json".Resolve(), DHI.Services.GIS.WebApi.SerializerOptionsDefault.Options))),
    "mc-grouped-map");

// Using MCLite as a provider for Map Source and postgreSQL for MapStyle, grouped Service
ServiceLocator.Register(new DHI.Services.GIS.Maps.GroupedMapService
    (new DHI.Services.Provider.MCLite.GroupedMapSource(sqliteConn),
    new DHI.Services.GIS.Maps.MapStyleService
    (new DHI.Services.Provider.PostgreSQL.MapStyleRepository("Your ConnectionString", logger))),
    "mc-grouped-postgre-map");
#endregion

#region MIKECore providers
// Map using MIKECore DFSU as a provider
ServiceLocator.Register(new DHI.Services.GIS.Maps.MapService
    (new DHI.Services.Provider.MIKECore.DfsuMapSource("[AppData]"),
    new DHI.Services.GIS.Maps.MapStyleService
    (new DHI.Services.GIS.Maps.MapStyleRepository("[AppData]styles.json".Resolve(), DHI.Services.GIS.WebApi.SerializerOptionsDefault.Options))),
    "dfsu-map");

// Map using MIKECore DFS2 as a provider
ServiceLocator.Register(new DHI.Services.GIS.Maps.MapService
    (new DHI.Services.Provider.MIKECore.Dfs2MapSource("[AppData]dfs2\\R20141001.dfs2".Resolve()),
    new DHI.Services.GIS.Maps.MapStyleService
    (new DHI.Services.GIS.Maps.MapStyleRepository("[AppData]styles.json".Resolve(), DHI.Services.GIS.WebApi.SerializerOptionsDefault.Options))),
    "dfs2-map");

// Map using MIKECore DFSU as a provider, postgre provider
ServiceLocator.Register(new DHI.Services.GIS.Maps.MapService
    (new DHI.Services.Provider.MIKECore.DfsuMapSource("[AppData]"),
    new DHI.Services.GIS.Maps.MapStyleService
    (new DHI.Services.Provider.PostgreSQL.MapStyleRepository("Your Connection String", logger))),
    "dfsu-map-postgre");

// Map using MIKECore DFS2 as a provider, postgre provider
ServiceLocator.Register(new DHI.Services.GIS.Maps.MapService
    (new DHI.Services.Provider.MIKECore.Dfs2MapSource("[AppData]dfs2\\R20141001.dfs2".Resolve()),
    new DHI.Services.GIS.Maps.MapStyleService
    (new DHI.Services.Provider.PostgreSQL.MapStyleRepository("Your Connection String", logger))),
    "dfs2-map-postgre");

// GIS using MIKECore DFS2
ServiceLocator.Register(new DHI.Services.GIS.GisService<string>
    (new DHI.Services.Provider.MIKECore.Dfs2FeatureRepository("[AppData]dfs2\\R20141001.dfs2".Resolve())),
    "dfs2-gis");

// GIS using MIKECore DFSU
ServiceLocator.Register(new DHI.Services.GIS.GisService<string>
    (new DHI.Services.Provider.MIKECore.DfsuFeatureRepository("[AppData]")),
    "dfsu-gis");

// If you want to have cache mechanism for Map, you can use injection below. Note, grouped doesn't support cache mechanism yet.
var parameters = new Parameters
{
    { "NameInfo", "Some info" },
    { "KeywordInfo", "keyword1,keyword2" },
    { "CachedImageWidth", "100" },
    { "CacheExpirationInMinutes", "2" }
};

// Map using MIKECore DFS2 as a provider for Map Source and .json-based for MapStyle, single Service, add cache mechanism
ServiceLocator.Register(new DHI.Services.GIS.Maps.MapService(
    new DHI.Services.GIS.Maps.CachedMapSource
    (new DHI.Services.Provider.MIKECore.Dfs2MapSource("[AppData]dfs2\\R20141001.dfs2".Resolve()), parameters),
    new DHI.Services.GIS.Maps.MapStyleService
    (new DHI.Services.GIS.WebApi.MapStyleRepository("[AppData]styles.json".Resolve(), DHI.Services.GIS.WebApi.SerializerOptionsDefault.Options))),
    "dfs2-map-cached");

// Map using MIKECore DFS2 as a provider for Map Source and .json-based for MapStyle, single Service, add file cache mechanism
ServiceLocator.Register(new DHI.Services.GIS.Maps.MapService(
    new DHI.Services.GIS.Maps.FileCachedMapSource
    (new DHI.Services.Provider.MIKECore.Dfs2MapSource("[AppData]dfs2\\R20141001.dfs2".Resolve()), parameters),
    new DHI.Services.GIS.Maps.MapStyleService
    (new DHI.Services.GIS.WebApi.MapStyleRepository("[AppData]styles.json".Resolve(), DHI.Services.GIS.WebApi.SerializerOptionsDefault.Options))),
    "dfs2-map-file-cached-postgre");

// Map using MIKECore DFS2 as a provider for Map Source and postgreSQL for MapStyle, single Service, add cache mechanism
ServiceLocator.Register(new DHI.Services.GIS.Maps.MapService(
    new DHI.Services.GIS.Maps.CachedMapSource
    (new DHI.Services.Provider.MIKECore.Dfs2MapSource("[AppData]dfs2\\R20141001.dfs2".Resolve()), parameters),
    new DHI.Services.GIS.Maps.MapStyleService
     (new DHI.Services.Provider.PostgreSQL.MapStyleRepository("Your ConnectionString", logger))),
    "dfs2-map-cached");

// Map using MIKECore DFS2 as a provider for Map Source and postgreSQL for MapStyle, single Service, add file cache mechanism
ServiceLocator.Register(new DHI.Services.GIS.Maps.MapService(
    new DHI.Services.GIS.Maps.FileCachedMapSource
    (new DHI.Services.Provider.MIKECore.Dfs2MapSource("[AppData]dfs2\\R20141001.dfs2".Resolve()), parameters),
    new DHI.Services.GIS.Maps.MapStyleService
     (new DHI.Services.Provider.PostgreSQL.MapStyleRepository("Your ConnectionString", logger))),
    "dfs2-map-file-cached-postgre");
#endregion

#region ShapeFile Provider
ServiceLocator.Register(new DHI.Services.GIS.GisService<string>
    (new DHI.Services.Provider.ShapeFile.FeatureRepository("[AppData]shp")),
    "gis-shape");
#endregion

# region GeoServer Provider
// Using MCLite as a provider for Map Source and GeoServer for MapStyle, grouped Service
ServiceLocator.Register(new DHI.Services.GIS.Maps.MapService
    (new DHI.Services.Provider.GeoServer.MapSource("BaseUrl=...;Query=...;UserName=...;Password=...;"),
    new DHI.Services.GIS.Maps.MapStyleService
    (new DHI.Services.Provider.GeoServer.MapStyleRepository("BaseUrl=...;UserName=...;Password=..."))),
    "geoserver-map");
# endregion

# region MIKECloud Provider
// Using MCLite as a provider, grouped Service
ServiceLocator.Register(new DHI.Services.GIS.GroupedGisService<string>
    (new DHI.Services.Provider.MIKECloud.GroupedGisRepository("apiKey=...;projectId=...;environment=Dev")),
    "mc-grouped-gis");
# endregion
#endregion

#region DHI.Services.Jobs.WebApi
// Using json-based persistance for Jobs
ServiceLocator.Register(
                new DHI.Services.Jobs.JobService<DHI.Services.Jobs.Workflows.Workflow, string>(
                    new DHI.Services.Jobs.WebApi.JobRepository("[AppData]jobs.json".Resolve()),
                    new DHI.Services.Jobs.TaskService<DHI.Services.Jobs.Workflows.Workflow, string>
                    (new DHI.Services.Jobs.Workflows.WorkflowRepository("[AppData]workflows.json".Resolve(), DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options.Converters)),
                    null),
                "wf-jobs"
                );

// Using postgreSQL persistance for Jobs
ServiceLocator.Register(
                new DHI.Services.Jobs.JobService<DHI.Services.Jobs.Workflows.Workflow, string>(
                    new DHI.Services.Provider.PostgreSQL.JobRepository("Your Connection String", logger),
                    new DHI.Services.Jobs.TaskService<DHI.Services.Jobs.Workflows.Workflow, string>
                    (new DHI.Services.Jobs.Workflows.WorkflowRepository("[AppData]workflows.json".Resolve(), DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options.Converters)),
                    null),
                "wf-jobs-postgre"
                );

// Using DS provider for Jobs
ServiceLocator.Register(
                new DHI.Services.Jobs.JobService<DHI.Services.Jobs.Workflows.Workflow, string>(
                    new DHI.Services.Provider.DS.JobRepository("Your BaseURL", null, 5, logger),
                    new DHI.Services.Jobs.TaskService<DHI.Services.Jobs.Workflows.Workflow, string>
                    (new DHI.Services.Jobs.Workflows.WorkflowRepository("[AppData]workflows.json".Resolve(), DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options.Converters)),
                    null),
                "wf-jobs-postgre"
                );

// Using json-based persistance for Task
ServiceLocator.Register(
    new DHI.Services.Jobs.TaskService<DHI.Services.Jobs.Workflows.Workflow, string>
    (new DHI.Services.Jobs.Workflows.WorkflowRepository("[AppData]workflows.json".Resolve())),
    "wf-tasks"
    );

// Using json-based persistance for Scenario
ServiceLocator.Register(
        new DHI.Services.Jobs.Scenarios.ScenarioService
        (new DHI.Services.Jobs.Scenarios.ScenarioRepository("[AppData]scenarios.json".Resolve(), DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options.Converters),
        new DHI.Services.Jobs.WebApi.JobRepository("[AppData]jobs.json".Resolve())
        ),
    "json-scenarios"
    );

// Using postgreSQL persistance for Scenario
ServiceLocator.Register(
        new DHI.Services.Jobs.Scenarios.ScenarioService
        (new DHI.Services.Provider.PostgreSQL.ScenarioRepository("Your Connection String", logger),
        new DHI.Services.Jobs.WebApi.JobRepository("[AppData]jobs.json".Resolve())
        ),
    "json-scenarios-postgre"
    );

// Using new CodeWorkflow for workflow
var workflowRepository = new DHI.Services.Jobs.Workflows.CodeWorkflowRepository("[AppData]workflows2.json".Resolve());
var workflowService = new DHI.Services.Jobs.Workflows.CodeWorkflowService(workflowRepository);
ServiceLocator.Register(workflowService, "wf-tasks2");

// Using new CodeWorkflow for Jobs
var jobRepository = new DHI.Services.Jobs.WebApi.JobRepository("[AppData]jobs2.json".Resolve());
var jobService = new DHI.Services.Jobs.JobService<CodeWorkflow, string>(jobRepository, workflowService);
ServiceLocator.Register(jobService, "wf-jobs2");

// Using new CodeWorkflow for workflow using DS provider
var dsWorkflow = new DHI.Services.Provider.DS.CodeWorkflowRepository("[Your BaseURL", null, 5, logger);
workflowService = new DHI.Services.Jobs.Workflows.CodeWorkflowService(dsWorkflow);
ServiceLocator.Register(workflowService, "wf-tasks3");
#endregion

# region DHI.Services.JsonDocuments.WebApi
ServiceLocator.Register(new DHI.Services.JsonDocuments.JsonDocumentService
    (new DHI.Services.Provider.PostgreSQL.JsonDocumentRepositorySecured("Your ConnectionString", logger)),
    "json-documents-postgre");
# endregion

# region DHI.Services.Logging.WebApi
ServiceLocator.Register(new DHI.Services.Logging.WebApi.LogService
    (new DHI.Services.Logging.WebApi.ClefLogRepository("[AppData]".Resolve())), "json-logger");
# endregion

# region DHI.Services.Meshes.WebApi
ServiceLocator.Register(new DHI.Services.Meshes.GroupedMeshService
    (new DHI.Services.Provider.MIKECore.DfsuMeshRepository(new FileSource("[AppData]\\dfsu".Resolve()))),
    "meshes-dfsu");
# endregion

# region DHI.Services.Models.WebApi
var modelService = new DHI.Services.Models.ModelDataReaderService
    (new DHI.Services.Models.ModelDataReaderRepository("[AppData]models.json".Resolve(),
    DHI.Services.Models.WebApi.SerializerOptionsDefault.Options.Converters));
ServiceLocator.Register(modelService, "json-models");
# endregion

# region DHI.Services.Notifications.WebApi
ServiceLocator.Register(new DHI.Services.Notifications.NotificationService
    (new DHI.Services.Notifications.WebApi.NotificationRepository("[AppData]notifications.json".Resolve())),
    "json-notifications");

ServiceLocator.Register(new DHI.Services.Notifications.NotificationService
    (new DHI.Services.Provider.PostgreSQL.NotificationRepository("Your Connection String", logger)),
    "postgre-notifications");
# endregion

# region DHI.Services.Places.WebApi
// Places.WebApi needs TimeSeries Service, ScalarService, and GisService. This example will only use .json-based persistance, shapefile, and csv for those three services.
// You can check on how to use other Providers for those three services in their own region.

ServiceLocator.Register(new DiscreteTimeSeriesService<string, double>
    (new DHI.Services.TimeSeries.CSV.TimeSeriesRepository("[AppData]".Resolve())),
    "time-series-csv-places");

ServiceLocator.Register(
    new GisService<string>(new DHI.Services.Provider.ShapeFile.FeatureRepository("[AppData]shp".Resolve())),
    "gis-shape-places");

Dictionary<string, IDiscreteTimeSeriesService<string, double>> timeSeriesServices = new Dictionary<string, IDiscreteTimeSeriesService<string, double>>();
timeSeriesServices.Add("time-series-csv-places", Services.Get<IDiscreteTimeSeriesService<string, double>>("time-series-csv-places"));

Dictionary<string, IScalarService<string, int>> scalarServices = new Dictionary<string, IScalarService<string, int>>();
var gisService = Services.Get<IGisService<string>>("gis-shape-places");

// Json-based persistance
ServiceLocator.Register(new PlaceService(new DHI.Services.Places.PlaceRepository("[AppData]places.json".Resolve()),
    timeSeriesServices, scalarServices, gisService),
    "places-json");

// MCLite persistance
ServiceLocator.Register(new PlaceService(new DHI.Services.Provider.MCLite.PlaceRepository("database=mc2014.2"),
    timeSeriesServices, scalarServices, gisService),
    "places-mclite");

// PostgreSQL persistance
ServiceLocator.Register(new PlaceService(new DHI.Services.Provider.PostgreSQL.PlaceRepository("Your Connection String", logger),
    timeSeriesServices, scalarServices, gisService),
    "places-postgre");
# endregion

# region DHI.Services.Rasters.WebApi
ServiceLocator.Register(new RadarImageService<AsciiImage>
    (new DelimitedAsciiRepository("[AppData]RadarImages;PM_{datetimeFormat}.txt;yyyyMMddHH_$$$".Resolve())),
    "ascii");
# endregion

# region DHI.Services.Scalars.WebApi
// json-based persistance
ServiceLocator.Register(new DHI.Services.Scalars.GroupedScalarService<string, int>
    (new DHI.Services.Scalars.ScalarRepository("[AppData]scalars.json".Resolve()),
    new SimpleLogger("[AppData]scalars.log".Resolve())),
    "scalars-json");

// DS provider scalars
ServiceLocator.Register(new DHI.Services.Scalars.GroupedScalarService<string, int>
    (new DHI.Services.Provider.DS.ScalarRepository("Your Base URI", null, 5, logger),
    new SimpleLogger("[AppData]scalars.log".Resolve())),
    "scalars-ds");

// PostgreSQL provider scalars
ServiceLocator.Register(new DHI.Services.Scalars.GroupedScalarService<string, int>
    (new DHI.Services.Provider.PostgreSQL.ScalarRepository("Your Connection String", logger),
    new SimpleLogger("[AppData]scalars.log".Resolve())),
    "scalars-postgre");
# endregion

# region DHI.Services.Spreadsheets.WebApi
// OpenXML provider
ServiceLocator.Register(new SpreadsheetService
    (new DHI.Services.Provider.OpenXML.SpreadsheetRepository("[AppData]".Resolve())),
    "spreadsheets-xlsx");

// MCLite provider
ServiceLocator.Register(new SpreadsheetService
    (new DHI.Services.Provider.MCLite.SpreadsheetRepository("database=mc2014.2")),
    "spreadsheets-mclite");
# endregion

# region DHI.Services.TimeSeries.WebApi
ServiceLocator.Register(
    new DHI.Services.TimeSeries.GroupedDiscreteTimeSeriesService<string, double>
    (new DHI.Services.TimeSeries.CSV.TimeSeriesRepository("[AppData]csv".Resolve())),
    "timeseries-csv");

ServiceLocator.Register(
    new CoreTimeSeriesService(new DHI.Services.TimeSeries.Daylight.TimeSeriesRepository()),
    "timeseries-daylight");

// For testing SignalR
var timeSeriesList = new List<TimeSeries> { new TimeSeries("myTimeSeries", "My Time Series") };
var repository = new InMemoryTimeSeriesRepository<string, double>(timeSeriesList);
var service = new UpdatableTimeSeriesService(repository);
ServiceLocator.Register(service, "timeseries-myTsConnection");

// For testing daylight time series repository
var daylightService = new CoreTimeSeriesService(new DHI.Services.TimeSeries.Daylight.TimeSeriesRepository());
ServiceLocator.Register(daylightService, "timeseries-daylight-ts");

// For testing logger injection into time series service
var tsRepo = new DHI.Services.TimeSeries.CSV.TimeSeriesRepository("[AppData]csv".Resolve());
var tsService = new GroupedDiscreteTimeSeriesService(tsRepo, new SimpleLogger("[AppData]Log.log".Resolve()));
ServiceLocator.Register(tsService, "timeseries-csv2");

// MCLite GroupedUpdatable
ServiceLocator.Register(new DHI.Services.TimeSeries.GroupedUpdatableTimeSeriesService<string, double>
    (new DHI.Services.Provider.MCLite.TimeSeriesRepository("database=mc2014.2")),
    "timeseries-mclite-grouped-updatable");

// MCLite Discrete
ServiceLocator.Register(new DHI.Services.TimeSeries.DiscreteTimeSeriesService<string, double>
    (new DHI.Services.Provider.MCLite.TimeSeriesFromSpreadsheetRepository("database=mc2014.2")),
    "timeseries-mclite-discrete-updatable");

// MIKECloud Grouped Updatable
ServiceLocator.Register(new DHI.Services.TimeSeries.GroupedUpdatableTimeSeriesService<string, double>
    (new DHI.Services.Provider.MIKECloud.GroupedTimeSeriesRepository("apiKey=...;projectId=...;environment=Dev")),
    "timeseries-mikecloud-grouped-updatable");

// DFS0 Grouped Updatable (MIKECore provider)
ServiceLocator.Register(new DHI.Services.TimeSeries.GroupedUpdatableTimeSeriesService<string, double>
    (new DHI.Services.Provider.MIKECore.Dfs0GroupedTimeSeriesRepository("[AppData]".Resolve())),
    "timeseries-dfs0-grouped-updatable");

// DFS0 Updatable (MIKECore provider)
ServiceLocator.Register(new DHI.Services.TimeSeries.UpdatableTimeSeriesService<string, double>
    (new DHI.Services.Provider.MIKECore.Dfs0TimeSeriesRepository("[AppData]TaarbaekRev_Spectral.dfs0".Resolve())),
    "timeseries-dfs0-updatable");

// DFS2 (MIKECore provider)
ServiceLocator.Register(new DHI.Services.TimeSeries.TimeSeriesService<string, double>
    (new DHI.Services.Provider.MIKECore.Dfs2TimeSeriesRepository("[AppData]R20141001.dfs2".Resolve())),
    "timeseries-dfs2");

// DFSU (MIKECore provider)
ServiceLocator.Register(new DHI.Services.TimeSeries.TimeSeriesService<string, double>
    (new DHI.Services.Provider.MIKECore.DfsuTimeSeriesRepository("[AppData]KBHEC3dF012.dfsu".Resolve())),
    "timeseries-dfsu");

// MIKE1D Discrete
ServiceLocator.Register(new DHI.Services.TimeSeries.DiscreteTimeSeriesService<string, double>
    (new DHI.Services.Provider.MIKE1D.ResultFileTimeSeriesRepository(".res1d")),
    "timeseries-mike1d");

// MIKE1D Grouped Discrete
ServiceLocator.Register(new DHI.Services.TimeSeries.GroupedDiscreteTimeSeriesService<string, double>
    (new DHI.Services.Provider.MIKE1D.ResultFileGroupedTimeSeriesRepository("[AppData]".Resolve())),
    "timeseries-mike1d-grouped");

// USGS
ServiceLocator.Register(new DHI.Services.TimeSeries.TimeSeriesService<string, double>
    (new DHI.Services.Provider.USGS.TimeSeriesRepository("Base Address")),
    "timeseries-usgs-grouped");
# endregion

# region DHI.Services.TimeSteps.WebApi
ServiceLocator.Register(new TimeStepService<string, object>(new Dfs2TimeStepServer("[AppData]R20141001.dfs2".Resolve())), "dfs2");
# endregion
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