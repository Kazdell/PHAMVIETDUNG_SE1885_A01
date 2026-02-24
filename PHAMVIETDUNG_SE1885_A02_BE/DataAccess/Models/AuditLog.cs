using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Models;

[Table("AuditLog")]
public class AuditLog
{
    [Key]
    public int AuditLogID { get; set; }

    [Required]
    [StringLength(100)]
    public string UserEmail { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string Action { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string TableName { get; set; } = null!;

    [StringLength(50)]
    public string? RecordID { get; set; }

    public DateTime Timestamp { get; set; }

    public string? Details { get; set; }
}
