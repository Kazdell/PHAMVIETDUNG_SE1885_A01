using LazyCache;
using PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Models;
using PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Repositories;

namespace PHAMVIETDUNG_SE1885_A02_BE.BusinessLogic.Services
{
  public class CategoryService(ICategoryRepository repository, INewsArticleRepository newsRepo, IAppCache cache)
    : ICategoryService
  {
    private const string CacheKey = "categories";

    public IEnumerable<Category> GetAllCategories()
    {
      return cache.GetOrAdd(CacheKey, () => repository.GetAll().ToList(), DateTimeOffset.Now.AddMinutes(10));
    }

    public IEnumerable<Presentation.Models.CategoryModel> GetCategoriesWithCounts()
    {
      var categories = repository.GetAll();
      var articles = newsRepo.GetAll(); // Fetch all to count. Inefficient for huge datasets but fine for assignment.

      return categories.Select(c => new Presentation.Models.CategoryModel
      {
        CategoryId = c.CategoryId,
        CategoryName = c.CategoryName ?? "",
        CategoryDesciption = c.CategoryDesciption ?? "",
        ParentCategoryId = c.ParentCategoryId,
        IsActive = c.IsActive,
        ArticleCount = articles.Count(n => n.CategoryId == c.CategoryId)
      });
    }

    public Category GetCategoryById(int id)
    {
      return repository.GetById((short)id); // Casting to short as per entity
    }

    public void CreateCategory(Category category)
    {
      if (repository.GetAll().Any(c => string.Equals(c.CategoryName, category.CategoryName, StringComparison.OrdinalIgnoreCase)))
      {
        throw new Exception(Common.SystemMessages.GetMessage(Common.SystemMessages.DuplicateCategory));
      }

      repository.Insert(category);
      cache.Remove(CacheKey); // Invalidate cache
    }

    public void UpdateCategory(Category category)
    {
      var existing = repository.GetById(category.CategoryId);
      if (existing != null)
      {
        if (!string.Equals(existing.CategoryName, category.CategoryName, StringComparison.OrdinalIgnoreCase))
        {
          if (repository.GetAll().Any(c => string.Equals(c.CategoryName, category.CategoryName, StringComparison.OrdinalIgnoreCase)))
          {
            throw new Exception(Common.SystemMessages.GetMessage(Common.SystemMessages.DuplicateCategory));
          }
        }
        existing.CategoryName = category.CategoryName;
        existing.CategoryDesciption = category.CategoryDesciption;
        existing.ParentCategoryId = category.ParentCategoryId;
        existing.IsActive = category.IsActive;
        repository.Update(existing);
        cache.Remove(CacheKey); // Invalidate cache
      }
    }

    public void DeleteCategory(int id)
    {
      // Rule: "delete action will delete an item in the case this item is not belong to any news articles"
      if (newsRepo.GetAll().Any(n => n.CategoryId == id))
      {
        throw new Exception(Common.SystemMessages.GetMessage(Common.SystemMessages.UsedCategoryDeleteError));
      }
      repository.Delete((short)id);
      cache.Remove(CacheKey); // Invalidate cache
    }

    public IEnumerable<Category> SearchCategories(string keyword)
    {
      var all = repository.GetAll();
      if (!string.IsNullOrEmpty(keyword))
      {
        all = all.Where(c => (c.CategoryName != null && c.CategoryName.Contains(keyword)) || (c.CategoryDesciption != null && c.CategoryDesciption.Contains(keyword)));
      }
      return all;
    }
  }
}
