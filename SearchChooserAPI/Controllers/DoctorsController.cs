using Microsoft.AspNetCore.Mvc;
using SearchChooserAPI.Models.Req;
using SearchChooserAPI.Models.Res;
using SearchChooserAPI.Services;

namespace SearchChooserAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(IEnumerable<DoctorSearchResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDoctors([FromBody] DoctorSearchRequest request)
        {
            try
            {
                var results = await _doctorService.SearchDoctorsAsync(request);
                return Ok(results);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
