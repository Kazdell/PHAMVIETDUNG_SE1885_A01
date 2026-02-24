using System.ComponentModel.DataAnnotations;

namespace PHAMVIETDUNG_SE1885_A02_FE.Presentation.ViewModels;

public class TagViewModel
{
  public int TagId { get; set; }

  [Required]
  [StringLength(50)]
  public string? TagName { get; set; }

  [StringLength(400)]
  public string? Note { get; set; }
}
