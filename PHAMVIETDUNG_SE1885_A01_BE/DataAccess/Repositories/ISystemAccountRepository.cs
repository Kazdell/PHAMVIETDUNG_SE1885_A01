using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;

namespace PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Repositories
{
    public interface ISystemAccountRepository : IGenericRepository<SystemAccount>
    {
        SystemAccount GetByEmail(string email);
        SystemAccount Login(string email, string password);
    }
}
