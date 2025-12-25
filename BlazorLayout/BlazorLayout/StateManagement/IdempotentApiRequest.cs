using BlazorLayout.Extensions;

namespace BlazorLayout.StateManagement;

/// <summary>
/// Wrapper for network requests that de-duplicates calls and remembers completion.
/// </summary>
/// <param name="run">
/// Performs the network request.
/// Should log errors.
/// </param>
/// <remarks>The <paramref name="run"/> implementation cannot return data. Instead, it should update stores as appropriate.</remarks>
public class IdempotentApiRequest(Func<CancellationToken, Task> run)
{
    private Task? task;

    /// <summary>
    /// Invokes <see cref="run"/> if it hasn't been invoked before or if <see cref="Reset"/> has been called since.
    /// </summary>
    /// <param name="cancellationToken">
    /// Provided by the caller.
    /// Forwarded to <see cref="run"/> if it is actually invoked. Otherwise, only cancels awaiting of the (elsewhere initiated) request.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask"/> which may complete synchronously (always success), or when <paramref name="cancellationToken"/> is cancelled, or with the (possibly error) result of <see cref="run"/>.
    /// </returns>
    /// <remarks>Calling this again after the request has completed is a no-op (=free).</remarks>
    public async ValueTask Run(CancellationToken cancellationToken)
    {
        if (HasCompleted) return;
        if (task is not null)
        {
            await task.WithCancellation(cancellationToken);
            return;
        }

        try
        {
            task = run(cancellationToken);
            await task;
            task = Task.CompletedTask;
        }
        catch
        {
            task = null;
            throw;
        }
    }

    public bool HasCompleted => task == Task.CompletedTask;

    /// <summary>
    /// Causes new calls to <see cref="Run"/> to actually do something again.
    /// </summary>
    /// <exception cref="InvalidOperationException">The stored request had not completed.</exception>
    public void Reset()
    {
        if (task is null) return;
        if (HasCompleted) task = null;
        // Possible race-condition. You have to first cancel the request.
        else throw new InvalidOperationException("Attempted to forget in-flight request.");
    }
}

