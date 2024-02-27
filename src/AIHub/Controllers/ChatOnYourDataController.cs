namespace MVCWeb.Controllers;

public class ChatOnYourDataController : Controller
{
    private readonly ILogger<ChatOnYourDataController> _logger;

    public ChatOnYourDataController(ILogger<ChatOnYourDataController> logger)
    {
        _logger = logger;
    }

    public IActionResult ChatOnYourData()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}