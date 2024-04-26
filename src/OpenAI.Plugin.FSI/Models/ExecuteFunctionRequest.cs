namespace Models;

#pragma warning disable CA1812
internal class ExecuteFunctionRequest
{
    [JsonPropertyName("product")]
    [OpenApiProperty(Description = "The product to compare", Default = "")]
    public string Product { get; set; } = string.Empty;
}
