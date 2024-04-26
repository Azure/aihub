// Copyright (c) Microsoft. All rights reserved.
namespace Models;

#pragma warning disable CA1056
#pragma warning disable CA1034

public class AIPluginSettings
{
    public const string DefaultConfigFile = "aiplugin.json";

    [JsonPropertyName("schema_version")]
    public string SchemaVersion { get; set; } = "v1";

    [JsonPropertyName("name_for_model")]
    public string NameForModel { get; set; } = string.Empty;

    [JsonPropertyName("name_for_human")]
    public string NameForHuman { get; set; } = string.Empty;

    [JsonPropertyName("description_for_model")]
    public string DescriptionForModel { get; set; } = string.Empty;

    [JsonPropertyName("description_for_human")]
    public string DescriptionForHuman { get; set; } = string.Empty;

    [JsonPropertyName("auth")]
    public AuthModel Auth { get; set; } = new AuthModel();

    [JsonPropertyName("api")]
    public ApiModel Api { get; set; } = new ApiModel();

    [JsonPropertyName("logo_url")]
    public string LogoUrl { get; set; } = string.Empty;

    [JsonPropertyName("contact_email")]
    public string ContactEmail { get; set; } = string.Empty;

    [JsonPropertyName("legal_info_url")]
    public string LegalInfoUrl { get; set; } = string.Empty;


    public class AuthModel
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("authorization_url")]
        public string AuthorizationType { get; set; } = string.Empty;
    }

    public class ApiModel
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "openapi";

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("has_user_authentication")]
        public bool HasUserAuthentication { get; set; } = false;
    }

    public static AIPluginSettings FromFile(string configFile = DefaultConfigFile)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName)
            .AddJsonFile(configFile, optional: false, reloadOnChange: true)
            .Build();

        return configuration.Get<AIPluginSettings>()
               ?? throw new InvalidDataException($"Invalid app settings in '{configFile}', please provide configuration settings using instructions in the README.");
    }
}