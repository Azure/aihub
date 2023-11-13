//
// Copyright (c) Microsoft. All rights reserved.
// To learn more, please visit the documentation - Quickstart: Azure Content Safety: https://aka.ms/acsstudiodoc
//

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ContentSafetySampleCode
{
    // Enumeration for media types
    public enum MediaType
    {
        Text = 0,
        Image = 1
    }

    // Enumeration for categories
    public enum Category
    {
        Hate = 0,
        SelfHarm = 1,
        Sexual = 2,
        Violence = 3
    }

    // Enumeration for actions
    public enum Action
    {
        Accept = 0,
        Reject = 1
    }

    /// <summary>
    /// Exception raised when there is an error in detecting the content.
    /// </summary>
    public class DetectionException : Exception
    {
        public string Code { get; set; }

        /// <summary>
        /// Constructor for the DetectionException class.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="message">The error message.</param>
        public DetectionException(string code, string message) : base(message)
        {
            Code = code;
        }
    }

    /// <summary>
    /// Class representing the decision made by the content moderation system.
    /// </summary>
    public class Decision
    {
        public Action SuggestedAction { get; set; }
        public Dictionary<Category, Action> ActionByCategory { get; set; }

        /// <summary>
        /// Constructor for the Decision class.
        /// </summary>
        /// <param name="suggestedAction">The overall action suggested by the system.</param>
        /// <param name="actionByCategory">The actions suggested by the system for each category.</param>
        public Decision(Action suggestedAction, Dictionary<Category, Action> actionByCategory)
        {
            SuggestedAction = suggestedAction;
            ActionByCategory = actionByCategory;
        }
    }

    /// <summary>
    /// Base class for detection requests.
    /// </summary>
    public class DetectionRequest
    {
    }

    /// <summary>
    /// Class representing an image.
    /// </summary>
    public class Image
    {
        public string Content { get; set; }

        /// <summary>
        /// Constructor for the Image class.
        /// </summary>
        /// <param name="content">The base64-encoded content of the image.</param>
        public Image(string content)
        {
            Content = content;
        }
    }

    /// <summary>
    /// Class representing an image detection request.
    /// </summary>
    public class ImageDetectionRequest : DetectionRequest
    {
        public Image Image { get; set; }

        /// <summary>
        /// Constructor for the ImageDetectionRequest class.
        /// </summary>
        /// <param name="content">The base64-encoded content of the image.</param>
        public ImageDetectionRequest(string content)
        {
            Image = new Image(content);
        }
    }

    /// <summary>
    /// Class representing a text detection request.
    /// </summary>
    public class TextDetectionRequest : DetectionRequest
    {
        public string Text { get; set; }
        public string[] BlocklistNames { get; set; }

        /// <summary>
        /// Constructor for the TextDetectionRequest class.
        /// </summary>
        /// <param name="text">The text to be detected.</param>
        /// <param name="blocklistNames">The names of the blocklists to use for detecting the text.</param>
        public TextDetectionRequest(string text, string[] blocklistNames)
        {
            Text = text;
            BlocklistNames = blocklistNames;
        }
    }

    /// <summary>
    /// Class representing a detailed detection result for a specific category.
    /// </summary>
    public class CategoriesAnalysis
    {
        /// <summary>
        /// The category of the detection result.
        /// </summary>
        public string? Category { get; set; }
        /// <summary>
        /// The severity of the detection result.
        /// </summary>
        public int? Severity { get; set; }
    }

    /// <summary>
    /// Base class for detection result.
    /// </summary>
    public class DetectionResult
    {
        /// <summary>
        /// The detailed result for categories analysis.
        /// </summary>
        public List<CategoriesAnalysis>? CategoriesAnalysis { get; set; }
    }

    /// <summary>
    /// Class representing an image detection result.
    /// </summary>
    public class ImageDetectionResult : DetectionResult
    {
        
    }

    /// <summary>
    /// Class representing a detailed detection result for a blocklist match.
    /// </summary>
    public class BlocklistsMatch
    {
        /// <summary>
        /// The name of the blocklist.
        /// </summary>
        public string? BlocklistName { get; set; }
        /// <summary>
        /// The ID of the block item that matched.
        /// </summary>
        public string? BlocklistItemId { get; set; }
        /// <summary>
        /// The text of the block item that matched.
        /// </summary>
        public string? BlocklistItemText { get; set; }
    }

    /// <summary>
    /// Class representing a text detection result.
    /// </summary>
    public class TextDetectionResult : DetectionResult
    {
        /// <summary>
        /// The list of detailed results for blocklist matches.
        /// </summary>
        public List<BlocklistsMatch>? BlocklistsMatch { get; set; }
    }

    /// <summary>
    /// Class representing a detection error response.
    /// </summary>
    public class DetectionErrorResponse
    {
        /// <summary>
        /// The detection error.
        /// </summary>
        public DetectionError? error { get; set; }
    }

    /// <summary>
    /// Class representing a detection error.
    /// </summary>
    public class DetectionError
    {
        /// <summary>
        /// The error code.
        /// </summary>
        public string? code { get; set; }
        /// <summary>
        /// The error message.
        /// </summary>
        public string? message { get; set; }
        /// <summary>
        /// The error target.
        /// </summary>
        public string? target { get; set; }
        /// <summary>
        /// The error details.
        /// </summary>
        public string[]? details { get; set; }
        /// <summary>
        /// The inner error.
        /// </summary>
        public DetectionInnerError? innererror { get; set; }
    }

    /// <summary>
    /// Class representing a detection inner error.
    /// </summary>
    public class DetectionInnerError
    {
        /// <summary>
        /// The inner error code.
        /// </summary>
        public string? code { get; set; }
        /// <summary>
        /// The inner error message.
        /// </summary>
        public string? innererror { get; set; }
    }

    public class ContentSafety
    {
        public string Endpoint { get; set; }
        public string SubscriptionKey { get; set; }

        /// <summary>
        /// The version of the Content Safety API to use.
        /// </summary>
        public static readonly string API_VERSION = "2023-10-01";

        /// <summary>
        /// The valid threshold values.
        /// </summary>
        public static readonly int[] VALID_THRESHOLD_VALUES = { -1, 0, 2, 4, 6 };

        /// <summary>
        /// The HTTP client.
        /// </summary>
        public static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// The JSON serializer options.
        /// </summary>
        public static readonly JsonSerializerOptions options =
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                        Converters = { new JsonStringEnumConverter() } };

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentSafety"/> class.
        /// </summary>
        /// <param name="endpoint">The endpoint URL for the Content Safety API.</param>
        /// <param name="subscriptionKey">The subscription key for the Content Safety API.</param>
        public ContentSafety(string endpoint, string subscriptionKey)
        {
            Endpoint = endpoint;
            SubscriptionKey = subscriptionKey;
        }

        /// <summary>
        /// Builds the URL for the Content Safety API based on the media type.
        /// </summary>
        /// <param name="mediaType">The type of media to analyze.</param>
        /// <returns>The URL for the Content Safety API.</returns>
        public string BuildUrl(MediaType mediaType)
        {
            switch (mediaType)
            {
            case MediaType.Text:
                return $"{Endpoint}/contentsafety/text:analyze?api-version={API_VERSION}";
            case MediaType.Image:
                return $"{Endpoint}/contentsafety/image:analyze?api-version={API_VERSION}";
            default:
                throw new ArgumentException($"Invalid Media Type {mediaType}");
            }
        }

        /// <summary>
        /// Builds the request body for the Content Safety API request.
        /// </summary>
        /// <param name="mediaType">The type of media to analyze.</param>
        /// <param name="content">The content to analyze.</param>
        /// <param name="blocklists">The blocklists to use for text analysis.</param>
        /// <returns>The request body for the Content Safety API request.</returns>
        public DetectionRequest BuildRequestBody(MediaType mediaType, string content, string[] blocklists)
        {
            switch (mediaType)
            {
            case MediaType.Text:
                return new TextDetectionRequest(content, blocklists);
            case MediaType.Image:
                return new ImageDetectionRequest(content);
            default:
                throw new ArgumentException($"Invalid Media Type {mediaType}");
            }
        }

        /// <summary>
        /// Deserializes the JSON string into a DetectionResult object based on the media type.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="mediaType">The media type of the detection result.</param>
        /// <returns>The deserialized DetectionResult object for the Content Safety API response.</returns>
        public DetectionResult? DeserializeDetectionResult(string json, MediaType mediaType)
        {
            switch (mediaType)
            {
            case MediaType.Text:
                return JsonSerializer.Deserialize<TextDetectionResult>(json, options);
            case MediaType.Image:
                return JsonSerializer.Deserialize<ImageDetectionResult>(json, options);
            default:
                throw new ArgumentException($"Invalid Media Type {mediaType}");
            }
        }

        /// <summary>
        /// Detects unsafe content using the Content Safety API.
        /// </summary>
        /// <param name="mediaType">The media type of the content to detect.</param>
        /// <param name="content">The content to detect.</param>
        /// <param name="blocklists">The blocklists to use for text detection.</param>
        /// <returns>The response from the Content Safety API.</returns>
        public async Task<DetectionResult> Detect(MediaType mediaType, string content, string[] blocklists)
        {
            string url = BuildUrl(mediaType);
            DetectionRequest requestBody = BuildRequestBody(mediaType, content, blocklists);
            string payload = JsonSerializer.Serialize(requestBody, requestBody.GetType(), options);

            var msg = new HttpRequestMessage(HttpMethod.Post, url);
            msg.Content = new StringContent(payload, Encoding.UTF8, "application/json");
            msg.Headers.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);

            HttpResponseMessage response = await client.SendAsync(msg);
            string responseText = await response.Content.ReadAsStringAsync();

            Console.WriteLine((int)response.StatusCode);
            foreach (var header in response.Headers)
            {
                Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }
            Console.WriteLine(responseText);

            if (!response.IsSuccessStatusCode)
            {
                DetectionErrorResponse? error =
                    JsonSerializer.Deserialize<DetectionErrorResponse>(responseText, options);
                if (error == null || error.error == null || error.error.code == null || error.error.message == null)
                {
                    throw new DetectionException(response.StatusCode.ToString(),
                                                 $"Error is null. Response text is {responseText}");
                }
                throw new DetectionException(error.error.code, error.error.message);
            }

            DetectionResult? result = DeserializeDetectionResult(responseText, mediaType);
            if (result == null)
            {
                throw new DetectionException(response.StatusCode.ToString(),
                                             $"HttpResponse is null. Response text is {responseText}");
            }

            return result;
        }

        /// <summary>
        /// Gets the severity score of the specified category from the given detection result.
        /// </summary>
        /// <param name="category">The category to get the severity score for.</param>
        /// <param name="detectionResult">The detection result object to retrieve the severity score from.</param>
        /// <returns>The severity score of the specified category.</returns>
        public int? GetDetectionResultByCategory(Category category, DetectionResult detectionResult)
        {
            int? severityResult = null;
            if (detectionResult.CategoriesAnalysis != null)
            {
                foreach (var detailedResult in detectionResult.CategoriesAnalysis)
                {
                    if (detailedResult.Category == category.ToString())
                    {
                        severityResult = detailedResult.Severity;
                    }
                }
            }

            return severityResult;
        }

        /// <summary>
        /// Makes a decision based on the detection result and the specified reject thresholds.
        /// Users can customize their decision-making method.
        /// </summary>
        /// <param name="detectionResult">The detection result object to make the decision on.</param>
        /// <param name="rejectThresholds">The reject thresholds for each category.</param>
        /// <returns>The decision made based on the detection result and the specified reject thresholds.</returns>
        public Decision MakeDecision(DetectionResult detectionResult, Dictionary<Category, int> rejectThresholds)
        {
            Dictionary<Category, Action> actionResult = new Dictionary<Category, Action>();
            Action finalAction = Action.Accept;
            foreach (KeyValuePair<Category, int> pair in rejectThresholds)
            {
                if (!VALID_THRESHOLD_VALUES.Contains(pair.Value))
                {
                    throw new ArgumentException("RejectThreshold can only be in (-1, 0, 2, 4, 6)");
                }

                int? severity = GetDetectionResultByCategory(pair.Key, detectionResult);
                if (severity == null)
                {
                    throw new ArgumentException($"Can not find detection result for {pair.Key}");
                }

                Action action;
                if (pair.Value != -1 && severity >= pair.Value)
                {
                    action = Action.Reject;
                }
                else
                {
                    action = Action.Accept;
                }
                actionResult[pair.Key] = action;

                if (action.CompareTo(finalAction) > 0)
                {
                    finalAction = action;
                }
            }

            // blocklists
            if (detectionResult is TextDetectionResult textDetectionResult)
            {
                if (textDetectionResult.BlocklistsMatch != null &&
                    textDetectionResult.BlocklistsMatch.Count > 0)
                {
                    finalAction = Action.Reject;
                }
            }

            Console.WriteLine(finalAction);
            foreach (var res in actionResult)
            {
                Console.WriteLine($"Category: {res.Key}, Action: {res.Value}");
            }

            return new Decision(finalAction, actionResult);
        }
    }

    public class Program
    {
        static async Task Main(string[] args)
        {
            // Replace the placeholders with your own values
            string endpoint = "<endpoint>";
            string subscriptionKey = "<subscription_key>";

            // Initialize the ContentSafety object
            ContentSafety contentSafety = new ContentSafety(endpoint, subscriptionKey);

            // Set the media type and blocklists
            MediaType mediaType = MediaType.Image;
            string[] blocklists = {};
            
            // Set the content to be tested
            string content = "<test_content>";

            // Detect content safety
            DetectionResult detectionResult = await contentSafety.Detect(mediaType, content, blocklists);

            // Set the reject thresholds for each category
            Dictionary<Category, int> rejectThresholds = new Dictionary<Category, int> {
                { Category.Hate, 4 }, { Category.SelfHarm, 4 }, { Category.Sexual, 4 }, { Category.Violence, 4 }
            };

            // Make a decision based on the detection result and reject thresholds
            Decision decisionResult = contentSafety.MakeDecision(detectionResult, rejectThresholds);
        }
    }
}