using Microsoft.Extensions.Options;

namespace User.Clients;

public class IszrClient : IIszrClient
{
    private readonly string _apiUrl;
    private readonly IHttpClientFactory _httpClientFactory;
    
    public IszrClient(IOptions<IszrClientConfiguration> configuration, IHttpClientFactory httpClientFactory)
    {
        _apiUrl = configuration.Value.ApiUrl;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<int?> GetRuianByBirthNumber(string birthNumber)
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetFromJsonAsync<GetRuianByBirthNumberResponse>(_apiUrl);
        return response?.Ruian;
    }
}