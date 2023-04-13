using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using User.Entities;
using User.Repositories;
using User.Services;

namespace User.Test.Integration;

public class UserRepositoryTest
{
    private readonly UserRepository _repository;
    private UserDbContext _context;
    private readonly UserEntity _userEntity = new UserEntity
    {
        Id = new Guid(),
        Ruian = 1234,
        BirthNumber = "881010/7890",
        FirstName = "Daniel",
        LastName = "Cipra",
        DateOfBirth = DateTimeOffset.Now.AddYears(-1)
    };

    public UserRepositoryTest()
    {
        _context = CreateContext();
        _repository = new UserRepository(_context);
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated(); // prostor na optimalizaci, mazání celé databáze je nákladné
    }

    private UserDbContext CreateContext()
    {
        var connectionString =
            "Data Source=localhost,5433;Initial Catalog=Test_Users;Integrated Security=False;User Id=sa;Password=Secret1234;MultipleActiveResultSets=True;TrustServerCertificate=True";

        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new UserDbContext(optionsBuilder.Options, new ClockService());
    }

    [Fact]
    public async Task AddUserAsync_AddsUser()
    {
        // Arrange

        // Act
        await _repository.Add(_userEntity);

        // Assert
        var secondaryContext = CreateContext();
        var userFromDb = await secondaryContext.Users.SingleAsync();
        userFromDb.Should().BeEquivalentTo(_userEntity,
            config => config.Excluding(e => e.CreatedAt).Excluding(e => e.ModifiedAt));
    }
    
    [Fact]
    public async Task GetUserAsync_GetsUser()
    {
        // Arrange
        _context.Users.Add(_userEntity);
        await _context.SaveChangesAsync();

        // Act
        var retrievedUser = await _repository.Get(_userEntity.Id);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser.Should().BeEquivalentTo(_userEntity, config => config.Excluding(e => e.CreatedAt).Excluding(e => e.ModifiedAt));
    }

    [Fact]
    public async Task UpdateUserAsync_UpdatesUser()
    {
        // Arrange
        _context.Users.Add(_userEntity);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        _userEntity.FirstName = "UpdatedFirstName";
        _userEntity.LastName = "UpdatedLastName";

        // Act
        await _repository.Update(_userEntity);

        // Assert
        var secondaryContext = CreateContext();
        var updatedUser = await secondaryContext.Users.FindAsync(_userEntity.Id);
        updatedUser.Should().BeEquivalentTo(_userEntity, config => config.Excluding(e => e.CreatedAt).Excluding(e => e.ModifiedAt));
    }

    [Fact]
    public async Task RemoveUserAsync_RemovesUser()
    {
        // Arrange
        _context.Users.Add(_userEntity);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        await _repository.Remove(_userEntity);

        // Assert
        var secondaryContext = CreateContext();
        var removedUser = await secondaryContext.Users.FindAsync(_userEntity.Id);
        removedUser.Should().BeNull();
    }
}