using System.ComponentModel.DataAnnotations;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq.Expressions;

namespace MVCWeb.Controllers;

public class VideoAnalyzerController : Controller
{
    private string AOAIendpoint;
    private string Visionendpoint;
    private string AOAIsubscriptionKey;
    private string VisionKey;
    private string storageconnstring;
    private string AOAIDeploymentName;
    private readonly BlobContainerClient containerClient;
    private readonly IEnumerable<BlobItem> blobs;
    private Uri sasUri;
    private VideoAnalyzerModel model;
    private HttpClient httpClient;


    public VideoAnalyzerController(IConfiguration config, IHttpClientFactory clientFactory)
    {
        AOAIendpoint = config.GetValue<string>("VideoAnalyzer:OpenAIEndpoint") ?? throw new ArgumentNullException("OpenAIEndpoint");
        AOAIsubscriptionKey = config.GetValue<string>("VideoAnalyzer:OpenAISubscriptionKey") ?? throw new ArgumentNullException("OpenAISubscriptionKey");
        storageconnstring = config.GetValue<string>("Storage:ConnectionString") ?? throw new ArgumentNullException("ConnectionString");
        BlobServiceClient blobServiceClient = new BlobServiceClient(storageconnstring);
        containerClient = blobServiceClient.GetBlobContainerClient(config.GetValue<string>("VideoAnalyzer:ContainerName"));
        sasUri = containerClient.GenerateSasUri(Azure.Storage.Sas.BlobContainerSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1));
        AOAIDeploymentName = config.GetValue<string>("VideoAnalyzer:DeploymentName") ?? throw new ArgumentNullException("DeploymentName");
        Visionendpoint = config.GetValue<string>("VideoAnalyzer:VisionEndpoint") ?? throw new ArgumentNullException("VisionEndpoint");
        VisionKey = config.GetValue<string>("VideoAnalyzer:VisionSubscriptionKey") ?? throw new ArgumentNullException("VisionSubscriptionKey");

        // Obtain the blobs list in the container
        blobs = containerClient.GetBlobs();
        httpClient = clientFactory.CreateClient();
        model = new VideoAnalyzerModel();
    }

    const string VIDEO_DOCUMENT_ID = "AOAIChatDocument";

    public class acvDocumentIdWrapper
    {
        [JsonPropertyName("acv-document-id")]
        public string? AcvDocumentId { get; set; }
    }

    async Task<HttpResponseMessage> CreateVideoIndex(string visionApiEndpoint, string visionApiKey, string indexName)
    {
        string url = $"{visionApiEndpoint}/retrieval/indexes/{indexName}?api-version=2023-05-01-preview";
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", visionApiKey);
        var data = new { features = new[] { new { name = "vision", domain = "surveillance" } } };
        var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        var response = await httpClient.PutAsync(url, content);
        return response;
    }

    async Task<HttpResponseMessage> AddVideoToIndex(string visionApiEndpoint, string visionApiKey, string indexName, string videoUrl, string videoId)
    {
        string url = $"{visionApiEndpoint}/retrieval/indexes/{indexName}/ingestions/my-ingestion?api-version=2023-05-01-preview";
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", visionApiKey);
        var data = new { videos = new[] { new { mode = "add", documentId = videoId, documentUrl = videoUrl } } };
        var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        var response = await httpClient.PutAsync(url, content);
        return response;
    }

    async Task<bool> WaitForIngestionCompletion(string visionApiEndpoint, string visionApiKey, string indexName, int maxRetries = 30)
    {
        string url = $"{visionApiEndpoint}/retrieval/indexes/{indexName}/ingestions?api-version=2023-05-01-preview";
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", visionApiKey);
        int retries = 0;
        while (retries < maxRetries)
        {
            await Task.Delay(10000);
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var stateData = JsonSerializer.Deserialize<dynamic>(await response.Content.ReadAsStringAsync());
                if (stateData?.GetProperty("value").GetArrayLength() > 0 && stateData?.GetProperty("value")[0].GetProperty("state").GetString() == "Completed")
                {
                    Console.WriteLine(stateData);
                    Console.WriteLine("Ingestion completed.");
                    return true;
                }
                else if (stateData?.GetProperty("value").GetArrayLength() > 0 && stateData?.GetProperty("value")[0].GetProperty("state").GetString() == "Failed")
                {
                    Console.WriteLine(stateData);
                    Console.WriteLine("Ingestion failed.");
                    return false;
                }
            }
            retries++;
        }
        return false;
    }

    public IActionResult VideoAnalyzer()
    {
        return View(new VideoAnalyzerModel());
    }

    [HttpPost]
    public async Task<IActionResult> DenseCaptionVideo(string video_url, string prompt)
    {
        string GPT4V_ENDPOINT = $"{AOAIendpoint}openai/deployments/{AOAIDeploymentName}/chat/completions?api-version=2024-02-15-preview";
        string VISION_API_ENDPOINT = $"{Visionendpoint}computervision";
        string VISION_API_KEY = VisionKey;
        string VIDEO_INDEX_NAME = Regex.Replace(video_url.Split("/").Last().Split(".").First().GetHashCode().ToString(), "[^a-zA-Z0-9]", "");
        string VIDEO_FILE_SAS_URL = video_url + sasUri.Query;

        // Step 1: Create an Index
        var response = await CreateVideoIndex(VISION_API_ENDPOINT, VISION_API_KEY, VIDEO_INDEX_NAME);
        Console.WriteLine(response.StatusCode);
        Console.WriteLine(await response.Content.ReadAsStringAsync());

        // Step 2: Add a video file to the index
        response = await AddVideoToIndex(VISION_API_ENDPOINT, VISION_API_KEY, VIDEO_INDEX_NAME, VIDEO_FILE_SAS_URL, VIDEO_DOCUMENT_ID);
        Console.WriteLine(response.StatusCode);
        Console.WriteLine(await response.Content.ReadAsStringAsync());

        // Step 3: Wait for ingestion to complete
        if (!await WaitForIngestionCompletion(VISION_API_ENDPOINT, VISION_API_KEY, VIDEO_INDEX_NAME))
        {
            Console.WriteLine("Ingestion did not complete within the expected time.");
        }

        if (string.IsNullOrEmpty(AOAIsubscriptionKey))
        {
            var credential = new DefaultAzureCredential();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", credential.GetToken(new TokenRequestContext(["https://cognitiveservices.azure.com/.default"])).Token);
        }
        else
        {
            httpClient.DefaultRequestHeaders.Add("api-key", AOAIsubscriptionKey);
        }
        var payload = new
        {
            model = "gpt-4-vision-preview",
            dataSources = new[]
            {
                new
                {
                    type = "AzureComputerVisionVideoIndex",
                    parameters = new
                    {
                        computerVisionBaseUrl = VISION_API_ENDPOINT,
                        computerVisionApiKey = VISION_API_KEY,
                        indexName = VIDEO_INDEX_NAME,
                        videoUrls = new[] { VIDEO_FILE_SAS_URL }
                    }
                }
            },
            enhancements = new
            {
                video = new { enabled = true }
            },
            messages = new object[]
            {
                new {
                    role = "system",
                    content = new object[]
                        {
                            "You are an AI assistant that helps people find information."
                        }
                },
                new {
                    role = "user",
                    content = new object[]
                    {
                        new {
                            type = "acv_document_id",
                            acv_document_id = VIDEO_DOCUMENT_ID
                        },
                        new {
                            type = "text",
                            text = prompt
                        }
                    },
                }
            },
            temperature = 0.7,
            top_p = 0.95,
            max_tokens = 4096
        };

        try
        {
            var chatResponse = await httpClient.PostAsync(GPT4V_ENDPOINT, new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));
            chatResponse.EnsureSuccessStatusCode();
            var responseContent = JsonSerializer.Deserialize<JsonObject>(await chatResponse.Content.ReadAsStringAsync());
            Console.WriteLine(responseContent);

            model.Message = responseContent?["choices"]?[0]?["message"]?["content"]?.ToString();
            model.Video = VIDEO_FILE_SAS_URL;
        }
        catch
        {
            Console.WriteLine($"Error after GPT4V: {response.StatusCode}, {response.ReasonPhrase}");
        }

        return View("VideoAnalyzer", model);
    }

    // Upload a file to my azure storage account
    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile videoFile)
    {
        // Check no video
        if (CheckNullValues(videoFile))
        {
            ViewBag.Message = "You must upload an video";
            return View("VideoAnalyzer");
        }
        if (string.IsNullOrEmpty(HttpContext.Request.Form["text"]))
        {
            ViewBag.Message = "You must enter a prompt to evaluate";
            return View("VideoAnalyzer", model);
        }
        model.Prompt = HttpContext.Request.Form["text"];
        // Upload file to azure storage account
        string url = videoFile.FileName.ToString();
        Console.WriteLine(url);

        BlobClient blobClient = containerClient.GetBlobClient(url);
        await blobClient.UploadAsync(videoFile.OpenReadStream(), true);

        // Get the url of the file
        Uri blobUrl = blobClient.Uri;

        if (CheckVideoExtension(blobUrl.ToString()))
        {
            ViewBag.Message = "You must upload an video with .mp4 extension";
            return View("VideoAnalyzer");
        }

        // Call EvaluateVideo with the url
        Console.WriteLine(blobUrl.ToString());
        await DenseCaptionVideo(blobUrl.ToString(), model.Prompt!);
        ViewBag.Waiting = null;

        return Ok(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private bool CheckNullValues(IFormFile videoFile)
    {
        if (videoFile == null)
        {
            return true;
        }
        return false;
    }

    private bool CheckVideoExtension(string blobUri)
    {
        string mp4 = ".mp4";
        string uri_lower = blobUri;
        if (uri_lower.Contains(mp4, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        return true;
    }
}