using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Models
{
  [Table("NewsView")]
  public class NewsView
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ViewId { get; set; }

    [Required]
    [StringLength(20)]
    public string NewsArticleId { get; set; } = null!;

    public short? ViewerId { get; set; }

    public DateTime? ViewDate { get; set; }

    [ForeignKey("NewsArticleId")]
    public virtual NewsArticle? NewsArticle { get; set; }

    [ForeignKey("ViewerId")]
    public virtual SystemAccount? Viewer { get; set; }
  }
}
