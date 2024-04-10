namespace Models;

internal class ErrorResponse
{
    [JsonPropertyName("message")]
    [OpenApiProperty(Description = "The error message.")]
    public string Message { get; set; } = string.Empty;
}