using DHI.Services;
using DHI.Services.GIS;
using DHI.Services.GIS.Maps;
using DHI.Services.GIS.WebApi;
using DHI.Services.Provider.MC.GIS;
using DHI.Services.Provider.MCLite;
using DHI.Services.Provider.MIKECore;
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
using System.Security.Claims;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var configuration = builder.Configuration;

bool allowAnonymousAccess = bool.Parse(configuration["AllowAnonymousAccess"]);

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
if (allowAnonymousAccess)
{
    builder.Services.AddSingleton<IAuthorizationHandler, AllowAnonymousHandler>();
}

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
            = SerializerOptionsDefault.Options.DefaultIgnoreCondition;
        options.JsonSerializerOptions.PropertyNamingPolicy
            = SerializerOptionsDefault.Options.PropertyNamingPolicy;
        options.JsonSerializerOptions
               .AddConverters(SerializerOptionsDefault.Options.Converters);
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

builder.Services.AddSerilog(seriLogger); // Register the Serilog logger with the dependency injection container
builder.Services.AddScoped(_ => new ConnectionTypeService(AppContext.BaseDirectory));
builder.Services.AddScoped<IMapStyleRepository>(_ => new DHI.Services.GIS.WebApi.MapStyleRepository("styles.json", SerializerOptionsDefault.Options));

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
// Custom services
var lazyCreation = builder.Configuration.GetValue("AppConfiguration:LazyCreation", false);
Services.Configure(new ConnectionRepository("connections.json", SerializerOptionsDefault.Options), lazyCreation);
#endregion

app.Run();

// Check