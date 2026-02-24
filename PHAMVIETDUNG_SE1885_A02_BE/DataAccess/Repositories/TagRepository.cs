
using PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Models;

namespace PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Repositories
{
    public class TagRepository : GenericRepository<Tag>, ITagRepository
    {
        public TagRepository(FUNewsManagementContext context) : base(context)
        {
        }

        public int GetMaxTagId()
        {
            if (!_context.Tags.Any())
            {
                return 0;
            }
            return _context.Tags.Max(t => t.TagId);
        }
    }
}
