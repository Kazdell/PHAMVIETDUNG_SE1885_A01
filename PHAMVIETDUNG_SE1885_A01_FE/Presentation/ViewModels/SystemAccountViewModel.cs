using System.ComponentModel.DataAnnotations;

namespace PHAMVIETDUNG_SE1885_A01_FE.Presentation.ViewModels;

public class SystemAccountViewModel
{
    public short AccountId { get; set; }

    [Required]
    [StringLength(100)]
    public string? AccountName { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string? AccountEmail { get; set; }

    [Required]
    public int AccountRole { get; set; }

    [StringLength(100)]
    public string? AccountPassword { get; set; } // Only for Create/Update if needed
    
    public string? OldPassword { get; set; } // For Profile Update Verification
    public string? NewPassword { get; set; } // For Profile Update Verification
}
