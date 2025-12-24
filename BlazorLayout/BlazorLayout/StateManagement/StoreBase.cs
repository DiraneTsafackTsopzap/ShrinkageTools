namespace BlazorLayout.StateManagement
{
    using System.ComponentModel;


    public abstract class StoreBase
    {
        private readonly HashSet<IStoreSubscriber> subscribers = [];

        protected void SubscribeCaller()
        {
            if (currentSubscriber is null) throw new InvalidOperationException($"The subscriber has not called {nameof(BeginSubscribing)}.");
            if (subscribers.Add(currentSubscriber))
                currentSubscriber.OnStoreSubscribed(this);
        }

        protected void NotifySubscribers()
        {
            foreach (var subscriber in subscribers.ToArray())
                subscriber.OnStoreStateChanged();
        }

        private static IStoreSubscriber? currentSubscriber;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void BeginSubscribing(IStoreSubscriber subscriber)
        {
#if DEBUG
            ArgumentNullException.ThrowIfNull(subscriber);
            if (currentSubscriber is not null) throw new InvalidOperationException("race condition");
#endif
            currentSubscriber = subscriber;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void EndSubscribing(IStoreSubscriber subscriber)
        {
#if DEBUG
            ArgumentNullException.ThrowIfNull(subscriber);
            if (!ReferenceEquals(currentSubscriber, subscriber)) throw new InvalidOperationException("race condition");
#endif
            currentSubscriber = null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void Unsubscribe(IStoreSubscriber subscriber)
        {
#if DEBUG
            ArgumentNullException.ThrowIfNull(subscriber);
            if (currentSubscriber is not null) throw new InvalidOperationException("race condition");
#endif
            subscribers.Remove(subscriber);
        }
    }

}
