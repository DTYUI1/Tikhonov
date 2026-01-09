using Microsoft.EntityFrameworkCore;
using WeatherAPI.API.Models.Entities;

namespace WeatherAPI.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    public DbSet<City> Cities => Set<City>();
    public DbSet<WeatherType> WeatherTypes => Set<WeatherType>();
    public DbSet<WeatherRecord> WeatherRecords => Set<WeatherRecord>();
    public DbSet<CityWeatherType> CityWeatherTypes => Set<CityWeatherType>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // City 
        modelBuilder.Entity<City>(entity =>
        {
            entity.ToTable("cities");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Country).HasColumnName("country").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Latitude).HasColumnName("latitude");
            entity.Property(e => e.Longitude).HasColumnName("longitude");
            entity.Property(e => e.TimeZone).HasColumnName("timezone").HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            
            entity.HasIndex(e => new { e.Name, e.Country }).IsUnique();
        });

        // WeatherType 
        modelBuilder.Entity<WeatherType>(entity =>
        {
            entity.ToTable("weather_types");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(200);
            entity.Property(e => e.IconCode).HasColumnName("icon_code").HasMaxLength(10);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // WeatherRecord 
        modelBuilder.Entity<WeatherRecord>(entity =>
        {
            entity.ToTable("weather_records");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.WeatherTypeId).HasColumnName("weather_type_id");
            entity.Property(e => e.RecordedAt).HasColumnName("recorded_at");
            entity.Property(e => e.Temperature).HasColumnName("temperature");
            entity.Property(e => e.FeelsLike).HasColumnName("feels_like");
            entity.Property(e => e.Humidity).HasColumnName("humidity");
            entity.Property(e => e.WindSpeed).HasColumnName("wind_speed");
            entity.Property(e => e.WindDirection).HasColumnName("wind_direction");
            entity.Property(e => e.Pressure).HasColumnName("pressure");
            entity.Property(e => e.Visibility).HasColumnName("visibility");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.City)
                .WithMany(c => c.WeatherRecords)
                .HasForeignKey(e => e.CityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.WeatherType)
                .WithMany(wt => wt.WeatherRecords)
                .HasForeignKey(e => e.WeatherTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.CityId, e.RecordedAt });
        });

        // CityWeatherType (Many-to-Many) 
        modelBuilder.Entity<CityWeatherType>(entity =>
        {
            entity.ToTable("city_weather_types");
            entity.HasKey(e => new { e.CityId, e.WeatherTypeId });
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.WeatherTypeId).HasColumnName("weather_type_id");
            entity.Property(e => e.Frequency).HasColumnName("frequency");
            entity.Property(e => e.Season).HasColumnName("season").HasMaxLength(20);

            entity.HasOne(e => e.City)
                .WithMany(c => c.CityWeatherTypes)
                .HasForeignKey(e => e.CityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.WeatherType)
                .WithMany(wt => wt.CityWeatherTypes)
                .HasForeignKey(e => e.WeatherTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // User 
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(100);
            entity.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(100);
            entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(20);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.Email).IsUnique();
        });

        // ApiKey 
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.ToTable("api_keys");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Key).HasColumnName("key").HasMaxLength(64).IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Permissions).HasColumnName("permissions");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.Key).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // IdempotencyKey 
        modelBuilder.Entity<IdempotencyKey>(entity =>
        {
            entity.ToTable("idempotency_keys");
            entity.HasKey(e => e.Key);
            entity.Property(e => e.Key).HasColumnName("key").HasMaxLength(64);
            entity.Property(e => e.RequestPath).HasColumnName("request_path").HasMaxLength(500);
            entity.Property(e => e.ResponseBody).HasColumnName("response_body");
            entity.Property(e => e.StatusCode).HasColumnName("status_code");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
        });
    }
}