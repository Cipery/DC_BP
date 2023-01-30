using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using User.Clients;
using User.Exceptions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;

namespace User.Test.Unit;

public class IszrClientMockTest
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

    public IszrClientMockTest()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_httpMessageHandlerMock.Object));
    }

    [Fact]
    public async Task GetRuianByBirthNumber_ReturnsExpectedRuian_WhenIszrResponseIsOk()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<IszrClient>>();
        var birthNumber = "141019891234";
        var ruianResponseModel = new GetRuianByBirthNumberResponseModel
        {
            Ruian = 12345
        };
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(ruianResponseModel))
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var iszrClient = new IszrClient(
            loggerMock.Object,
            Options.Create(new IszrClientConfiguration
            {
                ApiUrl = "http://iszr-api"
            }), _httpClientFactoryMock.Object);

        // Act
        var ruianResponse = await iszrClient.GetRuianByBirthNumber(birthNumber);

        // Assert
        ruianResponse.Should().Be(ruianResponseModel.Ruian);
    }
    
    [Fact]
    public async Task GetRuianByBirthNumber_ThrowsCorrectException_WhenIszrRespondsNotFound()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<IszrClient>>();
        var birthNumber = "141019891234";
        var ruianResponseModel = new GetRuianByBirthNumberResponseModel
        {
            Ruian = 12345
        };
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotFound,
            Content = new StringContent(JsonSerializer.Serialize(ruianResponseModel))
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var iszrClient = new IszrClient(
            loggerMock.Object,
            Options.Create(new IszrClientConfiguration
            {
                ApiUrl = "http://iszr-api"
            }), _httpClientFactoryMock.Object);

        Func<Task<int?>> call = () => iszrClient.GetRuianByBirthNumber(birthNumber);
        
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
        var ruianResponseModel = new GetRuianByBirthNumberResponseModel
        {
            Ruian = 12345
        };
        var response = new HttpResponseMessage
        {
            StatusCode = (HttpStatusCode) statusCodeToReturn,
            Content = new StringContent(JsonSerializer.Serialize(ruianResponseModel))
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var iszrClient = new IszrClient(
            loggerMock.Object,
            Options.Create(new IszrClientConfiguration
            {
                ApiUrl = "http://iszr-api"
            }), _httpClientFactoryMock.Object);

        Func<Task<int?>> call = () => iszrClient.GetRuianByBirthNumber(birthNumber);
        // Act and Assert
        var assertion = await call.Should().ThrowAsync<IszrException>();
        assertion.Where(e => e.HttpStatusCode == StatusCodes.Status500InternalServerError);
    }
}