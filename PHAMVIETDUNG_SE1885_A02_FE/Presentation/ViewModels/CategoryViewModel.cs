using System.ComponentModel.DataAnnotations;

namespace PHAMVIETDUNG_SE1885_A02_FE.Presentation.ViewModels;

public class CategoryViewModel
{
  public short CategoryId { get; set; }

  [Required]
  [StringLength(100)]
  public string? CategoryName { get; set; }

  [Required]
  [StringLength(250)]
  public string? CategoryDesciption { get; set; } // Note: Typo 'Desciption' matches Entity

  public short? ParentCategoryId { get; set; }

  [System.Text.Json.Serialization.JsonConverter(typeof(NullableBooleanConverter))]
  public bool IsActive { get; set; } = true;

  public int ArticleCount { get; set; }
}
