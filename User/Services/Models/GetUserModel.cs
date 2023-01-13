namespace User.Services.Models;

public class GetUserModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public int? Ruian { get; set; }
}