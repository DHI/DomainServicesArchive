namespace DHI.Services
{
    using System;

    /// <summary>
    ///     Abstract base class for a connection.
    /// </summary>
    [Serializable]
    public abstract class BaseConnection : BaseNamedEntity<string>, IConnection
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        protected BaseConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Creates a service instance.
        /// </summary>
        /// <returns>System.Object.</returns>
        public abstract object Create();
    }
}