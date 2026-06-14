using Microsoft.EntityFrameworkCore;
using SearchChooserAPI.Models;

namespace SearchChooserAPI.Data
{
    public class SearchDbContext : DbContext
    {
        public SearchDbContext(DbContextOptions<SearchDbContext> options) : base(options)
        {
        }

        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<DoctorTranslation> DoctorTranslations => Set<DoctorTranslation>();
        public DbSet<Specialty> Specialties => Set<Specialty>();
        public DbSet<SpecialtyTranslation> SpecialtyTranslations => Set<SpecialtyTranslation>();
        public DbSet<Degree> Degrees => Set<Degree>();
        public DbSet<DegreeTranslation> DegreeTranslations => Set<DegreeTranslation>();


    }
}
