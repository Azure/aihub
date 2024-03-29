namespace MVCWeb.Controllers;

public class ContentSafetyController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly BlobContainerClient containerClient;
    private readonly IEnumerable<BlobItem> blobs;
    public static readonly int[] VALID_THRESHOLD_VALUES = { -1, 0, 2, 3, 4, 5, 6 };

    private string endpoint;
    private string subscriptionKey;
    private string storageconnstring;
    private string jailbreakEndpoint;
    private ContentSafetyModel model;
    private readonly Uri sasUri;
    private HttpClient httpClient;

    public ContentSafetyController(IConfiguration config, ILogger<HomeController> logger, IHttpClientFactory clientFactory)
    {
        _logger = logger;
        endpoint = config.GetValue<string>("ContentModerator:Endpoint") ?? throw new ArgumentNullException("Endpoint");
        subscriptionKey = config.GetValue<string>("ContentModerator:SubscriptionKey") ?? throw new ArgumentNullException("SubscriptionKey");
        storageconnstring = config.GetValue<string>("Storage:ConnectionString") ?? throw new ArgumentNullException("ConnectionString");
        jailbreakEndpoint = config.GetValue<string>("ContentModerator:JailbreakDetectionEndpoint") ?? throw new ArgumentNullException("JailbreakDetectionEndpoint");
        BlobServiceClient blobServiceClient = new BlobServiceClient(storageconnstring);
        containerClient = blobServiceClient.GetBlobContainerClient(config.GetValue<string>("Storage:ContainerName"));
        sasUri = containerClient.GenerateSasUri(Azure.Storage.Sas.BlobContainerSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1));
        // Obtiene una lista de blobs en el contenedor
        blobs = containerClient.GetBlobs();
        model = new ContentSafetyModel();
        httpClient = clientFactory.CreateClient();

    }
    public IActionResult TextModerator()
    {
        return View();
    }

    public IActionResult ImageModerator()
    {
        return View();
    }

    public IActionResult JailbreakDetection()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    //Jailbreak Promp evaluation
    [HttpPost]
    public async Task<IActionResult> EvaluatePrompt()
    {
        if (string.IsNullOrEmpty(HttpContext.Request.Form["text"]))
        {
            ViewBag.Message = "You must enter a prompt to evaluate";
            return View("JailbreakDetection", model);
        }
        model.Prompt = HttpContext.Request.Form["text"];
        model.Approve = true;

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint + jailbreakEndpoint);
            request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            var content = new
            {
                text = model.Prompt
            };
            var json = System.Text.Json.JsonSerializer.Serialize(content);
            // Crear un HttpContent con el JSON y el tipo de contenido
            HttpContent content_body = new StringContent(json, Encoding.UTF8, "application/json");

            request.Content = content_body;
            var response = await httpClient.SendAsync(request);
            //response.EnsureSuccessStatusCode();
            String response_final = await response.Content.ReadAsStringAsync();
            if (response_final.Contains("true"))
            {
                ViewBag.Message = "JAILBREAKING ATTEMPT ** DETECTED **";
            }
            else
            {
                ViewBag.Message = "JAILBREAKING ATTEMPT ** NOT DETECTED **";

            }
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "An error occurred while evaluating the content." + ex.Message);
            ViewBag.Message = "An error occurred while evaluating the content. Please try again later.";
            return View("JailbreakDetection", model);
        }

        return View("JailbreakDetection", model);
    }

    [HttpPost]
    public IActionResult EvaluateImage(string imageUrl)
    {
        model.Approve = true;

        ContentSafetyClient client = new ContentSafetyClient(new Uri(endpoint), new AzureKeyCredential(subscriptionKey));

        ContentSafetyImageData image = new ContentSafetyImageData(new Uri(imageUrl));

        var request = new AnalyzeImageOptions(image);

        Response<AnalyzeImageResult> response;
        try
        {
            response = client.AnalyzeImage(request);
            if (response.Value.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.Hate)?.Severity > model.Hate)
            {
                model.Approve = false;
            }
            if (response.Value.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.SelfHarm)?.Severity > model.SelfHarm)
            {
                model.Approve = false;
            }
            if (response.Value.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.Sexual)?.Severity > model.Severity)
            {
                model.Approve = false;
            }
            if (response.Value.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.Violence)?.Severity > model.Violence)
            {
                model.Approve = false;
            }

            model.Message = "Resultado de la moderación: \n" +
                        (model.Approve.Value ? "APROBADO" : "RECHAZADO") + "\n" +
                    "Hate severity: " + (response.Value.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.Hate)?.Severity ?? 0) + "\n" +
                    "SelfHarm severity: " + (response.Value.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.SelfHarm)?.Severity ?? 0) + "\n" +
                    "Sexual severity: " + (response.Value.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.Sexual)?.Severity ?? 0) + "\n" +
                    "Violence severity: " + (response.Value.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.Violence)?.Severity ?? 0);

            ViewBag.Message = "Resultado de la moderación: \n" +
                        (model.Approve.Value ? "APROBADO" : "RECHAZADO") + "\n" +
                    "Hate severity: " + (response.Value.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.Hate)?.Severity ?? 0) + "\n" +
                    "SelfHarm severity: " + (response.Value.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.SelfHarm)?.Severity ?? 0) + "\n" +
                    "Sexual severity: " + (response.Value.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.Sexual)?.Severity ?? 0) + "\n" +
                    "Violence severity: " + (response.Value.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.Violence)?.Severity ?? 0);
            model.Image = imageUrl + sasUri.Query;
            ViewBag.Image = imageUrl + sasUri.Query;

        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "An error occurred while evaluating the content." + ex.Message);
            ViewBag.Message = "An error occurred while evaluating the content. Please try again later.";
            return View("ImageModerator", model);
        }

        return Ok(model);
    }

    [HttpPost]
    public IActionResult EvaluateText()
    {
        if (CheckNullValues(HttpContext))
        {
            ViewBag.Message = "You must enter a value for each threshold";
            return View("TextModerator", model);
        }
        if (string.IsNullOrEmpty(HttpContext.Request.Form["text"]))
        {
            ViewBag.Message = "You must enter a text to evaluate";
            return View("TextModerator", model);
        }
        model.Severity = Convert.ToInt32(HttpContext.Request.Form["severitytext"]);
        model.Violence = Convert.ToInt32(HttpContext.Request.Form["violencetext"]);
        model.SelfHarm = Convert.ToInt32(HttpContext.Request.Form["shtext"]);
        model.Hate = Convert.ToInt32(HttpContext.Request.Form["hatetext"]);
        model.Text = HttpContext.Request.Form["text"];
        model.Approve = true;

        ContentSafetyClient client = new ContentSafetyClient(new Uri(endpoint), new AzureKeyCredential(subscriptionKey));

        var request = new AnalyzeTextOptions(model.Text);

        Response<AnalyzeTextResult> response;
        try
        {
            response = client.AnalyzeText(request);

            if (response.Value.CategoriesAnalysis.FirstOrDefault(a => a.Category == TextCategory.Hate)?.Severity > model.Hate)
            {
                model.Approve = false;
            }
            if (response.Value.CategoriesAnalysis.FirstOrDefault(a => a.Category == TextCategory.SelfHarm)?.Severity > model.SelfHarm)
            {
                model.Approve = false;
            }
            if (response.Value.CategoriesAnalysis.FirstOrDefault(a => a.Category == TextCategory.Sexual)?.Severity > model.Severity)
            {
                model.Approve = false;
            }
            if (response.Value.CategoriesAnalysis.FirstOrDefault(a => a.Category == TextCategory.Violence)?.Severity > model.Violence)
            {
                model.Approve = false;
            }

            ViewBag.Message = "Resultado de la moderación: \n" +
                        (model.Approve.Value ? "APROBADO" : "RECHAZADO") + "\n" +
                        "Hate severity: " + (response.Value.CategoriesAnalysis.FirstOrDefault(a => a.Category == TextCategory.Hate)?.Severity ?? 0) + "\n" +
                        "SelfHarm severity: " + (response.Value.CategoriesAnalysis.FirstOrDefault(a => a.Category == TextCategory.SelfHarm)?.Severity ?? 0) + "\n" +
                        "Sexual severity: " + (response.Value.CategoriesAnalysis.FirstOrDefault(a => a.Category == TextCategory.Sexual)?.Severity ?? 0) + "\n" +
                        "Violence severity: " + (response.Value.CategoriesAnalysis.FirstOrDefault(a => a.Category == TextCategory.Violence)?.Severity ?? 0);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "An error occurred while evaluating the content." + ex.Message);
            ViewBag.Message = "An error occurred while evaluating the content. Please try again later.";
            return View("TextModerator", model);
        }

        return View("TextModerator", model);
    }

    // Upload a file to my azure storage account
    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile imageFile)
    {
        // Check if the file is null
        if (imageFile == null || imageFile.Length == 0)
        {
            ViewBag.Message = "You must select an image to upload";
            return View("ImageModerator", model);
        }
        // Check if the file is an image
        if (!imageFile.ContentType.Contains("image"))
        {
            ViewBag.Message = "You must select an image to upload";
            return View("ImageModerator", model);
        }
        // check if the file is too big
        if (imageFile.Length > 3000000)
        {
            ViewBag.Message = "The image is too big. File must be less than 3MB";
            return View("ImageModerator", model);
        }
        // Check if the threshold values are null
        if (CheckNullValues(HttpContext))
        {
            ViewBag.Message = "You must enter a value for each threshold";
            return View("ImageModerator", model);
        }
        model.Severity = Convert.ToInt32(HttpContext.Request.Form["severitytext"]);
        model.Violence = Convert.ToInt32(HttpContext.Request.Form["violencetext"]);
        model.SelfHarm = Convert.ToInt32(HttpContext.Request.Form["shtext"]);
        model.Hate = Convert.ToInt32(HttpContext.Request.Form["hatetext"]);

        // Upload file to azure storage account
        BlobClient blobClient = containerClient.GetBlobClient(imageFile.FileName);
        await blobClient.UploadAsync(imageFile.OpenReadStream(), true);

        // Get the url of the file
        Uri blobUrl = blobClient.Uri;

        // Call EvaluateImage with the url
        EvaluateImage(blobUrl.ToString());

        return Ok(model);
    }

    // Load the image from the storage account
    public IActionResult LoadImage(string imageName)
    {
        // Get the blob
        BlobClient blobClient = containerClient.GetBlobClient(imageName);

        // Get the image
        var image = blobClient.OpenReadAsync().Result;

        // Return the image
        return File(image, "image/jpeg");
    }

    // Check httpcontext for null values
    private bool CheckNullValues(HttpContext httpContext)
    {
        if (string.IsNullOrEmpty(httpContext.Request.Form["severitytext"]))
        {
            return true;
        }
        if (string.IsNullOrEmpty(httpContext.Request.Form["violencetext"]))
        {
            return true;
        }
        if (string.IsNullOrEmpty(httpContext.Request.Form["shtext"]))
        {
            return true;
        }
        if (string.IsNullOrEmpty(httpContext.Request.Form["hatetext"]))
        {
            return true;
        }
        return false;
    }
}
