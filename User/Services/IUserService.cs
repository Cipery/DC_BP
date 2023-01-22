using User.Services.Models;

namespace User.Services;

public interface IUserService
{
    Task<Guid> CreateUser(CreateUserModel createUserModel);
    Task<GetUserModel?> GetUser(Guid userId);
    Task DeleteUser(Guid id);
    Task UpdateUser(UpdateUserModel updateUserModel);
    Task<int> GetUserAge(Guid id);
}