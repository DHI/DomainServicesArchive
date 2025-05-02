namespace DHI.Services.Meshes
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using Spatial;
    using Spatial.Data;

    [Serializable]
    public class MeshInfo<TId> : BaseGroupedEntity<TId>
    {
        [JsonConstructor]
        public MeshInfo(TId id, string name, string? group, IEnumerable<Item> items, DateRange dateRange) : base(id, name, group)
        {
            Items = items;
            DateRange = dateRange;
        }

        public MeshInfo(TId id, string name, IEnumerable<Item> items, DateRange dateRange) 
            : this(id, name, null, items, dateRange)
        {
        }

        public IEnumerable<Item> Items { get; }

        public DateRange DateRange { get; }

        public string? Projection { get; set; }

        public BoundingBox? BoundingBox { get; set; }
    }

    public class MeshInfo : MeshInfo<string>
    {
        [JsonConstructor]
        public MeshInfo(string id, string name, string? group, IEnumerable<Item> items, DateRange dateRange) : base(id, name, group, items, dateRange)
        {
        }

        public MeshInfo(string id, string name, IEnumerable<Item> items, DateRange dateRange) : base(id, name, items, dateRange)
        {
        }
    }
}