namespace WeatherAPI.API.Models.Entities;

public class ApiKey : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;          // Название клиента
    public Guid? UserId { get; set; }                         // Опционально связан с пользователем
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Permissions { get; set; }                  // JSON с правами
    
    // Navigation
    public User? User { get; set; }
}