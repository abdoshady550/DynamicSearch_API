using Microsoft.AspNetCore.Mvc;
using SearchChooserAPI.Models.Req;
using SearchChooserAPI.Services.AI;

namespace SearchChooserAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiQueryController : ControllerBase
    {
        private readonly ILmStudioService _lmStudioService;

        public AiQueryController(ILmStudioService lmStudioService)
        {
            _lmStudioService = lmStudioService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(DoctorSearchRequest), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> ConvertToQuery([FromBody] AiQueryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return BadRequest(new { error = "Prompt cannot be empty." });

            if (request.Prompt.Length > 500)
                return BadRequest(new { error = "Prompt must be 500 characters or fewer." });

            try
            {
                var result = await _lmStudioService.ConvertNaturalLanguageToQueryAsync(request.Prompt);

                if (result == null)
                    return BadRequest(new { error = "Failed to parse query from AI response." });

                return Ok(result);
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new { error = "LM Studio is unreachable. Ensure it is running on http://localhost:1234." });
            }
        }
    }
}
