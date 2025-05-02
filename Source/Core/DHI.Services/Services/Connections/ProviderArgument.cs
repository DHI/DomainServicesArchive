namespace DHI.Services
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     Struct ProviderArgument
    /// </summary>
    [Serializable]
    public readonly struct ProviderArgument
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProviderArgument" /> struct.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="mandatory">if set to <c>true</c> [mandatory].</param>
        [JsonConstructor]
        public ProviderArgument(string name, Type type, bool mandatory = true)
            : this()
        {
            Name = name;
            Mandatory = mandatory;
            Type = type;
        }

        /// <summary>
        ///     Gets a value indicating whether this <see cref="ProviderArgument" /> is mandatory.
        /// </summary>
        /// <value><c>true</c> if mandatory; otherwise, <c>false</c>.</value>
        public bool Mandatory { get; }

        /// <summary>
        ///     Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        ///     Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public Type Type { get; }
    }
}