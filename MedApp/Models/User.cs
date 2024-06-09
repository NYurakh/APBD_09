using System.ComponentModel.DataAnnotations;

namespace MedApp.Models;

public class User
{
    [Key]
    public int Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Password { get; set; }
    public string Salt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExp { get; set; }
}
