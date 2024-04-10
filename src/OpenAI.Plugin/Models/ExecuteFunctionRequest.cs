namespace Models;

#pragma warning disable CA1812
internal class ExecuteFunctionRequest
{
    [JsonPropertyName("transcript")]
    [OpenApiProperty(Description = "The call transcript.", Default = "")]
    public string Transcript { get; set; } = string.Empty;
}
