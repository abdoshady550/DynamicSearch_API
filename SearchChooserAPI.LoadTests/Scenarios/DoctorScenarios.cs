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
}
