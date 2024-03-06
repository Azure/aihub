namespace MVCWeb.Models;

public class ImageAnalyzerModel
{

    public int? Severity { get; set; }
    public int? Violence { get; set; }
    public int? SelfHarm { get; set; }
    public int? Hate { get; set; }
    public string? Text { get; set; }
    public string? Image { get; set; }
    public string? Message { get; set; }
    public string? Prompt { get; set; }

}