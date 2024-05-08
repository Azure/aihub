using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Models;

namespace OpenAI.Plugin.FSI
{
    public class CompareProductsPlugin
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private readonly ILogger _logger;
        private readonly Kernel _kernel;

        public CompareProductsPlugin(ILoggerFactory loggerFactory, Kernel kernel)
        {
            _logger = loggerFactory.CreateLogger<CompareProductsPlugin>();
            _kernel = kernel;
        }

        [Function("Compare Financial Products Plugin")]
        [OpenApiOperation(operationId: "CompareProductsPlugin", tags: new[] { "CompareProductsPlugin" }, Description = "Compares a given financial product with those avialable in the market")]
        [OpenApiRequestBody("application/json", typeof(ExecuteFunctionRequest), Description = "Variables to use when executing the specified function.", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ExecuteFunctionResponse), Description = "Returns the response from the AI.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Returned if the request body is invalid.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Returned if the semantic function could not be found.")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "plugins/compare")] HttpRequestData req,
            FunctionContext executionContext)
        {
#pragma warning disable CA1062
            var functionRequest = await JsonSerializer.DeserializeAsync<ExecuteFunctionRequest>(req.Body, _jsonOptions).ConfigureAwait(false);
#pragma warning disable CA1062
            if (functionRequest == null)
            {
                return await CreateResponseAsync(req, HttpStatusCode.BadRequest, new ErrorResponse() { Message = $"Invalid request body {functionRequest}" }).ConfigureAwait(false);
            }

            try
            {
                // Retrieve the chat completion service from the kernel
                IChatCompletionService chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

                // Create the chat history
                ChatHistory chatMessages = new ChatHistory("""
                GET THE TOP FINANCIAL PRODUCTS IN SPAIN IN TERMS OF PROFITABILITY
                """);

                // Get the chat completions
                OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
                {
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                };

                var searchResult = await chatCompletionService.GetChatMessageContentAsync(
                    chatMessages,
                    executionSettings: openAIPromptExecutionSettings,
                    kernel: _kernel);

                var context = new KernelArguments
                {
                    { "products", searchResult.ToString() },
                    { "product", functionRequest.Product },
                };

                var result = await _kernel.InvokeAsync("Prompts", "CompareProducts", context).ConfigureAwait(false);

                return await CreateResponseAsync(
                    req,
                    HttpStatusCode.OK,
                    new ExecuteFunctionResponse() { Response = result.ToString() }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return await CreateResponseAsync(req, HttpStatusCode.BadRequest, new ErrorResponse() { Message = ex.Message }).ConfigureAwait(false);
            }
        }

        private static async Task<HttpResponseData> CreateResponseAsync(HttpRequestData requestData, HttpStatusCode statusCode, object responseBody)
        {
            var responseData = requestData.CreateResponse(statusCode);
            await responseData.WriteAsJsonAsync(responseBody).ConfigureAwait(false);
            return responseData;
        }
    }
}
