using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;
using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Repositories;

namespace PHAMVIETDUNG_SE1885_A01_BE.BusinessLogic.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly INewsArticleRepository _newsRepo;

        public CategoryService(ICategoryRepository repository, INewsArticleRepository newsRepo)
        {
            _repository = repository;
            _newsRepo = newsRepo;
        }

        public IEnumerable<Category> GetAllCategories()
        {
            return _repository.GetAll();
        }

        public IEnumerable<Presentation.Models.CategoryModel> GetCategoriesWithCounts()
        {
            var categories = _repository.GetAll();
            var articles = _newsRepo.GetAll(); // Fetch all to count. Inefficient for huge datasets but fine for assignment.

            return categories.Select(c => new Presentation.Models.CategoryModel
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                CategoryDesciption = c.CategoryDesciption,
                ParentCategoryId = c.ParentCategoryId,
                IsActive = c.IsActive,
                ArticleCount = articles.Count(n => n.CategoryId == c.CategoryId)
            });
        }

        public Category GetCategoryById(int id)
        {
            return _repository.GetById((short)id); // Casting to short as per entity
        }

        public void CreateCategory(Category category)
        {
            if (_repository.GetAll().Any(c => c.CategoryName.ToLower() == category.CategoryName.ToLower()))
            {
                throw new Exception(Common.SystemMessages.GetMessage(Common.SystemMessages.DuplicateCategory));
            }

            _repository.Insert(category);
        }

        public void UpdateCategory(Category category)
        {
            var existing = _repository.GetById(category.CategoryId);
            if (existing != null)
            {
                if (!existing.CategoryName.Equals(category.CategoryName, StringComparison.OrdinalIgnoreCase))
                {
                    if (_repository.GetAll().Any(c => c.CategoryName.ToLower() == category.CategoryName.ToLower()))
                    {
                        throw new Exception(Common.SystemMessages.GetMessage(Common.SystemMessages.DuplicateCategory));
                    }
                }
                existing.CategoryName = category.CategoryName;
                existing.CategoryDesciption = category.CategoryDesciption;
                existing.ParentCategoryId = category.ParentCategoryId;
                existing.IsActive = category.IsActive;
                _repository.Update(existing);
            }
        }

        public void DeleteCategory(int id)
        {
            // Rule: "delete action will delete an item in the case this item is not belong to any news articles"
            if (_newsRepo.GetAll().Any(n => n.CategoryId == id))
            {
                throw new Exception(Common.SystemMessages.GetMessage(Common.SystemMessages.UsedCategoryDeleteError));
            }
             _repository.Delete((short)id);
        }

        public IEnumerable<Category> SearchCategories(string keyword)
        {
             var all = _repository.GetAll();
             if (!string.IsNullOrEmpty(keyword))
             {
                 all = all.Where(c => (c.CategoryName != null && c.CategoryName.Contains(keyword)) || (c.CategoryDesciption != null && c.CategoryDesciption.Contains(keyword)));
             }
             return all;
        }
    }
}
