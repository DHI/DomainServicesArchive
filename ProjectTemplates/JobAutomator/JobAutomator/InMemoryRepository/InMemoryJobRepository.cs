using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using DHI.Services;
using DHI.Services.Jobs;

namespace JobAutomator.InMemoryRepository;
public sealed class InMemoryJobRepository : IJobRepository<Guid, string>
{
    private readonly ConcurrentDictionary<Guid, Job<Guid, string>> _mem = new();

    public Maybe<Job<Guid, string>> Get(Guid id, ClaimsPrincipal u = null) =>
        _mem.TryGetValue(id, out var j) ? j.ToMaybe() : Maybe.Empty<Job<Guid, string>>();
    public bool Contains(Guid id, ClaimsPrincipal u = null) => _mem.ContainsKey(id);
    public IEnumerable<Job<Guid, string>> GetAll(ClaimsPrincipal u = null) => _mem.Values;
    public IEnumerable<Guid> GetIds(ClaimsPrincipal u = null) => _mem.Keys;
    public void Add(Job<Guid, string> e, ClaimsPrincipal u = null) => _mem[e.Id] = e;
    public void Remove(Guid id, ClaimsPrincipal u = null) => _mem.TryRemove(id, out _);
    public void Update(Job<Guid, string> e, ClaimsPrincipal u = null) => _mem[e.Id] = e;
    public IEnumerable<Job<Guid, string>> Get(Query<Job<Guid, string>> q, ClaimsPrincipal u = null) => _mem.Values;
    public Job<Guid, string> GetLast(Query<Job<Guid, string>> q, ClaimsPrincipal u = null) =>
        _mem.Values.OrderByDescending(j => j.Requested).FirstOrDefault();
    public void Remove(Query<Job<Guid, string>> q, ClaimsPrincipal u = null)
    {
        foreach (var k in _mem.Keys)
            _mem.TryRemove(k, out _);
    }
    public void UpdateField<TField>(Guid jobId, string fieldName, TField value, ClaimsPrincipal u = null)
    {
        if (_mem.TryGetValue(jobId, out var j)
            && string.Equals(fieldName, "heartbeat", StringComparison.OrdinalIgnoreCase)
            && value is DateTime dt)
        {
            j.Heartbeat = dt;
        }
    }

    public int Count(ClaimsPrincipal user = null) => _mem.Count;
}
