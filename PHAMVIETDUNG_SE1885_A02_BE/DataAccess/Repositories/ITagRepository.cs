using PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Models;

namespace PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Repositories
{
    public interface ITagRepository : IGenericRepository<Tag>
    {
        int GetMaxTagId();
    }
}
