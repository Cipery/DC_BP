namespace User.Services.Models;

public class CreateUserModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public string BirthNumber { get; set; }
}