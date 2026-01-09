namespace WeatherAPI.API.Models.DTO;

public record PagedResponse<T>(
    List<T> Items,
    int Total,
    int Page,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

public record PaginationQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public record CityFilterQuery : PaginationQuery
{
    public string? Search { get; init; }
    public string? Country { get; init; }
}

public record WeatherRecordFilterQuery : PaginationQuery
{
    public Guid? CityId { get; init; }
    public Guid? WeatherTypeId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public double? MinTemperature { get; init; }
    public double? MaxTemperature { get; init; }
}