namespace DHI.Services.Jobs.WebApi.Host
{
    using Automations;
    using DHI.Services.Jobs.Scenarios;
    using DHI.Services.Scalars;
    using Filters;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
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
    using Notifications;
    using Swashbuckle.AspNetCore.SwaggerUI;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition.Hosting;
    using System.IO;
    using System.Security.Claims;
    using WebApiCore;
    using Workflows;
    using AutomationRepository = WebApi.AutomationRepository;

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
                    options.JsonSerializerOptions.DefaultIgnoreCondition = SerializerOptionsDefault.Options.DefaultIgnoreCondition;
                    options.JsonSerializerOptions.PropertyNamingPolicy = SerializerOptionsDefault.Options.PropertyNamingPolicy;
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
                setupAction.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.Jobs.WebApi.xml"));
                //setupAction.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.Connections.WebApi.xml"));
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

            // DHI Domain Services
            services.AddScoped<IHostRepository>(_ => new GroupedHostRepository("grouped_hosts.json"));
            services.AddScoped<IAutomationRepository>(_ => new AutomationRepository("automations.json"));
            services.AddScoped<ILogger>(_ => new SimpleLogger("[AppData]log.log".Resolve()));
            services.AddSingleton<IJobRepository<Guid, string>>(sp =>
            {
                var jobRepo1 = new JobRepository("[AppData]jobs.json".Resolve());
                var jobRepo2 = new JobRepository("[AppData]jobs2.json".Resolve());

                return new ReadOnlyCompositeJobRepository(new[] { jobRepo1, jobRepo2 });
            });
            services.AddScoped<IScalarRepository<string, int>>(_ => new ScalarRepository("[AppData]scalars.json".Resolve()));
            services.AddSingleton<IFilterRepository>(_ => new FilterRepository("[AppData]signalr-filters.json".Resolve()));
            //services.AddSingleton<IFilterRepository>(provider => new FakeFilterRepository());
            //services.AddScoped(_ => new ConnectionTypeService(AppContext.BaseDirectory));

            var container = GetTriggerCatalog();
            services.AddSingleton<ITriggerRepository>(new TriggerRepository(container));

            // SignalR
            services.AddSignalR(hubOptions => { hubOptions.EnableDetailedErrors = true; });
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
            });
            app.UseExceptionHandling();
            app.UseResponseCompression();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<NotificationHub>("/notificationhub");
            });

            // Set the data directory (App_Data folder)
            var contentRootPath = Configuration.GetValue("AppConfiguration:ContentRootPath", env.ContentRootPath);
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(contentRootPath, "App_Data"));

            //var lazyCreation = Configuration.GetValue("AppConfiguration:LazyCreation", true);
            //Services.Configure(new ConnectionRepository("connections.json"), lazyCreation);

            ServiceLocator.Register(
                new JobService<Workflow, string>(
                    new JobRepository("jobs.json"),
                    new TaskService<Workflow, string>(new WorkflowRepository("[AppData]workflows.json", SerializerOptionsDefault.Options.Converters)),
                    null),
                "wf-jobs"
                );

            ServiceLocator.Register(
                new TaskService<Workflow, string>(new WorkflowRepository("[AppData]workflows.json")),
                "wf-tasks"
                );

            ServiceLocator.Register(
                    new ScenarioService(new ScenarioRepository("[AppData]scenarios.json".Resolve(), SerializerOptionsDefault.Options.Converters),
                    new JobRepository("jobs.json")
                    ),
                "json-scenarios"
                );

            var workflowRepository = new CodeWorkflowRepository("[AppData]workflows2.json".Resolve());
            var workflowService = new CodeWorkflowService(workflowRepository);
            ServiceLocator.Register(workflowService, "wf-tasks2");

            var jobRepository = new JobRepository("jobs2.json");
            var jobService = new JobService<CodeWorkflow, string>(jobRepository, workflowService);
            ServiceLocator.Register(jobService, "wf-jobs2");
        }

        private CompositionContainer GetTriggerCatalog()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(BaseTrigger).Assembly));

            var catalogDirectory = Configuration.GetConnectionString("TriggerCatalogDirectory");
            if (!string.IsNullOrEmpty(catalogDirectory))
            {
                catalog.Catalogs.Add(new DirectoryCatalog(catalogDirectory));
            }

            return new CompositionContainer(catalog);
        }
    }
}