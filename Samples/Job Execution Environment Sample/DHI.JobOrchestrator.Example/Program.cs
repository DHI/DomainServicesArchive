using DHI.JobOrchestrator.Docker;
using DHI.JobOrchestrators;
using DHI.JobOrchestratorService.Docker;
using DHI.JobOrchestratorService.Settings;
using DHI.Services;
using DHI.Services.Jobs;
using DHI.Services.Jobs.Orchestrator;
using DHI.Services.Jobs.Workflows;
using DHI.Services.Jobs.WorkflowWorker;
using Markdig;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

// Get configuration values
IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)    
    .AddEnvironmentVariables()
    .AddUserSecrets<AppSettings>()
    .Build();

// Bind the AppSettings
var appSettings = new AppSettings();
configuration.GetSection("AppSettings").Bind(appSettings);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(s => 
    s.GetRequiredService<ILoggerFactory>().AddSeq(serverUrl: "http://seq").CreateLogger("JobOrchestrator")
);

// Data Protection
// This is to resolve the following error:
// Storing keys in a directory '/root/.aspnet/DataProtection-Keys' that may not be persisted outside of the container. Protected data will be unavailable when container is destroyed.
// See: https://jakeydocs.readthedocs.io/en/latest/security/data-protection/configuration/overview.html
// Also See: https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/introduction?view=aspnetcore-3.1
if (!string.IsNullOrEmpty(appSettings.DataProtectionFolderPath))
{
    // Log.Information("Data Protection Path: {path}", appSettings.DataProtectionFolderPath);
    builder.Services.AddDataProtection()
        .SetApplicationName(appSettings.ApplicationName)
        .PersistKeysToFileSystem(new DirectoryInfo(appSettings.DataProtectionFolderPath))
        .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
        {
            EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
            ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
        });
}

builder.Services.AddSingleton(_ => appSettings);

builder.Services.AddSingleton<JobOrchestrator>(c =>
{
    var service = c.GetRequiredService<JobOrchestratorFactory>();
    return service.Create();
});

builder.Services.AddSingleton(b => {
    var config = b.GetRequiredService<AppSettings>();
    var result = new Dictionary<string, CodeWorkflowService>();
    foreach (var kvp in config.Workers)
    {
        var fileName = $"DHI.Workflow.Engine.Example.json";
        var workflowRepository = new CodeWorkflowRepository(fileName);
        var workflows = new CodeWorkflowService(workflowRepository);

        result.Add(kvp.Key, workflows);
    }
    return result;
});

builder.Services.AddSingleton(b => {
    var config = b.GetRequiredService<AppSettings>();
    
    var workflowServices = b.GetRequiredService<Dictionary<string, CodeWorkflowService>>();
    var result = new Dictionary<string, JobService<CodeWorkflow, string>>();
    var logger = b.GetRequiredService<Microsoft.Extensions.Logging.ILogger>();
    foreach (var kvp in config.Workers)
    {
        var jobRepository = new DHI.Services.Provider.PostgreSQL.JobRepository(kvp.Value.JobRepositoryConnectionString);
        var jobService = new JobService<CodeWorkflow, string>(jobRepository, workflowServices[kvp.Key]);
        result.Add(kvp.Key, jobService);
    }

    return result;
});

builder.Services.AddSingleton<SignalRHostService>(c => new SignalRHostService(c.GetRequiredService<ISignalRHostCollection>(), appSettings.HostGroups!));
builder.Services.AddSingleton<JobOrchestratorFactory>();

builder.Services.AddSignalR(o =>
{
    o.EnableDetailedErrors = true;
});
builder.Services.AddHostedService<MonitoringBackgroundService>();
DependencyInjection.Adds(builder.Services, new LoggerFactory().AddSeq(serverUrl: "http://seq").CreateLogger("JobOrchestrator"));

builder.Services.AddSingleton<MarkdownPipeline>(s => new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build());
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = RSA.BuildSigningKey(configuration["Tokens:PublicRSAKey"])
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Headers["Authorization"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/WorkerHub")))
                {
                    context.Token = accessToken.ToString().Replace("Bearer ", string.Empty);

                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                return Task.CompletedTask;
            },
            OnForbidden = context =>
            {
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                return Task.CompletedTask;
            }

        };
    });
var app = builder.Build();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<WorkerHub>("/WorkerHub");

/*
 Following minimal api endpoints support this demonstation case, but should not be used in production.
 */
app.MapGet("/", (MarkdownPipeline pipeline) =>
{
    var readme = File.ReadAllText("../README.md");
    var html = Markdown.ToHtml(readme, pipeline);
    return Results.Text(html, "text/html");
});

app.MapGet("/job/queue/{taskId}", (Dictionary<string, JobService<CodeWorkflow, string>> workflowServices, string taskId) =>
{
    var id = Guid.NewGuid();

    var job = new Job<Guid, string>(id, taskId)
    {
        Status = JobStatus.Pending,
        HostGroup = "code"
    };
    job.Parameters.Add("WorkflowTimeout", TimeSpan.FromSeconds(5));
    job.Parameters.Add("TerminationGracePeriod", TimeSpan.FromSeconds(10));

    workflowServices.First().Value.Add(job);


    return Results.Redirect("/");
});

app.MapPut("/api/jobs/{connection}/heartbeat/{jobId}", (string connection, Guid jobId, [FromServices]Dictionary<string, JobService<CodeWorkflow, string>> jobServices) =>
{
    jobServices.First().Value.UpdateHeartbeat(jobId);
});

app.MapGet("/job/cancel", ([FromServices] Dictionary<string, JobService<CodeWorkflow, string>> jobServices) =>
{
    var jobService = jobServices.First().Value;
    var job = jobService.GetLast();
    job.Status = JobStatus.Cancel;
    jobService.Update(job);
    return Results.Redirect("/");
});

app.MapPut("/api/jobs/{connection}/status/{jobId}", (string connection, Guid jobId, [FromBody] JobStatusUpdateDTO jobStatusUpdateDTO, [FromServices]Dictionary<string, JobService<CodeWorkflow, string>> jobServices) => 
{
    jobServices.First().Value.UpdateStatus(jobId, jobStatusUpdateDTO.JobStatus, progress: jobStatusUpdateDTO.Progress, statusMessage: jobStatusUpdateDTO.StatusMessage);
});

app.MapPost("/tokens", (AuthDTO auth) => {
    return new Outer()
    {
        AccessToken = new Token()
        {
            BearerToken = "Bearer SomeToken",
            Expiration = DateTime.UtcNow.AddHours(1)
        }
    };
});
app.MapGet("/healthcheck", async (SignalRHostService signalRHostService, AppSettings appSettings) =>
{
    var hosts = signalRHostService.GetByGroups(appSettings.HostGroups);

    if (appSettings.HostGroups!.All(hg => hosts.Any(h => h.Group == hg)))
    {
        return Results.Ok();
    }

    return Results.StatusCode(500);
});


await app.RunAsync();

public class JobStatusUpdateDTO
{
    public int? Progress { get; set; }

    public string StatusMessage { get; set; }

    public JobStatus JobStatus { get; set; }
}

public class AuthDTO
{
    public string Id { get; set; }
    public string Password { get; set; }
}

public class Outer
{
    public Token AccessToken { get; set; }
}

public class Token
{
    [JsonPropertyName("token")]
    public string BearerToken { get; set; }

    public DateTime Expiration { get; set; }
}