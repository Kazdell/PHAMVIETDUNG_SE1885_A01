using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Models
{
    [Table("Category")]
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public short CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string? CategoryName { get; set; }

        [Required]
        [StringLength(250)]
        public string? CategoryDesciption { get; set; }

        public short? ParentCategoryId { get; set; }

        [ForeignKey("ParentCategoryId")]
        public virtual Category? ParentCategory { get; set; }

        public bool? IsActive { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();
        
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
    }
}
