using SearchChooserAPI.Models;

namespace SearchChooserAPI.Models.Req
{
    public class DoctorSearchRequest
    {
        public List<string>? Columns { get; set; }
        public ColumnMode Mode { get; set; } = ColumnMode.Include;
        public string? Search { get; set; }
        public List<FilterCriteria>? Filters { get; set; }

    }
}
