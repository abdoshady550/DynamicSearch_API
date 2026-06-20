using System.Reflection;
using System.Text.RegularExpressions;
using SearchChooserAPI.Models.Req;
using SearchChooserAPI.Models.Res;

namespace SearchChooserAPI.Services.AI
{
    public static class QuerySanitizer
    {
        private static readonly HashSet<string> AllowedFields = typeof(DoctorSearchResponse)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        public static DoctorSearchRequest? Sanitize(DoctorSearchRequest? request)
        {
            if (request == null) return null;

            request.Search = SanitizeSearch(request.Search);
            request.Filters = SanitizeFilters(request.Filters);
            request.SortOptions = SanitizeSortOptions(request.SortOptions);
            request.PageNumber = Math.Max(1, request.PageNumber);
            request.PageSize = Math.Clamp(request.PageSize, 1, 100);

            return request;
        }

        private static string? SanitizeSearch(string? search)
        {
            if (string.IsNullOrWhiteSpace(search)) return null;

            search = Regex.Replace(search, @"\p{C}+", "");
            search = search.Trim();

            return search.Length > 200 ? search[..200] : search;
        }

        private static List<Meccano.DynamicQuery.FilterCriteria>? SanitizeFilters(List<Meccano.DynamicQuery.FilterCriteria>? filters)
        {
            if (filters == null || filters.Count == 0) return null;

            filters.RemoveAll(f =>
                !AllowedFields.Contains(f.ColumnName) ||
                !Enum.IsDefined(typeof(Meccano.DynamicQuery.FilterOperator), f.Operator));

            return filters.Count > 0 ? filters : null;
        }

        private static List<Meccano.DynamicQuery.SortOption>? SanitizeSortOptions(List<Meccano.DynamicQuery.SortOption>? sortOptions)
        {
            if (sortOptions == null || sortOptions.Count == 0) return null;

            sortOptions.RemoveAll(s => !AllowedFields.Contains(s.PropertyName));

            return sortOptions.Count > 0 ? sortOptions : null;
        }
    }
}
