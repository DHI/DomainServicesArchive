using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using DHI.Services;
using DHI.Services.Scalars;

namespace JobAutomator.InMemoryRepository;
public sealed class InMemoryScalarRepository : IGroupedScalarRepository<string, int>
{
    private readonly ConcurrentDictionary<string, Scalar<string, int>> _map = new();

    public IEnumerable<Scalar<string, int>> GetAll(ClaimsPrincipal u = null) => _map.Values;

    public Maybe<Scalar<string, int>> Get(string id, ClaimsPrincipal u = null) =>
        _map.TryGetValue(id, out var s) ? s.ToMaybe() : Maybe.Empty<Scalar<string, int>>();

    public bool Contains(string id, ClaimsPrincipal u = null) => _map.ContainsKey(id);

    public void Add(Scalar<string, int> scalar, ClaimsPrincipal u = null) => _map[scalar.Id] = scalar;

    public void Remove(string id, ClaimsPrincipal u = null) => _map.TryRemove(id, out _);

    public void Update(Scalar<string, int> scalar, ClaimsPrincipal u = null) => _map[scalar.Id] = scalar;

    public void SetData(string id, ScalarData<int> data, ClaimsPrincipal u = null)
    {
        if (_map.TryGetValue(id, out var s))
            s.SetData(data);
    }

    public void SetLocked(string id, bool locked, ClaimsPrincipal u = null)
    {
        if (_map.TryGetValue(id, out var s))
            s.Locked = locked;
    }

    public bool ContainsGroup(string group, ClaimsPrincipal u = null) =>
        _map.Values.Any(s => (s.Group ?? "").StartsWith(group, StringComparison.Ordinal));

    public IEnumerable<Scalar<string, int>> GetByGroup(string group, ClaimsPrincipal u = null) =>
        _map.Values.Where(s => (s.Group ?? "").StartsWith(group, StringComparison.Ordinal));

    public int Count(ClaimsPrincipal user = null) => _map.Count;
    public IEnumerable<string> GetIds(ClaimsPrincipal user = null) => _map.Keys;

    public IEnumerable<string> GetFullNames(string group, ClaimsPrincipal user = null) =>
        _map.Values
            .Where(s => (s.Group ?? "").StartsWith(group, StringComparison.Ordinal))
            .Select(s => string.IsNullOrEmpty(s.Group) ? s.Name : $"{s.Group}/{s.Name}");

    public IEnumerable<string> GetFullNames(ClaimsPrincipal user = null) =>
        _map.Values.Select(s => string.IsNullOrEmpty(s.Group) ? s.Name : $"{s.Group}/{s.Name}");
}
