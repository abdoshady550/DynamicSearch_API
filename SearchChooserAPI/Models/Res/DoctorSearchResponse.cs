using SearchChooserAPI.Helper;

namespace SearchChooserAPI.Models.Res
{
    public class DoctorSearchResponse
    {
        [MandatoryColumn]
        public string DoctorId { get; set; } = string.Empty;

        public string? DoctorName { get; set; } 
        
        public string? SpecialtyName { get; set; } 
        
        public string? Degree { get; set; } 

        public int YearsOfExperience { get; set; }

        public decimal Rating { get; set; }

        public DateTime JoinDate { get; set; }

        public DateTime LastActive { get; set; }
    }
}
