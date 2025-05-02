using DHI.Services;
using DHI.Services.Accounts;
using DHI.Services.Authentication;
using DHI.Services.Authorization;
using DHI.Services.Mails;
using DHI.Services.Security.WebApi;
using DHI.Services.WebApiCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Serilog.Extensions.Logging;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Security.Claims;
using AccountRepository = DHI.Services.Accounts.AccountRepository;
using MailTemplateRepository = DHI.Services.Security.WebApi.MailTemplateRepository;
using RefreshTokenRepository = DHI.Services.Security.WebApi.RefreshTokenRepository;
using UserGroupRepository = DHI.Services.Security.WebApi.UserGroupRepository;
using DHI.Services.Authentication.PasswordHistory;
using AuthorizationServerSample.Eksternal;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var configuration = builder.Configuration;

var userManagementInDB = bool.Parse(configuration["UserManagementInDatabase"]);
var xxeVurnebility = new XXEVurnerability();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.SetIsOriginAllowed((string host) =>
        {
            string[] hostlist = configuration["CORS"].Split(';');
            bool isAllowed = hostlist.Any(origin => new Uri(host).Host == origin);
            return isAllowed;
        })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

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
            IssuerSigningKey = new RsaSecurityKey(xxeVurnebility.LoadRsaFromXml(configuration["Tokens:PublicRSAKey"]))
        };
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
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = SerializerOptionsDefault.Options.DefaultIgnoreCondition;
        options.JsonSerializerOptions.AddConverters(SerializerOptionsDefault.Options.Converters);
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
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.Security.WebApi.xml"));
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

// Pwned passwords
builder.Services
    .AddPwnedPasswordHttpClient(minimumFrequencyToConsiderPwned: 1)
    .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.RetryAsync(3))
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(2)));

// Password Policy
builder.Services.AddScoped(_ => new PasswordPolicy
{
    RequiredLength = 6,
    RequiredUniqueChars = 0,
    MinimumDigit = 1,
    MinimumNonAlphanumeric = 0,
    RequireNonAlphanumeric = false,
    RequireLowercase = false,
    RequireUppercase = false,
    RequireDigit = true
});


// Password History Policy
builder.Services.AddScoped(_ => new PasswordExpirationPolicy
{
    PreviousPasswordsReUseLimit = 3,
    PasswordExpiryDurationInDays = 5
});

// Login Attempt Policy
builder.Services.AddScoped(_ => new LoginAttemptPolicy
{
    MaxNumberOfLoginAttempts = 2,
    ResetInterval = TimeSpan.FromMinutes(1),
    LockedPeriod = TimeSpan.FromDays(10)

});


// Set the data directory (App_Data folder) for DHI Domain Services
var contentRootPath = builder.Configuration.GetValue("AppConfiguration:ContentRootPath", builder.Environment.ContentRootPath);
AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(contentRootPath, "App_Data"));

var seriLogger = new LoggerConfiguration()
    .WriteTo.File("[AppData]log.txt".Resolve(), outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Console()
    .CreateLogger();

var logger = new SerilogLoggerFactory(seriLogger)
   .CreateLogger<Program>();

builder.Services.AddSerilog(seriLogger);

if (userManagementInDB)
{
    string postgres_acc_connStr = configuration["postgres-accounts:RepositoryConnectionString"];

    builder.Services.AddScoped<IAccountRepository>(_ => new DHI.Services.Provider.PostgreSQL.AccountRepository(postgres_acc_connStr, logger, null, _.GetRequiredService<LoginAttemptPolicy>()));
    builder.Services.AddScoped<IMailTemplateRepository>(_ => new MailTemplateRepository("mail-templates.json", SerializerOptionsDefault.Options));
    builder.Services.AddScoped<IUserGroupRepository>(_ => new DHI.Services.Provider.PostgreSQL.UserGroupRepository(postgres_acc_connStr, logger));
    builder.Services.AddScoped<IRefreshTokenRepository>(_ => new DHI.Services.Provider.PostgreSQL.RefreshTokenRepository(postgres_acc_connStr, logger));
    builder.Services.AddScoped<IAuthenticationProvider>(_ => new DHI.Services.Provider.PostgreSQL.AccountRepository(postgres_acc_connStr, logger, null, _.GetRequiredService<LoginAttemptPolicy>()));
    builder.Services.AddScoped<IPasswordHistoryRepository>(_ => new DHI.Services.Provider.PostgreSQL.PasswordHistoryRepository(postgres_acc_connStr, logger));
}
else
{
    // Configure the necessary services for constructor injection into controllers of DHI Domain Services Web APIs
#warning In production code, you should use replace the JSON-file based repostiories with for example the PostgreSQL repositories

    builder.Services.AddScoped<IAccountRepository>(_ => new AccountRepository("accounts.json", SerializerOptionsDefault.Options, null, null, _.GetRequiredService<LoginAttemptPolicy>()));
    builder.Services.AddScoped<IMailTemplateRepository>(_ => new MailTemplateRepository("mail-templates.json", SerializerOptionsDefault.Options));
    builder.Services.AddScoped<IUserGroupRepository>(_ => new UserGroupRepository("user-groups.json", SerializerOptionsDefault.Options));
    builder.Services.AddScoped<IRefreshTokenRepository>(_ => new RefreshTokenRepository("refresh-tokens.json", SerializerOptionsDefault.Options));
    builder.Services.AddScoped<IAuthenticationProvider>(_ => new AccountRepository("accounts.json", SerializerOptionsDefault.Options, null, null, _.GetRequiredService<LoginAttemptPolicy>()));
    builder.Services.AddScoped<IPasswordHistoryRepository>(_ => new PasswordHistoryRepository("passwordhistory.json", SerializerOptionsDefault.Options));
}

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
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var specificationName = configuration["Swagger:SpecificationName"];
        options.SwaggerEndpoint($"../swagger/{specificationName}/swagger.json", configuration["Swagger:DocumentName"]);
        options.DocExpansion(DocExpansion.None);
        options.DefaultModelsExpandDepth(-1);
    });

    app.UseHsts();
}

app.UseCors("AllowSpecificOrigins"); // Use the defined CORS policy

app.UseAuthentication();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseExceptionHandling();
app.UseResponseCompression();
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();