namespace DHI.Services.Places
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     Class representing a FeatureId.
    /// </summary>
    [Serializable]
    public class FeatureId<TCollectionId> : IEquatable<FeatureId<TCollectionId>> where TCollectionId : notnull
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FeatureId{TCollectionId}" /> class.
        /// </summary>
        /// <param name="featureCollectionId">The feature collection identifier.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// 
        [JsonConstructor]
        public FeatureId(TCollectionId featureCollectionId, string attributeKey, object attributeValue)
        {
            Guard.Against.Null(featureCollectionId, nameof(featureCollectionId));
            Guard.Against.NullOrEmpty(attributeKey, nameof(attributeKey));
            Guard.Against.Null(attributeValue, nameof(attributeValue));
            FeatureCollectionId = featureCollectionId;
            AttributeKey = attributeKey;
            AttributeValue = attributeValue;
        }

        /// <summary>
        ///     Gets the feature collection identifier.
        /// </summary>
        public TCollectionId FeatureCollectionId { get; }

        /// <summary>
        ///     Gets the attribute key.
        /// </summary>
        public string AttributeKey { get; }

        /// <summary>
        ///     Gets the attribute value.
        /// </summary>
        public object AttributeValue { get; }

        /// <summary>
        ///     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///     true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise,
        ///     false.
        /// </returns>
        public bool Equals(FeatureId<TCollectionId>? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return FeatureCollectionId.Equals(other.FeatureCollectionId) &&
                   AttributeKey == other.AttributeKey &&
                   AttributeValue.Equals(other.AttributeValue);
        }

        /// <summary>
        ///     Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return $"{FeatureCollectionId}-{AttributeKey}-{AttributeValue}";
        }

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is FeatureId<TCollectionId> other && Equals(other);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = FeatureCollectionId.GetHashCode();
                hashCode = (hashCode * 397) ^ AttributeKey.GetHashCode();
                hashCode = (hashCode * 397) ^ AttributeValue.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        ///     Implements the == operator.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        public static bool operator ==(FeatureId<TCollectionId> left, FeatureId<TCollectionId> right)
        {
            return Equals(left, right);
        }

        /// <summary>
        ///     Implements the != operator.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        public static bool operator !=(FeatureId<TCollectionId> left, FeatureId<TCollectionId> right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    ///     Class representing a FeatureId.
    /// </summary>
    [Serializable]
    public class FeatureId : FeatureId<string>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FeatureId" /> class.
        /// </summary>
        /// <param name="featureCollectionId">The feature collection identifier.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// 
        [JsonConstructor]
        public FeatureId(string featureCollectionId, string attributeKey, object attributeValue)
            : base(featureCollectionId, attributeKey, attributeValue)
        {
            Guard.Against.NullOrEmpty(featureCollectionId, nameof(featureCollectionId));
        }
    }
}