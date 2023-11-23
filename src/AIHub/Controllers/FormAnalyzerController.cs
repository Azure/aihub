using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MVCWeb.Models;
using Azure.AI.ContentSafety;
using Azure;
using ContentSafetySampleCode;
using System;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Identity;
using Azure.AI.OpenAI;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace MVCWeb.Controllers;

public class FormAnalyzerController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _config;
    private string FormRecogEndpoint;
    private string FormRecogSubscriptionKey;
    private string AOAIendpoint;
    private string AOAIsubscriptionKey;
    private string storageconnstring;
    private readonly BlobServiceClient blobServiceClient;
    private readonly BlobContainerClient containerClient;
    private readonly IEnumerable<BlobItem> blobs;
    private Uri sasUri;


    //Results
    string result_image_front;
    string result_message_front;



    private FormAnalyzerModel model;


    public FormAnalyzerController(IConfiguration config)
    {
        _config = config;
        FormRecogEndpoint = _config.GetValue<string>("FormAnalyzer:FormRecogEndpoint");
        FormRecogSubscriptionKey = _config.GetValue<string>("FormAnalyzer:FormRecogSubscriptionKey");
        AOAIendpoint = _config.GetValue<string>("FormAnalyzer:OpenAIEndpoint");
        AOAIsubscriptionKey = _config.GetValue<string>("FormAnalyzer:OpenAISubscriptionKey");
        storageconnstring = _config.GetValue<string>("Storage:ConnectionString");
        BlobServiceClient blobServiceClient = new BlobServiceClient(storageconnstring);
        containerClient = blobServiceClient.GetBlobContainerClient(_config.GetValue<string>("FormAnalyzer:ContainerName"));
        sasUri = containerClient.GenerateSasUri(Azure.Storage.Sas.BlobContainerSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1));
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


        //1. Get Image
        string image = image_url + sasUri.Query;
        Console.WriteLine(image);
        //ViewBag.PdfUrl = "http://docs.google.com/gview?url="+image+"&embedded=true";
        ViewBag.PdfUrl = image;
        string output_result;

        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri(FormRecogEndpoint);

        // Add an Accept header for JSON format.
        client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", FormRecogSubscriptionKey);

        var content = new
        {
            urlSource = image
        };
        var json = System.Text.Json.JsonSerializer.Serialize(content);
        // Crear un HttpContent con el JSON y el tipo de contenido
        HttpContent content_body = new StringContent(json, Encoding.UTF8, "application/json");
        // List data response.
        HttpResponseMessage response = await client.PostAsync(FormRecogEndpoint, content_body);  // Blocking call! Program will wait here until a response is received or a timeout occurs.
        response.EnsureSuccessStatusCode();

        //string responseBody = await response.Content.ReadAsStringAsync();
        string operation_location_url = response.Headers.GetValues("Operation-Location").FirstOrDefault();


        client.Dispose();


        //llamar a GET OPERATION
        HttpClient client2 = new HttpClient();
        client2.BaseAddress = new Uri(operation_location_url);

        // Add an Accept header for JSON format.
        client2.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
        client2.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", FormRecogSubscriptionKey);

        // Crear un HttpContent con el JSON y el tipo de contenido
        // List data response.
        HttpResponseMessage response2 = await client2.GetAsync(operation_location_url);  // Blocking call! Program will wait here until a response is received or a timeout occurs.
        Console.WriteLine(response2);
        response2.EnsureSuccessStatusCode();
        var responseBody = await response2.Content.ReadAsStringAsync();
        var responsejson = JsonConvert.DeserializeObject<dynamic>(await response2.Content.ReadAsStringAsync());

        //var analyzeresult = responseBody.analyzeResult;            
        while (responsejson.status != "succeeded")
        {
            Thread.Sleep(10000);
            response2 = await client2.GetAsync(operation_location_url);
            responsejson = JsonConvert.DeserializeObject<dynamic>(await response2.Content.ReadAsStringAsync());
        }
        output_result = responsejson.analyzeResult.content.ToString();

        // Above three lines can be replaced with new helper method below
        // string responseBody = await client.GetStringAsync(uri);

        // Parse the response as JSON
        // var operationLocation= await response.Headers.ReadAsStringAsync();

        client2.Dispose();


        try
        {

            OpenAIClient client_oai = new OpenAIClient(
             new Uri(AOAIendpoint),
             new AzureKeyCredential(AOAIsubscriptionKey));

            // ### If streaming is not selected
            Response<ChatCompletions> responseWithoutStream = await client_oai.GetChatCompletionsAsync(
                "DemoBuild",
                new ChatCompletionsOptions()
                {
                    Messages =
                    {
                        new ChatMessage(ChatRole.System, @"You are specialized in understanding PDFs and answering questions about it. Document OCR result is: "+output_result),
                        new ChatMessage(ChatRole.User, @"User question: "+prompt ),
                    },
                    Temperature = (float)0.7,
                    MaxTokens = 1000,
                    NucleusSamplingFactor = (float)0.95,
                    FrequencyPenalty = 0,
                    PresencePenalty = 0,
                });

            ChatCompletions completions = responseWithoutStream.Value;
            ChatChoice results_analisis = completions.Choices[0];
            ViewBag.Message =
                   //"Hate severity: " + (response.Value.HateResult?.Severity ?? 0);
                   results_analisis.Message.Content
                   ;

            /* result_image_front=image;
            Console.WriteLine("1) "+result_image_front);
            Console.WriteLine("2) "+result_message_front);
             /* ViewBag.Message = 
                  results_analisis.Message.Content
                  ; */
            //ViewBag.Image=result_image_front+".jpg"; 

        }
        catch (RequestFailedException ex)
        {
            throw;
        }

        // var result = await _service.GetBuildingHomeAsync(); 
        // return Ok(result); 
        return View("FormAnalyzer", model);
    }

    //Upload a file to my azure storage account
    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile imageFile, string prompt)
    {
        //Check no image

        if (CheckNullValues(imageFile))
        {
            ViewBag.Message = "You must upload an image";
            return View("FormAnalyzer");
        }

        //Upload file to azure storage account
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

        //Get the url of the file
        Uri blobUrl = blobClient.Uri;

        if (CheckImageExtension(blobUrl.ToString()))
        {
            ViewBag.Message = "You must upload a document with .pdf extension";
            return View("FormAnalyzer", model);
        }


        //Call EvaluateImage with the url
        await AnalyzeForm(blobUrl.ToString(), prompt);
        ViewBag.Waiting = null;

        return View("FormAnalyzer", model);
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