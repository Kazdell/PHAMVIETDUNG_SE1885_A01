using System.ComponentModel.DataAnnotations;

namespace PHAMVIETDUNG_SE1885_A01_FE.Presentation.ViewModels;

public class NewsArticleViewModel
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

    [Required]
    public short? CategoryId { get; set; }
    
    public string? CategoryName { get; set; } // For display

    public bool? NewsStatus { get; set; }

    public short? CreatedById { get; set; }
    public string? CreatedByName { get; set; }

    public short? UpdatedById { get; set; }

    public DateTime? ModifiedDate { get; set; }
    
    // For handling Tags selection
    public List<int> SelectedTagIds { get; set; } = new List<int>();
    
    // For display
    public List<TagViewModel> Tags { get; set; } = new List<TagViewModel>();

    // For binding API response
    public List<NewsTagViewModel> NewsTags { get; set; } = new List<NewsTagViewModel>();
    
    public CategoryViewModel? Category { get; set; }
    public SystemAccountViewModel? CreatedBy { get; set; }
}
