﻿namespace Phetch.Blazor;

using Microsoft.AspNetCore.Components;
using Phetch.Core;

public sealed class ObserveQuery<TArg, TResult> : ComponentBase, IDisposable
{
    private Query<TArg, TResult>? _target;
    [Parameter, EditorRequired]
    public Query<TArg, TResult>? Target
    {
        get => _target;
        set
        {
            if (_target == value) return;
            TryUnsubscribe(_target);
            if (value is not null)
            {
                value.StateChanged += OnStateChanged;
                value.Succeeded += SuccessCallback;
                value.Failed += FailureCallback;
            }
            _target = value;
        }
    }

    [Parameter, EditorRequired]
    public Action OnChanged { get; set; } = null!;

    /// <summary>
    /// Callback function to call when the query succeeds. This will only be called if this
    /// component is still mounted when the query succeeds.
    /// </summary>
    [Parameter]
    public Action<QuerySuccessEventArgs<TArg, TResult>>? OnSuccess { get; set; }

    /// <summary>
    /// Callback function to call when the query fails. This will only be called if this component
    /// is still mounted when the query fails.
    /// </summary>
    [Parameter]
    public Action<QueryFailureEventArgs<TArg>>? OnFailure { get; set; }

    public void Dispose() => TryUnsubscribe(_target);

    private void TryUnsubscribe(Query<TArg, TResult>? query)
    {
        if (query is not null)
        {
            query.StateChanged -= StateHasChanged;
            query.Detach();
        }
    }

    private void OnStateChanged()
    {
        OnChanged?.Invoke();
    }

    private void SuccessCallback(QuerySuccessEventArgs<TArg, TResult> context) { OnSuccess?.Invoke(context); }

    private void FailureCallback(QueryFailureEventArgs<TArg> context) { OnFailure?.Invoke(context); }
}
