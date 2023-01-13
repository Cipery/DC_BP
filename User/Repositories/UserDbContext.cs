using Microsoft.EntityFrameworkCore;
using User.Entities;

namespace User.Repositories;

public class UserDbContext : DbContext
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    
    public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(options) { }
}