using DataAccess.CRUD.Modeles;

namespace DataAccess.CRUD.Repositories
{
    public interface IShrinkageUserRepository
    {

        Task<ShrinkageUserDataModel?> GetUserByEmail(string email, CancellationToken token);

        Task<ShrinkageUserDataModel> Create(ShrinkageUserDataModel user, CancellationToken token);
       


    }
}
