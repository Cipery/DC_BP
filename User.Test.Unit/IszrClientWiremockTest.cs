using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using User.Clients;
using User.Exceptions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;

namespace User.Test.Unit;

public class IszrClientWiremockTest : IClassFixture<IszrApiServerFixture>
{
    private readonly IszrApiServerFixture _iszrApiServer;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;

    public IszrClientWiremockTest(IszrApiServerFixture iszrApiServer)
    {
        _iszrApiServer = iszrApiServer.GetResetInstance();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(new HttpClient());
    }

    [Fact]
    public async Task GetRuianByBirthNumber_ReturnsExpectedRuian_WhenIszrResponseIsOk()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<IszrClient>>();
        var birthNUmber = "141019891234";
        var ruianResponseModel = new GetRuianByBirthNumberResponseModel
        {
            Ruian = 12345
        };
        var request = Request
            .Create()
            .WithPath($"/ruian/{birthNUmber}")
            .UsingGet();

        _iszrApiServer.Server.Given(request)
            .RespondWith(Response.Create()
                .WithStatusCode(StatusCodes.Status200OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(ruianResponseModel)));

        var iszrClient = new IszrClient(
            loggerMock.Object,
            Options.Create(new IszrClientConfiguration
            {
                ApiUrl = _iszrApiServer.Server.Url
            }), _httpClientFactoryMock.Object);

        // Act
        var ruianResponse = await iszrClient.GetRuianByBirthNumber(birthNUmber);

        // Assert
        ruianResponse.Should().Be(ruianResponseModel.Ruian);
    }
    
    [Fact]
    public async Task GetRuianByBirthNumber_ThrowsCorrectException_WhenIszrRespondsNotFound()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<IszrClient>>();
        var birthNumber = "141019891234";
        var request = Request
            .Create()
            .WithPath($"/ruian/{birthNumber}")
            .UsingGet();

        _iszrApiServer.Server.Given(request)
            .RespondWith(Response.Create()
                .WithStatusCode(StatusCodes.Status404NotFound)
                .WithHeader("Content-Type", "application/json"));

        var iszrClient = new IszrClient(
            loggerMock.Object,
            Options.Create(new IszrClientConfiguration
            {
                ApiUrl = _iszrApiServer.Server.Url
            }), _httpClientFactoryMock.Object);
        
        Func<Task<int>> call = () => iszrClient.GetRuianByBirthNumber(birthNumber);
        // Act and Assert
        var assertion = await call.Should().ThrowAsync<IszrException>();
        assertion.Where(e => e.HttpStatusCode == StatusCodes.Status404NotFound);
    }
    
        
    [Theory]
    [InlineData(StatusCodes.Status403Forbidden)]
    [InlineData(StatusCodes.Status500InternalServerError)]
    [InlineData(StatusCodes.Status400BadRequest)]
    public async Task GetRuianByBirthNumber_ThrowsCorrectException_WhenIszrRespondsIncorrectly(int statusCodeToReturn)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<IszrClient>>();
        var birthNumber = "141019891234";
        var request = Request
            .Create()
            .WithPath($"/ruian/{birthNumber}")
            .UsingGet();

        _iszrApiServer.Server.Given(request)
            .RespondWith(Response.Create()
                .WithStatusCode(statusCodeToReturn)
                .WithHeader("Content-Type", "application/json"));

        var iszrClient = new IszrClient(
            loggerMock.Object,
            Options.Create(new IszrClientConfiguration
            {
                ApiUrl = _iszrApiServer.Server.Url
            }), _httpClientFactoryMock.Object);
        
        Func<Task<int>> call = () => iszrClient.GetRuianByBirthNumber(birthNumber);
        // Act and Assert
        var assertion = await call.Should().ThrowAsync<IszrException>();
        assertion.Where(e => e.HttpStatusCode == StatusCodes.Status500InternalServerError);
    }
}