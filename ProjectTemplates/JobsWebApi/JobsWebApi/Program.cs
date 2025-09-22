using System.ComponentModel.Composition.Hosting;
using System.Text.Json.Serialization;
using DHI.Services;
using DHI.Services.Filters;
using DHI.Services.Jobs;
using DHI.Services.Jobs.Automations;
using DHI.Services.Jobs.WebApi;
using DHI.Services.Jobs.Workflows;
using DHI.Services.Logging.WebApi;
using DHI.Services.Notifications;
using DHI.Services.Provider.PostgreSQL;
using DHI.Services.Scalars;
using DHI.Services.WebApiCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Extensions.Logging;
using Swashbuckle.AspNetCore.SwaggerUI;
using JobsWebApi;
using ILogger = Microsoft.Extensions.Logging.ILogger;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var seriLogger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

var logger = new SerilogLoggerFactory(seriLogger)
   .CreateLogger<Program>();

builder.Services.AddSingleton<ILogger>(logger);

builder.Services.AddSerilog(seriLogger);

bool allowAnonymousAccess = bool.Parse(configuration["AllowAnonymousAccess"]);

var postgresJobs = new List<PostgresJobRepo>();
configuration.GetSection("Postgres-jobs").Bind(postgresJobs);

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
               ClockSkew = TimeSpan.Zero  // Default value is a 5 minutes delay
           };
           options.Events = new JwtBearerEvents
           {
               OnMessageReceived = context =>
               {
                   var accessToken = context.Request.Query["access_token"];
                   var path = context.HttpContext.Request.Path;
                   if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationhub"))
                   {
                       context.Token = accessToken;
                   }

                   return Task.CompletedTask;
               }
           };
       });

// Authorization
if (allowAnonymousAccess)
{
    builder.Services.AddSingleton<IAuthorizationHandler, AllowAnonymousHandler>();
}

//// Authorization
//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("AdministratorsOnly", policy => policy.RequireClaim(ClaimTypes.GroupSid, "Administrators"));
//    options.AddPolicy("EditorsOnly", policy => policy.RequireClaim(ClaimTypes.GroupSid, "Editors"));
//});

// bypass the checking of admin and editor user group roles and allow usage as long as user is authenticated
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdministratorsOnly", policy => policy.RequireAuthenticatedUser());
    options.AddPolicy("EditorsOnly", policy => policy.RequireAuthenticatedUser());
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
                         .AllowAnyMethod()
                         .AllowCredentials();
               });
       })
       .AddResponseCompression(options => { options.EnableForHttps = true; })
       .AddControllers()
       .AddJsonOptions(options =>
       {
           options.JsonSerializerOptions.DefaultIgnoreCondition = SerializerOptionsDefault.Options.DefaultIgnoreCondition;
           options.JsonSerializerOptions.PropertyNamingPolicy = SerializerOptionsDefault.Options.PropertyNamingPolicy;
           options.JsonSerializerOptions.AddConverters(SerializerOptionsDefault.Options.Converters);
           options.JsonSerializerOptions.WriteIndented = true;
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

// DHI Services
var jobAutomationsPath = configuration["JobAutomations"];
var postgreSqlScalarsConnectionString = configuration["Postgres-scalars:ConnectionString"];
builder.Services.AddScoped<IAutomationRepository>(_ => new DirectoryAutomationRepository(jobAutomationsPath));
builder.Services.AddSingleton<IJobRepository<Guid, string>>(sp =>
{
    var logger = sp.GetRequiredService<ILogger>();
    var jobRepositories = postgresJobs
        .Select(pj => new DHI.Services.Provider.PostgreSQL.JobRepository(pj.JobRepositoryConnectionString, logger))
        .ToList<IJobRepository<Guid, string>>();

    return new ReadOnlyCompositeJobRepository(jobRepositories);
});
builder.Services.AddScoped<IScalarRepository<string, int>>(_ => new DHI.Services.Provider.PostgreSQL.ScalarRepository(postgreSqlScalarsConnectionString));
builder.Services.AddScoped<ILogger>(_ => new SimpleLogger("[AppData]log.log".Resolve()));

// SignalR
builder.Services.AddSignalR(hubOptions =>
{
    hubOptions.EnableDetailedErrors = true;
}).AddJsonProtocol(options => {
    options.PayloadSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.PayloadSerializerOptions.WriteIndented = true;
    options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
});

// Configure the necessary services for constructor injection into controllers of DHI Domain Services Web APIs
// use grouped host setup - hostgroup is no longer defined here but as part of Job Orchestrator;
//builder.Services.AddScoped<IHostRepository>(_ => new GroupedHostRepository("hostGroups.json"));

//var postgreSqlConnectionString = "[env:PostgreSqlConnectionString]".Resolve();
var postgreSqlConnectionString = configuration["Postgres-webAPI:ConnectionString"];
var postgreSqlFilterConnectionString = configuration["Postgres-filter:ConnectionString"];
var postgreSqlLogConnectionString = configuration["Postgres-log:ConnectionString"];

// VDJ: need to create Microsoft ILogger for postgresDB Logging - for Web API logging
builder.Services.AddSingleton<IFilterRepository>(_ => new DHI.Services.Provider.PostgreSQL.FilterRepository(postgreSqlFilterConnectionString));
builder.Services.AddScoped<INotificationRepository>(_ => new DHI.Services.Provider.PostgreSQL.NotificationRepository(postgreSqlLogConnectionString));

// Triggers
var container = GetTriggerCatalog(configuration);
builder.Services.AddSingleton<ITriggerRepository>(new TriggerRepository(container));

var app = builder.Build();

// setup swagger
app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var specificationName = configuration["Swagger:SpecificationName"];
    options.SwaggerEndpoint($"../swagger/{specificationName}/swagger.json", configuration["Swagger:DocumentName"]);
    options.DocExpansion(DocExpansion.None);
    options.DefaultModelsExpandDepth(-1);
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseSerilogRequestLogging();

app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseHttpsRedirection();
app.UseExceptionHandling();
app.UseResponseCompression();
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<NotificationHub>("/notificationhub");
});

// DHI Domain Services
// Set the data directory (App_Data folder)
var contentRootPath = configuration.GetValue("AppConfiguration:ContentRootPath", app.Environment.ContentRootPath);
AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(contentRootPath, "App_Data"));

// Register log services
string jobLogPath = configuration["JobLogPath"];
var jobLogRespository = new ClefLogRepository(jobLogPath);
ServiceLocator.Register(new LogService(jobLogRespository), "wf-logs");

// add workflow respository - for checking if the workflow job submitted exists in the system and has the matching workflow parameters
var workflowRepoStr = configuration["TaskRepositoryConnectionString"].Resolve();
var workflowRepository = new CodeWorkflowRepository(workflowRepoStr);
var workflowService = new CodeWorkflowService(workflowRepository);
ServiceLocator.Register(workflowService, "wf-tasks");

// add job respositories for each host group
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

foreach (var postgresJob in postgresJobs)
{
    var jobRepository = new DHI.Services.Provider.PostgreSQL.JobRepository(postgresJob.JobRepositoryConnectionString, logger);
    var jobService = new JobService<CodeWorkflow, string>(jobRepository, workflowService);
    ServiceLocator.Register(jobService, postgresJob.Id);
}

ServiceLocator.Register(new ScalarService<int>(new DHI.Services.Provider.PostgreSQL.ScalarRepository(postgreSqlScalarsConnectionString), new SimpleLogger("[AppData]scalars.log".Resolve())), "wf-scalars");

try
{
    app.Run();
}
finally
{
    var logServices = ServiceLocator.GetAll<LogService>().ToList();
    logServices.ForEach(ls => ls.Dispose());
}

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
