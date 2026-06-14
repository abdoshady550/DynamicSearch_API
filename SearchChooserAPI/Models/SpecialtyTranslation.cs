namespace SearchChooserAPI.Models
{
    public class SpecialtyTranslation
    {
        public Guid Id { get; set; }
        public Guid SpecialtyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
    }
}
