namespace MVCWeb.Controllers;

public class DocumentComparisonController : Controller
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
    private HttpClient httpClient;

    private DocumentComparisonModel model;

    public DocumentComparisonController(IConfiguration config, IHttpClientFactory clientFactory)
    {
        FormRecogEndpoint = config.GetValue<string>("DocumentComparison:FormRecogEndpoint") ?? throw new ArgumentNullException("FormRecogEndpoint");
        FormRecogSubscriptionKey = config.GetValue<string>("DocumentComparison:FormRecogSubscriptionKey") ?? throw new ArgumentNullException("FormRecogSubscriptionKey");
        AOAIendpoint = config.GetValue<string>("DocumentComparison:OpenAIEndpoint") ?? throw new ArgumentNullException("OpenAIEndpoint");
        AOAIsubscriptionKey = config.GetValue<string>("DocumentComparison:OpenAISubscriptionKey") ?? throw new ArgumentNullException("OpenAISubscriptionKey");
        storageconnstring = config.GetValue<string>("Storage:ConnectionString") ?? throw new ArgumentNullException("ConnectionString");
        AOAIDeploymentName = config.GetValue<string>("BrandAnalyzer:DeploymentName") ?? throw new ArgumentNullException("DeploymentName");
        BlobServiceClient blobServiceClient = new BlobServiceClient(storageconnstring);
        containerClient = blobServiceClient.GetBlobContainerClient(config.GetValue<string>("DocumentComparison:ContainerName"));
        sasUri = containerClient.GenerateSasUri(Azure.Storage.Sas.BlobContainerSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1));
        // Obtiene una lista de blobs en el contenedor
        blobs = containerClient.GetBlobs();
        model = new DocumentComparisonModel();
        httpClient = clientFactory.CreateClient();
    }

    public IActionResult DocumentComparison()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> DocumentComparison(string[] document_urls, string prompt)
    {
        //1. Get Documents
        ViewBag.PdfUrl1 = document_urls[0] + sasUri.Query;
        ViewBag.PdfUrl2 = document_urls[1] + sasUri.Query;
        string[] output_result = new string[2];

        //2. Call Form Recognizer
        for (int i = 0; i < document_urls.Length; i++)
        {
            httpClient.BaseAddress = new Uri(FormRecogEndpoint);
            // Add an Accept header for JSON format.
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", FormRecogSubscriptionKey);

            var content = new
            {
                urlSource = document_urls[i] + sasUri.Query
            };

            var json = System.Text.Json.JsonSerializer.Serialize(content);
            // Crear un HttpContent con el JSON y el tipo de contenido
            HttpContent content_body = new StringContent(json, Encoding.UTF8, "application/json");
            // List data response.
            HttpResponseMessage response = await httpClient.PostAsync(FormRecogEndpoint, content_body);  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            response.EnsureSuccessStatusCode();

            // string responseBody = await response.Content.ReadAsStringAsync();
            string? operation_location_url = response.Headers.GetValues("Operation-Location").First();

            // llamar a GET OPERATION
            httpClient.BaseAddress = new Uri(operation_location_url);

            // Add an Accept header for JSON format.
            httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", FormRecogSubscriptionKey);

            // Crear un HttpContent con el JSON y el tipo de contenido
            // List data response.
            HttpResponseMessage response2 = await httpClient.GetAsync(operation_location_url);  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            Console.WriteLine(response2);
            response2.EnsureSuccessStatusCode();
            dynamic responsejson = JsonSerializer.Deserialize<dynamic>(await response2.Content.ReadAsStringAsync())!;

            while (responsejson.status != "succeeded")
            {
                if (response2 != null)
                {
                    Thread.Sleep(10000);
                    response2 = await httpClient.GetAsync(operation_location_url);
                    responsejson = JsonSerializer.Deserialize<dynamic>(await response2.Content.ReadAsStringAsync())!;
                }
            }
            output_result[i] = responsejson.analyzeResult.content.ToString();
        }

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

            // ### If streaming is not selected
            Response<ChatCompletions> responseWithoutStream = await aoaiClient.GetChatCompletionsAsync(
                new ChatCompletionsOptions()
                {
                    DeploymentName = AOAIDeploymentName,
                    Messages =
                    {
                        new ChatRequestSystemMessage(@"You are specialized in analyze different versions of the same PDF document. The first Document OCR result is: <<<"+output_result[0]+">>> and the second Document OCR result is: <<<"+output_result[1]+">>>"),
                        new ChatRequestUserMessage(@"User question: "+prompt ),
                    },
                    Temperature = (float)0.7,
                    MaxTokens = 1000,
                    NucleusSamplingFactor = (float)0.95,
                    FrequencyPenalty = 0,
                    PresencePenalty = 0,
                });

            ChatCompletions completions = responseWithoutStream.Value;
            ChatChoice results_analisis = completions.Choices[0];
            ViewBag.Message = results_analisis.Message.Content;

        }
        catch (RequestFailedException)
        {
            throw;
        }

        return View("DocumentComparison", model);
    }

    // Upload a file to my azure storage account
    [HttpPost]
    public async Task<IActionResult> UploadFile(List<IFormFile> documentFiles, string prompt)
    {
        // pre-validations
        if (documentFiles == null || !documentFiles.Count.Equals(2))
        {
            ViewBag.Message = "You must upload exactly two documents";
            return View("DocumentComparison");
        }

        foreach (var documentFile in documentFiles)
        {
            if (CheckImageExtension(documentFile.FileName.ToString()))
            {
                ViewBag.Message = "You must upload pdf documpents only";
                return View("DocumentComparison", model);
            }
        }

        string[] urls = new string[2];
        for (int i = 0; i < documentFiles.Count; i++)
        {
            string url = documentFiles[i].FileName.ToString();
            url = url.Replace(" ", "");
            BlobClient blobClient = containerClient.GetBlobClient(url);
            var httpHeaders = new BlobHttpHeaders
            {
                ContentType = "application/pdf",
            };
            await blobClient.UploadAsync(documentFiles[i].OpenReadStream(), new BlobUploadOptions { HttpHeaders = httpHeaders });
            urls[i] = blobClient.Uri.ToString();
        }

        // Call EvaluateImage with the url
        await DocumentComparison(urls, prompt);
        ViewBag.Waiting = null;

        return View("DocumentComparison", model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private bool CheckNullValues(IFormFile filename)
    {
        if (filename == null)
        {
            return true;
        }
        return false;
    }

    private bool CheckImageExtension(string filename)
    {
        if (filename.Contains(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        return true;
    }
}