using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models
{
    [Table("NewsArticle")]
    public class NewsArticle
    {
        [Key]
        [StringLength(20)]
        public string NewsArticleId { get; set; } = null!;

        [Required]
        [StringLength(400)] 
        public string? NewsTitle { get; set; }

        [Required]
        public DateTime? CreatedDate { get; set; }

        [Required]
        [StringLength(150)]
        public string? Headline { get; set; }

        [Required]

        public string? NewsContent { get; set; }

        [StringLength(400)]
        public string? NewsSource { get; set; }

        public short? CategoryId { get; set; }

        public bool? NewsStatus { get; set; } 

        public short? CreatedById { get; set; }

        public short? UpdatedById { get; set; }

        public DateTime? ModifiedDate { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [ForeignKey("CreatedById")]
        public virtual SystemAccount? CreatedBy { get; set; }
        
        [ForeignKey("UpdatedById")]
        public virtual SystemAccount? UpdatedBy { get; set; }

        public virtual ICollection<NewsTag> NewsTags { get; set; } = new List<NewsTag>();
    }
}
