using OpenAI.Chat;

namespace MVCWeb.Controllers;

public class CallCenterController : Controller
{
    private CallCenterModel model;
    private string endpoint;
    private string subscriptionKey;
    private string AOAIDeploymentName;


    public CallCenterController(IConfiguration config)
    {
        endpoint = config.GetValue<string>("CallCenter:OpenAIEndpoint") ?? throw new ArgumentNullException("OpenAIEndpoint");
        subscriptionKey = config.GetValue<string>("CallCenter:OpenAISubscriptionKey") ?? throw new ArgumentNullException("OpenAISubscriptionKey");
        AOAIDeploymentName = config.GetValue<string>("CallCenter:DeploymentName") ?? throw new ArgumentNullException("DeploymentName");
        model = new CallCenterModel();
    }

    public IActionResult CallCenter()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AnalyzeCall()
    {
        model.Transcript = HttpContext.Request.Form["Transcript"];
        model.Prompt = HttpContext.Request.Form["prompt"];

        if (CheckNullValues(model.Transcript, model.Prompt))
        {
            ViewBag.Message = "You must enter both a transcript and a prompt";
            return View("CallCenter", model);
        }
        try
        {
            Uri aoaiEndpointUri = new(endpoint);

            AzureOpenAIClient azureClient = string.IsNullOrEmpty(subscriptionKey)
              ? new(aoaiEndpointUri, new DefaultAzureCredential())
              : new(aoaiEndpointUri, new AzureKeyCredential(subscriptionKey));

            ChatClient chatClient = azureClient.GetChatClient(AOAIDeploymentName);

            var messages = new ChatMessage[]
            {
                new SystemChatMessage(model.Prompt),
                new UserChatMessage(@"Call transcript: "+model.Transcript),
            };

            ChatCompletionOptions chatCompletionOptions = new()
            {
                MaxTokens = 1000,
                Temperature = 0.1f,
                FrequencyPenalty = 0,
                PresencePenalty = 0,
                TopP = 0.95f,
            };

            ChatCompletion completion = await chatClient.CompleteChatAsync(messages, chatCompletionOptions);

            ViewBag.Message = completion.Content[0].Text;
        }
        catch (RequestFailedException)
        {
            throw;
        }

        return View("CallCenter", model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private static bool CheckNullValues(string? companyName, string? prompt)
    {
        if (string.IsNullOrEmpty(companyName))
        {
            return true;
        }
        if (string.IsNullOrEmpty(prompt))
        {
            return true;
        }
        return false;
    }
}