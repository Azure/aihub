namespace MVCWeb.Controllers;

public class BrandAnalyzerController : Controller
{
    private BrandAnalyzerModel model;
    private string Bingendpoint;
    private string BingsubscriptionKey;
    private string AOAIendpoint;
    private string AOAIsubscriptionKey;
    private string AOAIDeploymentName;
    private HttpClient httpClient;

    public BrandAnalyzerController(IConfiguration config, IHttpClientFactory clientFactory)
    {
        Bingendpoint = config.GetValue<string>("BrandAnalyzer:BingEndpoint") ?? throw new ArgumentNullException("BingEndpoint");
        BingsubscriptionKey = config.GetValue<string>("BrandAnalyzer:BingKey") ?? throw new ArgumentNullException("BingKey");
        AOAIendpoint = config.GetValue<string>("BrandAnalyzer:OpenAIEndpoint") ?? throw new ArgumentNullException("OpenAIEndpoint");
        AOAIsubscriptionKey = config.GetValue<string>("BrandAnalyzer:OpenAISubscriptionKey") ?? throw new ArgumentNullException("OpenAISubscriptionKey");
        AOAIDeploymentName = config.GetValue<string>("BrandAnalyzer:DeploymentName") ?? throw new ArgumentNullException("DeploymentName");
        model = new BrandAnalyzerModel();
        httpClient = clientFactory.CreateClient();
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
        httpClient.BaseAddress = new Uri(Bingendpoint);

        // Add an Accept header for JSON format.
        httpClient.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", BingsubscriptionKey);
        string uri = Bingendpoint + "?q=" + query_bing + "&mkt=es-ES&count=100";
        // List data response.
        HttpResponseMessage response = httpClient.GetAsync(uri).Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        // Above three lines can be replaced with new helper method below
        // string responseBody = await client.GetStringAsync(uri);

        response.EnsureSuccessStatusCode();

        // Parse the response as JSON
        try
        {
            var responsejson = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync())!;

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
            ViewBag.Message = results_analisis.Message.Content;
        }
        catch (RequestFailedException)
        {
            throw;
        }

        return View("BrandAnalyzer", model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private bool CheckNullValues(string? companyName)
    {
        if (string.IsNullOrEmpty(companyName))
        {
            return true;
        }
        return false;
    }
}