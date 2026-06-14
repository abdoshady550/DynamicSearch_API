namespace SearchChooserAPI.Models
{
    public class Degree
    {
        public Guid Id { get; set; }
        public List<DegreeTranslation> DegreeTranslations { get; set; } = new();
    }
}
