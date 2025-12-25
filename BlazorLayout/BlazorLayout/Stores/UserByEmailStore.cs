using BlazorLayout.Modeles;
using BlazorLayout.StateManagement;


namespace BlazorLayout.Stores;

    [AutoSubscribe]
    public sealed partial class UserByEmailStore : StoreBase
    {
        [AutoSubscribe]
        public partial UserDto? User { get; private set; }

        public bool IsInitialized => __User is not null;

        public void InitializeUser(UserDto user)
        {
            if (__User is not null) throw new InvalidOperationException("User was already been initialized.");
            User = user;
        }

        public void Reset()
        {
            User = null;
        }
    }

