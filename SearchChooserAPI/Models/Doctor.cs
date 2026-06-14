namespace SearchChooserAPI.Models
{
    public class Doctor
    {
        public Guid Id { get; set; }
        
        public Guid SpecialtyId { get; set; }
        public Specialty Specialty { get; set; } = null!;

        public Guid DegreeId { get; set; }
        public Degree Degree { get; set; } = null!;

        public List<DoctorTranslation> DoctorTranslations { get; set; } = new();

        // Target search columns with different datatypes
        public int YearsOfExperience { get; set; }
        public decimal Rating { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime LastActive { get; set; }
    }
}
