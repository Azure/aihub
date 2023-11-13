using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MVCWeb.Models;
using Azure.AI.ContentSafety;
using Azure;
using ContentSafetySampleCode;
using System;
using Azure.Storage.Blobs;
using Azure.Identity;
using Azure.AI.OpenAI;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace MVCWeb.Controllers;

public class CallCenterController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _config;
    private CallCenterModel model;
    private string endpoint;
    private string subscriptionKey;
    private string storageconnstring;


    public CallCenterController(IConfiguration config)
    {
        _config = config;
        endpoint= _config.GetValue<string>("CallCenter:OpenAIEndpoint");
        subscriptionKey= _config.GetValue<string>("CallCenter:OpenAISubscriptionKey");
        model = new CallCenterModel();

    }

    public IActionResult CallCenter()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AnalyzeCall()
    {
        model.Text = HttpContext.Request.Form["text"];
        model.Prompt = HttpContext.Request.Form["prompt"];

        if (CheckNullValues(model.Text, model.Prompt))
        {
            ViewBag.Message = "You must enter both a transcript and a prompt";
            return View("CallCenter", model);
        }
        try
        {
                        
           OpenAIClient client_oai = new OpenAIClient(
            new Uri(endpoint),
            new AzureKeyCredential(subscriptionKey));

            // ### If streaming is not selected
            Response<ChatCompletions> responseWithoutStream = await client_oai.GetChatCompletionsAsync(
                "DemoBuild",
                new ChatCompletionsOptions()
                {
                    Messages =
                    {
                        new ChatMessage(ChatRole.System, model.Prompt),
                        new ChatMessage(ChatRole.User, @"Call transcript: "+model.Text),
                    },
                    Temperature = (float)0.1,
                    MaxTokens = 1000,
                    NucleusSamplingFactor = (float)0.95,
                    FrequencyPenalty = 0,
                    PresencePenalty = 0,
                });

            ChatCompletions completions = responseWithoutStream.Value;
            ChatChoice results_analisis= completions.Choices[0];
            System.Console.WriteLine(results_analisis);    
            ViewBag.Message = 
                   results_analisis.Message.Content
                   ;
        }
        catch (RequestFailedException ex)
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

    private bool CheckNullValues(string companyName, string prompt)
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