namespace DHI.Services.Security.WebApi.Host
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Text.Json;
    using Accounts;
    using Authentication;
    using Authorization;
    using DHI.Services.Authentication.PasswordHistory;
    using Logging;
    using Mails;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;
    using Polly;
    using Swashbuckle.AspNetCore.SwaggerUI;
    using WebApiCore;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Tokens:Issuer"],
                        ValidAudience = Configuration["Tokens:Audience"],
                        IssuerSigningKey = RSA.BuildSigningKey(Configuration["Tokens:PublicRSAKey"].Resolve())
                    };
                });

            // Authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdministratorsOnly", policy => policy.RequireClaim(ClaimTypes.GroupSid, "Administrators"));
                options.AddPolicy("EditorsOnly", policy => policy.RequireClaim(ClaimTypes.GroupSid, "Editors"));
            });

            // API versioning
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new QueryStringApiVersionReader("api-version", "version", "ver"),
                    new HeaderApiVersionReader("api-version"));
            });

            // MVC
            services
                .AddCors()
                .AddResponseCompression()
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.WriteIndented = true;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = SerializerOptionsDefault.Options.DefaultIgnoreCondition;
                    options.JsonSerializerOptions.AddConverters(SerializerOptionsDefault.Options.Converters);
                });

            // HSTS
            services.AddHsts(options =>
            {
                options.Preload = true;
                options.MaxAge = TimeSpan.FromDays(Configuration.GetValue<double>("AppConfiguration:HstsMaxAgeInDays"));
            });

            // Swagger
            services.AddSwaggerGen(setupAction =>
            {
                setupAction.SwaggerDoc(Configuration["Swagger:SpecificationName"], new OpenApiInfo
                {
                    Title = Configuration["Swagger:DocumentTitle"],
                    Version = "1",
                    Description = File.ReadAllText(Configuration["Swagger:DocumentDescription"].Resolve())
                });

                setupAction.EnableAnnotations();
                setupAction.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.Security.WebApi.xml"));
                setupAction.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Enter the word 'Bearer' followed by a space and the JWT.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement()
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
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });

            // Pwned passwords
            services
                .AddPwnedPasswordHttpClient(minimumFrequencyToConsiderPwned: 10)
                .AddTransientHttpErrorPolicy(p => p.RetryAsync(3))
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(2)));

            // Password Policy
            services.AddScoped(_ => new PasswordPolicy 
            { 
                RequiredLength = 10, 
                RequiredUniqueChars = 5, 
                RequireDigit = true, 
                MinimumDigit = 1, 
                RequireNonAlphanumeric = true, 
                MinimumNonAlphanumeric = 1 
            });


            // Password History Policy
            services.AddScoped(_ => new PasswordExpirationPolicy
            {
                PreviousPasswordsReUseLimit = 3,
                PasswordExpiryDurationInDays = 5
            });
            // Login Attempt Policy
            services.AddScoped(_ => new LoginAttemptPolicy
            {
                MaxNumberOfLoginAttempts = 5,
                ResetInterval = TimeSpan.FromMinutes(2),
                LockedPeriod = TimeSpan.FromDays(10)

            });

            // DHI Domain Services
            services.AddScoped<IAccountRepository>(_ => new AccountRepository("accounts.json", SerializerOptionsDefault.Options));
            services.AddScoped<IMailTemplateRepository>(_ => new MailTemplateRepository("mail-templates.json", SerializerOptionsDefault.Options));
            services.AddScoped<IUserGroupRepository>(_ => new UserGroupRepository("user-groups.json", SerializerOptionsDefault.Options));
            services.AddScoped<IRefreshTokenRepository>(_ => new RefreshTokenRepository("refresh-tokens.json", SerializerOptionsDefault.Options));
            services.AddScoped<IAuthenticationProvider>(_ => new AccountRepository("accounts.json", SerializerOptionsDefault.Options));
            services.AddScoped<IPasswordHistoryRepository>(_ => new PasswordHistoryRepository("passwordhistory.json", SerializerOptionsDefault.Options));
            services.AddScoped<ILogger>(_ => new SimpleLogger("[AppData]log.log".Resolve()));
        }

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseSwagger();
            app.UseSwaggerUI(setupAction =>
            {
                var specificationName = Configuration["Swagger:SpecificationName"];
                setupAction.SwaggerEndpoint($"../swagger/{specificationName}/swagger.json", Configuration["Swagger:DocumentName"]);
                setupAction.DocExpansion(DocExpansion.None);
                setupAction.DefaultModelsExpandDepth(-1);
            });
            app.UseExceptionHandling();
            app.UseResponseCompression();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Set the data directory (App_Data folder)
            var contentRootPath = Configuration.GetValue("AppConfiguration:ContentRootPath", env.ContentRootPath);
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(contentRootPath, "App_Data"));
        }
    }
}