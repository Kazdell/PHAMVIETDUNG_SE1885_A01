using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models
{
    [Table("NewsTag")]
    [PrimaryKey(nameof(NewsArticleId), nameof(TagId))]
    public class NewsTag
    {
        [Key]
        [Column(Order = 0)]
        public string NewsArticleId { get; set; } = null!;

        [Key]
        [Column(Order = 1)]
        public int TagId { get; set; }

        [ForeignKey("NewsArticleId")]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual NewsArticle NewsArticle { get; set; } = null!;

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; } = null!;
    }
}
