namespace MVCWeb.Controllers;

public class BrandAnalyzerController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _config;
    private BrandAnalyzerModel model;
    private string Bingendpoint;
    private string BingsubscriptionKey;
    private string AOAIendpoint;
    private string AOAIsubscriptionKey;
    private string storageconnstring;
    private string AOAIDeploymentName;


    public BrandAnalyzerController(IConfiguration config)
    {
        _config = config;
        Bingendpoint = _config.GetValue<string>("BrandAnalyzer:BingEndpoint");
        BingsubscriptionKey = _config.GetValue<string>("BrandAnalyzer:BingKey");
        AOAIendpoint = _config.GetValue<string>("BrandAnalyzer:OpenAIEndpoint");
        AOAIsubscriptionKey = _config.GetValue<string>("BrandAnalyzer:OpenAISubscriptionKey");
        AOAIDeploymentName = _config.GetValue<string>("BrandAnalyzer:DeploymentName");
        model = new BrandAnalyzerModel();

    }

    public IActionResult BrandAnalyzer()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AnalyzeCompany()
    {

        model.CompanyName = HttpContext.Request.Form["companyName"];
        model.Prompt = HttpContext.Request.Form["prompt"];
        string input_context = "";

        if (CheckNullValues(model.CompanyName))
        {
            ViewBag.Message = "You must enter a value for Company name";
            return View("BrandAnalyzer");
        }

        string query_bing = model.CompanyName + " opiniones de usuarios";
        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri(Bingendpoint);

        // Add an Accept header for JSON format.
        client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", BingsubscriptionKey);
        string uri = Bingendpoint + "?q=" + query_bing + "&mkt=es-ES&count=100";
        // List data response.
        HttpResponseMessage response = client.GetAsync(uri).Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        // Above three lines can be replaced with new helper method below
        // string responseBody = await client.GetStringAsync(uri);

        response.EnsureSuccessStatusCode();

        // Parse the response as JSON
        try
        {
            var responsejson = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            // Get the web pages from the response
            var news = responsejson.webPages.value;
            // Iterate over the news items and print them
            foreach (var i in news)
            {
                input_context = input_context + i.name + "\n" + i.snippet + "\n" + i.url + "\n" + "-------";
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        client.Dispose();

        try
        {

            OpenAIClient client_oai = null;
            if (string.IsNullOrEmpty(AOAIsubscriptionKey))
            {
                client_oai = new OpenAIClient(
                    new Uri(AOAIendpoint),
                    new DefaultAzureCredential());
            }
            else
            {
                client_oai = new OpenAIClient(
                    new Uri(AOAIendpoint),
                    new AzureKeyCredential(AOAIsubscriptionKey));
            }

            // ### If streaming is not selected
            Response<ChatCompletions> responseWithoutStream = await client_oai.GetChatCompletionsAsync(
                new ChatCompletionsOptions()
                {
                    DeploymentName = AOAIDeploymentName,
                    Messages =
                    {
                        new ChatRequestSystemMessage(@"I will provide a list results from opinons on the internet about "+model.CompanyName+" Bing search. If "+model.CompanyName+" is not a company what the user is asking for, answer to provide a new Company name. The user will ask you what they want to get from the compay opinions. \n Results: "+ input_context),
                        new ChatRequestUserMessage(model.Prompt),
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
            ViewBag.Message =
                   //"Hate severity: " + (response.Value.HateResult?.Severity ?? 0);
                   results_analisis.Message.Content
                   ;
        }
        catch (RequestFailedException ex)
        {
            throw;
        }

        return View("BrandAnalyzer", model);
        //return Ok(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private bool CheckNullValues(string companyName)
    {
        if (string.IsNullOrEmpty(companyName))
        {
            return true;
        }
        return false;
    }
}