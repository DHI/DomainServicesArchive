namespace DHI.Services.Models
{
    using DHI.Services.Models.Converters;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     Json model data reader repository.
    /// </summary>
    public class ModelDataReaderRepository : JsonRepository<IModelDataReader, string>, IModelDataReaderRepository
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelDataReaderRepository" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public ModelDataReaderRepository(string filePath)
        : this(filePath, Array.Empty<JsonConverter>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelDataReaderRepository" /> class and add custom converters
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="converters"></param>
        public ModelDataReaderRepository(string filePath, IEnumerable<JsonConverter> converters)
        : base(filePath, PrepareConverters(filePath, converters))
        {
        }

        public virtual IEnumerable<Scenario> GetScenarios(string id, ClaimsPrincipal user = null)
        {
            return new List<Scenario>();
        }

        public virtual IEnumerable<Simulation> GetSimulations(string id, ClaimsPrincipal user = null)
        {
            return new List<Simulation>();
        }

        private static IEnumerable<JsonConverter> AppendDefaultConverter(IEnumerable<JsonConverter> converters)
        {
            var list = converters?.ToList() ?? new List<JsonConverter>();

            if (!list.OfType<IModelDataReaderConverter>().Any())
            {
                list.Add(new IModelDataReaderConverter());
            }

            return list;
        }

        private static IEnumerable<JsonConverter> PrepareConverters(string filePath, IEnumerable<JsonConverter> incomingConverters)
        {
            var json = File.ReadAllText(filePath);

            if (!ModelDataReaderTypeRegistry.GetAll().Any())
            {
                ModelDataReaderJsonHelper.AutoRegisterTypesFromJson(json);
            }

            return AppendDefaultConverter(incomingConverters);
        }
    }
}