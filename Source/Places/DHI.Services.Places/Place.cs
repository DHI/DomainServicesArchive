namespace DHI.Services.Places
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    [Serializable]
    public class Place<TCollectionId> : BaseGroupedEntity<string> where TCollectionId : notnull
    {
        [JsonConstructor]
        public Place(string id, string name, FeatureId<TCollectionId> featureId, string? group) : base(id, name, group)
        {
            Guard.Against.Null(featureId, nameof(featureId));
            Indicators = new Dictionary<string, Indicator>();
            FeatureId = featureId;
        }

        public Place(string id, string name, FeatureId<TCollectionId> featureId) : this(id, name, featureId, null)
        {
        }

        public Dictionary<string, Indicator> Indicators { get; }

        public FeatureId<TCollectionId> FeatureId { get; }
    }

    [Serializable]
    public class Place : Place<string>
    {

        [JsonConstructor]
        public Place(string id, string name, FeatureId featureId, string? group) : base(id, name, featureId, group)
        {
        }

        public Place(string id, string name, FeatureId featureId) : this(id, name, featureId, null)
        {
        }

        public new FeatureId FeatureId => (FeatureId)base.FeatureId;
    }
}