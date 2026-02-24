using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Models
{
    [Table("SystemAccount")]
    public class SystemAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short AccountId { get; set; }

        [Required]
        [StringLength(100)]
        public string? AccountName { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string? AccountEmail { get; set; }

        [Required]
        public int AccountRole { get; set; }

        [Required]
        [StringLength(100)]
        public string? AccountPassword { get; set; }

        // Navigation Properties can be added here if needed, but keeping it simple for now based on requirements
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<NewsArticle> CreatedArticles { get; set; } = new List<NewsArticle>();
    }
}
