using DHI.Services.GIS;
using DHI.Services.Provider.ShapeFile;
using DHI.Services.Samples.GIS.Shapefile.Composition;
using System.Data;
using System.IO;

namespace DHI.Services.Samples.GIS.Shapefile.DomainAdapters
{
    /// <summary>
    /// Thin application adapter for the WPF VM.
    /// Reads go through GisService; SaveAs uses a new FeatureRepository pointing to the destination folder.
    /// </summary>
    public sealed class GisPlaygroundAdapter
    {
        private readonly GisRuntime _runtime;

        public GisPlaygroundAdapter(GisRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public bool IsSingleFile => File.Exists(_runtime.RootPath);

        public IEnumerable<string> ListCollectionIds()
        {
            if (IsSingleFile)
            {
                var id = Path.GetFileName(_runtime.RootPath);
                if (_runtime.Service.Exists(id)) return new[] { id };

                if (_runtime.Service.Exists(_runtime.RootPath)) return new[] { _runtime.RootPath };

                return Array.Empty<string>();
            }

            return _runtime.Service.GetAll().Select(fc => fc.Id);
        }

        public FeatureCollection<string> GetCollection(string id, bool associations, string? outSrs = null)
            => _runtime.Service.Get(id, associations, outSrs);

        public FeatureCollection<string> GetCollectionFiltered(string id, string attributeName, QueryOperator op, object value, bool includeAssociations = false, string? outSrs = null)
        {
            var filter = new List<QueryCondition> { new QueryCondition(attributeName, op, value) };
            return _runtime.Service.Get(id, filter, includeAssociations, outSrs);
        }

        public FeatureCollectionInfo<string> GetCollectionInfo(string id)
            => _runtime.Service.GetInfo(id);

        public DataTable ToAttributesTable(FeatureCollection<string> collection)
        {
            var dt = new DataTable("Attributes");
            dt.Columns.Add("Index", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Type", typeof(string));
            dt.Columns.Add("Length", typeof(int));

            for (int i = 0; i < collection.Attributes.Count; i++)
            {
                var a = collection.Attributes[i];
                dt.Rows.Add(i, a.Name, a.DataType?.Name ?? "string", a.Length);
            }
            return dt;
        }

        public DataTable ToFeaturesTable(FeatureCollection<string> collection)
        {
            var names = collection.Attributes.Select(a => a.Name).ToList();
            var dt = Helpers.DataTableBuilder.Create(names);

            foreach (var feat in collection.Features)
            {
                var row = dt.NewRow();
                foreach (var n in names)
                {
                    feat.AttributeValues.TryGetValue(n, out var v);
                    row[n] = v ?? DBNull.Value;
                }
                dt.Rows.Add(row);
            }
            return dt;
        }

        /// <summary>
        /// Save the specified collection as a new shapefile at <paramref name="destShpPath"/>.
        /// This constructs a new FeatureRepository pointed at the destination folder and calls Add(...).
        /// </summary>
        public void SaveAs(string sourceCollectionId, string destShpPath)
        {
            if (string.IsNullOrWhiteSpace(destShpPath))
                throw new ArgumentNullException(nameof(destShpPath));

            var coll = _runtime.Service.Get(sourceCollectionId, associations: false);

            var destFolder = Path.GetDirectoryName(destShpPath)
                               ?? throw new InvalidOperationException("Invalid destination path.");
            var destFileName = Path.GetFileName(destShpPath);

            var dst = new FeatureCollection<string>(
                destFileName,
                Path.GetFileNameWithoutExtension(destFileName),
                coll.Features);

            foreach (var a in coll.Attributes)
                dst.Attributes.Add(a);

            foreach (var kv in coll.Metadata)
                dst.Metadata[kv.Key] = kv.Value;

            var destRepo = new FeatureRepository(destFolder);
            destRepo.Add(dst);
        }

        public FeatureCollection<string> GetCollection(string id)
            => _runtime.Service.Get(id, associations: false);
    }
}
