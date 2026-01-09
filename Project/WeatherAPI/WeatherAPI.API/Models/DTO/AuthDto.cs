namespace WeatherAPI.API.Models.DTO;

public record RegisterDto(
    string Email,
    string Password,
    string FirstName,
    string LastName
);

public record LoginDto(
    string Email,
    string Password
);

public record AuthResponseDto(
    string Token,
    DateTime ExpiresAt,
    UserResponseDto User
);

public record UserResponseDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsActive,
    DateTime CreatedAt
);

public record UserUpdateDto(
    string FirstName,
    string LastName
);

public record ChangeRoleDto(
    string Role
);