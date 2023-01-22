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

    public UserServiceTest()
    {
        IszrClientMock = new Mock<IIszrClient>();
        UserRepositorMock = new Mock<IUserRepository>();
        ClockServicemock = new Mock<IClockService>();
        Sut = new UserService(UserRepositorMock.Object, IszrClientMock.Object, ClockServicemock.Object);
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
        IszrClientMock.Setup(client => client.GetRuianByBirthNumber(It.IsAny<string>()))
            .ReturnsAsync(ruian);

        UserRepositorMock.Setup(repository => repository.Add(It.IsAny<UserEntity>()));

        // Act
        var result = await Sut.CreateUser(requestModel);

        // Assert
        result.Should().NotBeEmpty();
        IszrClientMock.Verify();
        UserRepositorMock.Verify();
        ClockServicemock.Verify();
    }
    
    [Fact]
    public async Task CreateUser_ShouldThrow_WhenRuainIsNotFound()
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
            .ReturnsAsync((int?) null);
        Func<Task<Guid>> result = () => Sut.CreateUser(requestModel);
        
        // Act and Assert
        await result.Should().ThrowAsync<RuianNotFoundException>();
        IszrClientMock.Verify();
        UserRepositorMock.Verify();
        ClockServicemock.Verify();
    } 
}