using PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Models;

namespace PHAMVIETDUNG_SE1885_A02_BE.BusinessLogic.Services
{
  public interface ICategoryService
  {
    IEnumerable<Category> GetAllCategories();
    IEnumerable<Presentation.Models.CategoryModel> GetCategoriesWithCounts();
    Category GetCategoryById(int id);
    void CreateCategory(Category category);
    void UpdateCategory(Category category);
    void DeleteCategory(int id);
    IEnumerable<Category> SearchCategories(string keyword);
  }
}
