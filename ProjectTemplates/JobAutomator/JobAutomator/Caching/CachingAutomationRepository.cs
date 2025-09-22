using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Claims;
using DHI.Services;
using DHI.Services.Jobs.Automations;
using DHI.Services.Provider.DS;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MemoryCache = Microsoft.Extensions.Caching.Memory.MemoryCache;

namespace JobAutomator.Caching;

/// <summary>
/// Transparent decorator that adds IMemoryCache to any IAutomationRepository.
/// </summary>
public sealed class CachingAutomationRepository : IAutomationRepository
{
    /* ------------------------------------------------------------------ */
    private readonly IAutomationRepository _inner;
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _opts;
    private readonly ILogger _log;

    private readonly string _localVersionFile;
    private readonly string _versionEndpoint;
    private readonly HttpClient _http;
    private readonly IAccessTokenProvider _token;

    private readonly ConcurrentDictionary<string, byte> _keys = new();

    private static string AllKey => "automations::all";
    private static string IdKey(string id) => $"automations::id::{id}";
    private static string GroupKey(string g) => $"automations::group::{g}";
    private static string CGrpKey(string g) => $"automations::contains::{g}";
    /* ------------------------------------------------------------------ */
    public CachingAutomationRepository(IAutomationRepository inner, CacheSettings cfg, IAccessTokenProvider tokenProvider, string versionEndpoint, ILogger log = null)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _token = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        _versionEndpoint = versionEndpoint ?? throw new ArgumentNullException(nameof(versionEndpoint));
        _localVersionFile = ResolvePath(cfg.LocalVersionFilePath);
        _log = log;

        Directory.CreateDirectory(Path.GetDirectoryName(_localVersionFile)!);

        _http = new HttpClient();
        _cache = new MemoryCache(new MemoryCacheOptions());

        _opts = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
        };
    }
    /* =====================  public READ  ============================== */
    public IEnumerable<Automation<string>> GetAll(ClaimsPrincipal u = null)
    {
        EnsureFreshness();
        return CacheGetOrCreate(AllKey, () => _inner.GetAll(u).ToArray());
    }
    public Maybe<Automation<string>> Get(string id, ClaimsPrincipal u = null)
    {
        EnsureFreshness();
        return CacheGetOrCreate(IdKey(id), () => _inner.Get(id, u));
    }
    public IEnumerable<Automation<string>> GetByGroup(string g, ClaimsPrincipal u = null)
    {
        EnsureFreshness();
        return CacheGetOrCreate(GroupKey(g), () => _inner.GetByGroup(g, u).ToArray());
    }
    public bool ContainsGroup(string g, ClaimsPrincipal u = null)
    {
        EnsureFreshness();
        return CacheGetOrCreate(CGrpKey(g), () => _inner.ContainsGroup(g, u));
    }

    /* ----------------------- passthroughs ----------------------------- */
    public int Count(ClaimsPrincipal u = null) => _inner.Count(u);
    public bool Contains(string id, ClaimsPrincipal u = null) => Get(id, u).HasValue;
    public IEnumerable<string> GetIds(ClaimsPrincipal u = null) => GetAll(u).Select(a => a.Id);
    public IEnumerable<string> GetFullNames(string g, ClaimsPrincipal u = null) => _inner.GetFullNames(g, u);
    public IEnumerable<string> GetFullNames(ClaimsPrincipal u = null) => _inner.GetFullNames(u);

    /* =====================  public WRITE  ============================= */
    public void Add(Automation<string> e, ClaimsPrincipal u = null) { _inner.Add(e, u); Invalidate(e); TouchLocal(); }
    public void Update(Automation<string> e, ClaimsPrincipal u = null) { _inner.Update(e, u); Invalidate(e); TouchLocal(); }
    public void Remove(string id, ClaimsPrincipal u = null)
    {
        _inner.Remove(id, u);
        RemoveKey(IdKey(id));
        RemoveKey(AllKey);
        RemovePrefix("automations::group::");
        RemovePrefix("automations::contains::");
        TouchLocal();
    }

    /* =====================  internal ================================= */
    private T CacheGetOrCreate<T>(string key, Func<T> factory) =>
        _cache.GetOrCreate(key, e =>
        {
            e.SetOptions(_opts);
            e.RegisterPostEvictionCallback((k, _, _, _) => _keys.TryRemove(k.ToString(), out _));
            _keys.TryAdd(key, 0);
            _log?.LogTrace("Cache miss → {Key}", key);
            return factory();
        });

    private void RemoveKey(string k) => _cache.Remove(k);
    private void RemovePrefix(string p)
    {
        foreach (var k in _keys.Keys.Where(k => k.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            _cache.Remove(k);
    }
    private void Invalidate(Automation<string> a)
    {
        RemoveKey(IdKey(a.Id));
        RemoveKey(AllKey);
        RemoveKey(GroupKey(a.Group));
        RemoveKey(CGrpKey(a.Group));
    }
    private void ClearAll()
    {
        foreach (var k in _keys.Keys)
            _cache.Remove(k);
    }

    /* --------------------  VERSION LOGIC ----------------------------- */
    private readonly object _versionLock = new();
    private void EnsureFreshness()
    {
        lock (_versionLock)
        {
            var serverTs = GetServerTimestamp();
            var localTs = GetVersionTimestamp();

            if (serverTs <= localTs)
                return;

            _log?.LogInformation("Automation cache stale (server {ServerTs:o} > local {LocalTs:o}) – refreshing.",
                                  serverTs, localTs);

            ClearAll();
            _inner.GetAll();
            WriteLocalTimestamp(serverTs);
        }
    }

    /* --- Helpers ------------------------------------------------------ */
    private void WriteLocalTimestamp(DateTime ts)
    {
        File.WriteAllText(_localVersionFile, ts.ToString("O"));
    }
    private void TouchLocal() => WriteLocalTimestamp(DateTime.UtcNow);

    private DateTime GetServerTimestamp()
    {
        var req = new HttpRequestMessage(HttpMethod.Get, _versionEndpoint);
        var token = _token.GetAccessToken().GetAwaiter().GetResult();
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var resp = _http.Send(req);
        resp.EnsureSuccessStatusCode();
        var txt = resp.Content.ReadAsStringAsync().Result.Trim('"', '\n', '\r');
        return DateTime.Parse(txt, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime();
    }

    private static string ResolvePath(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            input = "version.txt";

        var full = Path.GetFullPath(input);

        return Path.GetExtension(full).Equals(".txt", StringComparison.OrdinalIgnoreCase)
               ? full
               : Path.Combine(full, "version.txt");
    }

    public DateTime GetVersionTimestamp()
    {
        if (!File.Exists(_localVersionFile))
            return DateTime.MinValue;
        var txt = File.ReadAllText(_localVersionFile);
        return DateTime.TryParseExact(txt, "O", CultureInfo.InvariantCulture,
                                      DateTimeStyles.AssumeUniversal, out var ts)
               ? ts.ToUniversalTime() : DateTime.MinValue;
    }

    public DateTime TouchVersion()
    {
        throw new NotSupportedException();
    }
}
