namespace User.Clients;

public interface IIszrClient
{
    Task<int?> GetRuianByBirthNumber(string birthNumber);
}