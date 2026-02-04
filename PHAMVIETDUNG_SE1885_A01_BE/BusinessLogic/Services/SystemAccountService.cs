using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;
using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Repositories;
using Microsoft.Extensions.Configuration;

namespace PHAMVIETDUNG_SE1885_A01_BE.BusinessLogic.Services
{
    public class SystemAccountService : ISystemAccountService
    {
        private readonly ISystemAccountRepository _repository;
        private readonly INewsArticleRepository _newsRepo;
        private readonly IConfiguration _configuration;
        
        public SystemAccountService(ISystemAccountRepository repository, INewsArticleRepository newsRepo, IConfiguration configuration)
        {
            _repository = repository;
            _newsRepo = newsRepo;
            _configuration = configuration;
        }

        public IEnumerable<SystemAccount> GetAllAccounts()
        {
            return _repository.GetAll();
        }

        public SystemAccount GetAccountById(int id)
        {
            return _repository.GetById((short)id);
        }

        public SystemAccount GetAccountByEmail(string email)
        {
            return _repository.GetByEmail(email);
        }

        public SystemAccount Login(string email, string password)
        {
            // Check Admin from AppSettings
            var adminEmail = _configuration["AdminAccount:Email"];
            var adminPassword = _configuration["AdminAccount:Password"];
            var adminRole = _configuration["AdminAccount:Role"];

            if (email == adminEmail && password == adminPassword)
            {
                return new SystemAccount
                {
                    AccountId = 0, // Admin doesn't necessarily have a DB ID, or use 0
                    AccountName = "Administrator",
                    AccountEmail = adminEmail,
                    AccountRole = int.Parse(adminRole ?? "0"),
                    AccountPassword = adminPassword
                };
            }

            return _repository.Login(email, password);
        }

        public void CreateAccount(SystemAccount account)
        {
            // Validation: Duplicate email
            // Validation: Duplicate email
            if (_repository.GetByEmail(account.AccountEmail) != null)
            {
                 throw new Exception("Email already exists.");
            }

            // Manual ID Generation
            var maxId = _repository.GetAll().Max(a => (short?)a.AccountId) ?? 0;
            account.AccountId = (short)(maxId + 1);

            _repository.Insert(account);
        }

        public void UpdateAccount(SystemAccount account)
        {
             // Validation: Check if email changed and if it exists
             var existing = _repository.GetById(account.AccountId);
             if (existing == null) throw new Exception("Account not found.");

             if (existing.AccountEmail != account.AccountEmail)
             {
                 if(_repository.GetByEmail(account.AccountEmail) != null)
                    throw new Exception("Email already exists.");
             }
             
             // Map properties to existing tracked entity
             existing.AccountName = account.AccountName;
             existing.AccountEmail = account.AccountEmail;
             existing.AccountRole = account.AccountRole;
             // Only update password if provided (though Controller handles this, double safety)
             if (!string.IsNullOrEmpty(account.AccountPassword))
             {
                existing.AccountPassword = account.AccountPassword;
             }
             
            _repository.Update(existing);
        }

        public void DeleteAccount(int id)
        {
            // Rule: "an account cannot be deleted if it has created any record in NewsArticle.CreatedByID"
            if (_newsRepo.GetAll().Any(n => n.CreatedById == id))
            {
                 throw new Exception("Cannot delete account because they have created news articles.");
            }
            _repository.Delete((short)id);
        }

        public IEnumerable<SystemAccount> SearchAccounts(string keyword, int? role)
        {
            var all = _repository.GetAll();
            if (!string.IsNullOrEmpty(keyword))
            {
                all = all.Where(a => (a.AccountName != null && a.AccountName.Contains(keyword)) || (a.AccountEmail != null && a.AccountEmail.Contains(keyword)));
            }
            if (role.HasValue)
            {
                all = all.Where(a => a.AccountRole == role.Value);
            }
            return all;
        }
    }
}
