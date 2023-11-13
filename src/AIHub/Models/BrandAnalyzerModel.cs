namespace MVCWeb.Models;

public class BrandAnalyzerModel{
    
    public int? Severity { get; set; }
    public int? Violence { get; set; }
    public int? SelfHarm { get; set; }
    public int? Hate { get; set; }
    public string? CompanyName { get; set; }
    public string? Prompt { get; set; }
    public string? Message { get; set; }

}