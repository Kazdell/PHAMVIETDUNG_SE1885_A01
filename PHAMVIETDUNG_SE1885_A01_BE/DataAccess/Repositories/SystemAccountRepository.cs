using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;

namespace PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Repositories
{
    public class SystemAccountRepository : GenericRepository<SystemAccount>, ISystemAccountRepository
    {
        private readonly FUNewsManagementContext _context;

        public SystemAccountRepository(FUNewsManagementContext context) : base(context)
        {
            _context = context;
        }

        public SystemAccount GetByEmail(string email)
        {
            return _context.SystemAccounts.FirstOrDefault(s => s.AccountEmail == email);
        }

        public SystemAccount Login(string email, string password)
        {
            return _context.SystemAccounts.FirstOrDefault(s => s.AccountEmail == email && s.AccountPassword == password);
        }
    }
}
