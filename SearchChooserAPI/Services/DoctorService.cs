using Meccano.DynamicQuery;
using Microsoft.EntityFrameworkCore;
using SearchChooserAPI.Data;
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

        public IQueryable<DoctorSearchResponse> GetDoctorsQuery()
        {
            return _context.Doctors
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
                });
        }

        public async Task<PagedResult<DoctorSearchResponse>> SearchDoctorsAsync(DoctorSearchRequest request)
        {
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
                .ApplyDynamicQuery(request);

            var totalCount = await query.CountAsync();
            
            if (request.PageSize > 0){var skip = (request.PageNumber - 1) * request.PageSize;
            
                query = query.Skip(skip).Take(request.PageSize);}var items = await query.ToListAsync();
            
            return new PagedResult<DoctorSearchResponse>
            {  
                Items = items,
            
                TotalCount = totalCount,
            
                PageNumber = request.PageNumber,
            
                PageSize = request.PageSize > 0 ? request.PageSize : totalCount
            };
        }
    }
}
