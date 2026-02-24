using Microsoft.EntityFrameworkCore;
using PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Models;
using System.Linq;


namespace PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly FUNewsManagementContext _context;
        private DbSet<T> table;

        public GenericRepository(FUNewsManagementContext context)
        {
            _context = context;
            table = _context.Set<T>();
        }

        public IQueryable<T> GetAll()
        {
            return table.AsQueryable();
        }

        public T GetById(object id)
        {
            return table.Find(id);
        }

        public void Insert(T obj)
        {
            table.Add(obj);
            _context.Entry(obj).State = EntityState.Added; // Force Added state
            try
            {
                int result = _context.SaveChanges();
                if (result == 0) throw new Exception("No rows affected. Insertion failed silently.");
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"DbUpdateException: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
               throw new Exception($"Insert Error: {ex.Message}");
            }
        }

        public void Update(T obj)
        {
            table.Attach(obj);
            _context.Entry(obj).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void Delete(object id)
        {
            T existing = table.Find(id);
            table.Remove(existing);
            _context.SaveChanges();
        }

        public void Delete(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                table.Attach(entity);
            }
            table.Remove(entity);
            _context.SaveChanges();
        }
    }
}
