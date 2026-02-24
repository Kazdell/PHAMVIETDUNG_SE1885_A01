using PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Models;
using System.Linq;
using System.Collections.Generic;

namespace PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Repositories
{
  public interface INewsArticleRepository : IGenericRepository<NewsArticle>
  {
    IEnumerable<NewsArticle> GetActiveNews();
    new IQueryable<NewsArticle> GetAll();
    new NewsArticle GetById(object id);
  }
}
