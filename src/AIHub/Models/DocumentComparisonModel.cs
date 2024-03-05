namespace MVCWeb.Models;

public class DocumentComparisonModel
{
    public string? Prompt { get; set; }
    public string? Document { get; set; }
    public string? Message { get; set; }
    public string? PdfUrl1 { get; set; }
    public string? PdfUrl2 { get; set; }
    public string? tabName1 { get; set; }
    public string? tabName2 { get; set; }
}