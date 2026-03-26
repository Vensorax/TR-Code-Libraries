// MIT License
// Copyright (c) 2026 Team Radiance

using System;
using System.Threading;
using System.Threading.Tasks;

namespace TeamRadiance;

public static class Coroutine
{
    /// <summary>
    /// Waits for a specified duration using System Time (Real-time).
    /// </summary>
    public static async ValueTask WaitSeconds(double seconds)
    {
        if (seconds <= 0) return;
        await Task.Delay(TimeSpan.FromSeconds(seconds));
    }

    /// <summary>
    /// Waits until the provided condition evaluates to true.
    /// Evaluates efficiently without exhausting the thread pool.
    /// </summary>
    /// <param name="conditionCheck">The function to evaluate.</param>
    /// <param name="pollRateMs">How often to check the condition (Default is 16ms, roughly 60 FPS).</param>
    public static async ValueTask WaitUntil(Func<bool> conditionCheck, int pollRateMs = 16)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(pollRateMs));
        
        while (!conditionCheck.Invoke())
        {
            await timer.WaitForNextTickAsync();
        }
    }

    /// <summary>
    /// Waits while the provided condition evaluates to true.
    /// Evaluates efficiently without exhausting the thread pool.
    /// </summary>
    /// <param name="conditionCheck">The function to evaluate.</param>
    /// <param name="pollRateMs">How often to check the condition (Default is 16ms, roughly 60 FPS).</param>
    public static async ValueTask WaitWhile(Func<bool> conditionCheck, int pollRateMs = 16)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(pollRateMs));
        
        while (conditionCheck.Invoke())
        {
            await timer.WaitForNextTickAsync();
        }
    }

    /// <summary>
    /// Waits for a standard C# event (Action) to fire once.
    /// </summary>
    public static async ValueTask WaitEvent(Action<Action> subscribe, Action<Action> unsubscribe)
    {
        var tcs = new TaskCompletionSource<bool>();

        void handler() => tcs.TrySetResult(true);

        subscribe(handler);

        try
        {
            await tcs.Task;
        }
        finally
        {
            unsubscribe(handler);
        }
    }

    /// <summary>
    /// Waits for a generic event (Action<T>).
    /// </summary>
    public static async ValueTask<T> WaitEvent<T>(Action<Action<T>> subscribe, Action<Action<T>> unsubscribe)
    {
        var tcs = new TaskCompletionSource<T>();

        void handler(T val) => tcs.TrySetResult(val);

        subscribe(handler);

        try
        {
            return await tcs.Task;
        }
        finally
        {
            unsubscribe(handler);
        }
    }
}