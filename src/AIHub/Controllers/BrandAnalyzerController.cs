using OpenAI.Chat;
using Azure.AI.Projects;

namespace MVCWeb.Controllers;

public class BrandAnalyzerController : Controller
{
    private BrandAnalyzerModel model;
    private string connectionString;
    private string modelDeploymentName;
    private string bingConnectionName;
    private AIProjectClient projectClient;
    private AgentsClient agentClient;

    public BrandAnalyzerController(IConfiguration config, IHttpClientFactory clientFactory)
    {
        connectionString = System.Environment.GetEnvironmentVariable("AI_FOUNDRY_PROJECT_CONNECTION_STRING");
        modelDeploymentName = System.Environment.GetEnvironmentVariable("AI_SERVICES_MODEL_DEPLOYMENT_NAME");
        bingConnectionName = System.Environment.GetEnvironmentVariable("BING_CONNECTION_NAME");

        projectClient = new AIProjectClient(connectionString, new DefaultAzureCredential());

        agentClient = projectClient.GetAgentsClient();
    }

    public IActionResult BrandAnalyzer()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AnalyzeCompany()
    {
        if (CheckNullValues(model.CompanyName))
        {
            ViewBag.Message = "You must enter a value for Company name";
            return View("BrandAnalyzer");
        }

        model.CompanyName = HttpContext.Request.Form["companyName"];
        model.Prompt = HttpContext.Request.Form["prompt"];

        ConnectionResponse  bingConnection = await projectClient.GetConnectionsClient().GetConnectionAsync(bingConnectionName);
        var connectionId = bingConnection.Id;

        ToolConnectionList connectionList = new()
        {
            ConnectionList = { new ToolConnection(connectionId) }
        };
        BingGroundingToolDefinition bingGroundingTool = new(connectionList);

        Agent agent = await agentClient.CreateAgentAsync(
            model: modelDeploymentName,
            name: "my-assistant",
            instructions: $"You will provide a list results from opinions on the Internet about {model.CompanyName} Bing search. If {model.CompanyName} is not a company what the user is asking for, answer to provide a new Company name. The user will ask you what they want to get from the company opinions.",
            tools: [bingGroundingTool]);

        var agentId = agent.Id; AgentThread thread = await agentClient.CreateThreadAsync();

        // Create message to thread
        ThreadMessage message = await agentClient.CreateMessageAsync(
            thread.Id,
            MessageRole.User,
            model.Prompt);

        // Run the agent
        ThreadRun run = await agentClient.CreateRunAsync(thread, agent);
        do
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            run = await agentClient.GetRunAsync(thread.Id, run.Id);
        }
        while (run.Status == RunStatus.Queued
            || run.Status == RunStatus.InProgress);

        PageableList<ThreadMessage> messages = await agentClient.GetMessagesAsync(
            threadId: thread.Id,
            order: ListSortOrder.Ascending
        );

        StringBuilder sb = new();

        foreach (ThreadMessage threadMessage in messages)
        {
            foreach (MessageContent contentItem in threadMessage.ContentItems)
            {
                if (contentItem is MessageTextContent textItem)
                {
                    string response = textItem.Text;
                    if (textItem.Annotations != null)
                    {
                        foreach (MessageTextAnnotation annotation in textItem.Annotations)
                        {
                            if (annotation is MessageTextUrlCitationAnnotation urlAnnotation)
                            {
                                response = response.Replace(urlAnnotation.Text, $" [{urlAnnotation.UrlCitation.Title}]({urlAnnotation.UrlCitation.Url})");
                            }
                        }
                    }
                    sb.AppendLine(response);

                }
            }
        }

        ViewBag.Message = sb.ToString();

        return View("BrandAnalyzer", model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private static bool CheckNullValues(string? companyName)
    {
        return string.IsNullOrEmpty(companyName);
    }
}