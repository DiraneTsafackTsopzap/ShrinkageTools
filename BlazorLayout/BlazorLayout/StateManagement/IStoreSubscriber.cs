using System.Security.Cryptography.X509Certificates;

namespace BlazorLayout.StateManagement
{
    public interface IStoreSubscriber
    {
        public void OnStoreStateChanged();
        public void OnStoreSubscribed(StoreBase store);
    }
}
