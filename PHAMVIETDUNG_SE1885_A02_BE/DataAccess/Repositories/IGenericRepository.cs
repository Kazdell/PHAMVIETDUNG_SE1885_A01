namespace PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Repositories
{
    using System.Linq;
    using System.Collections.Generic;
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> GetAll();
        T GetById(object id);
        void Insert(T obj);
        void Update(T obj);
        void Delete(object id);
        void Delete(T entity);
    }
}
