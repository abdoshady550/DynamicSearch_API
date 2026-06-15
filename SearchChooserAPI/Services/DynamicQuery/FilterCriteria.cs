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
    Eq,
    Neq,
    Gt,
    Lt,
    Range
}
