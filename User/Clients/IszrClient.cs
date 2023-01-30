using Microsoft.Extensions.Options;
using User.Exceptions;

namespace User.Clients;

public class IszrClient : IIszrClient
{
    private readonly ILogger _logger;
    private readonly string _apiUrl;
    private readonly IHttpClientFactory _httpClientFactory;
    
    public IszrClient(ILogger<IszrClient> logger, IOptions<IszrClientConfiguration> configuration,
        IHttpClientFactory httpClientFactory)
    {
        _apiUrl = configuration.Value.ApiUrl;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<int?> GetRuianByBirthNumber(string birthNumber)
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync($"{_apiUrl}/ruian/{birthNumber}");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Response for GET Ruian from ISZR is not successful, status {IszrResponseStatusCode}", response.StatusCode);
            if ((int) response.StatusCode == StatusCodes.Status404NotFound)
            {
                throw new IszrException(StatusCodes.Status404NotFound);
            }
            throw new IszrException(StatusCodes.Status500InternalServerError);
        }

        var responseModel = await response.Content.ReadFromJsonAsync<GetRuianByBirthNumberResponseModel>();
        return responseModel?.Ruian;
    }
}