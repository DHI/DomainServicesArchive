namespace DHI.Services.GIS
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using Spatial;

    public class FeatureCollection<TId> : FeatureCollectionInfo<TId>, IFeatureCollection
    {
        public FeatureCollection(TId id, string name, IList<IFeature> features = null)
            : base(id, name, null)
        {
            if (features != null)
            {
                Features = features;
            }
        }

        [JsonConstructor]
        public FeatureCollection(TId id, string name, string group, IList<IFeature> features = null)
            : base(id, name, group)
        {
            if (features != null)
            {
                Features = features;
            }
        }

        public FeatureCollection(TId id, string name, string group, IFeatureCollection collection)
            : base(id, name, group)
        {
            Guard.Against.Null(collection, nameof(collection));
            if (collection.Features != null)
            {
                Features = collection.Features;
            }

            if (collection.Attributes is null)
            {
                return;
            }

            foreach (var attribute in collection.Attributes)
            {
                Attributes.Add(attribute);
            }
        }

        public IList<IFeature> Features { get; } = new List<IFeature>();

        public bool ShouldSerializeFeatures()
        {
            return Features.Count > 0;
        }

        public FeatureCollectionInfo<TId> GetInfo()
        {
            var info = new FeatureCollectionInfo<TId>(Id, Name, Group);
            foreach (var attribute in Attributes)
            {
                info.Attributes.Add(attribute);
            }

            foreach (var data in Metadata)
            {
                info.Metadata.Add(data.Key, data.Value);
            }

            return info;
        }
    }

    public class FeatureCollection : FeatureCollection<string>
    {
        public FeatureCollection(string id, string name, IList<IFeature> features = null)
            : base(id, name, features)
        {
        }

        public FeatureCollection(string id, string name, string group, IList<IFeature> features = null)
            : base(id, name, group, features)
        {
        }

        public FeatureCollection(string id, string name, string group, IFeatureCollection collection)
            : base(id, name, group, collection)
        {
        }
    }
}