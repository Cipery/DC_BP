using Microsoft.EntityFrameworkCore;
using User.Entities;
using User.Services;

namespace User.Repositories;

public class UserDbContext : DbContext
{
	private readonly IClockService _clockService;

	public DbSet<UserEntity> Users => Set<UserEntity>();

	public UserDbContext(DbContextOptions<UserDbContext> options, IClockService clockService)
		: base(options)
	{
		_clockService = clockService ?? throw new ArgumentNullException(nameof(clockService));
	}

	public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
	{
		ChangeTracker.DetectChanges();

		foreach (var entry in ChangeTracker.Entries<BaseEntity>())
		{
			switch (entry.State)
			{
				case EntityState.Added:
					entry.Entity.CreatedAt = _clockService.NowUtc();
					break;
				case EntityState.Modified:
					entry.Entity.ModifiedAt = _clockService.NowUtc();
					break;
			}
		}

		return await base.SaveChangesAsync(cancellationToken);
	}
}