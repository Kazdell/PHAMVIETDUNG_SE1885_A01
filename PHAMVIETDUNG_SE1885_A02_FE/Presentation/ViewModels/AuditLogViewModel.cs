namespace PHAMVIETDUNG_SE1885_A02_FE.Presentation.ViewModels;

public class AuditLogViewModel
{
  public int AuditLogID { get; set; }
  public string UserEmail { get; set; } = string.Empty;
  public string Action { get; set; } = string.Empty;
  public string TableName { get; set; } = string.Empty;
  public string? RecordID { get; set; }
  public DateTime Timestamp { get; set; }
  public string? Details { get; set; }
}

public class AuditLogPagedResult
{
  public int TotalCount { get; set; }
  public int Page { get; set; }
  public int PageSize { get; set; }
  public int TotalPages { get; set; }
  public List<AuditLogViewModel> Data { get; set; } = new();
}
