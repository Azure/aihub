using OpenAI.Chat;

namespace MVCWeb.Controllers;

public class FormAnalyzerController : Controller
{
    private string FormRecogEndpoint;
    private string FormRecogSubscriptionKey;
    private string AOAIendpoint;
    private string AOAIsubscriptionKey;
    private string storageconnstring;
    private string AOAIDeploymentName;
    private readonly BlobContainerClient containerClient;
    private readonly IEnumerable<BlobItem> blobs;
    private Uri sasUri;
    private FormAnalyzerModel model;

    public FormAnalyzerController(IConfiguration config)
    {
        FormRecogEndpoint = config.GetValue<string>("FormAnalyzer:FormRecogEndpoint") ?? throw new ArgumentNullException("FormRecogEndpoint");
        FormRecogSubscriptionKey = config.GetValue<string>("FormAnalyzer:FormRecogSubscriptionKey") ?? throw new ArgumentNullException("FormRecogSubscriptionKey");
        AOAIendpoint = config.GetValue<string>("FormAnalyzer:OpenAIEndpoint") ?? throw new ArgumentNullException("OpenAIEndpoint");
        AOAIsubscriptionKey = config.GetValue<string>("FormAnalyzer:OpenAISubscriptionKey") ?? throw new ArgumentNullException("OpenAISubscriptionKey");
        storageconnstring = config.GetValue<string>("Storage:ConnectionString") ?? throw new ArgumentNullException("ConnectionString");
        BlobServiceClient blobServiceClient = new BlobServiceClient(storageconnstring);
        containerClient = blobServiceClient.GetBlobContainerClient(config.GetValue<string>("FormAnalyzer:ContainerName"));
        sasUri = containerClient.GenerateSasUri(Azure.Storage.Sas.BlobContainerSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1));
        AOAIDeploymentName = config.GetValue<string>("FormAnalyzer:DeploymentName") ?? throw new ArgumentNullException("DeploymentName");
        // Obtiene una lista de blobs en el contenedor
        blobs = containerClient.GetBlobs();
        model = new FormAnalyzerModel();
    }

    public IActionResult FormAnalyzer()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AnalyzeForm(string image_url, string prompt)
    {
        // 1. Get Image
        string image = image_url + sasUri.Query;
        Console.WriteLine(image);
        ViewBag.PdfUrl = image;
        model.PdfUrl = image;

        var client = new DocumentAnalysisClient(new Uri(FormRecogEndpoint), new AzureKeyCredential(FormRecogSubscriptionKey));
        var operation = await client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-layout", new Uri(image));
        var result = operation.Value.Content;

        Uri aoaiEndpointUri = new(AOAIendpoint);

        AzureOpenAIClient azureClient = string.IsNullOrEmpty(AOAIsubscriptionKey)
            ? new(aoaiEndpointUri, new DefaultAzureCredential())
            : new(aoaiEndpointUri, new AzureKeyCredential(AOAIsubscriptionKey));

        ChatClient chatClient = azureClient.GetChatClient(AOAIDeploymentName);

        var messages = new ChatMessage[]
        {
            new SystemChatMessage($@"You are specialized in understanding PDFs and answering questions about it. Document OCR result is: {result}"),
            new UserChatMessage($@"User question: {prompt}"),
        };

        ChatCompletionOptions chatCompletionOptions = new()
        {
            MaxTokens = 1000,
            Temperature = 0.7f,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
            TopP = 0.95f
        };

        ChatCompletion completion = await chatClient.CompleteChatAsync(messages, chatCompletionOptions);
        
        model.Message = completion.Content[0].Text;
        ViewBag.Message = completion.Content[0].Text;
        
        model.Image = image;

        return Ok(model);
    }

    //Upload a file to my azure storage account
    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile imageFile, string prompt)
    {
        // Check no image
        if (CheckNullValues(imageFile))
        {
            ViewBag.Message = "You must upload an image";
            return View("FormAnalyzer");
        }

        // Upload file to azure storage account
        string url = imageFile.FileName.ToString();
        Console.WriteLine(url);
        url = url.Replace(" ", "");
        Console.WriteLine(url);
        BlobClient blobClient = containerClient.GetBlobClient(url);
        var httpHeaders = new BlobHttpHeaders
        {
            ContentType = "application/pdf",
        };
        await blobClient.UploadAsync(imageFile.OpenReadStream(), new BlobUploadOptions { HttpHeaders = httpHeaders });

        // Get the url of the file
        Uri blobUrl = blobClient.Uri;

        if (CheckImageExtension(blobUrl.ToString()))
        {
            ViewBag.Message = "You must upload a document with .pdf extension";
            return View("FormAnalyzer", model);
        }

        // Call EvaluateImage with the url
        await AnalyzeForm(blobUrl.ToString(), prompt);
        ViewBag.Waiting = null;

        return Ok(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private static bool CheckNullValues(IFormFile imageFile)
    {
        return imageFile == null;
    }

    private static bool CheckImageExtension(string blobUri)
    {
        string uri_lower = blobUri;
        return !uri_lower.Contains(@".pdf", StringComparison.OrdinalIgnoreCase);
    }
}