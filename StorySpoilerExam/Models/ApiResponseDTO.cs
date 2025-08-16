using System.Text.Json.Serialization;


namespace StorySpoilerExam.Models
{
    internal class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("storyid")]
        public string? StoryId { get; set; }
    }
}
