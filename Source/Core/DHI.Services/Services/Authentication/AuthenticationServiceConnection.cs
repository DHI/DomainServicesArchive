namespace DHI.Services.Authentication
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    /// <summary>
    ///     Class AuthenticationServiceConnection.
    /// </summary>
    /// <seealso cref="DHI.Services.BaseConnection" />
    [Obsolete("In the newest Web API component, the Authentication Service is injected through the controller constructor using standard ASP.NET DI. This type will eventually be removed.")]
    public class AuthenticationServiceConnection : BaseConnection
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AuthenticationServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public AuthenticationServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Gets or sets the authentication provider connection string.
        /// </summary>
        public string AuthenticationProviderConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets the type of the authentication provider.
        /// </summary>
        public string AuthenticationProviderType { get; set; }

        /// <summary>
        ///     Creates the connection type.
        /// </summary>
        /// <typeparam name="TConnection">The type of the connection.</typeparam>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>ConnectionType.</returns>
        public static ConnectionType CreateConnectionType<TConnection>(string path = null) where TConnection : AuthenticationServiceConnection
        {
            var connectionType = new ConnectionType("AuthenticationServiceConnection", typeof(TConnection));
            connectionType.ProviderTypes.Add(new ProviderType("AuthenticationProviderType", AuthenticationService.GetAuthenticationProviderTypes(path)));
            connectionType.ProviderArguments.Add(new ProviderArgument("AuthenticationProviderConnectionString", typeof(string)));
            return connectionType;
        }

        /// <summary>
        ///     Creates an authentication service instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var authenticationProviderType = Type.GetType(AuthenticationProviderType, true);
                var authenticationProvider = (IAuthenticationProvider)Activator.CreateInstance(authenticationProviderType, AuthenticationProviderConnectionString);
                return new AuthenticationService(authenticationProvider);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}