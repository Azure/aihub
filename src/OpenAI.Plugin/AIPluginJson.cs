public class AIPluginJson
{
    [Function("GetAIPluginJson")]
    public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ".well-known/ai-plugin.json")] HttpRequestData req)
    {
        var currentDomain = $"{req.Url.Scheme}://{req.Url.Host}:{req.Url.Port}/api";

        HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");

        var settings = AIPluginSettings.FromFile();

        // serialize app settings to json using System.Text.Json
        var json = System.Text.Json.JsonSerializer.Serialize(settings);

        // replace {url} with the current domain
        json = json.Replace("{url}", currentDomain, StringComparison.OrdinalIgnoreCase);

        response.WriteString(json);

        return response;
    }
}