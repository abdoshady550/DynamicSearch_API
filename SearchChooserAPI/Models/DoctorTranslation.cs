namespace SearchChooserAPI.Models
{
    public class DoctorTranslation
    {
        public Guid Id { get; set; }
        public Guid DoctorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
    }
}
