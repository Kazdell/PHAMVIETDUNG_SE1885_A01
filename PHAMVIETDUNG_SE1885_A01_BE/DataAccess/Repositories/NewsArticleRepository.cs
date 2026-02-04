using Microsoft.EntityFrameworkCore;
using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;
using System.Linq;

namespace PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Repositories
{
    public class NewsArticleRepository : GenericRepository<NewsArticle>, INewsArticleRepository
    {
        private readonly FUNewsManagementContext _context;
        public NewsArticleRepository(FUNewsManagementContext context) : base(context)
        {
            _context = context;
        }

        public IEnumerable<NewsArticle> GetActiveNews()
        {
            return _context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.NewsTags).ThenInclude(nt => nt.Tag)
                .Where(n => n.NewsStatus == true)
                .ToList();
        }

        public new IQueryable<NewsArticle> GetAll()
        {
            return _context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.NewsTags).ThenInclude(nt => nt.Tag)
                .AsQueryable();
        }

        public new NewsArticle GetById(object id)
        {
             return _context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.NewsTags).ThenInclude(nt => nt.Tag)
                .FirstOrDefault(n => n.NewsArticleId == id.ToString());
        }
    }
}
