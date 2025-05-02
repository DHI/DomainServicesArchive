namespace DHI.Services.Jobs.Automations;

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Security.Claims;
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
                               .GetProperties()
                               .Where(p => !p.GetCustomAttributes(typeof(TriggerParameterIgnoreAttribute), false).Any())
                               .Where(p => p.GetCustomAttributes(typeof(TriggerParameterAttribute), false).Any())
                               .Select(p =>
                                {
                                    var name = p.Name;
                                    var attribute = p.GetCustomAttributes(typeof(TriggerParameterAttribute), false).FirstOrDefault() as TriggerParameterAttribute;
                                    var required = attribute!.Required;
                                    var parameters = TriggerParameter.FromAttribute(attribute);
                                    return (name, parameters, required);
                                })
                               .ToArray();

        var required = triggerAttributes.Where(a => a.required).Select(a => a.name);

        var parameters = triggerAttributes.ToDictionary(a => a.name, a => a.parameters);

        return new TriggerParameters<string>(id, parameters, required);
    }
}