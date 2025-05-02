using System.Text.Json;
using System.Text.Json.Serialization;
using DHI.Services;
using DHI.Services.GIS.Maps;
using DHI.Services.Provider.MIKECloud;
using DHI.Services.TimeSeries;
using DHI.Services.TimeSeries.Converters;
using DHI.Services.WebApiCore;
using DHI.Spatial.GeoJson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using MIKECloudWebApi;
using Swashbuckle.AspNetCore.SwaggerUI;
using MapStyleRepository = DHI.Services.GIS.WebApi.MapStyleRepository;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var configuration = builder.Configuration;

// Authentication
#warning Modify the settings in the "AzureAd" section of appsettings.json
builder.Services.AddMicrosoftIdentityWebApiAuthentication(configuration);

// Authorization
#warning For more information on how to configure authorization policies, see https://developerdoc-mike-platform-prod.eu.mike-cloud.com/domain_services/web-api-mikecloud-authentication/#configuring-mike-cloud-authentication-in-a-web-api
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
builder.Services
    .AddResponseCompression(opts => opts.EnableForHttps = true)
    .AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.WriteIndented = true;

        opts.JsonSerializerOptions.DefaultIgnoreCondition
            = SerializerOptionsDefault.Options.DefaultIgnoreCondition;
        opts.JsonSerializerOptions.PropertyNamingPolicy
            = SerializerOptionsDefault.Options.PropertyNamingPolicy;
        opts.JsonSerializerOptions
               .AddConverters(SerializerOptionsDefault.Options.Converters);
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

#warning Depending on which Web API packages you install in this project, you need to register the XML-files from these packages
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.Documents.WebApi.xml"));
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.GIS.WebApi.xml"));
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.TimeSeries.WebApi.xml"));

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
#warning Enable if using SignalR capabilities
//builder.Services.AddSignalR();

// Configure the necessary services for constructor injection into controllers of DHI Domain Services Web APIs
#warning In production code, you should use replace the JSON-file based repostiories with for example the PostgreSQL repositories
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddScoped<IMapStyleRepository>(_ => new MapStyleRepository("styles.json"));

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

app.UseAuthentication();
app.UseHttpsRedirection();
app.UseExceptionHandling();
app.UseResponseCompression();
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
#warning Enable if using SignalR capabilities
    //endpoints.MapHub<NotificationHub>("/notificationhub");
});

// DHI Domain Services
// Set the data directory (App_Data folder)
var contentRootPath = configuration.GetValue("AppConfiguration:ContentRootPath", app.Environment.ContentRootPath);
AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(contentRootPath, "App_Data"));

// Register services
var apiKey = new Guid("90892a37-668b-4be8-bd7c-cc33eea4eda1");  // API key has to be renewed April 2023
var projectId = new Guid("536f2d5c-988f-423d-84c4-f1a2a0e07afe");  // https://dataadmin.mike-cloud.com/project/536f2d5c-988f-423d-84c4-f1a2a0e07afe
var timeSeriesRepository = new GroupedTimeSeriesRepository(apiKey, projectId);
ServiceLocator.Register(new GroupedDiscreteTimeSeriesService(timeSeriesRepository), "mikecloud-ts");

#warning Add more services as needed

app.Run();