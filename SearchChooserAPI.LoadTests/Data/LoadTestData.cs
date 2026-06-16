using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Meccano.DynamicQuery;
using SearchChooserAPI.Models.Req;

namespace SearchChooserAPI.LoadTests.Data;

public static class LoadTestData
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
    };

    private static readonly MediaTypeHeaderValue JsonMediaType = new("application/json");

    public static StringContent CreateJsonContent<T>(T body)
    {
        var json = JsonSerializer.Serialize(body, JsonOptions);
        return new StringContent(json, Encoding.UTF8, JsonMediaType);
    }

    public static HttpRequestMessage CreatePostRequest(string url, object body)
    {
        var json = JsonSerializer.Serialize(body, JsonOptions);
        return new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, JsonMediaType)
        };
    }

    public static readonly DoctorSearchRequest GetAllDoctorsRequest = new()
    {
        PageSize = 20,
        PageNumber = 1
    };

    public static readonly DoctorSearchRequest SearchByNameRequest = new()
    {
        Search = "John",
        PageSize = 20,
        PageNumber = 1
    };

    public static readonly DoctorSearchRequest FilterByExperienceRequest = new()
    {
        PageSize = 20,
        PageNumber = 1,
        Filters =
        [
            new FilterCriteria
            {
                ColumnName = "YearsOfExperience",
                Operator = FilterOperator.Gt,
                Value = "10"
            }
        ]
    };

    public static readonly DoctorSearchRequest IncludeColumnsRequest = new()
    {
        PageSize = 20,
        PageNumber = 1,
        Columns = ["DoctorId", "DoctorName", "SpecialtyName"],
        Mode = ColumnMode.Include
    };

    public static readonly DoctorSearchRequest MultiFilterRequest = new()
    {
        PageSize = 20,
        PageNumber = 1,
        Filters =
        [
            new FilterCriteria
            {
                ColumnName = "YearsOfExperience",
                Operator = FilterOperator.Gt,
                Value = "5"
            },
            new FilterCriteria
            {
                ColumnName = "Rating",
                Operator = FilterOperator.Gt,
                Value = "3.5"
            }
        ]
    };

    public static readonly DoctorSearchRequest FullPipelineRequest = new()
    {
        PageSize = 20,
        PageNumber = 1,
        Search = "a",
        Columns = ["DoctorId", "DoctorName", "SpecialtyName", "Degree", "YearsOfExperience"],
        Mode = ColumnMode.Include,
        Filters =
        [
            new FilterCriteria
            {
                ColumnName = "YearsOfExperience",
                Operator = FilterOperator.Gt,
                Value = "2"
            }
        ]
    };

    public static readonly IReadOnlyList<DoctorSearchRequest> AllRequests =
    [
        GetAllDoctorsRequest,
        SearchByNameRequest,
        FilterByExperienceRequest,
        IncludeColumnsRequest,
        MultiFilterRequest,
        FullPipelineRequest
    ];

    public static DoctorSearchRequest RandomRequest()
    {
        var idx = Random.Shared.Next(AllRequests.Count);
        return AllRequests[idx];
    }
}
