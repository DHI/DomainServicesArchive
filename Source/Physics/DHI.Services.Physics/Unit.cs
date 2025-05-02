namespace DHI.Services.Physics
{
    using DHI.Physics;
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using System.Text.Json;
    using ICloneable = ICloneable;
    using DHI.Services.Physics.Converters;

    [Serializable]
    public class Unit : DHI.Physics.Unit, IEntity<string>, ICloneable
    {
        private readonly Dictionary<string, object> _metadata = new Dictionary<string, object>();

        public Unit(string id, string description, string abbreviation, Dimension dimension)
            : base(id, description, abbreviation, dimension)
        {
        }

        public Unit(string id, string description, string abbreviation, IUnit unit)
            : base(id, description, abbreviation, unit)
        {
        }

        public Unit(string id, string description, string abbreviation, double factor, Dimension dimension, double offset = 0)
            : base(id, description, abbreviation, factor, dimension, offset)
        {
        }

        /// <summary>
        ///     Gets the metadata.
        /// </summary>
        /// <value>The metadata.</value>
        public virtual IDictionary<string, object> Metadata => _metadata;

        /// <summary>
        ///     Determines whether the Metadata property should be serialized
        /// </summary>
        public bool ShouldSerializeMetadata()
        {
            return _metadata.Count > 0;
        }

        public T Clone<T>()
        {
            var writeOptions = new JsonSerializerOptions();
            writeOptions.Converters.Add(new JsonStringEnumConverter());
            writeOptions.Converters.Add(new UnitConverter());
            writeOptions.Converters.Add(new DimensionConverter());
            var json = JsonSerializer.Serialize(this, typeof(T), writeOptions);
            
            var readOptions = new JsonSerializerOptions();
            readOptions.Converters.Add(new JsonStringEnumConverter());
            readOptions.Converters.Add(new UnitConverter());
            readOptions.Converters.Add(new DimensionConverter());
            return JsonSerializer.Deserialize<T>(json, readOptions);
        }
    }
}