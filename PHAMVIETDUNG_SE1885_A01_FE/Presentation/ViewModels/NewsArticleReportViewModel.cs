namespace PHAMVIETDUNG_SE1885_A01_FE.Presentation.ViewModels;

public class NewsArticleReportViewModel
{
    public string NewsArticleId { get; set; } = null!;
    public string? NewsTitle { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? Headline { get; set; }
    public string? NewsContent { get; set; }
    public string? NewsSource { get; set; }
    public short? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool? NewsStatus { get; set; }
    public short? CreatedById { get; set; }
    public string? CreatedByName { get; set; }
    public short? UpdatedById { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class GroupedStat
{
    public string Key { get; set; } = string.Empty;
    public int Count { get; set; }
}
