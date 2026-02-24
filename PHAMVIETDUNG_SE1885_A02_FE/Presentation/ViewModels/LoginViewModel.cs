using System.ComponentModel.DataAnnotations;

namespace PHAMVIETDUNG_SE1885_A02_FE.Presentation.ViewModels;

public class LoginViewModel
{
  [Required]
  [EmailAddress]
  public string Email { get; set; } = null!;

  [Required]
  [DataType(DataType.Password)]
  public string Password { get; set; } = null!;
}
