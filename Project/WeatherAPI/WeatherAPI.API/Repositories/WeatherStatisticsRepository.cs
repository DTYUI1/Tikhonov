using System.Data;
using Dapper;
using Npgsql;
using WeatherAPI.API.Repositories.Interfaces;

namespace WeatherAPI.API.Repositories;

public class WeatherStatisticsRepository : IWeatherStatisticsRepository
{
    private readonly string _connectionString;
    private readonly ILogger<WeatherStatisticsRepository> _logger;

    public WeatherStatisticsRepository(
        IConfiguration configuration, 
        ILogger<WeatherStatisticsRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException("Connection string not found");
        _logger = logger;
    }

    public async Task<WeatherStatisticsDto> GetCityStatisticsAsync(
        Guid cityId, 
        DateTime from, 
        DateTime to, 
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            WITH weather_stats AS (
                SELECT 
                    c.id as city_id,
                    c.name as city_name,
                    AVG(wr.temperature) as avg_temperature,
                    MIN(wr.temperature) as min_temperature,
                    MAX(wr.temperature) as max_temperature,
                    AVG(wr.humidity) as avg_humidity,
                    AVG(wr.wind_speed) as avg_wind_speed,
                    COUNT(*) as record_count
                FROM weather_records wr
                JOIN cities c ON c.id = wr.city_id
                WHERE wr.city_id = @CityId 
                    AND wr.recorded_at >= @From 
                    AND wr.recorded_at <= @To
                GROUP BY c.id, c.name
            ),
            most_common AS (
                SELECT 
                    wt.name as weather_type_name,
                    COUNT(*) as cnt
                FROM weather_records wr
                JOIN weather_types wt ON wt.id = wr.weather_type_id
                WHERE wr.city_id = @CityId 
                    AND wr.recorded_at >= @From 
                    AND wr.recorded_at <= @To
                GROUP BY wt.name
                ORDER BY cnt DESC
                LIMIT 1
            )
            SELECT 
                ws.city_id as CityId,
                ws.city_name as CityName,
                COALESCE(ws.avg_temperature, 0) as AvgTemperature,
                COALESCE(ws.min_temperature, 0) as MinTemperature,
                COALESCE(ws.max_temperature, 0) as MaxTemperature,
                COALESCE(ws.avg_humidity, 0) as AvgHumidity,
                COALESCE(ws.avg_wind_speed, 0) as AvgWindSpeed,
                COALESCE(ws.record_count, 0) as RecordCount,
                COALESCE(mc.weather_type_name, 'N/A') as MostCommonWeatherType
            FROM weather_stats ws
            LEFT JOIN most_common mc ON true";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // Используем транзакцию для Dapper
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            _logger.LogInformation(
                "Fetching weather statistics for city {CityId} from {From} to {To}", 
                cityId, from, to);

            var result = await connection.QueryFirstOrDefaultAsync<WeatherStatisticsDto>(
                sql,
                new { CityId = cityId, From = from, To = to },
                transaction);

            await transaction.CommitAsync(cancellationToken);

            return result ?? new WeatherStatisticsDto(
                cityId, 
                "Unknown", 
                0, 0, 0, 0, 0, 0, 
                "N/A");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error fetching weather statistics for city {CityId}", cityId);
            throw;
        }
    }

    public async Task<IEnumerable<DailyAverageDto>> GetDailyAveragesAsync(
        Guid cityId, 
        DateTime from, 
        DateTime to, 
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                DATE(recorded_at) as Date,
                AVG(temperature) as AvgTemperature,
                MIN(temperature) as MinTemperature,
                MAX(temperature) as MaxTemperature,
                AVG(humidity) as AvgHumidity
            FROM weather_records
            WHERE city_id = @CityId 
                AND recorded_at >= @From 
                AND recorded_at <= @To
            GROUP BY DATE(recorded_at)
            ORDER BY DATE(recorded_at)";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            _logger.LogInformation(
                "Fetching daily averages for city {CityId} from {From} to {To}", 
                cityId, from, to);

            var result = await connection.QueryAsync<DailyAverageDto>(
                sql,
                new { CityId = cityId, From = from, To = to },
                transaction);

            await transaction.CommitAsync(cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error fetching daily averages for city {CityId}", cityId);
            throw;
        }
    }
}