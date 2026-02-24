using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Models
{
  [Table("Notification")]
  public class Notification
  {
    [Key]
    public int NotificationId { get; set; }

    [Required]
    public short AccountId { get; set; }

    [Required]
    [StringLength(100)]
    public string? Title { get; set; }

    [Required]
    [StringLength(500)]
    public string? Message { get; set; }

    public DateTime? CreatedDate { get; set; } = DateTime.Now;

    public bool? IsRead { get; set; } = false;

    [StringLength(20)]
    public string? ArticleId { get; set; } // Link to article for clickable notifications

    [ForeignKey("AccountId")]
    public virtual SystemAccount? SystemAccount { get; set; }
  }
}
