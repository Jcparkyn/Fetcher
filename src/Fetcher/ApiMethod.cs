﻿namespace Fetcher;

using System;
using System.Threading.Tasks;

public class ApiMethod<TArg, TResult>
{
    private readonly QueryCache<TArg, TResult> _cache;
    private readonly QueryObserverOptions<TResult> _options;

    public ApiMethod(
        Func<TArg, Task<TResult>> queryFn,
        QueryObserverOptions<TResult>? options = null)
    {
        options ??= new();
        _cache = new(queryFn, options);
        _options = options;
    }

    public QueryObserver<TArg, TResult> Use()
    {
        return new QueryObserver<TArg, TResult>(_cache, _options);
    }

    public void InvalidateAll()
    {
        _cache.InvalidateAll();
    }

    public void Invalidate(TArg arg)
    {
        _cache.Invalidate(arg);
    }

    public void UpdateQueryData(TArg arg, TResult resultData) => _cache.UpdateQueryData(arg, resultData);
}