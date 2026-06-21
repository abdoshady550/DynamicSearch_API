using System.Text.Json.Nodes;
using SearchChooserAPI.IntegrationTests.Fixtures;

namespace SearchChooserAPI.IntegrationTests.Tests;

public sealed class DoctorsODataControllerTests : IClassFixture<SearchApiFactory>, IAsyncLifetime
{
    private readonly SearchApiFactory _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public DoctorsODataControllerTests(SearchApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    private async Task<(HttpResponseMessage Response, List<DoctorSearchResponse>? Data)> SendAsync(string query = "")
    {
        var url = "/odata/DoctorsOData" + query;
        var response = await _client.GetAsync(url);
        List<DoctorSearchResponse>? data = null;
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonNode.Parse(json);
            var valueArray = doc?["value"]?.AsArray();
            if (valueArray != null)
            {
                data = valueArray.Deserialize<List<DoctorSearchResponse>>(_jsonOptions);
            }
        }
        return (response, data);
    }

    private async Task<List<DoctorSearchResponse>> AssertSuccessAsync(string query = "", int? expectedCount = null)
    {
        var (response, data) = await SendAsync(query);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        data.Should().NotBeNull();
        if (expectedCount.HasValue)
            data!.Count.Should().Be(expectedCount.Value);
        return data!;
    }

    [Fact(DisplayName = "ODATA_HP1: Default request returns all 12 doctors with all properties")]
    public async Task DefaultRequest_ReturnsAllDoctors_WithAllProperties()
    {
        var data = await AssertSuccessAsync(expectedCount: 12);
        data.Should().AllSatisfy(d =>
        {
            d.DoctorId.Should().NotBeNullOrEmpty();
            d.DoctorName.Should().NotBeNull();
            d.SpecialtyName.Should().NotBeNull();
            d.Degree.Should().NotBeNull();
            d.YearsOfExperience.Should().BeGreaterThan(0);
            d.Rating.Should().BeGreaterThan(0);
            d.JoinDate.Should().BeAfter(new DateTime(2000, 1, 1));
            d.LastActive.Should().BeAfter(new DateTime(2020, 1, 1));
        });
    }

    [Fact(DisplayName = "ODATA_HP2: Select projection returns only requested columns plus key")]
    public async Task SelectProjection_ReturnsRequestedColumns()
    {
        var data = await AssertSuccessAsync("?$select=DoctorName,Rating", 12);
        data.Should().AllSatisfy(d =>
        {
            d.DoctorName.Should().NotBeNull();
            d.Rating.Should().BeGreaterThan(0);
            d.SpecialtyName.Should().BeNull();
            d.Degree.Should().BeNull();
            d.YearsOfExperience.Should().Be(0);
            d.JoinDate.Should().Be(default);
            d.LastActive.Should().Be(default);
        });
    }

    [Fact(DisplayName = "ODATA_HP3: Filter by Rating gt 4.0 returns doctors with rating above 4")]
    public async Task FilterByRatingGt_ReturnsCorrectSet()
    {
        var data = await AssertSuccessAsync("?$filter=Rating gt 4.0");
        data.Should().NotBeEmpty();
        data.Should().AllSatisfy(d => d.Rating.Should().BeGreaterThan(4.0m));
    }

    [Fact(DisplayName = "ODATA_HP4: Filter by Rating ge 4.0 and le 4.5 returns doctors in range")]
    public async Task FilterByRatingRange_ReturnsDoctorsInRange()
    {
        var data = await AssertSuccessAsync("?$filter=Rating ge 4.0 and Rating le 4.5");
        data.Should().NotBeEmpty();
        data.Should().AllSatisfy(d => d.Rating.Should().BeInRange(4.0m, 4.5m));
    }

    [Fact(DisplayName = "ODATA_HP5: Filter by SpecialtyName eq Cardiology returns correct doctors")]
    public async Task FilterBySpecialtyEq_ReturnsCorrectSet()
    {
        var data = await AssertSuccessAsync("?$filter=SpecialtyName eq 'Cardiology'");
        data.Should().HaveCount(2);
        data.Should().AllSatisfy(d => d.SpecialtyName.Should().Be("Cardiology"));
    }

    [Fact(DisplayName = "ODATA_HP6: Sort by Rating descending returns ordered results")]
    public async Task SortByRatingDescending_ReturnsOrderedResults()
    {
        var data = await AssertSuccessAsync("?$orderby=Rating desc", 12);
        data.Should().BeInDescendingOrder(d => d.Rating);
        data[0].Rating.Should().Be(4.9m);
        data[^1].Rating.Should().Be(3.2m);
    }

    [Fact(DisplayName = "ODATA_HP7: Sort by Rating ascending returns ordered results")]
    public async Task SortByRatingAscending_ReturnsOrderedResults()
    {
        var data = await AssertSuccessAsync("?$orderby=Rating asc", 12);
        data.Should().BeInAscendingOrder(d => d.Rating);
        data[0].Rating.Should().Be(3.2m);
        data[^1].Rating.Should().Be(4.9m);
    }

    [Fact(DisplayName = "ODATA_HP8: Paging with top and skip returns correct subset")]
    public async Task PagingWithTopSkip_ReturnsCorrectSubset()
    {
        var allData = await AssertSuccessAsync("?$orderby=Rating asc", 12);

        var pageData = await AssertSuccessAsync("?$orderby=Rating asc&$top=5&$skip=0", 5);
        pageData.Should().BeInAscendingOrder(d => d.Rating);
        pageData[0].Rating.Should().Be(allData[0].Rating);
        pageData[^1].Rating.Should().Be(allData[4].Rating);
    }

    [Fact(DisplayName = "ODATA_HP9: Filter by YearsOfExperience gt 15 returns senior doctors")]
    public async Task FilterByExperienceGt_ReturnsSeniorDoctors()
    {
        var data = await AssertSuccessAsync("?$filter=YearsOfExperience gt 15");
        data.Should().NotBeEmpty();
        data.Should().AllSatisfy(d => d.YearsOfExperience.Should().BeGreaterThan(15));
    }

    [Fact(DisplayName = "ODATA_HP10: Combined filter and sort returns correct results")]
    public async Task CombinedFilterAndSort_ReturnsCorrectResults()
    {
        var data = await AssertSuccessAsync("?$filter=SpecialtyName eq 'Cardiology'&$orderby=Rating desc", 2);
        data.Should().AllSatisfy(d => d.SpecialtyName.Should().Be("Cardiology"));
        data.Should().BeInDescendingOrder(d => d.Rating);
        data[0].Rating.Should().Be(4.8m);
        data[1].Rating.Should().Be(4.6m);
    }

    [Fact(DisplayName = "ODATA_HP11: Select with filter returns filtered projected results")]
    public async Task SelectWithFilter_ReturnsFilteredProjectedResults()
    {
        var data = await AssertSuccessAsync("?$select=DoctorName,Rating&$filter=Rating gt 4.5");
        data.Should().NotBeEmpty();
        data.Should().AllSatisfy(d =>
        {
            d.DoctorName.Should().NotBeNull();
            d.Rating.Should().BeGreaterThan(4.5m);
            d.SpecialtyName.Should().BeNull();
            d.Degree.Should().BeNull();
        });
    }

    [Fact(DisplayName = "ODATA_HP12: Sort by multiple columns returns correct order")]
    public async Task SortByMultipleColumns_ReturnsCorrectOrder()
    {
        var data = await AssertSuccessAsync("?$orderby=SpecialtyName asc, Rating desc", 12);
        data.Should().BeInAscendingOrder(d => d.SpecialtyName);
        var cardio = data.Where(d => d.SpecialtyName == "Cardiology").ToList();
        cardio.Should().BeInDescendingOrder(d => d.Rating);
    }

    [Fact(DisplayName = "ODATA_V1: Invalid filter value returns 400")]
    public async Task InvalidFilterValue_Returns400()
    {
        var (response, _) = await SendAsync("?$filter=Rating eq 'not-a-number'");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "ODATA_EC1: No matches returns empty list")]
    public async Task NoMatches_ReturnsEmpty()
    {
        var data = await AssertSuccessAsync("?$filter=DoctorName eq 'NonexistentXYZDoctor'", 0);
    }

    [Fact(DisplayName = "ODATA_EC2: Top with skip beyond results returns empty")]
    public async Task TopSkipBeyondResults_ReturnsEmpty()
    {
        var data = await AssertSuccessAsync("?$top=5&$skip=100", 0);
    }

    [Fact(DisplayName = "ODATA_EC3: Select single column returns key plus that column")]
    public async Task SelectSingleColumn_ReturnsKeyPlusThatColumn()
    {
        var data = await AssertSuccessAsync("?$select=DoctorName", 12);
        data.Should().AllSatisfy(d =>
        {
            d.DoctorName.Should().NotBeNull();
            d.SpecialtyName.Should().BeNull();
        });
    }
}
