namespace SearchChooserAPI.Models
{
    public class Specialty
    {
        public Guid Id { get; set; }
        public string SnomedCode { get; set; } = string.Empty;
        public List<SpecialtyTranslation> SpecialtyTranslations { get; set; } = new();
    }
}
