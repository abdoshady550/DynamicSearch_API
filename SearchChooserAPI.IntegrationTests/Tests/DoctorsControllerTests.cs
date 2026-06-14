using SearchChooserAPI.IntegrationTests.Fixtures;

namespace SearchChooserAPI.IntegrationTests.Tests;

public sealed class DoctorsControllerTests : IClassFixture<SearchApiFactory>, IAsyncLifetime
{
    private readonly SearchApiFactory _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public DoctorsControllerTests(SearchApiFactory factory)
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

    private async Task<(HttpResponseMessage Response, List<DoctorSearchResponse>? Data)> SendAsync(
        DoctorSearchRequest request)
    {
        var content = JsonContent.Create(request, options: new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var response = await _client.PostAsync("/api/Doctors", content);
        List<DoctorSearchResponse>? data = null;
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            data = JsonSerializer.Deserialize<List<DoctorSearchResponse>>(json, _jsonOptions);
        }
        return (response, data);
    }

    private async Task<List<DoctorSearchResponse>> AssertSuccessAsync(
        DoctorSearchRequest request, int? expectedCount = null)
    {
        var (response, data) = await SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        data.Should().NotBeNull();
        if (expectedCount.HasValue)
            data!.Count.Should().Be(expectedCount.Value);
        return data!;
    }

    private async Task AssertBadRequestAsync(DoctorSearchRequest request, string? errorContains = null)
    {
        var content = JsonContent.Create(request, options: new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var response = await _client.PostAsync("/api/Doctors", content);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        if (errorContains != null)
        {
            var body = await response.Content.ReadAsStringAsync();
            body.ToLower().Should().Contain(errorContains.ToLower());
        }
    }

    // ========================================================================
    // HAPPY PATH
    // ========================================================================

    [Fact(DisplayName = "HP1: Default request returns all 12 doctors with all properties")]
    public async Task DefaultRequest_ReturnsAllDoctors_WithAllProperties()
    {
        var data = await AssertSuccessAsync(new DoctorSearchRequest(), 12);
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

    [Fact(DisplayName = "HP2: Include mode returns only requested columns plus mandatory DoctorId")]
    public async Task IncludeMode_ReturnsOnlyRequestedColumns()
    {
        var request = new DoctorSearchRequestBuilder()
            .WithColumns("DoctorName", "Rating")
            .WithMode(ColumnMode.Include);

        var data = await AssertSuccessAsync(request, 12);
        data.Should().AllSatisfy(d =>
        {
            d.DoctorId.Should().NotBeNullOrEmpty();
            d.DoctorName.Should().NotBeNull();
            d.Rating.Should().BeGreaterThan(0);
            d.SpecialtyName.Should().BeNull();
            d.Degree.Should().BeNull();
            d.YearsOfExperience.Should().Be(0);
            d.JoinDate.Should().Be(default);
            d.LastActive.Should().Be(default);
        });
    }

    [Fact(DisplayName = "HP3: Exclude mode returns all columns except the excluded ones")]
    public async Task ExcludeMode_ExcludesSpecifiedColumns()
    {
        var request = new DoctorSearchRequestBuilder()
            .WithColumns("Rating", "LastActive")
            .WithMode(ColumnMode.Exclude);

        var data = await AssertSuccessAsync(request, 12);
        data.Should().AllSatisfy(d =>
        {
            d.DoctorId.Should().NotBeNullOrEmpty();
            d.DoctorName.Should().NotBeNull();
            d.SpecialtyName.Should().NotBeNull();
            d.Degree.Should().NotBeNull();
            d.YearsOfExperience.Should().BeGreaterThan(0);
            d.JoinDate.Should().BeAfter(new DateTime(2000, 1, 1));
            d.Rating.Should().Be(0);
            d.LastActive.Should().Be(default);
        });
    }

    [Fact(DisplayName = "HP4: Single column Include returns DoctorId plus that column")]
    public async Task SingleColumnInclude_ReturnsMandatoryPlusOne()
    {
        var request = new DoctorSearchRequestBuilder()
            .WithColumns("DoctorName")
            .WithMode(ColumnMode.Include);

        var data = await AssertSuccessAsync(request, 12);
        data.Should().AllSatisfy(d =>
        {
            d.DoctorId.Should().NotBeNullOrEmpty();
            d.DoctorName.Should().NotBeNull();
            d.SpecialtyName.Should().BeNull();
            d.Degree.Should().BeNull();
            d.YearsOfExperience.Should().Be(0);
            d.Rating.Should().Be(0);
            d.JoinDate.Should().Be(default);
            d.LastActive.Should().Be(default);
        });
    }

    [Fact(DisplayName = "HP5: Search by exact doctor name returns single match")]
    public async Task SearchByName_ReturnsExactMatch()
    {
        var request = new DoctorSearchRequestBuilder().WithSearch("Dr. Sarah Jenkins");
        var data = await AssertSuccessAsync(request, 1);
        data[0].DoctorName.Should().Be("Dr. Sarah Jenkins");
        data[0].SpecialtyName.Should().Be("Cardiology");
        data[0].YearsOfExperience.Should().Be(15);
        data[0].Rating.Should().Be(4.8m);
    }

    [Fact(DisplayName = "HP6: Search by partial name fragment returns matches")]
    public async Task SearchByPartialName_ReturnsMatches()
    {
        var request = new DoctorSearchRequestBuilder().WithSearch("Jen");
        var data = await AssertSuccessAsync(request, 1);
        data[0].DoctorName.Should().Contain("Jenkins");
    }

    [Fact(DisplayName = "HP7: Search across selected columns finds numeric matches")]
    public async Task SearchByNumericValue_FindsMatches()
    {
        var request = new DoctorSearchRequestBuilder().WithSearch("15");
        var data = await AssertSuccessAsync(request);
        data.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "HP8: Search by specialty name returns matching doctors")]
    public async Task SearchBySpecialty_ReturnsMatchingDoctors()
    {
        var request = new DoctorSearchRequestBuilder().WithSearch("Cardiology");
        var data = await AssertSuccessAsync(request);
        data.Should().NotBeEmpty();
        data.Should().AllSatisfy(d => d.SpecialtyName.Should().Contain("Cardiology"));
    }

    [Fact(DisplayName = "HP9: Filter by Eq on SpecialtyName returns correct doctors")]
    public async Task FilterByEq_OnSpecialty_ReturnsCorrectSet()
    {
        var request = new DoctorSearchRequestBuilder()
            .AddFilter("SpecialtyName", FilterOperator.Eq, "Cardiology");
        var data = await AssertSuccessAsync(request);
        data.Should().HaveCount(2);
        data.Should().AllSatisfy(d => d.SpecialtyName.Should().Be("Cardiology"));
    }

    [Fact(DisplayName = "HP10: Filter by Gt on YearsOfExperience returns senior doctors")]
    public async Task FilterByGt_OnExperience_ReturnsSeniorDoctors()
    {
        var request = new DoctorSearchRequestBuilder()
            .AddFilter("YearsOfExperience", FilterOperator.Gt, "15");
        var data = await AssertSuccessAsync(request);
        data.Should().NotBeEmpty();
        data.Should().AllSatisfy(d => d.YearsOfExperience.Should().BeGreaterThan(15));
    }

    [Fact(DisplayName = "HP11: Filter by Lt on YearsOfExperience returns junior doctors")]
    public async Task FilterByLt_OnExperience_ReturnsJuniorDoctors()
    {
        var request = new DoctorSearchRequestBuilder()
            .AddFilter("YearsOfExperience", FilterOperator.Lt, "10");
        var data = await AssertSuccessAsync(request);
        data.Should().NotBeEmpty();
        data.Should().AllSatisfy(d => d.YearsOfExperience.Should().BeLessThan(10));
    }

    [Fact(DisplayName = "HP12: Filter by Range on Rating returns doctors in range")]
    public async Task FilterByRange_OnRating_ReturnsDoctorsInRange()
    {
        var request = new DoctorSearchRequestBuilder()
            .AddFilter("Rating", FilterOperator.Range, "4.0", "4.5");
        var data = await AssertSuccessAsync(request);
        data.Should().NotBeEmpty();
        data.Should().AllSatisfy(d => d.Rating.Should().BeInRange(4.0m, 4.5m));
    }

    [Fact(DisplayName = "HP13: Filter by Neq on Degree excludes matching doctors")]
    public async Task FilterByNeq_OnDegree_ExcludesMatching()
    {
        var request = new DoctorSearchRequestBuilder()
            .AddFilter("Degree", FilterOperator.Neq, "PhD in Immunology");
        var data = await AssertSuccessAsync(request);
        data.Should().NotBeEmpty();
        data.Should().AllSatisfy(d => d.Degree.Should().NotBe("PhD in Immunology"));
    }

    [Fact(DisplayName = "HP14: Combined search and filter narrows results correctly")]
    public async Task CombinedSearchAndFilter_NarrowsResults()
    {
        var request = new DoctorSearchRequestBuilder()
            .WithSearch("Dr.")
            .AddFilter("YearsOfExperience", FilterOperator.Gt, "10");
        var data = await AssertSuccessAsync(request);
        data.Should().NotBeEmpty();
        data.Should().AllSatisfy(d => d.YearsOfExperience.Should().BeGreaterThan(10));
    }

    [Fact(DisplayName = "HP15: Include mode search scans projected columns only")]
    public async Task IncludeModeSearch_ScansProjectedColumnsOnly()
    {
        var request = new DoctorSearchRequestBuilder()
            .WithColumns("DoctorName")
            .WithMode(ColumnMode.Include)
            .WithSearch("Dr.");
        var data = await AssertSuccessAsync(request);
        data.Should().NotBeEmpty();
        data.Should().AllSatisfy(d =>
        {
            d.DoctorId.Should().NotBeNullOrEmpty();
            d.DoctorName.Should().NotBeNull();
            d.SpecialtyName.Should().BeNull();
        });
    }

    // ========================================================================
    // VALIDATION
    // ========================================================================

    [Fact(DisplayName = "V1: Invalid column name in Include mode returns 400")]
    public async Task InvalidColumn_IncludeMode_Returns400()
    {
        await AssertBadRequestAsync(
            new DoctorSearchRequestBuilder()
                .WithColumns("FakeColumn")
                .WithMode(ColumnMode.Include),
            errorContains: "FakeColumn");
    }

    [Fact(DisplayName = "V2: Invalid column name in Exclude mode returns 400")]
    public async Task InvalidColumn_ExcludeMode_Returns400()
    {
        await AssertBadRequestAsync(
            new DoctorSearchRequestBuilder()
                .WithColumns("FakeColumn")
                .WithMode(ColumnMode.Exclude),
            errorContains: "FakeColumn");
    }

    [Fact(DisplayName = "V3: Multiple invalid columns all reported in error message")]
    public async Task MultipleInvalidColumns_ReturnsAllInMessage()
    {
        await AssertBadRequestAsync(
            new DoctorSearchRequestBuilder()
                .WithColumns("A", "B", "C")
                .WithMode(ColumnMode.Include),
            errorContains: "A, B, C");
    }

    [Fact(DisplayName = "V4: Empty columns array returns all doctors")]
    public async Task EmptyColumnsArray_ReturnsAllDoctors()
    {
        await AssertSuccessAsync(new DoctorSearchRequestBuilder().WithColumns(), 12);
    }

    [Fact(DisplayName = "V5: Whitespace column name triggers invalid column error")]
    public async Task WhitespaceColumnName_Returns400()
    {
        await AssertBadRequestAsync(
            new DoctorSearchRequestBuilder()
                .WithColumns(" ")
                .WithMode(ColumnMode.Include),
            errorContains: "Invalid column");
    }

    // ========================================================================
    // BUSINESS RULES
    // ========================================================================

    [Fact(DisplayName = "BR1: MandatoryColumn DoctorId always present even when excluded")]
    public async Task MandatoryColumn_AlwaysPresent_WhenExcluded()
    {
        var request = new DoctorSearchRequestBuilder()
            .WithColumns("DoctorId")
            .WithMode(ColumnMode.Exclude);

        var data = await AssertSuccessAsync(request, 12);
        data.Should().AllSatisfy(d => d.DoctorId.Should().NotBeNullOrEmpty());
    }

    [Fact(DisplayName = "BR2: Default Mode is Include when not specified")]
    public async Task DefaultMode_IsInclude()
    {
        var request = new DoctorSearchRequestBuilder().WithColumns("DoctorName");
        var data = await AssertSuccessAsync(request, 12);
        data.Should().AllSatisfy(d =>
        {
            d.DoctorId.Should().NotBeNullOrEmpty();
            d.DoctorName.Should().NotBeNull();
            d.SpecialtyName.Should().BeNull();
        });
    }

    [Fact(DisplayName = "BR3: Column names are case-insensitive")]
    public async Task ColumnNames_AreCaseInsensitive()
    {
        var request = new DoctorSearchRequestBuilder()
            .WithColumns("DOCTORNAME", "rating")
            .WithMode(ColumnMode.Include);

        var data = await AssertSuccessAsync(request, 12);
        data.Should().AllSatisfy(d =>
        {
            d.DoctorId.Should().NotBeNullOrEmpty();
            d.DoctorName.Should().NotBeNull();
            d.Rating.Should().BeGreaterThan(0);
            d.SpecialtyName.Should().BeNull();
        });
    }

    [Fact(DisplayName = "BR4: Search is case-insensitive")]
    public async Task Search_IsCaseInsensitive()
    {
        var request = new DoctorSearchRequestBuilder().WithSearch("SARAH");
        var data = await AssertSuccessAsync(request, 1);
        data[0].DoctorName.Should().Be("Dr. Sarah Jenkins");
    }

    // ========================================================================
    // DATABASE STATE
    // ========================================================================

    [Fact(DisplayName = "DS1: Response count matches database record count")]
    public async Task ResponseCount_MatchesDatabaseCount()
    {
        await AssertSuccessAsync(new DoctorSearchRequest(), 12);
    }

    [Fact(DisplayName = "DS2: Specific doctor data matches seeded values")]
    public async Task SpecificDoctor_DataMatchesSeed()
    {
        var request = new DoctorSearchRequestBuilder().WithSearch("Dr. Emily Chen");
        var data = await AssertSuccessAsync(request, 1);
        var emily = data[0];
        emily.DoctorName.Should().Be("Dr. Emily Chen");
        emily.SpecialtyName.Should().Be("Neurology");
        emily.YearsOfExperience.Should().Be(22);
        emily.Rating.Should().Be(4.9m);
        emily.JoinDate.Should().Be(new DateTime(2004, 3, 10));
        emily.LastActive.Should().Be(new DateTime(2026, 6, 11, 8, 0, 0));
    }

    [Fact(DisplayName = "DS3: Excluded columns absent from deserialized response")]
    public async Task ExcludedColumns_AreAbsentFromResponse()
    {
        var request = new DoctorSearchRequestBuilder()
            .WithColumns("Rating")
            .WithMode(ColumnMode.Exclude);

        var data = await AssertSuccessAsync(request);
        data.Should().NotBeEmpty();
        data.Should().AllSatisfy(d =>
        {
            d.DoctorId.Should().NotBeNullOrEmpty();
            d.Rating.Should().Be(0m);
            d.DoctorName.Should().NotBeNull();
        });
    }

    // ========================================================================
    // EDGE CASES
    // ========================================================================

    [Fact(DisplayName = "EC1: Empty search string returns all doctors")]
    public async Task SearchEmptyString_ReturnsAll()
    {
        await AssertSuccessAsync(new DoctorSearchRequestBuilder().WithSearch(""), 12);
    }

    [Fact(DisplayName = "EC2: Whitespace-only search returns all doctors")]
    public async Task SearchWhitespace_ReturnsAll()
    {
        await AssertSuccessAsync(new DoctorSearchRequestBuilder().WithSearch("   "), 12);
    }

    [Fact(DisplayName = "EC3: Search with special characters like apostrophes works")]
    public async Task SearchSpecialChars_MatchesCorrectly()
    {
        var data = await AssertSuccessAsync(
            new DoctorSearchRequestBuilder().WithSearch("O'Brien"), 1);
        data[0].DoctorName.Should().Be("Dr. Margaret O'Brien");
    }

    [Fact(DisplayName = "EC4: Search with no matches returns empty list")]
    public async Task SearchNoMatches_ReturnsEmpty()
    {
        await AssertSuccessAsync(
            new DoctorSearchRequestBuilder().WithSearch("NonexistentXYZDoctor"), 0);
    }

    [Fact(DisplayName = "EC5: Filter on non-existent column silently ignored")]
    public async Task FilterNonExistentColumn_SilentlyIgnored()
    {
        await AssertSuccessAsync(
            new DoctorSearchRequestBuilder()
                .AddFilter("NonExistentColumn", FilterOperator.Eq, "value"),
            12);
    }

    [Fact(DisplayName = "EC6: Filter with incompatible type silently skipped")]
    public async Task FilterIncompatibleType_SilentlyIgnored()
    {
        await AssertSuccessAsync(
            new DoctorSearchRequestBuilder()
                .AddFilter("Rating", FilterOperator.Eq, "not-a-number"),
            12);
    }

    [Fact(DisplayName = "EC12: Filter with empty column name is skipped and returns all")]
    public async Task Search_FilterWithEmptyColumnName_IsSkipped_ReturnsAll()
    {
        await AssertSuccessAsync(
            new DoctorSearchRequestBuilder()
                .AddFilter("", FilterOperator.Eq, "value"),
            12);
    }

    [Fact(DisplayName = "EC13: Multiple filters are combined with AND logic")]
    public async Task Filter_MultipleFilters_AreAnded()
    {
        var request = new DoctorSearchRequestBuilder()
            .AddFilter("YearsOfExperience", FilterOperator.Gt, "10")
            .AddFilter("SpecialtyName", FilterOperator.Eq, "Cardiology");
        var data = await AssertSuccessAsync(request);
        data.Should().NotBeEmpty();
        data.Should().AllSatisfy(d => d.YearsOfExperience.Should().BeGreaterThan(10));
        data.Should().AllSatisfy(d => d.SpecialtyName.Should().Be("Cardiology"));
    }


    [Fact(DisplayName = "EC7: Very long search string handled without error")]
    public async Task VeryLongSearch_HandledGracefully()
    {
        var request = new DoctorSearchRequestBuilder().WithSearch(new string('a', 1000));
        var (response, data) = await SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        data.Should().NotBeNull();
    }

    [Fact(DisplayName = "EC8: Filter by DoctorId resolves single doctor")]
    public async Task FilterByDoctorId_ReturnsSingleMatch()
    {
        var all = await AssertSuccessAsync(new DoctorSearchRequest(), 12);
        var ahmed = all.First(d => d.DoctorName == "Dr. Ahmed Hassan");

        var data = await AssertSuccessAsync(
            new DoctorSearchRequestBuilder()
                .AddFilter("DoctorId", FilterOperator.Eq, ahmed.DoctorId),
            1);
        data[0].DoctorId.Should().Be(ahmed.DoctorId);
    }

    [Fact(DisplayName = "EC9: All columns explicitly listed in Include matches default")]
    public async Task AllColumnsExplicitlyIncluded_MatchesDefault()
    {
        var defaultData = await AssertSuccessAsync(new DoctorSearchRequest(), 12);

        var request = new DoctorSearchRequestBuilder()
            .WithColumns("DoctorId", "DoctorName", "SpecialtyName", "Degree",
                         "YearsOfExperience", "Rating", "JoinDate", "LastActive")
            .WithMode(ColumnMode.Include);

        var explicitData = await AssertSuccessAsync(request, 12);
        explicitData.Should().BeEquivalentTo(defaultData);
    }

    [Fact(DisplayName = "EC10: Exclude all non-mandatory returns only DoctorId")]
    public async Task ExcludeAllNonMandatory_ReturnsOnlyDoctorId()
    {
        var request = new DoctorSearchRequestBuilder()
            .WithColumns("DoctorName", "SpecialtyName", "Degree",
                         "YearsOfExperience", "Rating", "JoinDate", "LastActive")
            .WithMode(ColumnMode.Exclude);

        var data = await AssertSuccessAsync(request, 12);
        data.Should().AllSatisfy(d =>
        {
            d.DoctorId.Should().NotBeNullOrEmpty();
            d.DoctorName.Should().BeNull();
            d.SpecialtyName.Should().BeNull();
            d.Degree.Should().BeNull();
            d.YearsOfExperience.Should().Be(0);
            d.Rating.Should().Be(0);
            d.JoinDate.Should().Be(default);
            d.LastActive.Should().Be(default);
        });
    }

    [Fact(DisplayName = "EC11: Include mode with all columns explicitly listed")]
    public async Task IncludeWithAll_ReturnsAll()
    {
        var request = new DoctorSearchRequestBuilder()
            .WithColumns("DoctorName", "SpecialtyName", "Degree",
                         "YearsOfExperience", "Rating", "JoinDate", "LastActive",
                         "DoctorId")
            .WithMode(ColumnMode.Include);

        var data = await AssertSuccessAsync(request, 12);
        data.Should().AllSatisfy(d =>
        {
            d.DoctorId.Should().NotBeNullOrEmpty();
            d.DoctorName.Should().NotBeNull();
        });
    }

    // ========================================================================
    // FAILURE SCENARIOS
    // ========================================================================

    [Fact(DisplayName = "FS1: Malformed JSON returns 400")]
    public async Task MalformedJson_Returns400()
    {
        var content = new StringContent("{invalid json}", System.Text.Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/Doctors", content);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "FS2: Wrong content type returns 415")]
    public async Task WrongContentType_Returns415()
    {
        var content = new StringContent("plain text", System.Text.Encoding.UTF8, "text/plain");
        var response = await _client.PostAsync("/api/Doctors", content);
        response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
    }

    [Fact(DisplayName = "FS3: Null request body returns 400")]
    public async Task NullBody_Returns400()
    {
        var content = new StringContent("null", System.Text.Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/Doctors", content);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ========================================================================
    // CONCURRENCY
    // ========================================================================

    [Fact(DisplayName = "C1: 10 simultaneous requests all succeed with consistent results")]
    public async Task ConcurrentRequests_AllSucceed_ConsistentResults()
    {
        var tasks = Enumerable.Range(0, 10).Select(_ => SendAsync(new DoctorSearchRequest()));
        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(r =>
        {
            r.Response.StatusCode.Should().Be(HttpStatusCode.OK);
            r.Data.Should().HaveCount(12);
        });
    }

    [Fact(DisplayName = "C2: Sequential varying requests return correct individual results")]
    public async Task SequentialVaryingRequests_ReturnCorrectResults()
    {
        var r1 = await SendAsync(new DoctorSearchRequestBuilder().WithSearch("Cardiology"));
        r1.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        r1.Data.Should().AllSatisfy(d => d.SpecialtyName.Should().Contain("Cardiology"));

        var r2 = await SendAsync(new DoctorSearchRequestBuilder()
            .WithColumns("DoctorName")
            .WithMode(ColumnMode.Include));
        r2.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        r2.Data.Should().AllSatisfy(d =>
        {
            d.DoctorName.Should().NotBeNull();
            d.SpecialtyName.Should().BeNull();
        });

        var r3 = await SendAsync(new DoctorSearchRequestBuilder()
            .AddFilter("YearsOfExperience", FilterOperator.Gt, "20"));
        r3.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        r3.Data.Should().AllSatisfy(d => d.YearsOfExperience.Should().BeGreaterThan(20));
    }

    // ========================================================================
    // SECURITY
    // ========================================================================

    [Fact(DisplayName = "S1: SQL injection attempt in search returns no crash")]
    public async Task SqlInjectionSearch_NoCrash()
    {
        var (response, data) = await SendAsync(
            new DoctorSearchRequestBuilder().WithSearch("'; DROP TABLE Doctors; --"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        data.Should().NotBeNull();
    }

    [Fact(DisplayName = "S2: XSS script in search returns no crash")]
    public async Task XssScriptSearch_NoCrash()
    {
        var (response, data) = await SendAsync(
            new DoctorSearchRequestBuilder().WithSearch("<script>alert('xss')</script>"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        data.Should().NotBeNull();
    }

    [Fact(DisplayName = "S3: Many invalid columns returns 400")]
    public async Task ManyInvalidColumns_Returns400()
    {
        var columns = Enumerable.Range(0, 100).Select(i => $"Column{i}").ToArray();
        var (response, _) = await SendAsync(
            new DoctorSearchRequestBuilder()
                .WithColumns(columns)
                .WithMode(ColumnMode.Include));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "S4: Duplicate column names deduplicated")]
    public async Task DuplicateColumns_Deduplicated()
    {
        var request = new DoctorSearchRequestBuilder()
            .WithColumns("DoctorName", "DoctorName", "DOCTORNAME", "Rating")
            .WithMode(ColumnMode.Include);

        var data = await AssertSuccessAsync(request, 12);
        data.Should().AllSatisfy(d =>
        {
            d.DoctorId.Should().NotBeNullOrEmpty();
            d.DoctorName.Should().NotBeNull();
            d.Rating.Should().BeGreaterThan(0);
        });
    }
}
