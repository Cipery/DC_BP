using System.ComponentModel.DataAnnotations;

namespace User.Services.Models;

public class CreateUserModel
{
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    [Range(typeof(DateTime), "1900-01-01", "2040-01-01", ErrorMessage="Date is out of Range")]
    public DateTime DateOfBirth { get; set; }
    [Required]
    public string BirthNumber { get; set; }
}