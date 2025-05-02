using DHI.Services;
using DHI.Services.Accounts;
using DHI.Services.Authentication;
using DHI.Services.Authorization;
using DHI.Services.Mails;
using DHI.Services.Security.WebApi;
using DHI.Services.WebApiCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Security.Claims;
using AccountRepository = DHI.Services.Accounts.AccountRepository;
using ILogger = DHI.Services.Logging.SimpleLogger;
using MailTemplateRepository = DHI.Services.Security.WebApi.MailTemplateRepository;
using RefreshTokenRepository = DHI.Services.Security.WebApi.RefreshTokenRepository;
using UserGroupRepository = DHI.Services.Security.WebApi.UserGroupRepository;
using DHI.Services.Authentication.PasswordHistory;
using Serilog.Extensions.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args); // Create a builder for configuring the application

builder.Services.AddControllers(); // Add controllers to the services container, enabling MVC

var configuration = builder.Configuration; // Access the configuration settings

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
    // Configure API versioning
    options.ReportApiVersions = true; 
    options.AssumeDefaultVersionWhenUnspecified = true; 
    options.DefaultApiVersion = new ApiVersion(1, 0); 
    options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version", "version", "ver"), 
        new HeaderApiVersionReader("api-version") 
    );
});

// MVC configuration
builder.Services
    .AddResponseCompression(options => { options.EnableForHttps = true; }) // Enable response compression for HTTPS
    .AddControllers() // enabling MVC
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization options
        options.JsonSerializerOptions.WriteIndented = true; 
        options.JsonSerializerOptions.DefaultIgnoreCondition = SerializerOptionsDefault.Options.DefaultIgnoreCondition; 
        options.JsonSerializerOptions.AddConverters(SerializerOptionsDefault.Options.Converters); // Add custom converters
    });

// HSTS (HTTP Strict Transport Security)
builder.Services.AddHsts(options =>
{
    // Configure HSTS for better security
    options.Preload = true;
    options.MaxAge = TimeSpan.FromDays(configuration.GetValue<double>("AppConfiguration:HstsMaxAgeInDays"));
});

// Swagger configuration
builder.Services.AddSwaggerGen(options =>
{
    // Configure Swagger for API documentation
    options.SwaggerDoc(configuration["Swagger:SpecificationName"], new OpenApiInfo
    {
        Title = configuration["Swagger:DocumentTitle"], // Set Swagger document title
        Version = "1", // Set Swagger document version
        Description = File.ReadAllText(configuration["Swagger:DocumentDescription"].Resolve()) // Set Swagger document description
    });

    options.EnableAnnotations(); // Enable annotations for Swagger
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.Security.WebApi.xml")); // Include XML comments for documentation

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        // Define Bearer token security scheme
        Description = "Enter the word 'Bearer' followed by a space and the JWT.",
        Name = "Authorization", // Set header name for the token
        In = ParameterLocation.Header, // Set token location in the request
        Type = SecuritySchemeType.ApiKey, // Define the security scheme type as an API key
        Scheme = "Bearer" // Set the scheme name
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        // Define security requirements for the API
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer" // Reference the Bearer scheme
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

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
#warning In production code, you should use replace the JSON-file based repositories with, for example, the PostgreSQL repositories
builder.Services.AddScoped<IAccountRepository>(_ => new AccountRepository("accounts.json", SerializerOptionsDefault.Options, null, null, _.GetRequiredService<LoginAttemptPolicy>())); // Register IAccountRepository with a JSON-based implementation
builder.Services.AddScoped<IMailTemplateRepository>(_ => new MailTemplateRepository("mail-templates.json", SerializerOptionsDefault.Options)); // Register IMailTemplateRepository with a JSON-based implementation
builder.Services.AddScoped<IUserGroupRepository>(_ => new UserGroupRepository("user-groups.json", SerializerOptionsDefault.Options)); // Register IUserGroupRepository with a JSON-based implementation
builder.Services.AddScoped<IRefreshTokenRepository>(_ => new RefreshTokenRepository("refresh-tokens.json", SerializerOptionsDefault.Options)); // Register IRefreshTokenRepository with a JSON-based implementation
builder.Services.AddScoped<IAuthenticationProvider>(_ => new AccountRepository("accounts.json", SerializerOptionsDefault.Options, null, null, _.GetRequiredService<LoginAttemptPolicy>())); // Register IAuthenticationProvider with a JSON-based implementation
builder.Services.AddScoped<IPasswordHistoryRepository>(_ => new PasswordHistoryRepository("passwordhistory.json", SerializerOptionsDefault.Options)); // Register IPasswordHistoryRepository with a JSON-based implementation

var app = builder.Build(); // Build the application

if (app.Environment.IsDevelopment())
{
    // Development-specific configuration
    app.UseDeveloperExceptionPage(); // Use the developer exception page for detailed error information
    app.UseSwagger(); // Enable Swagger middleware
    app.UseSwaggerUI(options =>
    {
        var specificationName = configuration["Swagger:SpecificationName"];
        options.SwaggerEndpoint($"../swagger/{specificationName}/swagger.json", configuration["Swagger:DocumentName"]); // Set Swagger endpoint
        options.DocExpansion(DocExpansion.None); // Set Swagger UI to not expand document tree
        options.DefaultModelsExpandDepth(-1); // Disable model expansion by default
    });
}
else
{
    app.UseHsts(); // Use HSTS in non-development environments
}

app.UseAuthentication(); // Enable authentication middleware
app.UseDefaultFiles(); // Serve default files (e.g., index.html)
app.UseStaticFiles(); // Serve static files
app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
app.UseExceptionHandling(); // Custom exception handling middleware
app.UseResponseCompression(); // Enable response compression
app.UseRouting(); // Enable routing middleware
app.UseAuthorization(); // Enable authorization middleware
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // Map controller endpoints
});

app.Run(); // Run the application
