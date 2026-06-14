namespace SearchChooserAPI.IntegrationTests.Fixtures;

public class DoctorSearchRequestBuilder
{
    private readonly DoctorSearchRequest _request = new()
    {
        Columns = null,
        Mode = ColumnMode.Include,
        Search = null,
        Filters = null
    };

    public DoctorSearchRequestBuilder WithColumns(params string[] columns)
    {
        _request.Columns = columns.ToList();
        return this;
    }

    public DoctorSearchRequestBuilder WithMode(ColumnMode mode)
    {
        _request.Mode = mode;
        return this;
    }

    public DoctorSearchRequestBuilder WithSearch(string? search)
    {
        _request.Search = search;
        return this;
    }

    public DoctorSearchRequestBuilder WithFilters(params FilterCriteria[] filters)
    {
        _request.Filters = filters.ToList();
        return this;
    }

    public DoctorSearchRequestBuilder AddFilter(string columnName, FilterOperator op, string value, string? value2 = null)
    {
        _request.Filters ??= new List<FilterCriteria>();
        _request.Filters.Add(new FilterCriteria
        {
            ColumnName = columnName,
            Operator = op,
            Value = value,
            Value2 = value2
        });
        return this;
    }

    public DoctorSearchRequest Build() => _request;

    public JsonContent BuildJsonContent() =>
        JsonContent.Create(_request, options: new JsonSerializerOptions(JsonSerializerDefaults.Web));

    public static implicit operator DoctorSearchRequest(DoctorSearchRequestBuilder builder) => builder.Build();
}
