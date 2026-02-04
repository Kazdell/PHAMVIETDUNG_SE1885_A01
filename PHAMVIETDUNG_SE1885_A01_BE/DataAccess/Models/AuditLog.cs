using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;

[Table("AuditLog")]
public class AuditLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string UserEmail { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string Action { get; set; } = null!; // Create, Update, Delete

    [Required]
    [StringLength(100)]
    public string EntityName { get; set; } = null!;

    public string? EntityId { get; set; }

    public DateTime Timestamp { get; set; }

    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
}
