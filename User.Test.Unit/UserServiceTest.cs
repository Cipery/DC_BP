using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using User.Clients;
using User.Entities;
using User.Exceptions;
using User.Repositories;
using User.Services;
using User.Services.Models;
using Xunit;

namespace User.Test.Unit;

public class UserServiceTest
{
    private readonly Mock<IIszrClient> IszrClientMock;
    private readonly Mock<IUserRepository> UserRepositorMock;
    private readonly Mock<IClockService> ClockServicemock;
    private readonly UserService Sut;
    private readonly UserEntity UserEntity_1;

    public UserServiceTest()
    {
        IszrClientMock = new Mock<IIszrClient>(MockBehavior.Strict);
        UserRepositorMock = new Mock<IUserRepository>(MockBehavior.Strict);
        ClockServicemock = new Mock<IClockService>(MockBehavior.Strict);
        Sut = new UserService(UserRepositorMock.Object, IszrClientMock.Object, ClockServicemock.Object);
        UserEntity_1 = new UserEntity
        {
            BirthNumber = "8910141234",
            DateOfBirth = DateTime.Parse("1989-10-14"),
            FirstName = "Daniel",
            LastName = "Gurblanski",
            Id = Guid.NewGuid(),
            Ruian = 223300
        };
    }
    
    [Fact]
    public async Task CreateUser_ShouldCreateNewUser()
    {
        // Arrange
        var ruian = 12345;
        var requestModel = new CreateUserModel
        {
            BirthNumber = "8910141234",
            DateOfBirth = DateTime.Parse("1989-10-14"),
            FirstName = "Daniel",
            LastName = "Gurblanski"
        };
        
        IszrClientMock
            .Setup(client => client.GetRuianByBirthNumber("8910141234"))
            .ReturnsAsync(112233);
        
        UserRepositorMock.Setup(repository => repository.Add(It.IsAny<UserEntity>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await Sut.CreateUser(requestModel);

        // Assert
        result.Should().NotBeEmpty();
        IszrClientMock.VerifyAll();
        UserRepositorMock.VerifyAll();
        ClockServicemock.VerifyNoOtherCalls();
    }
    
    [Fact]
    public async Task CreateUser_ShouldThrow_WhenIszrClientThrows()
    {
        // Arrange
        var ruian = 12345;
        var requestModel = new CreateUserModel
        {
            BirthNumber = "8910141234",
            DateOfBirth = DateTime.Parse("1989-10-14"),
            FirstName = "Daniel",
            LastName = "Gurblanski"
        };
        IszrClientMock.Setup(client => client.GetRuianByBirthNumber(requestModel.BirthNumber))
            .ThrowsAsync(new RuianNotFoundException());
        Func<Task<Guid>> result = () => Sut.CreateUser(requestModel);
        
        // Act and Assert
        await result.Should().ThrowAsync<RuianNotFoundException>();
        IszrClientMock.VerifyAll();
        UserRepositorMock.VerifyAll();
        ClockServicemock.VerifyAll();
    }

    [Fact]
    public async Task GetUser_ShouldReturnUser()
    {
        // Arrange
        var userId = UserEntity_1.Id;

        UserRepositorMock.Setup(repository => repository.Get(userId)).ReturnsAsync(UserEntity_1);

        // Act
        var getUserResponse = await Sut.GetUser(userId);

        // Assert
        getUserResponse.Should().NotBeNull();
        getUserResponse.Id.Should().Be(userId);
        getUserResponse.FirstName.Should().Be(UserEntity_1.FirstName);
        getUserResponse.LastName.Should().Be(UserEntity_1.LastName);
        getUserResponse.Ruian.Should().Be(UserEntity_1.Ruian);
        getUserResponse.DateOfBirth.Should().Be(UserEntity_1.DateOfBirth.DateTime);
        UserRepositorMock.VerifyAll();
    }
    
    [Fact]
    public async Task GetUser_ShouldThrow_WhenUserIsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        UserRepositorMock.Setup(repository => repository.Get(userId)).ReturnsAsync((UserEntity?)null);
        Func<Task<GetUserModel?>> result = () => Sut.GetUser(userId);
        // Act and Assert
        await result.Should().ThrowAsync<EntityNotFoundException>();
        UserRepositorMock.VerifyAll();
    }

    [Fact]
    public async Task DeleteUser_ShouldDeleteUser()
    {
        // Arrange
        UserRepositorMock.Setup(repository => repository.Get(UserEntity_1.Id)).ReturnsAsync(UserEntity_1);
        UserRepositorMock.Setup(repository => repository.Remove(UserEntity_1)).Returns(Task.CompletedTask);

        // Act  
        await Sut.DeleteUser(UserEntity_1.Id);

        // Assert
        UserRepositorMock.VerifyAll();
    }
    
    [Fact]
    public async Task DeleteUser_ShouldThrow_WhenUserIsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        UserRepositorMock.Setup(repository => repository.Get(userId)).ReturnsAsync((UserEntity?)null);

        Func<Task> result = () => Sut.DeleteUser(userId);

        // Act and Assert
        await result.Should().ThrowAsync<EntityNotFoundException>();
        UserRepositorMock.VerifyAll();
    }

    [Fact]
    public async Task UpdateUser_ShouldUpdateFirstName_WhenOnlyFirstNameIsProvided()
    {
        // Arrange
        var newFirstName = "Tomáš";

        UserRepositorMock.Setup(repository => repository.Get(UserEntity_1.Id)).ReturnsAsync(UserEntity_1);
        UserRepositorMock.Setup(repository => repository.Update(It.Is<UserEntity>(u =>
                u.FirstName == newFirstName && u.LastName == UserEntity_1.LastName && u.Id == UserEntity_1.Id &&
                u.DateOfBirth == UserEntity_1.DateOfBirth && u.BirthNumber == UserEntity_1.BirthNumber)))
            .Returns(Task.CompletedTask);

        // Act
        await Sut.UpdateUser(new UpdateUserModel { Id = UserEntity_1.Id, FirstName = newFirstName });

        // Assert
        UserRepositorMock.VerifyAll();
    }
    
    [Fact]
    public async Task UpdateUser_ShouldUpdateLastName_WhenOnlyLastNameIsProvided()
    {
        // Arrange
        var newLastName = "Jeremenko";

        UserRepositorMock.Setup(repository => repository.Get(UserEntity_1.Id)).ReturnsAsync(UserEntity_1);
        UserRepositorMock.Setup(repository => repository.Update(It.Is<UserEntity>(u =>
                u.FirstName == UserEntity_1.FirstName && u.LastName == newLastName && u.Id == UserEntity_1.Id &&
                u.DateOfBirth == UserEntity_1.DateOfBirth && u.BirthNumber == UserEntity_1.BirthNumber)))
            .Returns(Task.CompletedTask);

        // Act
        await Sut.UpdateUser(new UpdateUserModel { Id = UserEntity_1.Id, LastName = newLastName });

        // Assert
        UserRepositorMock.VerifyAll();
    }
    
    [Fact]
    public async Task UpdateUser_ShouldThrow_WhenNothingToUpdateIsProvided()
    {
        // Arrange
        // Act and Assert
        Func<Task> result = () => Sut.UpdateUser(new UpdateUserModel { Id = UserEntity_1.Id });
        await result.Should().ThrowAsync<ApiException>();
        UserRepositorMock.VerifyAll();
    }
    
    [Fact]
    public async Task UpdateUser_ShouldThrow_WhenUserIsNotFound()
    {
        // Arrange
        UserRepositorMock.Setup(repository => repository.Get(UserEntity_1.Id)).ReturnsAsync((UserEntity?)null);
        // Act and Assert
        Func<Task> result = () => Sut.UpdateUser(new UpdateUserModel { Id = UserEntity_1.Id, FirstName = "Adam"});
        await result.Should().ThrowAsync<EntityNotFoundException>();
        UserRepositorMock.VerifyAll();
    }

    [Theory]
    [InlineData(1, 1, 2000, 23)]
    [InlineData(2, 15, 2000, 22)]
    [InlineData(12, 31, 1999, 23)]
    [InlineData(1, 1, 1998, 25)]
    [InlineData(2, 29, 1996, 26)]
    [InlineData(12, 31, 1994, 28)]
    [InlineData(1, 1, 1990, 33)]
    [InlineData(1, 1, 2010, 13)]
    [InlineData(5, 15, 2011, 11)]
    [InlineData(7, 4, 2012, 10)]
    [InlineData(12, 25, 2013, 9)]
    [InlineData(9, 30, 2014, 8)]
    public async Task GetUserAge_VariousDates_ReturnsCorrectAge(int month, int day, int year, int expectedAge)
    {
        // Arrange
        var dateOfBirth = new DateTime(year, month, day);
        var setDateTime = new DateTime(2023, 2, 12);
        UserEntity_1.DateOfBirth = dateOfBirth;
        UserRepositorMock.Setup(repository => repository.Get(UserEntity_1.Id)).ReturnsAsync(UserEntity_1);
        ClockServicemock.Setup(service => service.NowUtc()).Returns(setDateTime);

        // Act
        var calculatedAge = await Sut.GetUserAge(UserEntity_1.Id);

        // Assert
        calculatedAge.Should().Be(expectedAge);
        UserRepositorMock.VerifyAll();
        ClockServicemock.VerifyAll();
    }

    [Fact]
    public async Task GetUserAge_ShouldThrow_WhenUserIsNotFound()
    {
        // Arrange
        UserRepositorMock.Setup(repository => repository.Get(UserEntity_1.Id)).ReturnsAsync((UserEntity?)null);
        // Act and Assert
        Func<Task<int>> result = () => Sut.GetUserAge(UserEntity_1.Id);
        await result.Should().ThrowAsync<EntityNotFoundException>();
        UserRepositorMock.VerifyAll();
    }
}