namespace User.Entities;

public class UserEntity : BaseEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTimeOffset DateOfBirth { get; set; }
    public string BirthNumber { get; set; }
    public int? Ruian { get; set; }
}