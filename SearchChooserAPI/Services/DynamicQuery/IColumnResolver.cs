namespace Meccano.DynamicQuery;

public interface IColumnResolver<TResponse> where TResponse : class
{
    ResolvedColumns<TResponse> Resolve(List<string>? columns, ColumnMode mode);
}
