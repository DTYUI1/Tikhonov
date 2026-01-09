using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WeatherAPI.API.Data;
using WeatherAPI.API.Models.DTO;
using WeatherAPI.API.Models.Entities;
using WeatherAPI.API.Repositories;
using Xunit;

namespace WeatherAPI.Tests.Repositories;

public class WeatherRecordRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly WeatherRecordRepository _repository;
    private readonly Guid _cityId = Guid.NewGuid();
    private readonly Guid _weatherTypeId = Guid.NewGuid();

    public WeatherRecordRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new WeatherRecordRepository(_context);

        SeedData();
    }

    private void SeedData()
    {
        var city = new City
        {
            Id = _cityId,
            Name = "Moscow",
            Country = "Russia",
            Latitude = 55.7558,
            Longitude = 37.6173,
            CreatedAt = DateTime.UtcNow
        };

        var weatherType = new WeatherType
        {
            Id = _weatherTypeId,
            Name = "Sunny",
            Description = "Clear sky",
            IconCode = "01d",
            CreatedAt = DateTime.UtcNow
        };

        _context.Cities.Add(city);
        _context.WeatherTypes.Add(weatherType);

        var records = new List<WeatherRecord>
        {
            new WeatherRecord
            {
                Id = Guid.NewGuid(),
                CityId = _cityId,
                WeatherTypeId = _weatherTypeId,
                RecordedAt = DateTime.UtcNow.AddHours(-1),
                Temperature = 20.5,
                FeelsLike = 19.0,
                Humidity = 60,
                WindSpeed = 5.0,
                CreatedAt = DateTime.UtcNow
            },
            new WeatherRecord
            {
                Id = Guid.NewGuid(),
                CityId = _cityId,
                WeatherTypeId = _weatherTypeId,
                RecordedAt = DateTime.UtcNow,
                Temperature = 22.0,
                FeelsLike = 21.0,
                Humidity = 55,
                WindSpeed = 4.5,
                CreatedAt = DateTime.UtcNow
            },
            new WeatherRecord
            {
                Id = Guid.NewGuid(),
                CityId = _cityId,
                WeatherTypeId = _weatherTypeId,
                RecordedAt = DateTime.UtcNow.AddDays(-2),
                Temperature = 18.0,
                FeelsLike = 17.0,
                Humidity = 70,
                WindSpeed = 6.0,
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.WeatherRecords.AddRange(records);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetLatestForCityAsync_ReturnsLatestRecord()
    {
        // Act
        var result = await _repository.GetLatestForCityAsync(_cityId);

        // Assert
        result.Should().NotBeNull();
        result!.Temperature.Should().Be(22.0);
        result.City.Should().NotBeNull();
        result.WeatherType.Should().NotBeNull();
    }

    [Fact]
    public async Task GetLatestForCityAsync_NonExistingCity_ReturnsNull()
    {
        // Act
        var result = await _repository.GetLatestForCityAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_WithCityFilter_ReturnsFilteredResults()
    {
        // Arrange
        var filter = new WeatherRecordFilterQuery { CityId = _cityId, Page = 1, PageSize = 10 };

        // Act
        var (items, total) = await _repository.GetPagedAsync(filter);

        // Assert
        total.Should().Be(3);
        items.Should().HaveCount(3);
        items.Should().AllSatisfy(r => r.CityId.Should().Be(_cityId));
    }

    [Fact]
    public async Task GetPagedAsync_WithTemperatureFilter_ReturnsFilteredResults()
    {
        // Arrange
        var filter = new WeatherRecordFilterQuery 
        { 
            MinTemperature = 20, 
            MaxTemperature = 25,
            Page = 1, 
            PageSize = 10 
        };

        // Act
        var (items, total) = await _repository.GetPagedAsync(filter);

        // Assert
        total.Should().Be(2);
        items.Should().AllSatisfy(r => r.Temperature.Should().BeInRange(20, 25));
    }

    [Fact]
    public async Task GetPagedAsync_WithDateFilter_ReturnsFilteredResults()
    {
        // Arrange
        var filter = new WeatherRecordFilterQuery 
        { 
            FromDate = DateTime.UtcNow.AddDays(-1),
            ToDate = DateTime.UtcNow.AddDays(1),
            Page = 1, 
            PageSize = 10 
        };

        // Act
        var (items, total) = await _repository.GetPagedAsync(filter);

        // Assert
        total.Should().Be(2);
    }

    [Fact]
    public async Task GetWithDetailsAsync_ReturnsRecordWithNavigationProperties()
    {
        // Arrange
        var record = await _context.WeatherRecords.FirstAsync();

        // Act
        var result = await _repository.GetWithDetailsAsync(record.Id);

        // Assert
        result.Should().NotBeNull();
        result!.City.Should().NotBeNull();
        result.City.Name.Should().Be("Moscow");
        result.WeatherType.Should().NotBeNull();
        result.WeatherType.Name.Should().Be("Sunny");
    }

    [Fact]
    public async Task GetByCityIdAsync_ReturnsRecordsForCity()
    {
        // Act
        var result = await _repository.GetByCityIdAsync(_cityId);

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(r => r.CityId.Should().Be(_cityId));
    }

    [Fact]
    public async Task GetByCityIdAsync_WithDateRange_ReturnsFilteredRecords()
    {
        // Act
        var result = await _repository.GetByCityIdAsync(
            _cityId, 
            DateTime.UtcNow.AddDays(-1), 
            DateTime.UtcNow.AddDays(1));

        // Assert
        result.Should().HaveCount(2);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}