namespace DHI.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Struct ProviderType
    /// </summary>
    [Serializable]
    public readonly struct ProviderType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderType"/> struct.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">The available compatible provider types.</param>
        /// <param name="mandatory">if set to <c>true</c> this provider type is mandatory.</param>
        [JsonConstructor]
        public ProviderType(string name, IEnumerable<Type> options, bool mandatory = true) : this()
        {
            Name = name;
            Mandatory = mandatory;
            Options = options;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ProviderType"/> is mandatory.
        /// </summary>
        /// <value><c>true</c> if mandatory; otherwise, <c>false</c>.</value>
        public bool Mandatory { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the available compatible provider types.
        /// </summary>
        public IEnumerable<Type> Options { get; }
    }
}