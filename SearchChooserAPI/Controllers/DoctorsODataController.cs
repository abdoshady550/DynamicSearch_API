using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using SearchChooserAPI.Models.Res;
using SearchChooserAPI.Services;

namespace SearchChooserAPI.Controllers
{
    [ApiExplorerSettings(IgnoreApi = false)]
    public class DoctorsODataController : ODataController
    {
        private readonly IDoctorService _doctorService;

        public DoctorsODataController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [EnableQuery]
        public IQueryable<DoctorSearchResponse> Get()
        {
            return _doctorService.GetDoctorsQuery();
        }
    }
}
