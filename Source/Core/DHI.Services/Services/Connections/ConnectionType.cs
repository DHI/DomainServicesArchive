namespace DHI.Services
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Connection Type.
    /// </summary>
    public class ConnectionType : BaseEntity<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionType"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="type">The type.</param>
        public ConnectionType(string id, Type type)
            : base(id)
        {
            Type = type;
            ProviderArguments = new List<ProviderArgument>();
            ProviderTypes = new List<ProviderType>();
        }

        /// <summary>
        /// Gets the provider arguments.
        /// </summary>
        /// <value>The provider arguments.</value>
        public List<ProviderArgument> ProviderArguments { get; }

        /// <summary>
        /// Gets the provider types.
        /// </summary>
        /// <value>The provider types.</value>
        public List<ProviderType> ProviderTypes { get; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public Type Type { get; }
    }
}