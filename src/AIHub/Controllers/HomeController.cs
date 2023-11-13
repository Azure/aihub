using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MVCWeb.Models;
using Azure.AI.ContentSafety;
using Azure;
using ContentSafetySampleCode;
using System;
using Azure.Storage.Blobs;
using Azure.Identity;

namespace MVCWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _config;
    private readonly ContentSafetyClient _client;
    
    private string endpoint;
    private string subscriptionKey;
    private string storageconnstring;


    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;

    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}