using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using SearchChooserAPI.LoadTests.Data;

namespace SearchChooserAPI.LoadTests.Scenarios;

public static class DoctorScenarios
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
    };

    private static readonly MediaTypeHeaderValue JsonMediaType = new("application/json");

    private static StringContent ToJsonContent(object body)
    {
        var json = JsonSerializer.Serialize(body, JsonOptions);
        return new StringContent(json, Encoding.UTF8, JsonMediaType);
    }

    public static ScenarioProps CreateGetAllDoctorsScenario(HttpClient client, string baseUrl)
    {
        return Scenario.Create("get_all_doctors", async context =>
        {
            var request = Http.CreateRequest("POST", $"{baseUrl}/api/Doctors")
                .WithHeader("Content-Type", "application/json")
                .WithBody(ToJsonContent(LoadTestData.GetAllDoctorsRequest));

            return await Http.Send(client, request);
        });
    }

    public static ScenarioProps CreateSearchByNameScenario(HttpClient client, string baseUrl)
    {
        return Scenario.Create("search_by_name", async context =>
        {
            var request = Http.CreateRequest("POST", $"{baseUrl}/api/Doctors")
                .WithHeader("Content-Type", "application/json")
                .WithBody(ToJsonContent(LoadTestData.SearchByNameRequest));

            return await Http.Send(client, request);
        });
    }

    public static ScenarioProps CreateFilterByExperienceScenario(HttpClient client, string baseUrl)
    {
        return Scenario.Create("filter_by_experience", async context =>
        {
            var request = Http.CreateRequest("POST", $"{baseUrl}/api/Doctors")
                .WithHeader("Content-Type", "application/json")
                .WithBody(ToJsonContent(LoadTestData.FilterByExperienceRequest));

            return await Http.Send(client, request);
        });
    }

    public static ScenarioProps CreateRandomWorkloadScenario(HttpClient client, string baseUrl)
    {
        return Scenario.Create("random_workload", async context =>
        {
            var body = LoadTestData.RandomRequest();
            var request = Http.CreateRequest("POST", $"{baseUrl}/api/Doctors")
                .WithHeader("Content-Type", "application/json")
                .WithBody(ToJsonContent(body));

            return await Http.Send(client, request);
        });
    }

    public static ScenarioProps CreateMixedScenario(HttpClient client, string baseUrl)
    {
        var bodies = LoadTestData.AllRequests.ToArray();

        return Scenario.Create("mixed_workload", async context =>
        {
            var idx = Random.Shared.Next(bodies.Length);
            var body = bodies[idx];
            var request = Http.CreateRequest("POST", $"{baseUrl}/api/Doctors")
                .WithHeader("Content-Type", "application/json")
                .WithBody(ToJsonContent(body));

            return await Http.Send(client, request);
        });
    }

    // ==========================================
    // DYNAMIC COMPLEX SCENARIOS (POST /api/Doctors)
    // ==========================================

    public static ScenarioProps CreateComplexAScenario(HttpClient client, string baseUrl)
    {
        return Scenario.Create("complex_a", async context =>
        {
            var request = Http.CreateRequest("POST", $"{baseUrl}/api/Doctors")
                .WithHeader("Content-Type", "application/json")
                .WithBody(ToJsonContent(LoadTestData.ComplexARequest));

            return await Http.Send(client, request);
        });
    }

    public static ScenarioProps CreateComplexBScenario(HttpClient client, string baseUrl)
    {
        return Scenario.Create("complex_b", async context =>
        {
            var request = Http.CreateRequest("POST", $"{baseUrl}/api/Doctors")
                .WithHeader("Content-Type", "application/json")
                .WithBody(ToJsonContent(LoadTestData.ComplexBRequest));

            return await Http.Send(client, request);
        });
    }

    public static ScenarioProps CreateComplexCScenario(HttpClient client, string baseUrl)
    {
        return Scenario.Create("complex_c", async context =>
        {
            var request = Http.CreateRequest("POST", $"{baseUrl}/api/Doctors")
                .WithHeader("Content-Type", "application/json")
                .WithBody(ToJsonContent(LoadTestData.ComplexCRequest));

            return await Http.Send(client, request);
        });
    }

    // ==========================================
    // ODATA SCENARIOS (GET /odata/DoctorsOData)
    // ==========================================

    private static string ODataUrl(string baseUrl, params (string key, string value)[] parameters)
    {
        if (parameters.Length == 0)
            return $"{baseUrl}/odata/DoctorsOData";

        var encoded = string.Join("&",
            parameters.Select(p => $"{p.key}={Uri.EscapeDataString(p.value)}"));
        return $"{baseUrl}/odata/DoctorsOData?{encoded}";
    }

    public static ScenarioProps CreateODataGetAllDoctorsScenario(HttpClient client, string baseUrl)
    {
        return Scenario.Create("odata_get_all_doctors", async context =>
        {
            var request = Http.CreateRequest("GET", ODataUrl(baseUrl));
            return await Http.Send(client, request);
        });
    }

    public static ScenarioProps CreateODataSearchByNameScenario(HttpClient client, string baseUrl)
    {
        return Scenario.Create("odata_search_by_name", async context =>
        {
            var url = ODataUrl(baseUrl,
                ("$filter", "contains(DoctorName,'John') or contains(SpecialtyName,'John') or contains(Degree,'John')"));
            var request = Http.CreateRequest("GET", url);
            return await Http.Send(client, request);
        });
    }

    public static ScenarioProps CreateODataFilterByExperienceScenario(HttpClient client, string baseUrl)
    {
        return Scenario.Create("odata_filter_by_experience", async context =>
        {
            var url = ODataUrl(baseUrl,
                ("$filter", "YearsOfExperience gt 10"));
            var request = Http.CreateRequest("GET", url);
            return await Http.Send(client, request);
        });
    }

    public static ScenarioProps CreateODataComplexAScenario(HttpClient client, string baseUrl)
    {
        return Scenario.Create("odata_complex_a", async context =>
        {
            var url = ODataUrl(baseUrl,
                ("$select", "DoctorId,DoctorName,SpecialtyName,YearsOfExperience,Rating"),
                ("$orderby", "SpecialtyName asc,Rating desc"),
                ("$top", "10"),
                ("$skip", "10"));
            var request = Http.CreateRequest("GET", url);
            return await Http.Send(client, request);
        });
    }

    public static ScenarioProps CreateODataComplexBScenario(HttpClient client, string baseUrl)
    {
        return Scenario.Create("odata_complex_b", async context =>
        {
            var url = ODataUrl(baseUrl,
                ("$select", "DoctorId,DoctorName,YearsOfExperience,Rating"),
                ("$filter", "YearsOfExperience gt 5 and Rating gt 3.0"),
                ("$orderby", "Rating desc"),
                ("$top", "5"),
                ("$skip", "0"));
            var request = Http.CreateRequest("GET", url);
            return await Http.Send(client, request);
        });
    }

    public static ScenarioProps CreateODataComplexCScenario(HttpClient client, string baseUrl)
    {
        return Scenario.Create("odata_complex_c", async context =>
        {
            var url = ODataUrl(baseUrl,
                ("$filter", "YearsOfExperience gt 10 and (contains(DoctorName,'Cardiology') or contains(SpecialtyName,'Cardiology') or contains(Degree,'Cardiology'))"),
                ("$top", "10"),
                ("$skip", "0"));
            var request = Http.CreateRequest("GET", url);
            return await Http.Send(client, request);
        });
    }

    public static ScenarioProps CreateODataRandomWorkloadScenario(HttpClient client, string baseUrl)
    {
        var urls = new[]
        {
            ODataUrl(baseUrl),
            ODataUrl(baseUrl, ("$filter", "contains(DoctorName,'John') or contains(SpecialtyName,'John') or contains(Degree,'John')")),
            ODataUrl(baseUrl, ("$filter", "YearsOfExperience gt 10")),
            ODataUrl(baseUrl,
                ("$select", "DoctorId,DoctorName,SpecialtyName,YearsOfExperience,Rating"),
                ("$orderby", "SpecialtyName asc,Rating desc"),
                ("$top", "10"),
                ("$skip", "10")),
            ODataUrl(baseUrl,
                ("$select", "DoctorId,DoctorName,YearsOfExperience,Rating"),
                ("$filter", "YearsOfExperience gt 5 and Rating gt 3.0"),
                ("$orderby", "Rating desc"),
                ("$top", "5"),
                ("$skip", "0")),
            ODataUrl(baseUrl,
                ("$filter", "YearsOfExperience gt 10 and (contains(DoctorName,'Cardiology') or contains(SpecialtyName,'Cardiology') or contains(Degree,'Cardiology'))"),
                ("$top", "10"),
                ("$skip", "0"))
        };

        return Scenario.Create("odata_random_workload", async context =>
        {
            var idx = Random.Shared.Next(urls.Length);
            var request = Http.CreateRequest("GET", urls[idx]);
            return await Http.Send(client, request);
        });
    }

    public static ScenarioProps CreateODataMixedScenario(HttpClient client, string baseUrl)
    {
        var urls = new[]
        {
            ODataUrl(baseUrl),
            ODataUrl(baseUrl, ("$filter", "contains(DoctorName,'John') or contains(SpecialtyName,'John') or contains(Degree,'John')")),
            ODataUrl(baseUrl, ("$filter", "YearsOfExperience gt 10")),
            ODataUrl(baseUrl,
                ("$select", "DoctorId,DoctorName,SpecialtyName,YearsOfExperience,Rating"),
                ("$orderby", "SpecialtyName asc,Rating desc"),
                ("$top", "10"),
                ("$skip", "10")),
            ODataUrl(baseUrl,
                ("$select", "DoctorId,DoctorName,YearsOfExperience,Rating"),
                ("$filter", "YearsOfExperience gt 5 and Rating gt 3.0"),
                ("$orderby", "Rating desc"),
                ("$top", "5"),
                ("$skip", "0")),
            ODataUrl(baseUrl,
                ("$filter", "YearsOfExperience gt 10 and (contains(DoctorName,'Cardiology') or contains(SpecialtyName,'Cardiology') or contains(Degree,'Cardiology'))"),
                ("$top", "10"),
                ("$skip", "0"))
        };

        return Scenario.Create("odata_mixed_workload", async context =>
        {
            var idx = Random.Shared.Next(urls.Length);
            var request = Http.CreateRequest("GET", urls[idx]);
            return await Http.Send(client, request);
        });
    }
}
