using Microsoft.EntityFrameworkCore;
using User.Entities;

namespace User.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _dbContext;

    public UserRepository(UserDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public ValueTask<UserEntity?> Get(Guid id)
    {
        return _dbContext.Users.FindAsync(id);
    }

    public Task Add(UserEntity user)
    {
        _dbContext.Users.Add(user);
        return Save();
    }

    public async Task Remove(UserEntity user)
    {
        _dbContext.Users.Remove(user);
        await Save();
    }

    public Task Update(UserEntity user)
    {
        _dbContext.Entry(user).State = EntityState.Modified;
        return Save();
    }

    public Task Save()
    {
        return _dbContext.SaveChangesAsync();
    }
}