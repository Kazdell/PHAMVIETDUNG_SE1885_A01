using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PHAMVIETDUNG_SE1885_A01_BE.Presentation.Models
{
    public class NewsArticleModel
    {
        public string? NewsArticleId { get; set; }

        [Required]
        [StringLength(400)]
        public string? NewsTitle { get; set; }

        [Required]
        [StringLength(150)]
        public string? Headline { get; set; }

        public DateTime? CreatedDate { get; set; }

        [Required]
        public string? NewsContent { get; set; }

        [StringLength(400)]
        public string? NewsSource { get; set; }

        [StringLength(400)]
        public string? NewsImage { get; set; }

        public IFormFile? ImageFile { get; set; }

        public int ViewCount { get; set; }

        public short? CategoryId { get; set; }

        public bool? NewsStatus { get; set; }

        public short? CreatedById { get; set; }

        public short? UpdatedById { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public List<int>? SelectedTagIds { get; set; }
    }
}
