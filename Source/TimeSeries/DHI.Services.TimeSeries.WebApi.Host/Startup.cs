namespace DHI.Services.TimeSeries.WebApi.Host
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Claims;
    using DHI.Services.Filters;
    using DHI.Services.TimeSeries.Converters;
    using DHI.Services.WebApiCore;
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
    using Swashbuckle.AspNetCore.SwaggerUI;

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

            CustomSerializationSettings.UseNullForNaN = Configuration.GetValue<bool>("Serialization:UseNullForNaN");

            // MVC
            services
                .AddCors()
                .AddResponseCompression()
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = SerializerOptionsDefault.Options.DefaultIgnoreCondition;
                    //options.JsonSerializerOptions.PropertyNamingPolicy = SerializerOptionsDefault.Options.PropertyNamingPolicy;
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
                setupAction.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.TimeSeries.WebApi.xml"));
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
            //services.AddScoped(_ => new ConnectionTypeService(AppContext.BaseDirectory));

            services.AddSingleton<IFilterRepository>(new FilterRepository("[AppData]signalr-filters.json".Resolve()));

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

            // Set the data directory (App_Data folder) for DHI Domain Services
            var contentRootPath = Configuration.GetValue("AppConfiguration:ContentRootPath", env.ContentRootPath);
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(contentRootPath, "App_Data"));

            // DHI Domain Services  
            //var lazyCreation = Configuration.GetValue("AppConfiguration:LazyCreation", true);
            //Services.Configure(new ConnectionRepository("connections.json", SerializerOptionsDefault.Options), lazyCreation);

            ServiceLocator.Register(
                new GroupedDiscreteTimeSeriesService<string, double>(new DHI.Services.TimeSeries.CSV.TimeSeriesRepository("[AppData]csv".Resolve())),
                "csv"
            );
            ServiceLocator.Register(
                new CoreTimeSeriesService(new DHI.Services.TimeSeries.Daylight.TimeSeriesRepository()),
                "daylight"
                );

            // For testing SignalR
            var timeSeriesList = new List<TimeSeries> { new TimeSeries("myTimeSeries", "My Time Series") };
            var repository = new InMemoryTimeSeriesRepository<string, double>(timeSeriesList);
            var service = new UpdatableTimeSeriesService(repository);
            ServiceLocator.Register(service, "myTsConnection");

            // For testing daylight time series repository
            var daylightService = new CoreTimeSeriesService(new Daylight.TimeSeriesRepository());
            ServiceLocator.Register(daylightService, "daylight-ts");

            // For testing logger injection into time series service
            var tsRepo = new CSV.TimeSeriesRepository("[AppData]csv".Resolve());
            var csvLogger = app.ApplicationServices
                   .GetRequiredService<
                       ILogger<GroupedDiscreteTimeSeriesService>>();
            var tsService = new GroupedDiscreteTimeSeriesService(tsRepo, csvLogger);
            ServiceLocator.Register(tsService, "csv2");
        }
    }
}