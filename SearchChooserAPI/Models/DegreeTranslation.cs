namespace SearchChooserAPI.Models
{
    public class DegreeTranslation
    {
        public Guid Id { get; set; }
        public Guid DegreeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
    }
}
