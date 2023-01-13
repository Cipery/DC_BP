namespace User.Services.Models;

public class UpdateUserModel
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}