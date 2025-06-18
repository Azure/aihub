namespace MVCWeb.Controllers;

public class ChatOnYourDataController : Controller
{
    private readonly ILogger<ChatOnYourDataController> _logger;
    private readonly IConfiguration _configuration;

    public ChatOnYourDataController(ILogger<ChatOnYourDataController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public IActionResult ChatOnYourData()
    {
        var model = new ChatOnYourDataModel
        {
            Link = _configuration.GetValue<string>("ChatOnYourData:Link") ?? string.Empty,
            DefaultQuestions = _configuration.GetSection("ChatOnYourData:DefaultQuestions").Get<List<string>>() ?? new List<string>()
        };
        
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}