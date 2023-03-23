using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using User.Clients;
using User.Repositories;
using User.Services.Models;
using User.Test.Component.Utils;
using User.Test.Unit;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace User.Test.Component;

[Collection(DatabaseWiremockCollectionFixture.CollectionName)]
public class UserTest
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly IszrApiServerFixture _iszrApiServer;

    private readonly CreateUserModel _createUserModel = new CreateUserModel
    {
        BirthNumber = "1410891234",
        FirstName = "Jan",
        LastName = "Novák",
        DateOfBirth = new DateTime(1989, 10, 14)
    };
    
    public UserTest(UserWebApplicationFactory factory, IszrApiServerFixture iszrApiServer,
        ITestOutputHelper testOutputHelper)
    {
        _iszrApiServer = iszrApiServer.GetResetInstance();
        _factory = factory.WithTestOutputHelper(testOutputHelper);
    }

    [Fact]
    public async Task PostUser_ShouldCreateUser()
    {
        // Arrange and Act
        var (response, _) = await CreateUserViaApi(12345);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    private async Task<(HttpResponseMessage response, HttpClient client)> CreateUserViaApi(int ruian)
    {
        var ruianResponseModel = new GetRuianByBirthNumberResponseModel { Ruian = ruian };
        var request = Request
            .Create()
            .WithPath($"/ruian/{_createUserModel.BirthNumber}")
            .UsingGet();

        _iszrApiServer.Server.Given(request)
            .RespondWith(Response.Create()
                .WithStatusCode(StatusCodes.Status200OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(ruianResponseModel)));

        var client = _factory.WithWebHostBuilder(conf =>
        {
            conf.ConfigureTestServices(services =>
            {
                services.Configure<IszrClientConfiguration>(opts => { opts.ApiUrl = _iszrApiServer.Server.Url; });
            });
        }).CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/user", _createUserModel);
        return (response, client);
    }

    [Fact]
    public async Task PostUser_ShouldReturnBadRequest_WhenInputModelIsEmpty()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(conf =>
        {
            conf.ConfigureServices(services =>
            {
            });
        }).CreateClient();

        // Act
        var response = await client.PostAsJsonAsync( "/user", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [InlineData(StatusCodes.Status500InternalServerError)]
    [InlineData(StatusCodes.Status501NotImplemented)]
    [InlineData(StatusCodes.Status400BadRequest)]
    public async Task PostUser_ShouldReturnServerError_WhenIszrReturnsError(int statusCodeFromIszr)
    {
        var request = Request
            .Create()
            .WithPath($"/ruian/{_createUserModel.BirthNumber}")
            .UsingGet();

        _iszrApiServer.Server.Given(request)
            .RespondWith(Response.Create()
                .WithStatusCode(statusCodeFromIszr));

        var client = _factory.WithWebHostBuilder(conf =>
        {
            conf.ConfigureTestServices(services =>
            {
                services.Configure<IszrClientConfiguration>(opts => { opts.ApiUrl = _iszrApiServer.Server.Url; });
            });
        }).CreateClient();

        // Act
        var response = await client.PostAsJsonAsync( "/user", _createUserModel);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
    
    [Fact]
    public async Task PostUser_ShouldReturnNotFound_WhenIszrReturnsNotFound()
    {
        var request = Request
            .Create()
            .WithPath($"/ruian/{_createUserModel.BirthNumber}")
            .UsingGet();

        _iszrApiServer.Server.Given(request)
            .RespondWith(Response.Create()
                .WithStatusCode(StatusCodes.Status404NotFound));

        var client = _factory.WithWebHostBuilder(conf =>
        {
            conf.ConfigureTestServices(services =>
            {
                services.Configure<IszrClientConfiguration>(opts => { opts.ApiUrl = _iszrApiServer.Server.Url; });
            });
        }).CreateClient();

        // Act
        var response = await client.PostAsJsonAsync( "/user", _createUserModel);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task PostUser_ShouldReturnServerError_WhenIszrReturnsEmptyBody()
    {
        var request = Request
            .Create()
            .WithPath($"/ruian/{_createUserModel.BirthNumber}")
            .UsingGet();

        _iszrApiServer.Server.Given(request)
            .RespondWith(Response.Create()
                .WithStatusCode(StatusCodes.Status200OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(string.Empty));

        var client = _factory.WithWebHostBuilder(conf =>
        {
            conf.ConfigureTestServices(services =>
            {
                services.Configure<IszrClientConfiguration>(opts => { opts.ApiUrl = _iszrApiServer.Server.Url; });
            });
        }).CreateClient();

        // Act
        var response = await client.PostAsJsonAsync( "/user", _createUserModel);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
    
    [Fact]
    public async Task PostUser_ShouldReturnNotFound_WhenIszrReturnsEmptyRuian()
    {
        var request = Request
            .Create()
            .WithPath($"/ruian/{_createUserModel.BirthNumber}")
            .UsingGet();

        _iszrApiServer.Server.Given(request)
            .RespondWith(Response.Create()
                .WithStatusCode(StatusCodes.Status200OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new GetRuianByBirthNumberResponseModel()
                {
                    Ruian = null
                })));

        var client = _factory.WithWebHostBuilder(conf =>
        {
            conf.ConfigureTestServices(services =>
            {
                services.Configure<IszrClientConfiguration>(opts => { opts.ApiUrl = _iszrApiServer.Server.Url; });
            });
        }).CreateClient();

        // Act
        var response = await client.PostAsJsonAsync( "/user", _createUserModel);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetUser_ShouldGetUser_WhenUserExists()
    {
        // Arrange
        var (createUserResponse, client) = await CreateUserViaApi(12345);

        // Act
        var response = await client.GetAsync(createUserResponse.Headers.Location);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var getUserModel = await response.Content.ReadFromJsonAsync<GetUserModel>();
        getUserModel.Should().BeEquivalentTo(_createUserModel, config => config.ExcludingMissingMembers());
    }
    
    [Fact]
    public async Task GetUser_ShouldReturn404_WhenUserDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"User/{Guid.NewGuid()}");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    
    [Fact]
    public async Task DeleteUser_ShouldDeleteUser_WhenUserIsFound()
    {
        // Arrange
        var (createUserResponse, client) = await CreateUserViaApi(12345);

        // Act
        var response = await client.DeleteAsync(createUserResponse.Headers.Location);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var getUserResponse = await client.GetAsync(createUserResponse.Headers.Location);
        getUserResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task DeleteUser_ShouldReturn404_WhenUserDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"User/{Guid.NewGuid()}");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task PutUser_ShouldUpdateUser()
    {
        // Arrange
        var (createUserResponse, client) = await CreateUserViaApi(12345);
        var userId = createUserResponse.Headers.Location.ToString().Split("/").Last();
        var updateUserModel = new UpdateUserModel
        {
            Id = Guid.Parse(userId),
            FirstName = "Isaac",
            LastName = "Asimov",
        };
        
        // Act
        var response = await client.PutAsJsonAsync($"user/{userId}", updateUserModel);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var getUserResponse = await client.GetAsync(createUserResponse.Headers.Location);
        var getUserModel = await getUserResponse.Content.ReadFromJsonAsync<GetUserModel>();
        getUserModel.Should().BeEquivalentTo(updateUserModel, config => config.ExcludingMissingMembers());
    }
    
    [Fact]
    public async Task PutUser_ShouldReturnBadRequest_WhenNothingToUpdateIsProvidedInInputModel()
    {
        // Arrange
        var (createUserResponse, client) = await CreateUserViaApi(12345);
        var userId = createUserResponse.Headers.Location.ToString().Split("/").Last();
        var updateUserModel = new UpdateUserModel
        {
            Id = Guid.Parse(userId),
        };
        
        // Act
        var response = await client.PutAsJsonAsync($"user/{userId}", updateUserModel);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task PutUser_ShouldReturnBadRequest_WhenIdInPathDoesNotMatchIdInBody()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var updateUserModel = new UpdateUserModel
        {
            Id = Guid.NewGuid(),
            FirstName = "Isaac",
            LastName = "Asimov",
        };
        
        // Act
        var response = await client.PutAsJsonAsync($"user/{userId}", updateUserModel);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task PutUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var updateUserModel = new UpdateUserModel
        {
            Id = userId,
            FirstName = "Isaac",
            LastName = "Asimov",
        };
        
        // Act
        var response = await client.PutAsJsonAsync($"user/{userId}", updateUserModel);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task GetAge_ShouldReturnCalculatedAge_WhenUserExists()
    {
        // Arrange
        var (createUserResponse, client) = await CreateUserViaApi(12345);
        var userId = createUserResponse.Headers.Location!.ToString().Split("/").Last();
        
        // Act
        var response = await client.GetAsync($"user/{userId}/age");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var responseModel = await response.Content.ReadFromJsonAsync<GetUserAgeModel>();
        responseModel.Should().NotBeNull();
        responseModel!.Age.Should().BePositive();
    }
    
    [Fact]
    public async Task GetAge_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var (createUserResponse, client) = await CreateUserViaApi(12345);
        
        // Act
        var response = await client.GetAsync($"user/{Guid.NewGuid()}/age");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}