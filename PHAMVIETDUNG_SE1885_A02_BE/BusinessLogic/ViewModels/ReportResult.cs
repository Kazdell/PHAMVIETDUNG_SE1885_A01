using PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Models;

namespace PHAMVIETDUNG_SE1885_A02_BE.BusinessLogic.ViewModels;

public class ReportResult
{
    public List<NewsArticleReportViewModel> Articles { get; set; } = new List<NewsArticleReportViewModel>();
    public int TotalRecords { get; set; }
    public int ActiveCount { get; set; }
    public int InactiveCount { get; set; }
    public List<GroupedStat> GroupedByStatus { get; set; } = new List<GroupedStat>();
}

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
