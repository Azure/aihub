namespace MVCWeb.Models;

public class FormAnalyzerModel
{

    public int? Severity { get; set; }
    public int? Violence { get; set; }
    public int? SelfHarm { get; set; }
    public int? Hate { get; set; }
    public string? Prompt { get; set; }
    public string? Image { get; set; }
    public string? Message { get; set; }

}