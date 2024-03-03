using System.ComponentModel.DataAnnotations;

namespace MVCWeb.Controllers;

public class ImageAnalyzerController : Controller
{
    private string Visionendpoint;
    private string OCRendpoint;
    private string VisionsubscriptionKey;
    private string AOAIendpoint;
    private string AOAIsubscriptionKey;
    private string storageconnstring;
    private string AOAIDeploymentName;
    private readonly BlobContainerClient containerClient;
    private readonly IEnumerable<BlobItem> blobs;
    private Uri sasUri;
    private ImageAnalyzerModel model;

    public ImageAnalyzerController(IConfiguration config)
    {
        Visionendpoint = config.GetValue<string>("ImageAnalyzer:VisionEndpoint") ?? throw new ArgumentNullException("VisionEndpoint");
        OCRendpoint = config.GetValue<string>("ImageAnalyzer:OCREndpoint") ?? throw new ArgumentNullException("OCREndpoint");
        VisionsubscriptionKey = config.GetValue<string>("ImageAnalyzer:VisionSubscriptionKey") ?? throw new ArgumentNullException("VisionSubscriptionKey");
        AOAIendpoint = config.GetValue<string>("ImageAnalyzer:OpenAIEndpoint") ?? throw new ArgumentNullException("OpenAIEndpoint");
        AOAIsubscriptionKey = config.GetValue<string>("ImageAnalyzer:OpenAISubscriptionKey") ?? throw new ArgumentNullException("OpenAISubscriptionKey");
        storageconnstring = config.GetValue<string>("Storage:ConnectionString") ?? throw new ArgumentNullException("ConnectionString");
        BlobServiceClient blobServiceClient = new BlobServiceClient(storageconnstring);
        containerClient = blobServiceClient.GetBlobContainerClient(config.GetValue<string>("Storage:ContainerName"));
        sasUri = containerClient.GenerateSasUri(Azure.Storage.Sas.BlobContainerSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1));
        AOAIDeploymentName = config.GetValue<string>("ImageAnalyzer:DeploymentName") ?? throw new ArgumentNullException("DeploymentName");
        // Obtiene una lista de blobs en el contenedor
        blobs = containerClient.GetBlobs();
        model = new ImageAnalyzerModel();
    }

    public IActionResult ImageAnalyzer()
    {
        return View(new ImageAnalyzerModel());
    }

    [HttpPost]
    public async Task<IActionResult> DenseCaptionImage(string image_url)
    {
        // 1. Get Image
        model.Image = image_url;
        // 2. Dense Captioning and OCR
        var sb = new StringBuilder();

        ImageAnalysisClient client = new(
            new Uri(Visionendpoint),
            new AzureKeyCredential(VisionsubscriptionKey));

        ImageAnalysisResult result = client.Analyze(
            new Uri(model.Image + sasUri.Query),
            VisualFeatures.DenseCaptions | VisualFeatures.Read,
            new ImageAnalysisOptions { GenderNeutralCaption = false, Language = "en" });

        foreach (var caption in result.DenseCaptions.Values)
        {
            sb.Append(caption.Text);
        }

        var captions = sb.ToString();

        var ocr = "there is no text in the image";
        if (result.Read.Blocks.Count > 0)
        {
            ocr = result.Read.Blocks[0].ToString();
        }

        // 4. Tags 


        // 5. Objects


        // 6. Trancript of image


        // 7. Describe Image GPT4
        try
        {
            OpenAIClient aoaiClient;
            if (string.IsNullOrEmpty(AOAIsubscriptionKey))
            {
                aoaiClient = new OpenAIClient(
                    new Uri(AOAIendpoint),
                    new DefaultAzureCredential());
            }
            else
            {
                aoaiClient = new OpenAIClient(
                    new Uri(AOAIendpoint),
                    new AzureKeyCredential(AOAIsubscriptionKey));
            }

            // If streaming is not selected
            Response<ChatCompletions> responseWithoutStream = await aoaiClient.GetChatCompletionsAsync(
                new ChatCompletionsOptions()
                {
                    DeploymentName = AOAIDeploymentName,
                    Messages =
                    {
                        new ChatRequestSystemMessage(@"The user will provide a list of descriptions of an image. I want you to create a unified and complete description of the image based of the list provided. Each suggested description is separated by a \ symbol. Also, it will provide the text detected in the image, try to associate the text detected (if any) with the rest of the captions of the image. If you are not sure, say to user something like 'MIGHT BE'. "),
                        new ChatRequestUserMessage($"Descriptions: {captions}. & OCR: {ocr}" ),
                    },
                    Temperature = (float)0.7,
                    MaxTokens = 1000,
                    NucleusSamplingFactor = (float)0.95,
                    FrequencyPenalty = 0,
                    PresencePenalty = 0,
                });

            ChatCompletions completions = responseWithoutStream.Value;
            ChatChoice results_analisis = completions.Choices[0];
            model.Message = results_analisis.Message.Content;
            ViewBag.Message = results_analisis.Message.Content;
            ViewBag.Image = model.Image + sasUri.Query;
            model.Image = model.Image + sasUri.Query;
            Console.WriteLine(ViewBag.Message);
            Console.WriteLine(ViewBag.Image);
        }
        catch (RequestFailedException)
        {
            throw;
        }

        return Ok(model);
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
        await DenseCaptionImage(blobUrl.ToString());
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