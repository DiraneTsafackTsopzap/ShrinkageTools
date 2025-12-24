namespace BlazorLayout.StateManagement
{
#if DEBUG
    // #define DEBUG_BLAZOR_LIFECYCLE
#endif


    public abstract class AppComponentBase :
#if DEBUG_BLAZOR_LIFECYCLE
    DebugLifecycleComponentBase, IDisposable
#else
        Microsoft.AspNetCore.Components.ComponentBase, IDisposable
#endif
    {
        private CancellationTokenSource? cts;

        /// <summary>
        /// Use to know why the <see cref="TimeoutToken"/> was cancelled.
        /// </summary>
        protected bool IsDisposing { get; private set; }

        protected CancellationToken TimeoutToken(int milliSeconds)
        {
            if (IsDisposing) throw new ObjectDisposedException(GetType().Name);

            cts ??= new();
            return milliSeconds switch
            {
                0 => new(canceled: true),
                Timeout.Infinite => cts.Token,
                > 0 => CancellationTokenSource.CreateLinkedTokenSource(cts.Token, new CancellationTokenSource(milliSeconds).Token).Token,
                < 0 => throw new ArgumentOutOfRangeException(nameof(milliSeconds), milliSeconds, @"must be positive"),
            };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && cts is not null)
            {
                cts.Cancel();
                cts.Dispose();
            }
        }

        void IDisposable.Dispose()
        {
#if DEBUG_BLAZOR_LIFECYCLE
        LifecycleDebugPrint(nameof(Dispose));
#endif
            IsDisposing = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

}
