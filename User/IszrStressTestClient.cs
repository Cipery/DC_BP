using User.Clients;

namespace User;

public class IszrStressTestClient : IIszrClient
{
    public async Task<int> GetRuianByBirthNumber(string birthNumber)
    {
        await Task.Delay(500 + Random.Shared.Next(1000));
        return Random.Shared.Next(100000, 999999);
    }
}