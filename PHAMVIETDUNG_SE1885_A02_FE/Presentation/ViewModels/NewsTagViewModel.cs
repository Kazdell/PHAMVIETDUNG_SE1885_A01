namespace PHAMVIETDUNG_SE1885_A02_FE.Presentation.ViewModels
{
  public class NewsTagViewModel
  {
    public required string NewsArticleId { get; set; }
    public int TagId { get; set; }
    public required TagViewModel Tag { get; set; }
  }
}
