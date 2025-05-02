namespace DHI.Services.GIS
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using Spatial;

    public class FeatureCollectionInfo<TId> : BaseGroupedEntity<TId>
    {
        public FeatureCollectionInfo(TId id, string name)
            : base(id, name, null)
        {
        }

        [JsonConstructor]
        public FeatureCollectionInfo(TId id, string name, string group, IList<Attribute> attributes = null)
            : base(id, name, group)
        {
            if (attributes is null)
            {
                return;
            }

            foreach (var attribute in attributes)
            {
                Attributes.Add(attribute);
            }
        }

        public IList<IAttribute> Attributes { get; } = new List<IAttribute>();

        public bool ShouldSerializeAttributes()
        {
            return Attributes.Count > 0;
        }
    }

    public class FeatureCollectionInfo : FeatureCollectionInfo<string>
    {
        public FeatureCollectionInfo(string id, string name)
            : base(id, name)
        {
        }

        public FeatureCollectionInfo(string id, string name, string group, IList<Attribute> attributes = null)
            : base(id, name, group, attributes)
        {
        }
    }
}