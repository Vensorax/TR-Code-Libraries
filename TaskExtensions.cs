using System;
using System.Threading;
using System.Threading.Tasks;

namespace TeamRadiance;

/// <summary>
/// 
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Awaiting a task with a watchdog timer that will kill the task if it exceeds the specified timeout.
    /// </summary>
    /// <param name="valueTask"></param>
    /// <param name="timeoutSeconds"></param>
    /// <param name="taskName"></param>
    /// <param name="throwOnTimeout"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async ValueTask WithWatchdog(this ValueTask valueTask, float timeoutSeconds = 4.0f, string taskName = "Unknown", bool throwOnTimeout = true, CancellationToken token = default)
    {
        try
        {
            await valueTask.AsTask().WaitAsync(TimeSpan.FromSeconds(timeoutSeconds), token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[Task] 🧊 '{taskName}' was cancelled.");
        }
        catch (TimeoutException)
        {
            Console.WriteLine($"[Watchdog] ⏱️ FATAL TIMEOUT: '{taskName}' hung for {timeoutSeconds}s and was aborted!");
            if (throwOnTimeout) throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Watchdog] 💥 TASK CRASHED: '{taskName}' threw {ex.GetType().Name} -> {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Awaiting a task with a watchdog timer that will kill the task if it exceeds the specified timeout.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="timeoutSeconds"></param>
    /// <param name="taskName"></param>
    /// <param name="throwOnTimeout"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task WithWatchdog(this Task task, float timeoutSeconds = 4.0f, string taskName = "Unknown", bool throwOnTimeout = true, CancellationToken token = default)
    {
        try
        {
            await task.WaitAsync(TimeSpan.FromSeconds(timeoutSeconds), token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[Task] 🧊 '{taskName}' was cancelled.");
        }
        catch (TimeoutException)
        {
            Console.WriteLine($"[Watchdog] ⏱️ FATAL TIMEOUT: '{taskName}' hung for {timeoutSeconds}s and was aborted!");
            if (throwOnTimeout) throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Watchdog] 💥 TASK CRASHED: '{taskName}' threw {ex.GetType().Name} -> {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Awaiting a task with a watchdog timer that will kill the task if it exceeds the specified timeout.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="valueTask"></param>
    /// <param name="timeoutSeconds"></param>
    /// <param name="taskName"></param>
    /// <param name="throwOnTimeout"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async ValueTask<T> WithWatchdog<T>(this ValueTask<T> valueTask, float timeoutSeconds = 4.0f, string taskName = "Unknown", bool throwOnTimeout = false, CancellationToken token = default)
    {
        try
        {
            return await valueTask.AsTask().WaitAsync(TimeSpan.FromSeconds(timeoutSeconds), token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[Task] 🧊 '{taskName}' was cancelled.");
            return default;
        }
        catch (TimeoutException)
        {
            Console.WriteLine($"[Watchdog] ⏱️ FATAL TIMEOUT: '{taskName}' hung for {timeoutSeconds}s and was aborted!");
            if (throwOnTimeout) throw;
            return default;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Watchdog] 💥 TASK CRASHED: '{taskName}' threw {ex.GetType().Name} -> {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Awaiting a task with a watchdog timer that will kill the task if it exceeds the specified timeout.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="task"></param>
    /// <param name="timeoutSeconds"></param>
    /// <param name="taskName"></param>
    /// <param name="throwOnTimeout"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<T> WithWatchdog<T>(this Task<T> task, float timeoutSeconds = 4.0f, string taskName = "Unknown", bool throwOnTimeout = false, CancellationToken token = default)
    {
        try
        {
            return await task.WaitAsync(TimeSpan.FromSeconds(timeoutSeconds), token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[Task] 🧊 '{taskName}' was cancelled.");
            return default;
        }
        catch (TimeoutException)
        {
            Console.WriteLine($"[Watchdog] ⏱️ FATAL TIMEOUT: '{taskName}' hung for {timeoutSeconds}s and was aborted!");
            if (throwOnTimeout) throw;
            return default;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Watchdog] 💥 TASK CRASHED: '{taskName}' threw {ex.GetType().Name} -> {ex.Message}");
            throw;
        }
    }
}