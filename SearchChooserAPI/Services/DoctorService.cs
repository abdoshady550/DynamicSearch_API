using Microsoft.EntityFrameworkCore;
using SearchChooserAPI.Data;
using SearchChooserAPI.Helper;
using SearchChooserAPI.Models.Req;
using SearchChooserAPI.Models.Res;

namespace SearchChooserAPI.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly SearchDbContext _context;

        public DoctorService(SearchDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DoctorSearchResponse>> SearchDoctorsAsync(DoctorSearchRequest request)
        {
            var columns = request.Columns?.Select(c => c.Trim()).ToList()
                ?? typeof(DoctorSearchResponse).GetProperties().Select(p => p.Name).ToList();

            var query = _context.Doctors
                .Select(d => new DoctorSearchResponse
                {
                    DoctorId = d.Id.ToString(),
                    DoctorName = d.DoctorTranslations.Where(t => t.Language == "en").Select(t => t.Name).FirstOrDefault(),
                    SpecialtyName = d.Specialty.SpecialtyTranslations.Where(t => t.Language == "en").Select(t => t.Name).FirstOrDefault(),
                    Degree = d.Degree.DegreeTranslations.Where(t => t.Language == "en").Select(t => t.Name).FirstOrDefault(),
                    YearsOfExperience = d.YearsOfExperience,
                    Rating = d.Rating,
                    JoinDate = d.JoinDate,
                    LastActive = d.LastActive
                })
                .SelectColumns(columns, request.Mode);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.SearchColumns(request.Search);
            }

            return await query.ToListAsync();
        }
    }
}
