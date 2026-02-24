using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Models
{
  [Table("Tag")]
  public class Tag
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int TagId { get; set; }

    [Required]
    [StringLength(100)]
    public string? TagName { get; set; }

    [StringLength(400)]
    public string? Note { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ICollection<NewsTag> NewsTags { get; set; } = new List<NewsTag>();
  }
}
