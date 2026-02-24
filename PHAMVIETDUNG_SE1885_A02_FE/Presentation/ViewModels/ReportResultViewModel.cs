using PHAMVIETDUNG_SE1885_A02_FE.Presentation.ViewModels;

namespace PHAMVIETDUNG_SE1885_A02_FE.Presentation.ViewModels;

public class ReportResultViewModel
{
  public List<NewsArticleReportViewModel> Articles { get; set; } = new List<NewsArticleReportViewModel>();
  public int TotalRecords { get; set; }
  public int ActiveCount { get; set; }
  public int InactiveCount { get; set; }
  public List<GroupedStat> GroupedByStatus { get; set; } = new List<GroupedStat>();
}
