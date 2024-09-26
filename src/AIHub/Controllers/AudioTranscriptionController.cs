namespace MVCWeb.Controllers;

public class AudioTranscriptionController : Controller
{
    private string speechRegion;
    private string speechSubscriptionKey;
    private string storageconnstring;
    private readonly BlobContainerClient containerClient;
    private readonly IEnumerable<BlobItem> blobs;
    private Uri sasUri;
    private AudioTranscriptionModel model;
    private HttpClient httpClient;

    public AudioTranscriptionController(IConfiguration config, IHttpClientFactory clientFactory)
    {
        speechRegion = config.GetValue<string>("AudioTranscription:SpeechLocation") ?? throw new ArgumentNullException("SpeechLocation");
        speechSubscriptionKey = config.GetValue<string>("AudioTranscription:SpeechSubscriptionKey") ?? throw new ArgumentNullException("SpeechSubscriptionKey");
        storageconnstring = config.GetValue<string>("Storage:ConnectionString") ?? throw new ArgumentNullException("ConnectionString");
        BlobServiceClient blobServiceClient = new BlobServiceClient(storageconnstring);
        containerClient = blobServiceClient.GetBlobContainerClient(config.GetValue<string>("AudioTranscription:ContainerName"));
        sasUri = containerClient.GenerateSasUri(Azure.Storage.Sas.BlobContainerSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1));
        // Obtiene una lista de blobs en el contenedor
        blobs = containerClient.GetBlobs();
        model = new AudioTranscriptionModel();
        httpClient = clientFactory.CreateClient();
    }

    public IActionResult AudioTranscription()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> TranscribeAudio(string audio_url)
    {
        string audio = audio_url + sasUri.Query;

        // CALL 1: STT 3.1
        var request = new HttpRequestMessage(HttpMethod.Post, "https://" + speechRegion + ".api.cognitive.microsoft.com/speechtotext/v3.1/transcriptions");
        request.Headers.Add("Ocp-Apim-Subscription-Key", speechSubscriptionKey);
        var requestBody = new
        {
            contentUrls = new[] { audio },
            locale = "es-es",
            displayName = "My Transcription",
            model = (string?)null,
            properties = new
            {
                wordLevelTimestampsEnabled = true,
                languageIdentification = new
                {
                    candidateLocales = new[] { "en-US", "de-DE", "es-ES" }
                }
            }
        };
        Console.WriteLine(JsonSerializer.Serialize(requestBody));
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responsejson = JsonSerializer.Deserialize<JsonObject>(await response.Content.ReadAsStringAsync())!;
        Console.WriteLine(responsejson["self"]!.ToString());
        if (responsejson["self"] == null || responsejson["self"]!.ToString() == string.Empty)
        {
            ViewBag.Message = "Error in the transcription process";
            return View("AudioTranscription", model);
        }
        var output_result = responsejson["self"]!.ToString();
        Console.WriteLine("SELF: " + output_result);
        // CALL 2: CHECK FOR FINISH
        var request2 = new HttpRequestMessage(HttpMethod.Get, output_result);
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", speechSubscriptionKey);
        var content2 = new StringContent(string.Empty);
        content2.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        request2.Content = content2;
        var response2 = await httpClient.SendAsync(request2);
        response2.EnsureSuccessStatusCode();
        //Console.WriteLine(await response2.Content.ReadAsStringAsync());
        var responsejson2 = JsonSerializer.Deserialize<JsonObject>(await response.Content.ReadAsStringAsync())!;
        Console.WriteLine(responsejson2);
        while (responsejson2["status"]!.ToString() != "Succeeded")
        {
            Thread.Sleep(10000);
            response2 = await httpClient.GetAsync(output_result);
            responsejson2 = JsonSerializer.Deserialize<JsonObject>(await response2.Content.ReadAsStringAsync())!;
            Console.WriteLine(responsejson2["status"]!.ToString());
        }

        // CALL 3: GET RESULTS URL
        var request3 = new HttpRequestMessage(HttpMethod.Get, output_result + "/files/");
        request3.Headers.Add("Ocp-Apim-Subscription-Key", speechSubscriptionKey);
        var content3 = new StringContent(string.Empty);
        content3.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        request3.Content = content3;
        var response3 = await httpClient.SendAsync(request3);
        response3.EnsureSuccessStatusCode();
        var responsejson3 = JsonSerializer.Deserialize<JsonObject>(await response3.Content.ReadAsStringAsync())!;
        Console.WriteLine(responsejson3);
        // Extract contentUrl field
        string output_result3 = (string)responsejson3["values"]![0]!["links"]!["contentUrl"]!;
        Console.WriteLine(output_result3);

        // CALL 4: GET RESULTS (TRANSCRIPTION)
        var request4 = new HttpRequestMessage(HttpMethod.Get, output_result3);
        request4.Headers.Add("Ocp-Apim-Subscription-Key", speechSubscriptionKey);
        var content4 = new StringContent(string.Empty);
        content4.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        request4.Content = content4;
        var response4 = await httpClient.SendAsync(request4);
        response4.EnsureSuccessStatusCode();
        Console.WriteLine(await response4.Content.ReadAsStringAsync());
        var jsonObject4 = JsonSerializer.Deserialize<JsonObject>(await response4.Content.ReadAsStringAsync())!;
        string output_result4 = (string)jsonObject4["combinedRecognizedPhrases"]![0]!["lexical"]!;
        Console.WriteLine(output_result4);

        // Show transcript results
        model.Message = output_result4;
        ViewBag.Message = "TRANSCRIPTION RESULTS: \n\n" + output_result4;

        // return View("AudioTranscription", model);
        return Ok(model);
    }

    public class SpeechToTextResponse
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    // Upload a file to my azure storage account
    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile audioFile, string prompt)
    {
        // Check no audio file was uploaded
        if (CheckNullValues(audioFile))
        {
            ViewBag.Message = "You must upload an mp3 audio file";
            return View("AudioTranscription");
        }

        // Upload file to azure storage account
        string url = audioFile.FileName.ToString();
        // Console.WriteLine(url);
        url = url.Replace(" ", "");
        // Console.WriteLine(url);
        BlobClient blobClient = containerClient.GetBlobClient(url);
        var httpHeaders = new BlobHttpHeaders
        {
            ContentType = "audio/mpeg",
        };
        await blobClient.UploadAsync(audioFile.OpenReadStream(), new BlobUploadOptions { HttpHeaders = httpHeaders });

        // Get the url of the file
        Uri blobUrl = blobClient.Uri;

        if (CheckImageExtension(blobUrl.ToString()))
        {
            ViewBag.Message = "You must upload an audio file with .mp3 extension";
            return View("AudioTranscription", model);
        }

        // Call EvaluateImage with the url
        await TranscribeAudio(blobUrl.ToString());
        ViewBag.Waiting = null;

        // return View("AudioTranscription", model);

        return Ok(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private static bool CheckNullValues(IFormFile? imageFile)
    {
        return imageFile == null;
    }

    private static bool CheckImageExtension(string blobUri)
    {
        string uri_lower = blobUri;
        return !uri_lower.Contains(@".mp3", StringComparison.OrdinalIgnoreCase);
    }
}