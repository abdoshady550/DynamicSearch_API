namespace Meccano.DynamicQuery;

public interface IDynamicQueryRequest<TResponse> where TResponse : class
{
    List<string>? Columns { get; set; }
    ColumnMode Mode { get; set; }
    IColumnResolver<TResponse> ColumnResolver { get; }
    string? Search { get; set; }
    List<FilterCriteria>? Filters { get; set; }
}
