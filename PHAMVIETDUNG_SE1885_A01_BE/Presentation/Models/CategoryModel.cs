namespace PHAMVIETDUNG_SE1885_A01_BE.Presentation.Models
{
    public class CategoryModel
    {
        public short CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string CategoryDesciption { get; set; } = null!;
        public short? ParentCategoryId { get; set; }
        public bool? IsActive { get; set; }
        public int ArticleCount { get; set; }
    }
}
