using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WeatherAPI.API.Data;
using WeatherAPI.API.Models.Entities;
using WeatherAPI.API.Repositories;
using Xunit;

namespace WeatherAPI.Tests.Repositories;

public class CityWeatherTypeRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CityWeatherTypeRepository _repository;
    private readonly Guid _cityId = Guid.NewGuid();
    private readonly Guid _city2Id = Guid.NewGuid();
    private readonly Guid _weatherTypeId = Guid.NewGuid();
    private readonly Guid _weatherType2Id = Guid.NewGuid();

    public CityWeatherTypeRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new CityWeatherTypeRepository(_context);

        SeedData();
    }

    private void SeedData()
    {
        var cities = new List<City>
        {
            new City { Id = _cityId, Name = "Moscow", Country = "Russia", Latitude = 55.75, Longitude = 37.61, CreatedAt = DateTime.UtcNow },
            new City { Id = _city2Id, Name = "London", Country = "UK", Latitude = 51.50, Longitude = -0.12, CreatedAt = DateTime.UtcNow }
        };

        var weatherTypes = new List<WeatherType>
        {
            new WeatherType { Id = _weatherTypeId, Name = "Sunny", Description = "Clear", IconCode = "01d", CreatedAt = DateTime.UtcNow },
            new WeatherType { Id = _weatherType2Id, Name = "Rainy", Description = "Rain", IconCode = "10d", CreatedAt = DateTime.UtcNow }
        };

        var cityWeatherTypes = new List<CityWeatherType>
        {
            new CityWeatherType { CityId = _cityId, WeatherTypeId = _weatherTypeId, Frequency = 30, Season = "Summer" },
            new CityWeatherType { CityId = _cityId, WeatherTypeId = _weatherType2Id, Frequency = 20, Season = "Autumn" },
            new CityWeatherType { CityId = _city2Id, WeatherTypeId = _weatherType2Id, Frequency = 50, Season = "All" }
        };

        _context.Cities.AddRange(cities);
        _context.WeatherTypes.AddRange(weatherTypes);
        _context.CityWeatherTypes.AddRange(cityWeatherTypes);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAsync_ExistingAssociation_ReturnsAssociation()
    {
        // Act
        var result = await _repository.GetAsync(_cityId, _weatherTypeId);

        // Assert
        result.Should().NotBeNull();
        result!.Frequency.Should().Be(30);
        result.Season.Should().Be("Summer");
        result.City.Should().NotBeNull();
        result.WeatherType.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAsync_NonExistingAssociation_ReturnsNull()
    {
        // Act
        var result = await _repository.GetAsync(_city2Id, _weatherTypeId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCityIdAsync_ReturnsAllAssociationsForCity()
    {
        // Act
        var result = await _repository.GetByCityIdAsync(_cityId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(cwt => cwt.CityId.Should().Be(_cityId));
    }

    [Fact]
    public async Task GetByWeatherTypeIdAsync_ReturnsAllAssociationsForWeatherType()
    {
        // Act
        var result = await _repository.GetByWeatherTypeIdAsync(_weatherType2Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(cwt => cwt.WeatherTypeId.Should().Be(_weatherType2Id));
    }

    [Fact]
    public async Task AddAsync_NewAssociation_CreatesAssociation()
    {
        // Arrange
        var newAssociation = new CityWeatherType
        {
            CityId = _city2Id,
            WeatherTypeId = _weatherTypeId,
            Frequency = 25,
            Season = "Winter"
        };

        // Act
        var result = await _repository.AddAsync(newAssociation);

        // Assert
        result.Should().NotBeNull();
        result.Frequency.Should().Be(25);

        var exists = await _repository.ExistsAsync(_city2Id, _weatherTypeId);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ExistingAssociation_UpdatesAssociation()
    {
        // Arrange
        var association = await _context.CityWeatherTypes
            .FirstAsync(cwt => cwt.CityId == _cityId && cwt.WeatherTypeId == _weatherTypeId);
        association.Frequency = 45;

        // Act
        var result = await _repository.UpdateAsync(association);

        // Assert
        result.Frequency.Should().Be(45);
    }

    [Fact]
    public async Task DeleteAsync_ExistingAssociation_RemovesAssociation()
    {
        // Arrange
        var association = await _context.CityWeatherTypes
            .FirstAsync(cwt => cwt.CityId == _cityId && cwt.WeatherTypeId == _weatherTypeId);

        // Act
        await _repository.DeleteAsync(association);

        // Assert
        var exists = await _repository.ExistsAsync(_cityId, _weatherTypeId);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_ExistingAssociation_ReturnsTrue()
    {
        // Act
        var result = await _repository.ExistsAsync(_cityId, _weatherTypeId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistingAssociation_ReturnsFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(_city2Id, _weatherTypeId);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}