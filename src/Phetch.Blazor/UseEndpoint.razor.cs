﻿namespace Phetch.Blazor;

using Microsoft.AspNetCore.Components;
using Phetch.Core;

public sealed partial class UseEndpoint<TArg, TResult>
    : IDisposable, IUseEndpoint<TArg, TResult>
{
    private Query<TArg, TResult>? _query;
    private Endpoint<TArg, TResult>? _endpoint;

    /// <summary>
    /// The endpoint to use.
    /// </summary>
    [Parameter, EditorRequired]
    public Endpoint<TArg, TResult>? Endpoint
    {
        get => _endpoint;
        set
        {
            if (ReferenceEquals(_endpoint, value))
                return;
            TryUnsubscribe(_query);
            if (value is not null)
            {
                _query = GetQuery(value, _options);
                _endpoint = value;
            }
        }
    }

    [Parameter, EditorRequired]
    public RenderFragment<Query<TArg, TResult>> ChildContent { get; set; } = null!;

    /// <inheritdoc/>
    [Parameter]
    public bool UseErrorBoundary { get; set; } = false;

    // Other parameters can be set before Arg is set, so don't use Arg until it has been set. A
    // nullable here would not work for queries where null is a valid argument.
    private bool _hasSetArg = false;
    private TArg _arg = default!;

    /// <summary>
    /// The argument to supply to the query. If not supplied, the query will not be run automatically.
    /// </summary>
    [Parameter]
    public TArg Arg
    {
        get => _arg;
        set
        {
            _arg = value;
            _hasSetArg = true;
            _query?.SetArg(value);
        }
    }

    private QueryOptions<TArg, TResult>? _options;

    /// <inheritdoc/>
    [Parameter]
    public QueryOptions<TArg, TResult>? Options
    {
        get => _options;
        set
        {
            if (_options == value)
                return;
            TryUnsubscribe(_query);
            if (_endpoint is not null)
            {
                _query = GetQuery(_endpoint, value);
            }

            _options = value;
        }
    }

    /// <inheritdoc/>
    [Parameter]
    public Action<QuerySuccessContext<TArg, TResult>>? OnSuccess { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public Action<QueryFailureContext<TArg>>? OnFailure { get; set; }

    void IDisposable.Dispose()
    {
        TryUnsubscribe(_query);
    }

    private Query<TArg, TResult> GetQuery(Endpoint<TArg, TResult> endpoint, QueryOptions<TArg, TResult>? options)
    {
        var newQuery = endpoint.Use(options);
        newQuery.StateChanged += StateHasChanged;
        newQuery.Succeeded += SuccessCallback;
        newQuery.Failed += FailureCallback;
        if (_hasSetArg)
            newQuery.SetArg(_arg);
        return newQuery;
    }

    private void TryUnsubscribe(Query<TArg, TResult>? query)
    {
        if (query is not null)
        {
            query.StateChanged -= StateHasChanged;
            query.Succeeded -= SuccessCallback;
            query.Failed -= FailureCallback;
            query.Detach();
        }
    }

    private void SuccessCallback(QuerySuccessContext<TArg, TResult> context) { OnSuccess?.Invoke(context); }

    private void FailureCallback(QueryFailureContext<TArg> context) { OnFailure?.Invoke(context); }

}