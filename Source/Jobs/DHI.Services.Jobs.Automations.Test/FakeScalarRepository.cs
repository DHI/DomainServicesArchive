namespace DHI.Services.Jobs.Automations.Test;

using System;
using System.Collections.Generic;
using System.Security.Claims;
using DHI.Services.Scalars;

public class FakeScalarRepository<T> : IScalarRepository<string, T> where T : struct
{
    public Maybe<Scalar<string, T>> Get(string id, ClaimsPrincipal user = null)
    {
        return new Maybe<Scalar<string, T>>();
    }

    public int Count(ClaimsPrincipal user = null)
    {
        return 0;
    }

    public bool Contains(string id, ClaimsPrincipal user = null)
    {
        return false;
    }

    public IEnumerable<Scalar<string, T>> GetAll(ClaimsPrincipal user = null)
    {
        return Array.Empty<Scalar<string, T>>();
    }

    public IEnumerable<string> GetIds(ClaimsPrincipal user = null)
    {
        return Array.Empty<string>();
    }

    public void Add(Scalar<string, T> entity, ClaimsPrincipal user = null)
    {
        // Do nothing
    }

    public void Remove(string id, ClaimsPrincipal user = null)
    {
        // Do nothing
    }

    public void Update(Scalar<string, T> entity, ClaimsPrincipal user = null)
    {
        // Do nothing
    }

    public void SetData(string id, ScalarData<T> data, ClaimsPrincipal user = null)
    {
        // Do nothing
    }

    public void SetLocked(string id, bool locked, ClaimsPrincipal user = null)
    {
        // Do nothing
    }
}