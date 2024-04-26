OpenApiConfigurationOptions configOptions = new()
{
    Info = new OpenApiInfo()
    {
        Version = "1.0.0",
        Title = "Call Transcript Plugin",
        Description = "This is a plugin that analyze a call given the transcript and a prompt.",
    },
    Servers = DefaultOpenApiConfigurationOptions.GetHostNames(),
    OpenApiVersion = OpenApiVersionType.V3,
    ForceHttps = false,
    ForceHttp = false,
};

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
        {
            services.AddSingleton<IOpenApiConfigurationOptions>(_ => configOptions);
            services.AddTransient((provider) => CreateKernel(provider));
        })
    .Build();

host.Run();

Kernel CreateKernel(IServiceProvider provider)
{
    const string DefaultSemanticPromptsFolder = "Prompts";
    string semanticPromptsFolder = Environment.GetEnvironmentVariable("SEMANTIC_PLUGINS_FOLDER") ?? DefaultSemanticPromptsFolder;
    var modelId = Environment.GetEnvironmentVariable("MODEL_ID")!;
    var endpoint = Environment.GetEnvironmentVariable("ENDPOINT")!;
    var apiKey = Environment.GetEnvironmentVariable("API_KEY")!;
    var bingKey = Environment.GetEnvironmentVariable("BING_KEY")!;

    var builder = Kernel.CreateBuilder();
    builder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Trace).AddDebug());
    builder.Services.AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);
    builder.Plugins.AddFromPromptDirectory(semanticPromptsFolder, "Prompts");
#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    builder.Plugins.AddFromObject(new WebSearchEnginePlugin(new BingConnector(bingKey)));
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    var kernel = builder.Build();
    return kernel;
}
