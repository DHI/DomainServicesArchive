namespace DHI.Services.Jobs.Automations;

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using TriggerParametersExport;

public class TriggerRepository : ITriggerRepository
{
    private readonly CompositionContainer _container;

    public TriggerRepository(string searchDirectory)
    {
        var catalog = new AggregateCatalog();

        var baseTriggerCatalog = new DirectoryCatalog(searchDirectory);
        catalog.Catalogs.Add(baseTriggerCatalog);

        _container = new CompositionContainer(catalog);
    }

    public TriggerRepository(CompositionContainer container)
    {
        _container = container;
    }

    /// <summary>
    ///     Get the trigger parameters for the specified id.
    /// </summary>
    public Maybe<TriggerParameters<string>> Get(string id, ClaimsPrincipal user = null)
    {
        var export = _container.GetExports<ITriggerParameters, IIdMetadata>()
                               .FirstOrDefault(e => e.Metadata.Id.Equals(id));

        return export?.Value != default
            ? GetParameters(id, export.Value.GetType()).ToMaybe()
            : Maybe.Empty<TriggerParameters<string>>();
    }

    public int Count(ClaimsPrincipal user = null)
    {
        return _container.GetExports<ITriggerParameters>().Count();
    }

    /// <summary>
    ///     Check if the repository contains the specified id.
    /// </summary>
    public bool Contains(string id, ClaimsPrincipal user = null)
    {
        var export = _container.GetExports<ITriggerParameters, IIdMetadata>()
                               .FirstOrDefault(e => e.Metadata.Id.Equals(id));

        return export?.Value != default;
    }

    /// <summary>
    ///     Get all trigger parameters in the repository.
    /// </summary>
    public IEnumerable<TriggerParameters<string>> GetAll(ClaimsPrincipal user = null)
    {
        return _container.GetExports<ITriggerParameters, IIdMetadata>()
                         .Where(e => e.Value != default)
                         .Select(e => GetParameters(e.Metadata.Id, e.Value.GetType()));
    }

    /// <summary>
    ///     Get all trigger ids in the repository.
    /// </summary>
    public IEnumerable<string> GetIds(ClaimsPrincipal user = null)
    {
        return GetAll().Select(a => a.Id);
    }

    private TriggerParameters<string> GetParameters(string id, Type type)
    {
        var triggerAttributes = type
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => !p.GetCustomAttributes(typeof(TriggerParameterIgnoreAttribute), false).Any())
        .Where(p => p.GetCustomAttributes(typeof(TriggerParameterAttribute), false).Any())
        .Select(p =>
        {
            var name = p.Name;
            var attribute = (TriggerParameterAttribute)p
                .GetCustomAttributes(typeof(TriggerParameterAttribute), false)
                .First();

            var param = TriggerParameter.FromAttribute(attribute);
            ApplyClrType(name, p.PropertyType, param);

            var jsonName = JsonNamingPolicy.CamelCase.ConvertName(name);

            var required = attribute.Required;
            return (name: jsonName, parameters: param, required);
        })
        .ToArray();

        var required = triggerAttributes.Where(a => a.required).Select(a => a.name);
        var parameters = triggerAttributes.ToDictionary(a => a.name, a => a.parameters);

        var triggerTypeName = id;
        var knownTriggerNamespace = "DHI.Services.Jobs.Automations.Triggers";
        var fullTriggerTypeName = $"{knownTriggerNamespace}.{triggerTypeName}";

        var triggerType = AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(a => a.GetType(fullTriggerTypeName, throwOnError: false))
            .FirstOrDefault(t => t != null);

        var result = new TriggerParameters<string>(id, parameters, required);

        if (triggerType != null)
        {
            result.FullTypeName = triggerType.FullName;
            result.AssemblyName = triggerType.Assembly.GetName().Name;
            result.AssemblyQualifiedTypeName = $"{triggerType.FullName}, {triggerType.Assembly.GetName().Name}";
        }
        else
        {
            result.FullTypeName = type.FullName;
            result.AssemblyName = type.Assembly.GetName().Name;
            result.AssemblyQualifiedTypeName = $"{type.FullName}, {type.Assembly.GetName().Name}";
        }

        return result;
    }

    private static void ApplyClrType(string propName, Type t, TriggerParameter tp)
    {
        if (Nullable.GetUnderlyingType(t) is Type nt) t = nt;

        if (t.IsArray)
        {
            tp.Type = "array";
            tp.ItemsType = MapScalarType(t.GetElementType());
            return;
        }

        if (t != typeof(string) && t.IsGenericType &&
            typeof(System.Collections.IEnumerable).IsAssignableFrom(t))
        {
            var gt = t.GetGenericArguments()[0];
            tp.Type = "array";
            tp.ItemsType = MapScalarType(gt);
            return;
        }

        if (t.IsEnum)
        {
            tp.Type = "string";
            tp.EnumValues = Enum.GetNames(t);
            return;
        }


        tp.Type = MapScalarType(t);

    }

    private static string MapScalarType(Type t, TriggerParameter tp = null)
    {
        if (t == typeof(string)) return "string";
        if (t == typeof(bool)) return "boolean";
        if (t == typeof(byte) || t == typeof(sbyte) ||
            t == typeof(short) || t == typeof(ushort) ||
            t == typeof(int) || t == typeof(uint) ||
            t == typeof(long) || t == typeof(ulong)) return "integer";
        if (t == typeof(float) || t == typeof(double) || t == typeof(decimal)) return "number";

        if (t == typeof(DateTime) || t == typeof(DateTimeOffset)) { if (tp != null) tp.Format = "date-time"; return "string"; }
        if (t == typeof(TimeSpan)) { if (tp != null) tp.Format = "duration"; return "string"; }
        if (t == typeof(Guid)) { if (tp != null) tp.Format = "uuid"; return "string"; }

        return "object";
    }
}