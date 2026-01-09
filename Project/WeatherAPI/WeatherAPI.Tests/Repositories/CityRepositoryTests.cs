using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WeatherAPI.API.Data;
using WeatherAPI.API.Models.DTO;
using WeatherAPI.API.Models.Entities;
using WeatherAPI.API.Repositories;
using Xunit;

namespace WeatherAPI.Tests.Repositories;

public class CityRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CityRepository _repository;

    public CityRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new CityRepository(_context);

        SeedData();
    }

    private void SeedData()
    {
        var cities = new List<City>
        {
            new City
            {
                Id = Guid.NewGuid(),
                Name = "Moscow",
                Country = "Russia",
                Latitude = 55.7558,
                Longitude = 37.6173,
                TimeZone = "Europe/Moscow",
                CreatedAt = DateTime.UtcNow
            },
            new City
            {
                Id = Guid.NewGuid(),
                Name = "London",
                Country = "United Kingdom",
                Latitude = 51.5074,
                Longitude = -0.1278,
                TimeZone = "Europe/London",
                CreatedAt = DateTime.UtcNow
            },
            new City
            {
                Id = Guid.NewGuid(),
                Name = "Manchester",
                Country = "United Kingdom",
                Latitude = 53.4808,
                Longitude = -2.2426,
                TimeZone = "Europe/London",
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Cities.AddRange(cities);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingCity_ReturnsCity()
    {
        // Arrange
        var existingCity = await _context.Cities.FirstAsync();

        // Act
        var result = await _repository.GetByIdAsync(existingCity.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(existingCity.Id);
        result.Name.Should().Be(existingCity.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingCity_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCities()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetPagedAsync_WithSearch_ReturnsFilteredResults()
    {
        // Arrange
        var filter = new CityFilterQuery { Search = "London", Page = 1, PageSize = 10 };

        // Act
        var (items, total) = await _repository.GetPagedAsync(filter);

        // Assert
        total.Should().Be(1);
        items.Should().HaveCount(1);
        items.First().Name.Should().Be("London");
    }

    [Fact]
    public async Task GetPagedAsync_WithCountryFilter_ReturnsFilteredResults()
    {
        // Arrange
        var filter = new CityFilterQuery { Country = "United Kingdom", Page = 1, PageSize = 10 };

        // Act
        var (items, total) = await _repository.GetPagedAsync(filter);

        // Assert
        total.Should().Be(2);
        items.Should().HaveCount(2);
        items.Should().AllSatisfy(c => c.Country.Should().Be("United Kingdom"));
    }

    [Fact]
    public async Task GetPagedAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var filter = new CityFilterQuery { Page = 1, PageSize = 2 };

        // Act
        var (items, total) = await _repository.GetPagedAsync(filter);

        // Assert
        total.Should().Be(3);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddAsync_ValidCity_AddsAndReturnsCity()
    {
        // Arrange
        var newCity = new City
        {
            Id = Guid.NewGuid(),
            Name = "Paris",
            Country = "France",
            Latitude = 48.8566,
            Longitude = 2.3522,
            TimeZone = "Europe/Paris"
        };

        // Act
        var result = await _repository.AddAsync(newCity);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(newCity.Id);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var savedCity = await _context.Cities.FindAsync(newCity.Id);
        savedCity.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ExistingCity_UpdatesCity()
    {
        // Arrange
        var existingCity = await _context.Cities.FirstAsync();
        var originalName = existingCity.Name;
        existingCity.Name = "Updated Name";

        // Act
        var result = await _repository.UpdateAsync(existingCity);

        // Assert
        result.Name.Should().Be("Updated Name");
        result.UpdatedAt.Should().NotBeNull();
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DeleteAsync_ExistingCity_RemovesCity()
    {
        // Arrange
        var cityToDelete = await _context.Cities.FirstAsync();
        var cityId = cityToDelete.Id;

        // Act
        await _repository.DeleteAsync(cityToDelete);

        // Assert
        var deletedCity = await _context.Cities.FindAsync(cityId);
        deletedCity.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_ExistingCity_ReturnsTrue()
    {
        // Arrange
        var existingCity = await _context.Cities.FirstAsync();

        // Act
        var result = await _repository.ExistsAsync(existingCity.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistingCity_ReturnsFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByNameAndCountryAsync_ExistingCity_ReturnsCity()
    {
        // Act
        var result = await _repository.GetByNameAndCountryAsync("Moscow", "Russia");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Moscow");
        result.Country.Should().Be("Russia");
    }

    [Fact]
    public async Task GetByNameAndCountryAsync_NonExistingCity_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByNameAndCountryAsync("Berlin", "Germany");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CountAsync_ReturnsCorrectCount()
    {
        // Act
        var result = await _repository.CountAsync();

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ReturnsFilteredCount()
    {
        // Act
        var result = await _repository.CountAsync(c => c.Country == "United Kingdom");

        // Assert
        result.Should().Be(2);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}