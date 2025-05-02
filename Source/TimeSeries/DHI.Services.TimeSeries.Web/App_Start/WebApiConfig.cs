namespace DHI.Services.TimeSeries.Web
{
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http.Extensions.Compression.Core.Compressors;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.Routing;
    using Accounts;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Thinktecture.IdentityModel.WebApi.Authentication.Handler;
    using Microsoft.AspNet.WebApi.Extensions.Compression.Server;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning;
    using Newtonsoft.Json.Serialization;
    using Properties;

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // CORS support
            config.EnableCors();

            // JSON configuration
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple;
            ((DefaultContractResolver)config.Formatters.JsonFormatter.SerializerSettings.ContractResolver).IgnoreSerializableAttribute = true;
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new IsoDateTimeConverter());
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new KeyValuePairConverter());
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new DataPointConverter<double, int?>());
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new TimeSeriesDataWFlagConverter<double, Dictionary<string, object>>());
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new TimeSeriesDataWFlagConverter<double, int?>());
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new TimeSeriesDataWFlagConverter<Vector<double>, int?>());
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new TimeSeriesDataConverter<double>());
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new TimeSeriesDataConverter<Vector<double>>());

            // BSON support
            config.Formatters.Add(new BsonMediaTypeFormatter());

            // Custom route constraints
            var constraintResolver = new DefaultInlineConstraintResolver();
            constraintResolver.ConstraintMap.Add("date", typeof(DateTimeConstraint));

            // System services
            ServiceLocator.Register(new ConnectionTypeService(HttpRuntime.BinDirectory), ServiceId.ConnectionTypes);

            // Custom services
            var connectionsFolder = Path.Combine(HttpRuntime.AppDomainAppPath, "App_Data");
            Services.Configure(new ConnectionRepository(Path.Combine(connectionsFolder, "connections.json")), Settings.Default.LazyCreation);

            // Default Account service
            if (!Services.Connections.Exists(ServiceId.Accounts))
            {
                var accountServiceConnection = new AccountServiceConnection("Accounts", "Accounts connection")
                {
                    ConnectionString = "[AppData]accounts.json",
                    RepositoryType = typeof(AccountRepository).AssemblyQualifiedName
                };

                ServiceLocator.Register(accountServiceConnection.Create(), ServiceId.Accounts);
            }

            // Security
            var authenticationConfiguration = new AuthenticationConfiguration { RequireSsl = false };
            var accountService = Services.Get<AccountService>(ServiceId.Accounts);
            authenticationConfiguration.AddBasicAuthentication((userName, password) => accountService.Validate(userName, password), username => accountService.GetRoles(username), "DHI Web API");
            config.MessageHandlers.Add(new AuthenticationHandler(authenticationConfiguration));

            // Routing
            config.MapHttpAttributeRoutes(constraintResolver);
            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new { id = RouteParameter.Optional });

            // Compression support
            var serverCompressionHandler = new ServerCompressionHandler(Settings.Default.CompressionThresshold, new GZipCompressor(), new DeflateCompressor());
            GlobalConfiguration.Configuration.MessageHandlers.Insert(0, serverCompressionHandler);

            // Versioning support
            config.AddApiVersioning(o =>
            {
                o.ApiVersionReader = new MediaTypeApiVersionReader("api-version");
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
            });
        }
    }
}