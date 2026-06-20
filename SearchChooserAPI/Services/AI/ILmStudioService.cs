using SearchChooserAPI.Models.Req;

namespace SearchChooserAPI.Services.AI
{
    public interface ILmStudioService
    {
        Task<DoctorSearchRequest?> ConvertNaturalLanguageToQueryAsync(string userPrompt);
    }
}
