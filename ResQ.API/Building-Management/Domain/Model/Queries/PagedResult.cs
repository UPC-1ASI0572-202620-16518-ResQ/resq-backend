namespace ResQ.API.Building_Management.Domain.Model.Queries;

/// <summary>
/// Represents a paginated query result.
/// </summary>
public record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int Size,
    long TotalElements,
    int TotalPages);
