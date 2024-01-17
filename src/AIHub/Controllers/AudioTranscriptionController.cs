namespace MVCWeb.Controllers;

public class AudioTranscriptionController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _config;
    private string SpeechRegion;
    private string SpeechSubscriptionKey;
    private string storageconnstring;
    private readonly BlobServiceClient blobServiceClient;
    private readonly BlobContainerClient containerClient;
    private readonly IEnumerable<BlobItem> blobs;
    private Uri sasUri;


    //Results
    string result_message_front;



    private AudioTranscriptionModel model;


    public AudioTranscriptionController(IConfiguration config)
    {
        _config = config;
        SpeechRegion = _config.GetValue<string>("AudioTranscription:SpeechLocation");
        SpeechSubscriptionKey = _config.GetValue<string>("AudioTranscription:SpeechSubscriptionKey");
        storageconnstring = _config.GetValue<string>("Storage:ConnectionString");
        BlobServiceClient blobServiceClient = new BlobServiceClient(storageconnstring);
        containerClient = blobServiceClient.GetBlobContainerClient(_config.GetValue<string>("AudioTranscription:ContainerName"));
        sasUri = containerClient.GenerateSasUri(Azure.Storage.Sas.BlobContainerSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1));
        // Obtiene una lista de blobs en el contenedor
        blobs = containerClient.GetBlobs();
        model = new AudioTranscriptionModel();
    }

    public IActionResult AudioTranscription()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> TranscribeAudio(string audio_url, IFormFile imageFile)
    {

        string audio = audio_url + sasUri.Query;

    // CALL 1: STT 3.1

        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://"+SpeechRegion+".api.cognitive.microsoft.com/speechtotext/v3.1/transcriptions");
        request.Headers.Add("Ocp-Apim-Subscription-Key", SpeechSubscriptionKey);
        var content = new StringContent("{\r\n\"contentUrls\": [\r\n    \"" + audio + "\"\r\n  ],\r\n  \"locale\": \"es-es\",\r\n  \"displayName\": \"My Transcription\",\r\n  \"model\": null,\r\n  \"properties\": {\r\n    \"wordLevelTimestampsEnabled\": true,\r\n    \"languageIdentification\": {\r\n      \"candidateLocales\": [\r\n        \"en-US\", \"de-DE\", \"es-ES\"\r\n      ]\r\n    }\r\n    }\r\n}", null, "application/json");
        request.Content = content;
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        //Console.WriteLine(await response.Content.ReadAsStringAsync());
        var responsejson = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
        Console.WriteLine(responsejson);
        var output_result = responsejson.self.ToString();
        Console.WriteLine("SELF: "+output_result);

        client.Dispose();

    // CALL 2: CHECK FOR FINISH
        var client2 = new HttpClient();
        var request2 = new HttpRequestMessage(HttpMethod.Get, output_result);
        client2.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SpeechSubscriptionKey);
        var content2 = new StringContent(string.Empty);
        content2.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        request2.Content = content2;
        var response2 = await client2.SendAsync(request2);
        response2.EnsureSuccessStatusCode();
        //Console.WriteLine(await response2.Content.ReadAsStringAsync());
        var responsejson2 = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
        Console.WriteLine(responsejson2);
        while (responsejson2.status != "Succeeded")
            {
                Thread.Sleep(10000);
                response2 = await client2.GetAsync(output_result);
                responsejson2 = JsonConvert.DeserializeObject<dynamic>(await response2.Content.ReadAsStringAsync());
                Console.WriteLine(responsejson2.status);
            }
        client2.Dispose();


    // CALL 3: GET RESULTS URL

        var client3 = new HttpClient();
        var request3 = new HttpRequestMessage(HttpMethod.Get, output_result+"/files/");
        request3.Headers.Add("Ocp-Apim-Subscription-Key", SpeechSubscriptionKey);
        var content3 = new StringContent(string.Empty);
        content3.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        request3.Content = content3;
        var response3 = await client3.SendAsync(request3);
        response3.EnsureSuccessStatusCode();
        var responsejson3 = JsonConvert.DeserializeObject<dynamic>(await response3.Content.ReadAsStringAsync());
        Console.WriteLine(responsejson3);
        // Extract contentUrl field
        string output_result3 = (string)responsejson3["values"][0]["links"]["contentUrl"];       
        Console.WriteLine(output_result3);
        client3.Dispose();

    // CALL 4: GET RESULTS (TRANSCRIPTION)

        var client4 = new HttpClient();
        var request4 = new HttpRequestMessage(HttpMethod.Get, output_result3);
        request4.Headers.Add("Ocp-Apim-Subscription-Key", SpeechSubscriptionKey);
        var content4 = new StringContent(string.Empty);
        content4.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        request4.Content = content4;
        var response4 = await client4.SendAsync(request4);
        response4.EnsureSuccessStatusCode();
        Console.WriteLine(await response4.Content.ReadAsStringAsync());
        var jsonObject4 = JsonConvert.DeserializeObject<JObject>(await response4.Content.ReadAsStringAsync());
        string output_result4 = (string)jsonObject4["combinedRecognizedPhrases"][0]["lexical"];
        Console.WriteLine(output_result4);
        client4.Dispose();


        //Show transcript results
        ViewBag.Message = "TRANSCRIPTION RESULTS: \n\n"+output_result4;


        return View("AudioTranscription", model);
    }
        public class SpeechToTextResponse
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }

    //Upload a file to my azure storage account
    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile imageFile, string prompt)
    {
        //Check no image

        if (CheckNullValues(imageFile))
        {
            ViewBag.Message = "You must upload an mp3 audio file";
            return View("AudioTranscription");
        }

        //Upload file to azure storage account
        string url = imageFile.FileName.ToString();
        //Console.WriteLine(url);
        url = url.Replace(" ", "");
        //Console.WriteLine(url);
        BlobClient blobClient = containerClient.GetBlobClient(url);
        var httpHeaders = new BlobHttpHeaders
        {
            ContentType = "audio/mpeg",
        };
        await blobClient.UploadAsync(imageFile.OpenReadStream(), new BlobUploadOptions { HttpHeaders = httpHeaders });

        //Get the url of the file
        Uri blobUrl = blobClient.Uri;

        if (CheckImageExtension(blobUrl.ToString()))
        {
            ViewBag.Message = "You must upload an audio file with .mp3 extension";
            return View("AudioTranscription", model);
        }


        //Call EvaluateImage with the url
        await TranscribeAudio(blobUrl.ToString(), imageFile);
        ViewBag.Waiting = null;

        return View("AudioTranscription", model);
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
        if (uri_lower.Contains(".mp3", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        return true;
    }
}