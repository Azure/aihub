using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Models;

namespace OpenAI.Plugin
{
    public class CallTranscriptPlugin
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private readonly ILogger _logger;
        private readonly Kernel _kernel;
        private readonly string _endpoint = Environment.GetEnvironmentVariable("ENDPOINT")!;
        private readonly string _deploymentName = Environment.GetEnvironmentVariable("MODEL_ID")!;
        private readonly string _subscriptionKey = Environment.GetEnvironmentVariable("API_KEY")!;

        public CallTranscriptPlugin(ILoggerFactory loggerFactory, Kernel kernel)
        {
            _logger = loggerFactory.CreateLogger<CallTranscriptPlugin>();
            _kernel = kernel;
        }

        [Function("Call Transcript Plugin")]
        [OpenApiOperation(operationId: "CallTranscriptPlugin", tags: new[] { "CallTranscriptPlugin" }, Description = "Used to analyze a call given the transcript and a prompt")]
        [OpenApiRequestBody("application/json", typeof(ExecuteFunctionRequest), Description = "Variables to use when executing the specified function.", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ExecuteFunctionResponse), Description = "Returns the response from the AI.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Returned if the request body is invalid.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Returned if the semantic function could not be found.")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "plugins/transcript")] HttpRequestData req,
            FunctionContext executionContext)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

#pragma warning disable CA1062
            var functionRequest = await JsonSerializer.DeserializeAsync<ExecuteFunctionRequest>(req.Body, _jsonOptions).ConfigureAwait(false);
#pragma warning disable CA1062
            if (functionRequest == null)
            {
                return await CreateResponseAsync(req, HttpStatusCode.BadRequest, new ErrorResponse() { Message = $"Invalid request body {functionRequest}" }).ConfigureAwait(false);
            }

            try
            {
                var context = new KernelArguments
                {
                    { "transcript", functionRequest.Transcript }
                };

                var result = await _kernel.InvokeAsync("Prompts", "CallAnalyzer", context).ConfigureAwait(false);

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
