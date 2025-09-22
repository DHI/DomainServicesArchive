using DHI.Services;
using DHI.Services.Notifications;
using DHI.Services.Notifications.WebApi;
using DHI.Services.WebApiCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// === Authentication ===
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
            IssuerSigningKey = RSA.BuildSigningKey(configuration["Tokens:PublicRSAKey"].Resolve())
        };
    });

// === Authorization ===
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdministratorsOnly", policy => policy.RequireClaim(ClaimTypes.GroupSid, "Administrators"));
    options.AddPolicy("EditorsOnly", policy => policy.RequireClaim(ClaimTypes.GroupSid, "Editors"));
});

// === API Versioning ===
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version", "version", "ver"),
        new HeaderApiVersionReader("api-version"));
});

// === CORS, Compression, MVC, JSON Settings ===
builder.Services
    .AddCors()
    .AddResponseCompression()
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// === HSTS ===
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.MaxAge = TimeSpan.FromDays(configuration.GetValue<double>("AppConfiguration:HstsMaxAgeInDays"));
});

// === Swagger ===
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(configuration["Swagger:SpecificationName"], new OpenApiInfo
    {
        Title = configuration["Swagger:DocumentTitle"],
        Version = "1",
        Description = File.ReadAllText(configuration["Swagger:DocumentDescription"].Resolve())
    });

    options.EnableAnnotations();

    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.Notifications.WebApi.xml"));

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter 'Bearer' followed by space and JWT",
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

builder.Services.AddScoped(_ => new ConnectionTypeService(AppContext.BaseDirectory));
builder.Services.AddSingleton<IAuthorizationHandler, AllowAnonymousHandler>();

var app = builder.Build();

// === Middleware Setup ===
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHsts();
app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var specificationName = configuration["Swagger:SpecificationName"];
    options.SwaggerEndpoint($"../swagger/{specificationName}/swagger.json", configuration["Swagger:DocumentName"]);
    options.DocExpansion(DocExpansion.None);
    options.DefaultModelsExpandDepth(-1);
});

app.UseAuthentication();
app.UseCors();
app.UseResponseCompression();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// === App_Data directory setup ===
var contentRootPath = configuration.GetValue("AppConfiguration:ContentRootPath", app.Environment.ContentRootPath);
AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(contentRootPath, "App_Data"));

// === Register Notification Service ===
var notificationRepositoryPath = "[AppData]notifications.json".Resolve();
var notificationRepository = new NotificationRepository(notificationRepositoryPath);
ServiceLocator.Register(new NotificationService(notificationRepository), "json-notifications");

app.Run();

public partial class Program { }