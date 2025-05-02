namespace DHI.Services.Places
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     Structure representing a DataSource
    /// </summary>
    [Serializable]
    public struct DataSource : IEquatable<DataSource>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DataSource" /> struct.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        [JsonConstructor]
        public DataSource(DataSourceType type, string connectionId, object entityId)
        {
            Guard.Against.NullOrEmpty(connectionId, nameof(connectionId));
            Guard.Against.Null(entityId, nameof(entityId));
            ConnectionId = connectionId;
            EntityId = entityId;
            Type = type;
        }

        /// <summary>
        ///     Gets the connection identifier.
        /// </summary>
        public string ConnectionId { get; }

        /// <summary>
        ///     Gets the entity identifier.
        /// </summary>
        public object EntityId { get; }

        /// <summary>
        ///     Gets the type.
        /// </summary>
        public DataSourceType Type { get; }

        /// <summary>
        ///     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///     true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise,
        ///     false.
        /// </returns>
        public bool Equals(DataSource other)
        {
            return ConnectionId == other.ConnectionId && EntityId.Equals(other.EntityId) && Type == other.Type;
        }

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is DataSource other && Equals(other);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ConnectionId.GetHashCode();
                hashCode = (hashCode * 397) ^ EntityId.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Type;
                return hashCode;
            }
        }

        /// <summary>
        ///     Implements the == operator.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(DataSource left, DataSource right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Implements the != operator.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(DataSource left, DataSource right)
        {
            return !left.Equals(right);
        }
    }
}