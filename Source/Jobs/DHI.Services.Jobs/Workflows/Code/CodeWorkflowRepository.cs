namespace DHI.Services.Jobs.Workflows
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;
    using DHI.Services;

    public class CodeWorkflowRepository : ImmutableJsonRepository<CodeWorkflow, string>, ICodeWorkflowRepository
    {
        public CodeWorkflowRepository(string filePath, IEnumerable<JsonConverter> converters = null)
            : base(filePath)
        {
            if (converters?.Any() == true)
            {
                ConfigureJsonSerializer((serializer, deserializer) =>
                {
                    serializer.AddConverters(converters);
                    deserializer.AddConverters(converters);
                });
            }
        }
    }
}