using System.ComponentModel.DataAnnotations;

namespace User.Services.Models;

public class CreateUserModel
{
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    public DateTime DateOfBirth { get; set; }
    [Required]
    public string BirthNumber { get; set; }
}