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
    HttpClient httpClient;


    public FormAnalyzerController(IConfiguration config, IHttpClientFactory httpClientFactory)
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
        httpClient = httpClientFactory.CreateClient();
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
        string output_result;

        httpClient.BaseAddress = new Uri(FormRecogEndpoint);

        // Add an Accept header for JSON format.
        httpClient.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", FormRecogSubscriptionKey);

        var content = new
        {
            urlSource = image
        };
        var json = System.Text.Json.JsonSerializer.Serialize(content);
        // Crear un HttpContent con el JSON y el tipo de contenido
        HttpContent content_body = new StringContent(json, Encoding.UTF8, "application/json");
        // List data response.
        HttpResponseMessage response = await httpClient.PostAsync(FormRecogEndpoint, content_body);  // Blocking call! Program will wait here until a response is received or a timeout occurs.
        response.EnsureSuccessStatusCode();

        string operation_location_url = response.Headers.GetValues("Operation-Location").First();

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
        var responseBody = await response2.Content.ReadAsStringAsync();
        var responsejson = JsonSerializer.Deserialize<dynamic>(await response2.Content.ReadAsStringAsync())!;

        //var analyzeresult = responseBody.analyzeResult;            
        while (responsejson.status != "succeeded")
        {
            Thread.Sleep(10000);
            response2 = await httpClient.GetAsync(operation_location_url);
            responsejson = JsonSerializer.Deserialize<dynamic>(await response2.Content.ReadAsStringAsync())!;
        }
        output_result = responsejson.analyzeResult.content.ToString();

        // Above three lines can be replaced with new helper method below
        // string responseBody = await client.GetStringAsync(uri);

        // Parse the response as JSON
        // var operationLocation= await response.Headers.ReadAsStringAsync();

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
                        new ChatRequestSystemMessage(@"You are specialized in understanding PDFs and answering questions about it. Document OCR result is: "+output_result),
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
            model.Message = results_analisis.Message.Content;
            ViewBag.Message = results_analisis.Message.Content;

            model.Image = model.Image + sasUri.Query;
        }
        catch (RequestFailedException)
        {
            throw;
        }

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
        string uri_lower = blobUri;
        if (uri_lower.Contains(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        return true;
    }
}