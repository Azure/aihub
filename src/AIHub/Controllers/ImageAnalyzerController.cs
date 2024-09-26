using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MVCWeb.Controllers;

public class ImageAnalyzerController : Controller
{
    private string AOAIendpoint;
    private string AOAIsubscriptionKey;
    private string storageconnstring;
    private string AOAIDeploymentName;
    private readonly BlobContainerClient containerClient;
    private readonly IEnumerable<BlobItem> blobs;
    private Uri sasUri;
    private ImageAnalyzerModel model;
    private HttpClient httpClient;

    public ImageAnalyzerController(IConfiguration config, IHttpClientFactory clientFactory)
    {
        AOAIendpoint = config.GetValue<string>("ImageAnalyzer:OpenAIEndpoint") ?? throw new ArgumentNullException("OpenAIEndpoint");
        AOAIsubscriptionKey = config.GetValue<string>("ImageAnalyzer:OpenAISubscriptionKey") ?? throw new ArgumentNullException("OpenAISubscriptionKey");
        storageconnstring = config.GetValue<string>("Storage:ConnectionString") ?? throw new ArgumentNullException("ConnectionString");
        BlobServiceClient blobServiceClient = new BlobServiceClient(storageconnstring);
        containerClient = blobServiceClient.GetBlobContainerClient(config.GetValue<string>("Storage:ContainerName"));
        sasUri = containerClient.GenerateSasUri(Azure.Storage.Sas.BlobContainerSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1));
        AOAIDeploymentName = config.GetValue<string>("ImageAnalyzer:DeploymentName") ?? throw new ArgumentNullException("DeploymentName");
        // Obtain the blobs list in the container
        blobs = containerClient.GetBlobs();
        httpClient = clientFactory.CreateClient();
        model = new ImageAnalyzerModel();
    }

    public IActionResult ImageAnalyzer()
    {
        return View(new ImageAnalyzerModel());
    }

    [HttpPost]
    public async Task<IActionResult> DenseCaptionImage(string image_url, string prompt)
    {
        string GPT4o_ENDPOINT = $"{AOAIendpoint}openai/deployments/{AOAIDeploymentName}/chat/completions?api-version=2024-02-15-preview";
        image_url = image_url + sasUri.Query;

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
            messages = new object[]
            {
                new {
                    role = "system",
                    content = new object[] {
                        new {
                            type = "text",
                            text = "You are an AI assistant that helps people find information."
                        }
                    }
                },
                new {
                    role = "user",
                    content = new object[] {
                        new {
                            type = "image_url",
                            image_url = new {
                                url = image_url
                            }
                        },
                        new {
                            type = "text",
                            text = prompt
                        }
                    }
                }
            },
            temperature = 0.7,
            top_p = 0.95,
            max_tokens = 800,
            stream = false
        };
        Console.WriteLine(JsonConvert.SerializeObject(payload));
        var response = await httpClient.PostAsync(GPT4o_ENDPOINT, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

        if (response.IsSuccessStatusCode)
        {
            var responseData = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            // Get the web pages from the response
            var response_final = responseData!.choices[0];
            string final = response_final.message.content;
            model.Message = final;
            model.Image = image_url;
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error after GPT4o: {response.StatusCode}, {errorContent}");
        }

        return View("ImageAnalyzer", model);
    }

    // Upload a file to my azure storage account
    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile imageFile)
    {
        // Check no image
        if (CheckNullValues(imageFile))
        {
            ViewBag.Message = "You must upload an image";
            return View("ImageAnalyzer");
        }
        if (string.IsNullOrEmpty(HttpContext.Request.Form["text"]))
        {
            ViewBag.Message = "You must enter a prompt to evaluate";
            return View("ImageAnalyzer", model);
        }
        model.Prompt = HttpContext.Request.Form["text"];
        // Upload file to azure storage account
        string url = imageFile.FileName.ToString();
        Console.WriteLine(url);

        BlobClient blobClient = containerClient.GetBlobClient(url);
        await blobClient.UploadAsync(imageFile.OpenReadStream(), true);

        // Get the url of the file
        Uri blobUrl = blobClient.Uri;

        if (CheckImageExtension(blobUrl.ToString()))
        {
            ViewBag.Message = "You must upload an image with .jpg, .jpeg or .png extension";
            return View("ImageAnalyzer");
        }

        // Call EvaluateImage with the url
        Console.WriteLine(blobUrl.ToString());
        await DenseCaptionImage(blobUrl.ToString(), model.Prompt!);
        ViewBag.Waiting = null;

        return Ok(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private bool CheckNullValues(IFormFile imageFile)
    {
        if (imageFile == null)
        {
            return true;
        }
        return false;
    }

    private bool CheckImageExtension(string blobUri)
    {
        string jpg = ".jpg";
        string jpeg = ".jpeg";
        string png = ".png";
        string uri_lower = blobUri;
        if (uri_lower.Contains(jpg, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        if (uri_lower.Contains(jpeg, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        if (uri_lower.Contains(png, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        return true;
    }
}