using User.Entities;

namespace User.Repositories;

public interface IUserRepository
{
    ValueTask<UserEntity?> Get(Guid id);
    Task Add(UserEntity user);
    Task Remove(Guid id);
    Task Update(UserEntity user);
    Task Save();
    Task Remove(UserEntity user);
}