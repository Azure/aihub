namespace Models;

#pragma warning disable CA1812
internal class ExecuteFunctionRequest
{
    [JsonPropertyName("product")]
    [OpenApiProperty(Description = "The product to compare", Default = "")]
    public string Product { get; set; } = string.Empty;

    [JsonPropertyName("queryPrompt")]
    [OpenApiProperty(Description = "The query prompt used to retrive products to compare to", Default = "")]
    public string QueryPrompt { get; set; } = string.Empty;
}
