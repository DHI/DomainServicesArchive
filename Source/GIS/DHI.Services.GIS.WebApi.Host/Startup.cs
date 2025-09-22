namespace DHI.Services.GIS.WebApi.Host
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Claims;
    using DHI.Services.GIS.Maps;
    using DHI.Services.WebApiCore;
    using DHI.Services;
    using DHI.Services.GIS.WebApi;
    using DHI.Services.Provider.MCLite;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerUI;
    using MapStyleRepository = WebApi.MapStyleRepository;
    using DHI.Services.Provider.MIKECore;

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
                    //options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    //options.JsonSerializerOptions.PropertyNamingPolicy = SerializerOptionsDefault.Options.PropertyNamingPolicy;
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
                setupAction.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DHI.Services.GIS.WebApi.xml"));
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
            services.AddScoped(_ => new ConnectionTypeService(AppContext.BaseDirectory));
            services.AddScoped<IMapStyleRepository>(_ => new MapStyleRepository("styles.json", SerializerOptionsDefault.Options));
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
            });

            // Set the data directory (App_Data folder)
            var contentRootPath = Configuration.GetValue("AppConfiguration:ContentRootPath", env.ContentRootPath);
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(contentRootPath, "App_Data"));

            var contentRoot = Configuration.GetValue("AppConfiguration:ContentRootPath", env.ContentRootPath);
            var appData = Path.Combine(contentRoot, "App_Data");
            AppDomain.CurrentDomain.SetData("DataDirectory", appData);

            ServiceLocator.Register(
              new GroupedGisService(
                new Provider.MCLite.FeatureRepository("database=mc2014.2")
              ),
              "mc-ws1"
            );

            var sqliteConn = $"database={Path.Combine(appData, "MCSQLiteTest.sqlite")};dbflavour=SQLite";
            ServiceLocator.Register(
              new GroupedMapService(
                new GroupedMapSource(sqliteConn),
                new MapStyleService(
                  new MapStyleRepository(Path.Combine(appData, "styles.json"), SerializerOptionsDefault.Options)
                )
              ),
              "mc-groupedmap"
            );

            ServiceLocator.Register(
              new GisService(
                new DHI.Services.Provider.ShapeFile.FeatureRepository(appData)
              ),
              "shape"
            );

            //ServiceLocator.Register(
            //  new GroupedGisService(
            //    new DHI.Services.Provider.MIKE.Res1DGroupedGisRepository(
            //      Path.Combine(appData, "res1d")
            //    )
            //  ),
            //  "res1d"
            //);

            var dfs2File = Path.Combine(appData, "dfs2", "R20141001.dfs2");
            ServiceLocator.Register(
              new MapService(
                new Dfs2MapSource(dfs2File),
                new MapStyleService(
                  new MapStyleRepository(Path.Combine(appData, "styles.json"), SerializerOptionsDefault.Options)
                )
              ),
              "dfs2-map"
            );

            //ServiceLocator.Register(
            //  new WebApi.CachedMapServiceConnection(
            //    new Dfs2MapSource(dfs2File),
            //    new MapStyleService(
            //      new MapStyleRepository(Path.Combine(appData, "styles.json"), SerializerOptionsDefault.Options)
            //    ),
            //    new DHI.Services.Parameters
            //    {
            //        NameInfo = "Some info",
            //        KeywordInfo = "keyword1,keyword2",
            //        CachedImageWidth = 100,
            //        CacheExpirationInMinutes = 2
            //    }
            //  ),
            //  "dfs2-map-cached"
            //);

            ServiceLocator.Register(
              new MapService(
                new DfsuMapSource(appData),
                new MapStyleService(
                  new MapStyleRepository(Path.Combine(appData, "styles.json"), SerializerOptionsDefault.Options)
                )
              ),
              "dfsu-map"
            );

            ServiceLocator.Register(
              new GroupedUpdatableGisService(
                new Provider.MCLite.FeatureRepository(sqliteConn)
              ),
              "mclite"
            );

            ServiceLocator.Register(
              new GroupedMapService(
                new GroupedMapSource(sqliteConn),
                new MapStyleService(
                  new MapStyleRepository(Path.Combine(appData, "styles.json"), SerializerOptionsDefault.Options)
                )
              ),
              "groupedmap-mclite"
            );
        }
    }
}
