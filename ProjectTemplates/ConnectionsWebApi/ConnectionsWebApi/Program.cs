using System.Security.Claims;
using DHI.Services;
using DHI.Services.WebApiCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using ConnectionRepository = DHI.Services.Connections.WebApi.ConnectionRepository;
using Serilog.Extensions.Logging;
using Serilog;
using System.Text.Json.Serialization;
using DHI.Services.Connections.WebApi;

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

#warning Enable if using SignalR capabilities
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
        options.JsonSerializerOptions.DefaultIgnoreCondition
           = SerializerOptionsDefault.Options.DefaultIgnoreCondition;
        options.JsonSerializerOptions.PropertyNamingPolicy
            = SerializerOptionsDefault.Options.PropertyNamingPolicy;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions
               .AddConverters(SerializerOptionsDefault.Options.Converters);
#warning Depending on which Web API packages you install in this project, you need to register domain-specific JSON converters for these packages
        // --> GIS service JSON converters. Install NuGet package DHI.Spatial.GeoJson
        //options.SerializerSettings.Converters.Add(new PositionConverter());
        //options.SerializerSettings.Converters.Add(new GeometryConverter());
        //options.SerializerSettings.Converters.Add(new AttributeConverter());
        //options.SerializerSettings.Converters.Add(new FeatureConverter());
        //options.SerializerSettings.Converters.Add(new FeatureCollectionConverter());
        //options.SerializerSettings.Converters.Add(new GeometryCollectionConverter());

        // --> Timeseries services JSON converters
        //options.SerializerSettings.Converters.Add(new DataPointConverter<double, int?>());
        //options.SerializerSettings.Converters.Add(new TimeSeriesDataWFlagConverter<double, Dictionary<string, object>>());
        //options.SerializerSettings.Converters.Add(new TimeSeriesDataWFlagConverter<double, int?>());
        //options.SerializerSettings.Converters.Add(new TimeSeriesDataWFlagConverter<Vector<double>, int?>());
        //options.SerializerSettings.Converters.Add(new TimeSeriesDataConverter<double>());
        //options.SerializerSettings.Converters.Add(new TimeSeriesDataConverter<Vector<double>>());
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
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.Connections.WebApi.xml"));
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.Logging.WebApi.xml"));
#warning Depending on which Web API packages you install in this project, you need to register the XML-files from these packages for descriptions in Swagger UI
    //options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.Documents.WebApi.xml"));
    //options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.GIS.WebApi.xml"));
    //options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.Jobs.WebApi.xml"));
    //options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.JsonDocuments.WebApi.xml"));
    //options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.Meshes.WebApi.xml"));
    //options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.Models.WebApi.xml"));
    //options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.Places.WebApi.xml"));
    //options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.Rasters.WebApi.xml"));
    //options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.Scalars.WebApi.xml"));
    //options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.Spreadsheets.WebApi.xml"));
    //options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.Tables.WebApi.xml"));
    //options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.TimeSeries.WebApi.xml"));
    //options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.TimeSteps.WebApi.xml"));

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

// Configure the necessary services for constructor injection into controllers of DHI Domain Services Web APIs
#warning In production code, you should use replace the JSON-file based repostiories with for example the PostgreSQL repositories
builder.Services.AddScoped(_ => new DHI.Services.Connections.WebApi.ConnectionTypeService(AppContext.BaseDirectory));

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

// Register services
var lazyCreation = configuration.GetValue("AppConfiguration:LazyCreation", true);
Services.Configure(new ConnectionRepository("connections.json"), lazyCreation);

app.Run();