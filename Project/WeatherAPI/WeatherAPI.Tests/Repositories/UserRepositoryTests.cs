using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WeatherAPI.API.Data;
using WeatherAPI.API.Models.Entities;
using WeatherAPI.API.Repositories;
using Xunit;

namespace WeatherAPI.Tests.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new UserRepository(_context);

        SeedData();
    }

    private void SeedData()
    {
        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@test.com",
                PasswordHash = "hash123",
                FirstName = "Admin",
                LastName = "User",
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = Guid.NewGuid(),
                Email = "user@test.com",
                PasswordHash = "hash456",
                FirstName = "Regular",
                LastName = "User",
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = Guid.NewGuid(),
                Email = "inactive@test.com",
                PasswordHash = "hash789",
                FirstName = "Inactive",
                LastName = "User",
                Role = "User",
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Users.AddRange(users);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetByEmailAsync_ExistingUser_ReturnsUser()
    {
        // Act
        var result = await _repository.GetByEmailAsync("admin@test.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("admin@test.com");
        result.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task GetByEmailAsync_CaseInsensitive_ReturnsUser()
    {
        // Act
        var result = await _repository.GetByEmailAsync("ADMIN@TEST.COM");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("admin@test.com");
    }

    [Fact]
    public async Task GetByEmailAsync_NonExistingUser_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByEmailAsync("nonexistent@test.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task EmailExistsAsync_ExistingEmail_ReturnsTrue()
    {
        // Act
        var result = await _repository.EmailExistsAsync("user@test.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_NonExistingEmail_ReturnsFalse()
    {
        // Act
        var result = await _repository.EmailExistsAsync("newuser@test.com");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_NewUser_CreatesUser()
    {
        // Arrange
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "new@test.com",
            PasswordHash = "newhash",
            FirstName = "New",
            LastName = "User",
            Role = "User",
            IsActive = true
        };

        // Act
        var result = await _repository.AddAsync(newUser);

        // Assert
        result.Should().NotBeNull();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var exists = await _repository.EmailExistsAsync("new@test.com");
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ExistingUser_UpdatesUser()
    {
        // Arrange
        var user = await _context.Users.FirstAsync(u => u.Email == "user@test.com");
        user.FirstName = "Updated";
        user.LastLoginAt = DateTime.UtcNow;

        // Act
        var result = await _repository.UpdateAsync(user);

        // Assert
        result.FirstName.Should().Be("Updated");
        result.UpdatedAt.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}