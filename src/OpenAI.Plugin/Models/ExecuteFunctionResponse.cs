namespace Models;

internal class ExecuteFunctionResponse
{
    [JsonPropertyName("response")]
    [OpenApiProperty(Description = "The response from the AI.")]
    public string? Response { get; set; }
}