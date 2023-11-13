namespace MVCWeb.Models;

public class ContentSafetyModel{
    
    public ContentSafetyModel()
    {
        Severity = 1; // Default value
        Violence = 1; // Default value
        SelfHarm = 1; // Default value
        Hate = 1; // Default value
    }
    public int? Severity { get; set; }
    public int? Violence { get; set; }
    public int? SelfHarm { get; set; }
    public int? Hate { get; set; }
    public string? Text { get; set; }
    public string? Image { get; set; }
    public string? Message { get; set; }
    public bool? Approve { get; set; }

}