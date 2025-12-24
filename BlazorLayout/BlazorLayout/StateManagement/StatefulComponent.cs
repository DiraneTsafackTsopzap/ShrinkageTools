using Microsoft.AspNetCore.Components;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;


namespace BlazorLayout.StateManagement
{
    public abstract class StatefulComponent<TState> : AppComponentBase, IStoreSubscriber
    where TState : class, IEquatable<TState>
    {
        private StoreBase[] subscribedStores = [];

#nullable disable
        [DisallowNull]
        protected TState State { get; private set; } = null!;
#nullable restore

        /// <summary>
        /// Overriding implementations should pull data from <see cref="StoreBase"/>-derived objects.
        /// This will auto-subscribe to accessed stores, enabling automatic re-rendering of this component when data changes.
        /// </summary>
        /// <returns>The new state of the component.</returns>
        /// <remarks>
        /// The returned state object should be treated as immutable and never be modified.
        /// Changes to data have to be performed on the stores instead.
        /// This guarantees integrity across all components.
        /// </remarks>
        protected abstract TState BuildState();

        void IStoreSubscriber.OnStoreStateChanged() => BuildStateFromStores();

        void IStoreSubscriber.OnStoreSubscribed(StoreBase store)
        {
#if DEBUG
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var subscribedStore in subscribedStores)
            {
                if (ReferenceEquals(subscribedStore, store))
                    throw new InvalidOperationException();
            }
#endif
            var oldLen = subscribedStores.Length;
            Array.Resize(ref subscribedStores, oldLen + 1);
            subscribedStores[oldLen] = store;
        }

        private void BuildStateFromStores()
        {
#if DEBUG_BLAZOR_LIFECYCLE
        LifecycleDebugPrint(nameof(BuildStateFromStores));
#endif
            StoreBase.BeginSubscribing(this);
            var needsFlush = false;
            try
            {
                if (BuildState() is not { } newState) throw new InvalidOperationException(nameof(BuildState) + " returned null.");
                if (!newState.Equals(State))
                {
                    State = newState;
                    needsFlush = true;
                }
#if DEBUG
                if (subscribedStores is []) throw new InvalidOperationException(GetType().Name + " has not subscribed to any stores.");
#endif
            }
            finally
            {
                StoreBase.EndSubscribing(this);
            }

            if (needsFlush) StateHasChanged();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var i = subscribedStores.Length;
                while (i-- > 0) subscribedStores[i].Unsubscribe(this);
            }

            base.Dispose(disposing);
        }

        public override Task SetParametersAsync(ParameterView parameters)
        {
#if DEBUG_BLAZOR_LIFECYCLE
        LifecycleDebugPrint(nameof(SetParametersAsync));
#endif
            parameters.SetParameterProperties(this);
            BuildStateFromStores();
            if (initialized) return CallOnParametersSetAsync();
            initialized = true;
            return RunInitAndSetParametersAsync();
        }

        #region Copied from Blazor's ComponentBase

        // ReSharper disable MethodHasAsyncOverload
#pragma warning disable VSTHRD103
#pragma warning disable VSTHRD003

        private bool initialized;

        [DebuggerNonUserCode]
        private async Task RunInitAndSetParametersAsync()
        {
            OnInitialized();
            var task = OnInitializedAsync();

            if (task.Status is not (TaskStatus.RanToCompletion or TaskStatus.Canceled))
            {
                StateHasChanged();

                try
                {
                    await task;
                }
                catch
                {
                    if (!task.IsCanceled)
                    {
                        throw;
                    }
                }
            }

            await CallOnParametersSetAsync();
        }

        [DebuggerNonUserCode]
        private Task CallOnParametersSetAsync()
        {
            OnParametersSet();
            var task = OnParametersSetAsync();
            var shouldAwaitTask = task.Status is not (TaskStatus.RanToCompletion or TaskStatus.Canceled);
            StateHasChanged();
            return shouldAwaitTask ? CallStateHasChangedOnAsyncCompletion(task) : Task.CompletedTask;
        }

        [DebuggerNonUserCode]
        private async Task CallStateHasChangedOnAsyncCompletion(Task task)
        {
            try
            {
                await task;
            }
            catch
            {
                if (task.IsCanceled) return;
                throw;
            }

            StateHasChanged();
        }

        #endregion
    }
}

#if DEBUG
// #define DEBUG_BLAZOR_LIFECYCLE
#endif



