using SearchChooserAPI.Models.Req;
using SearchChooserAPI.Models.Res;

namespace SearchChooserAPI.Services
{
    public interface IDoctorService
    {
        Task<IEnumerable<DoctorSearchResponse>> SearchDoctorsAsync(DoctorSearchRequest request);
    }
}
