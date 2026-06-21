using Meccano.DynamicQuery;
using SearchChooserAPI.Models.Req;
using SearchChooserAPI.Models.Res;

namespace SearchChooserAPI.Services
{
    public interface IDoctorService
    {
        Task<PagedResult<DoctorSearchResponse>> SearchDoctorsAsync(DoctorSearchRequest request);

        IQueryable<DoctorSearchResponse> GetDoctorsQuery();
    }
}
