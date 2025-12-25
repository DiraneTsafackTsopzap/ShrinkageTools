namespace BlazorLayout.Extensions;
public static class SynchronizationExtensions
{
    public static void CancelWhenTaskFails(this CancellationTokenSource cts, Task task)
    {
        _ = task.ContinueWith(_ => cts.Cancel(), cts.Token, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }

    /// <summary>
    /// Wraps <paramref name="task"/> in a new task that can be cancelled.
    /// </summary>
    /// <param name="task">The original task.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A new task that will cancel when <paramref name="cancellationToken"/> is cancelled.</returns>
    /// <remarks>This cannot cancel the original <paramref name="task"/>.</remarks>
    public static Task WithCancellation(this Task task, CancellationToken cancellationToken) =>
        task.ContinueWith(static t => t.GetAwaiter().GetResult(), cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);

    /// <summary>
    /// Wraps <paramref name="task"/> in a new task that can be cancelled.
    /// </summary>
    /// <param name="task">The original task.</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>A new task that will cancel when <paramref name="cancellationToken"/> is cancelled.</returns>
    /// <remarks>This cannot cancel the original <paramref name="task"/>.</remarks>
    public static Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken) =>
        task.ContinueWith(static t => t.GetAwaiter().GetResult(), cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
}

