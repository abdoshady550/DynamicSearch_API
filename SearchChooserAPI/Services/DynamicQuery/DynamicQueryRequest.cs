namespace Meccano.DynamicQuery;

public abstract class DynamicQueryRequest<TResponse> : IDynamicQueryRequest<TResponse>
    where TResponse : class
{
    public List<string>? Columns { get; set; }
    public ColumnMode Mode { get; set; } = ColumnMode.Include;
    public string? Search { get; set; }
    public List<FilterCriteria>? Filters { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
