using System;
using System.Security.Claims;
using DHI.Services;
using DHI.Services.Jobs.Workflows;

namespace JobAutomator.InMemoryRepository;
public sealed class InMemoryTaskRepository : ICodeWorkflowRepository
{
    public bool Contains(string id, ClaimsPrincipal u = null) => true;
    public Maybe<CodeWorkflow> Get(string id, ClaimsPrincipal u = null) =>
            new CodeWorkflow(id, id, "JobAutomator.Local").ToMaybe();
    public IEnumerable<CodeWorkflow> GetAll(ClaimsPrincipal u = null) => Enumerable.Empty<CodeWorkflow>();
    public IEnumerable<string> GetIds(ClaimsPrincipal u = null) => Enumerable.Empty<string>();
    public void Add(CodeWorkflow e, ClaimsPrincipal u = null) { }
    public void Update(CodeWorkflow e, ClaimsPrincipal u = null) { }
    public void Remove(string id, ClaimsPrincipal u = null) { }

    public int Count(ClaimsPrincipal user = null) => 0;
}
