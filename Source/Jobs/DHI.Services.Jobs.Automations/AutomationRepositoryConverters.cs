namespace DHI.Services.Jobs.Automations;

using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Converters;

internal static class AutomationRepositoryConverters
{
    internal static IEnumerable<JsonConverter> Required =>
        new JsonConverter[]
        {
            new TriggerConverter(),
            new DictionaryTypeResolverConverter<string, Automation<string>>(isNestedDictionary: true),
            new JsonCollectionItemConverter<ITrigger, TriggerConverter>(),
            new TypeResolverConverter<Parameters>()
        };

    internal static IEnumerable<JsonConverter> All =>
        Required.Concat(
            new JsonConverter[]
            {
                new JsonStringEnumConverter(),
                new EnumerationConverter(),
                new TypeStringConverter(),
                new ObjectToInferredTypeConverter(),
                new AutoNumberToStringConverter(),
                new PermissionConverter(),
            });
}