using User.Entities;

namespace User.Repositories;

public interface IUserRepository
{
    ValueTask<UserEntity?> Get(Guid id);
    Task Add(UserEntity user);
    Task Update(UserEntity user);
    Task Save();
    Task Remove(UserEntity user);
}