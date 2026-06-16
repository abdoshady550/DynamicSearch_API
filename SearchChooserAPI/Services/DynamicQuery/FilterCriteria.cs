namespace Meccano.DynamicQuery;

public class FilterCriteria
{
    public string ColumnName { get; set; } = string.Empty;
    public FilterOperator Operator { get; set; } = FilterOperator.Eq;
    public string Value { get; set; } = string.Empty;
    public string? Value2 { get; set; }
}

public enum FilterOperator
{
    Eq=0,
    Neq=1,
    Gt=2,
    Lt=3,
    Range=4
}
