using System.Text.Json.Serialization;

namespace TutorCopiloto.Services.Dto
{
    public class IntelligentAnalysisResponseDto
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("issues")]
        public List<string>? Issues { get; set; }

        [JsonPropertyName("recommendations")]
        public List<string>? Recommendations { get; set; }

        [JsonPropertyName("severity")]
        public string? Severity { get; set; }

        [JsonPropertyName("estimatedResolutionMinutes")]
        public int? EstimatedResolutionMinutes { get; set; }
    }
}
