namespace SearchChooserAPI.Services.AI
{
    public class LmStudioOptions
    {
        public string BaseUrl { get; set; } = "http://localhost:1234";
        public string Model { get; set; } = "gemma-4-26b-a4b-it";
        public int MaxTokens { get; set; } = 512;
        public double Temperature { get; set; } = 0.1;
        public int TimeoutSeconds { get; set; } = 30;
    }
}
