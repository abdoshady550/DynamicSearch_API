using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using SearchChooserAPI.Models.Req;

namespace SearchChooserAPI.Services.AI
{
    public class LmStudioService : ILmStudioService
    {
        private static readonly string SystemPrompt =
            """
            You are a query builder. Convert the user's natural language request into a JSON object.

            You must ONLY return valid JSON. No explanation. No markdown.

            Available fields: DoctorId, DoctorName, SpecialtyName, Degree, YearsOfExperience, Rating, JoinDate, LastActive.
            Available filter operators: Eq (0), Neq (1), Gt (2), Lt (3), Range (4).

            Rules:
            - Extract search keywords into "search".
            - Extract filtering conditions into "filters" (array of {columnName, operator, value, value2?}).
            - Extract sorting requirements into "sortOptions" (array of {propertyName, isDescending}).
            - If no page number is specified, use 1.
            - If no page size is specified, use 10.
            - If the user asks for top/best/highest rated, sort by Rating descending.
            - If the user asks for cheapest, sort by ConsultationPrice ascending.
            - If the user asks for newest, sort by CreatedAt descending.
            - Never invent fields outside the allowed list.
            - Return JSON only.
            """;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly HttpClient _httpClient;
        private readonly LmStudioOptions _options;

        public LmStudioService(HttpClient httpClient, IOptions<LmStudioOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<DoctorSearchRequest?> ConvertNaturalLanguageToQueryAsync(string userPrompt)
        {
            var requestBody = new
            {
                model = _options.Model,
                messages = new[]
                {
                    new { role = "system", content = SystemPrompt },
                    new { role = "user", content = userPrompt }
                },
                max_tokens = _options.MaxTokens,
                temperature = _options.Temperature
            };

            var json = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/v1/chat/completions", httpContent);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            var chatResponse = JsonSerializer.Deserialize<LmChatResponse>(responseJson, JsonOptions);
            var content = chatResponse?.Choices?.FirstOrDefault()?.Message?.Content;

            if (string.IsNullOrWhiteSpace(content)) return null;

            var rawRequest = JsonSerializer.Deserialize<RawAiQueryResponse>(content, JsonOptions);
            if (rawRequest == null) return null;

            var request = new DoctorSearchRequest
            {
                Search = rawRequest.Search,
                PageNumber = rawRequest.PageNumber,
                PageSize = rawRequest.PageSize,
                Filters = rawRequest.Filters?
                    .Select(f => new Meccano.DynamicQuery.FilterCriteria
                    {
                        ColumnName = f.ColumnName ?? string.Empty,
                        Operator = Enum.IsDefined(typeof(Meccano.DynamicQuery.FilterOperator), f.Operator)
                            ? f.Operator
                            : Meccano.DynamicQuery.FilterOperator.Eq,
                        Value = NormalizeFilterValue(f.Value),
                        Value2 = NormalizeFilterValue(f.Value2)
                    })
                    .ToList(),
                SortOptions = rawRequest.SortOptions?
                    .Select(s => new Meccano.DynamicQuery.SortOption
                    {
                        PropertyName = s.PropertyName ?? string.Empty,
                        IsDescending = s.IsDescending
                    })
                    .ToList()
            };

            return QuerySanitizer.Sanitize(request);
        }

        private static string NormalizeFilterValue(JsonElement? element)
        {
            if (element == null) return string.Empty;
            return element.Value.ValueKind switch
            {
                JsonValueKind.String => element.Value.GetString() ?? string.Empty,
                JsonValueKind.Number => element.Value.GetRawText(),
                _ => string.Empty
            };
        }

        private class RawAiQueryResponse
        {
            public string? Search { get; set; }
            public List<RawFilterCriteria>? Filters { get; set; }
            public List<RawSortOption>? SortOptions { get; set; }
            public int PageNumber { get; set; } = 1;
            public int PageSize { get; set; } = 10;
        }

        private class RawFilterCriteria
        {
            public string? ColumnName { get; set; }
            public Meccano.DynamicQuery.FilterOperator Operator { get; set; }
            public JsonElement? Value { get; set; }
            public JsonElement? Value2 { get; set; }
        }

        private class RawSortOption
        {
            public string? PropertyName { get; set; }
            public bool IsDescending { get; set; }
        }

        private class LmChatResponse
        {
            public List<LmChoice>? Choices { get; set; }
        }

        private class LmChoice
        {
            public LmMessage? Message { get; set; }
        }

        private class LmMessage
        {
            public string? Content { get; set; }
        }
    }
}
