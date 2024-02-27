namespace MVCWeb.Controllers;

public class DashboardController : Controller
{
    private readonly ILogger<DashboardController> _logger;
    private readonly IConfiguration _config;
    private readonly ContentSafetyClient _client;

    private string endpoint;
    private string subscriptionKey;
    private string storageconnstring;


    public DashboardController(ILogger<DashboardController> logger)
    {
        _logger = logger;

    }

    public IActionResult Banking()
    {
        return View();
    }

    public IActionResult Industry()
    {
        return View();
    }

    public IActionResult Utilities()
    {
        return View();
    }

    public IActionResult Insurance()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}