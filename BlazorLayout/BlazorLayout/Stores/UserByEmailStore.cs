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
            if (IsInitialized) throw new InvalidOperationException("User was already been initialized.");

            User = user;
        }

        public void Reset()
        {
            User = null;
        }
    }


// 1- is not null : veut dire que l'objet n'est pas null , c'est a dire que l'objet existe 
// 2- IsInitialized : est une propriete booleenne qui indique si l'utilisateur a ete initialise ou non
// 