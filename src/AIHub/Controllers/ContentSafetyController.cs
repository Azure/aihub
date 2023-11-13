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

namespace MVCWeb.Controllers;

public class ContentSafetyController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ContentSafetyClient _client;
    private readonly IConfiguration _config;
    private readonly BlobServiceClient blobServiceClient;
    private readonly BlobContainerClient containerClient;
    private readonly IEnumerable<BlobItem> blobs;
    public static readonly int[] VALID_THRESHOLD_VALUES = { -1, 0, 2, 3, 4, 5, 6 };

    private string endpoint;
    private string subscriptionKey;
    private string storageconnstring;
    private ContentSafetyModel model;
    private readonly Uri sasUri;
    //private int severity = 0;
    //private int violence = 0;
    //private int selfHarm = 0;
    //private int hate = 0;

    public enum Category
    {
        Hate = 0,
        SelfHarm = 1,
        Sexual = 2,
        Violence = 3
    }

    // Enumeration for actions
    public enum Action
    {
        Accept = 0,
        Reject = 1
    }

    public ContentSafetyController(IConfiguration config, ILogger<HomeController> logger)
    {
        _config = config;
        _logger = logger;
        endpoint = _config.GetValue<string>("ContentModerator:Endpoint");
        subscriptionKey = _config.GetValue<string>("ContentModerator:SubscriptionKey");
        storageconnstring = _config.GetValue<string>("Storage:ConnectionString");
        BlobServiceClient blobServiceClient = new BlobServiceClient(storageconnstring);
        containerClient = blobServiceClient.GetBlobContainerClient(_config.GetValue<string>("Storage:ContainerName"));
        sasUri = containerClient.GenerateSasUri(Azure.Storage.Sas.BlobContainerSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1));
        // Obtiene una lista de blobs en el contenedor
        blobs = containerClient.GetBlobs();
        model = new ContentSafetyModel();       
        
    }

    public IActionResult TextModerator()
    {
        return View();
    }

    public IActionResult ImageModerator()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    public IActionResult EvaluateImage(string imageUrl)
    {
        model.Approve = true;

        ContentSafetyClient client = new ContentSafetyClient(new Uri(endpoint), new AzureKeyCredential(subscriptionKey));

        ImageData image = new ImageData() { BlobUrl = new Uri(imageUrl) };
        var request = new AnalyzeImageOptions(image);


        Response<AnalyzeImageResult> response;
        try
        {
            response = client.AnalyzeImage(request);
            if (response.Value.HateResult?.Severity > model.Hate)
            {
                model.Approve = false;
            }
            if (response.Value.SelfHarmResult?.Severity > model.SelfHarm)
            {
                model.Approve = false;
            }
            if (response.Value.SexualResult?.Severity > model.Severity)
            {
                model.Approve = false;
            }
            if (response.Value.ViolenceResult?.Severity > model.Violence)
            {
                model.Approve = false;
            }
            ViewBag.Message = "Resultado de la moderación: \n" +
                        (model.Approve.Value ? "APROBADO" : "RECHAZADO") + "\n" +
                    "Hate severity: " + (response.Value.HateResult?.Severity ?? 0) + "\n" +
                    "SelfHarm severity: " + (response.Value.SelfHarmResult?.Severity ?? 0) + "\n" +
                    "Sexual severity: " + (response.Value.SexualResult?.Severity ?? 0) + "\n" +
                    "Violence severity: " + (response.Value.ViolenceResult?.Severity ?? 0);
            ViewBag.Image=imageUrl + sasUri.Query;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "An error occurred while evaluating the content." + ex.Message);
            ViewBag.Message = "An error occurred while evaluating the content. Please try again later.";
            return View("ImageModerator", model);
        }

        return View("ImageModerator", model);
    }

    [HttpPost]
    public IActionResult EvaluateText()
    {
        if(CheckNullValues(HttpContext))
        {
            ViewBag.Message = "You must enter a value for each threshold";
            return View("TextModerator", model);
        }
        //check if text is null
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
            if (response.Value.HateResult?.Severity > model.Hate)
            {
                model.Approve = false;
            }
            if (response.Value.SelfHarmResult?.Severity > model.SelfHarm)
            {
                model.Approve = false;
            }
            if (response.Value.SexualResult?.Severity > model.Severity)
            {
                model.Approve = false;
            }
            if (response.Value.ViolenceResult?.Severity > model.Violence)
            {
                model.Approve = false;
            }
            ViewBag.Message = "Resultado de la moderación: \n" +
                        (model.Approve.Value ? "APROBADO" : "RECHAZADO") + "\n" +
                        "Hate severity: " + (response.Value.HateResult?.Severity ?? 0) + "\n" +
                        "SelfHarm severity: " + (response.Value.SelfHarmResult?.Severity ?? 0) + "\n" +
                        "Sexual severity: " + (response.Value.SexualResult?.Severity ?? 0) + "\n" +
                        "Violence severity: " + (response.Value.ViolenceResult?.Severity ?? 0);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "An error occurred while evaluating the content." + ex.Message);
            ViewBag.Message = "An error occurred while evaluating the content. Please try again later.";
            return View("TextModerator", model);
        }

        return View("TextModerator", model);
    }

    //Upload a file to my azure storage account
    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile imageFile)
    {
        //Check if the file is null
        if (imageFile == null || imageFile.Length == 0)
        {
            ViewBag.Message = "You must select an image to upload";
            return View("ImageModerator", model);
        }
        //Check if the file is an image
        if (!imageFile.ContentType.Contains("image"))
        {
            ViewBag.Message = "You must select an image to upload";
            return View("ImageModerator", model);
        }
        //check if the file is too big
        if (imageFile.Length > 3000000)
        {
            ViewBag.Message = "The image is too big. File must be less than 3MB";
            return View("ImageModerator", model);
        }
        //Check if the threshold values are null
        if (CheckNullValues(HttpContext))
        {
            ViewBag.Message = "You must enter a value for each threshold";
            return View("ImageModerator", model);
        }
        model.Severity = Convert.ToInt32(HttpContext.Request.Form["severitytext"]);
        model.Violence = Convert.ToInt32(HttpContext.Request.Form["violencetext"]);
        model.SelfHarm = Convert.ToInt32(HttpContext.Request.Form["shtext"]);
        model.Hate = Convert.ToInt32(HttpContext.Request.Form["hatetext"]);


        //Upload file to azure storage account
        BlobClient blobClient = containerClient.GetBlobClient(imageFile.FileName);
        await blobClient.UploadAsync(imageFile.OpenReadStream(), true);

        //Get the url of the file
        Uri blobUrl = blobClient.Uri;

        //Call EvaluateImage with the url
        EvaluateImage(blobUrl.ToString());

        //Return the result
        return View("ImageModerator", model);
    }

    //Load the image from the storage account
    public IActionResult LoadImage(string imageName)
    {
        //Get the blob
        BlobClient blobClient = containerClient.GetBlobClient(imageName);

        //Get the image
        var image = blobClient.OpenReadAsync().Result;

        //Return the image
        return File(image, "image/jpeg");
    }

    //Check httpcontext for null values
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
