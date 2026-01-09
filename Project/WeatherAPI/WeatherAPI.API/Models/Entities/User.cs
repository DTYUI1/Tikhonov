namespace WeatherAPI.API.Models.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = "User";      // Admin, Manager, User
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
}