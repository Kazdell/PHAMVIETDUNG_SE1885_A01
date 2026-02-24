using PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Models;

namespace PHAMVIETDUNG_SE1885_A02_BE.BusinessLogic.Services
{
    public interface ISystemAccountService
    {
        IEnumerable<SystemAccount> GetAllAccounts();
        SystemAccount GetAccountById(int id);
        SystemAccount GetAccountByEmail(string email);
        SystemAccount Login(string email, string password);
        void CreateAccount(SystemAccount account);
        void UpdateAccount(SystemAccount account);
        void DeleteAccount(int id);
        IEnumerable<SystemAccount> SearchAccounts(string keyword, int? role);
    }
}
