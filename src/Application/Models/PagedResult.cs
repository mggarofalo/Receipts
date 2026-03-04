namespace Application.Models;

public record PagedResult<T>(List<T> Data, int Total, int Offset, int Limit);
