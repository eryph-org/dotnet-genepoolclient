// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Eryph.GenePool.Client.Internal;

public static class TaskExtensions
{
    public static WithCancellationTaskAwaitable AwaitWithCancellation(this Task task, CancellationToken cancellationToken)
        => new WithCancellationTaskAwaitable(task, cancellationToken);

    public static WithCancellationTaskAwaitable<T> AwaitWithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        => new WithCancellationTaskAwaitable<T>(task, cancellationToken);

    public static WithCancellationValueTaskAwaitable<T> AwaitWithCancellation<T>(this ValueTask<T> task, CancellationToken cancellationToken)
        => new WithCancellationValueTaskAwaitable<T>(task, cancellationToken);

    public static T EnsureCompleted<T>(this Task<T> task)
    {
#if DEBUG
        VerifyTaskCompleted(task.IsCompleted);
#endif
#pragma warning disable AZC0102 // Do not use GetAwaiter().GetResult(). Use the TaskExtensions.EnsureCompleted() extension method instead.
        return task.GetAwaiter().GetResult();
#pragma warning restore AZC0102 // Do not use GetAwaiter().GetResult(). Use the TaskExtensions.EnsureCompleted() extension method instead.
    }

    public static void EnsureCompleted(this Task task)
    {
#if DEBUG
        VerifyTaskCompleted(task.IsCompleted);
#endif
#pragma warning disable AZC0102 // Do not use GetAwaiter().GetResult(). Use the TaskExtensions.EnsureCompleted() extension method instead.
        task.GetAwaiter().GetResult();
#pragma warning restore AZC0102 // Do not use GetAwaiter().GetResult(). Use the TaskExtensions.EnsureCompleted() extension method instead.
    }

    public static T EnsureCompleted<T>(this ValueTask<T> task)
    {
#if DEBUG
        VerifyTaskCompleted(task.IsCompleted);
#endif
#pragma warning disable AZC0102 // Do not use GetAwaiter().GetResult(). Use the TaskExtensions.EnsureCompleted() extension method instead.
        return task.GetAwaiter().GetResult();
#pragma warning restore AZC0102 // Do not use GetAwaiter().GetResult(). Use the TaskExtensions.EnsureCompleted() extension method instead.
    }

    public static void EnsureCompleted(this ValueTask task)
    {
#if DEBUG
        VerifyTaskCompleted(task.IsCompleted);
#endif
#pragma warning disable AZC0102 // Do not use GetAwaiter().GetResult(). Use the TaskExtensions.EnsureCompleted() extension method instead.
        task.GetAwaiter().GetResult();
#pragma warning restore AZC0102 // Do not use GetAwaiter().GetResult(). Use the TaskExtensions.EnsureCompleted() extension method instead.
    }

    public static Enumerable<T> EnsureSyncEnumerable<T>(this IAsyncEnumerable<T> asyncEnumerable) => new Enumerable<T>(asyncEnumerable);

    public static ConfiguredValueTaskAwaitable<T> EnsureCompleted<T>(this ConfiguredValueTaskAwaitable<T> awaitable, bool async)
    {
        if (!async)
        {
#if DEBUG
            VerifyTaskCompleted(awaitable.GetAwaiter().IsCompleted);
#endif
        }
        return awaitable;
    }

    public static ConfiguredValueTaskAwaitable EnsureCompleted(this ConfiguredValueTaskAwaitable awaitable, bool async)
    {
        if (!async)
        {
#if DEBUG
            VerifyTaskCompleted(awaitable.GetAwaiter().IsCompleted);
#endif
        }
        return awaitable;
    }

    [Conditional("DEBUG")]
    private static void VerifyTaskCompleted(bool isCompleted)
    {
        if (!isCompleted)
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
            // Throw an InvalidOperationException instead of using
            // Debug.Assert because that brings down nUnit immediately
            throw new InvalidOperationException("Task is not completed");
        }
    }

    /// <summary>
    /// Both <see cref="Enumerable{T}"/> and <see cref="Enumerator{T}"/> are defined as public structs so that foreach can use duck typing
    /// to call <see cref="Enumerable{T}.GetEnumerator"/> and avoid heap memory allocation.
    /// Please don't delete this method and don't make these types private.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct Enumerable<T>(IAsyncEnumerable<T> asyncEnumerable) : IEnumerable<T>
    {
        public Enumerator<T> GetEnumerator() => new Enumerator<T>(asyncEnumerable.GetAsyncEnumerator());

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator<T>(asyncEnumerable.GetAsyncEnumerator());

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public readonly struct Enumerator<T>(IAsyncEnumerator<T> asyncEnumerator) : IEnumerator<T>
    {
#pragma warning disable AZC0107 // Do not call public asynchronous method in synchronous scope.
        public bool MoveNext() => asyncEnumerator.MoveNextAsync().EnsureCompleted();
#pragma warning restore AZC0107 // Do not call public asynchronous method in synchronous scope.

        public void Reset() => throw new NotSupportedException($"{GetType()} is a synchronous wrapper for {asyncEnumerator.GetType()} async enumerator, which can't be reset, so IEnumerable.Reset() calls aren't supported.");

        public T Current => asyncEnumerator.Current;

        object IEnumerator.Current => Current;

#pragma warning disable AZC0107 // Do not call public asynchronous method in synchronous scope.
        public void Dispose() => asyncEnumerator.DisposeAsync().EnsureCompleted();
#pragma warning restore AZC0107 // Do not call public asynchronous method in synchronous scope.
    }

    public readonly struct WithCancellationTaskAwaitable(Task task, CancellationToken cancellationToken)
    {
        private readonly ConfiguredTaskAwaitable _awaitable = task.ConfigureAwait(false);

        public WithCancellationTaskAwaiter GetAwaiter() => new WithCancellationTaskAwaiter(_awaitable.GetAwaiter(), cancellationToken);
    }

    public readonly struct WithCancellationTaskAwaitable<T>(Task<T> task, CancellationToken cancellationToken)
    {
        private readonly ConfiguredTaskAwaitable<T> _awaitable = task.ConfigureAwait(false);

        public WithCancellationTaskAwaiter<T> GetAwaiter() => new WithCancellationTaskAwaiter<T>(_awaitable.GetAwaiter(), cancellationToken);
    }

    public readonly struct WithCancellationValueTaskAwaitable<T>(
        ValueTask<T> task,
        CancellationToken cancellationToken)
    {
        private readonly ConfiguredValueTaskAwaitable<T> _awaitable = task.ConfigureAwait(false);

        public WithCancellationValueTaskAwaiter<T> GetAwaiter() => new WithCancellationValueTaskAwaiter<T>(_awaitable.GetAwaiter(), cancellationToken);
    }

    public readonly struct WithCancellationTaskAwaiter(
        ConfiguredTaskAwaitable.ConfiguredTaskAwaiter awaiter,
        CancellationToken cancellationToken)
        : ICriticalNotifyCompletion
    {
        public bool IsCompleted => awaiter.IsCompleted || cancellationToken.IsCancellationRequested;

        public void OnCompleted(Action continuation) => awaiter.OnCompleted(WrapContinuation(continuation));

        public void UnsafeOnCompleted(Action continuation) => awaiter.UnsafeOnCompleted(WrapContinuation(continuation));

        public void GetResult()
        {
            Debug.Assert(IsCompleted);
            if (!awaiter.IsCompleted)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
            awaiter.GetResult();
        }

        private Action WrapContinuation(in Action originalContinuation)
            => cancellationToken.CanBeCanceled
                ? new WithCancellationContinuationWrapper(originalContinuation, cancellationToken).Continuation
                : originalContinuation;
    }

    public readonly struct WithCancellationTaskAwaiter<T>(
        ConfiguredTaskAwaitable<T>.ConfiguredTaskAwaiter awaiter,
        CancellationToken cancellationToken)
        : ICriticalNotifyCompletion
    {
        public bool IsCompleted => awaiter.IsCompleted || cancellationToken.IsCancellationRequested;

        public void OnCompleted(Action continuation) => awaiter.OnCompleted(WrapContinuation(continuation));

        public void UnsafeOnCompleted(Action continuation) => awaiter.UnsafeOnCompleted(WrapContinuation(continuation));

        public T GetResult()
        {
            Debug.Assert(IsCompleted);
            if (!awaiter.IsCompleted)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
            return awaiter.GetResult();
        }

        private Action WrapContinuation(in Action originalContinuation)
            => cancellationToken.CanBeCanceled
                ? new WithCancellationContinuationWrapper(originalContinuation, cancellationToken).Continuation
                : originalContinuation;
    }

    public readonly struct WithCancellationValueTaskAwaiter<T>(
        ConfiguredValueTaskAwaitable<T>.ConfiguredValueTaskAwaiter awaiter,
        CancellationToken cancellationToken)
        : ICriticalNotifyCompletion
    {
        public bool IsCompleted => awaiter.IsCompleted || cancellationToken.IsCancellationRequested;

        public void OnCompleted(Action continuation) => awaiter.OnCompleted(WrapContinuation(continuation));

        public void UnsafeOnCompleted(Action continuation) => awaiter.UnsafeOnCompleted(WrapContinuation(continuation));

        public T GetResult()
        {
            Debug.Assert(IsCompleted);
            if (!awaiter.IsCompleted)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
            return awaiter.GetResult();
        }

        private Action WrapContinuation(in Action originalContinuation)
            => cancellationToken.CanBeCanceled
                ? new WithCancellationContinuationWrapper(originalContinuation, cancellationToken).Continuation
                : originalContinuation;
    }

    private class WithCancellationContinuationWrapper
    {
        private Action _originalContinuation;
        private readonly CancellationTokenRegistration _registration;

        public WithCancellationContinuationWrapper(Action originalContinuation, CancellationToken cancellationToken)
        {
            var continuation = ContinuationImplementation;
            _originalContinuation = originalContinuation;
            _registration = cancellationToken.Register(continuation);
            Continuation = continuation;
        }

        public Action Continuation { get; }

        private void ContinuationImplementation()
        {
            var originalContinuation = Interlocked.Exchange(ref _originalContinuation, null);
            if (originalContinuation != null)
            {
                _registration.Dispose();
                originalContinuation();
            }
        }
    }
}