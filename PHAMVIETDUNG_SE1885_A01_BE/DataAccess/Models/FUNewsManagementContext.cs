using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;

namespace PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models
{
    public class FUNewsManagementContext : DbContext
    {
        public FUNewsManagementContext() { }
        public FUNewsManagementContext(DbContextOptions<FUNewsManagementContext> options) : base(options) { }

        public virtual DbSet<SystemAccount> SystemAccounts { get; set; } = null!;
        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<NewsArticle> NewsArticles { get; set; } = null!;
        public virtual DbSet<Tag> Tags { get; set; } = null!;
        public virtual DbSet<NewsTag> NewsTags { get; set; } = null!;
        public virtual DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                IConfigurationRoot configuration = builder.Build();
                optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NewsTag>()
                .HasKey(nt => new { nt.NewsArticleId, nt.TagId });

            modelBuilder.Entity<NewsTag>()
                .HasOne(nt => nt.NewsArticle)
                .WithMany(n => n.NewsTags)
                .HasForeignKey(nt => nt.NewsArticleId);

            modelBuilder.Entity<NewsTag>()
                .HasOne(nt => nt.Tag)
                .WithMany(t => t.NewsTags)
                .HasForeignKey(nt => nt.TagId);

            modelBuilder.Entity<NewsArticle>()
                .HasOne(n => n.CreatedBy)
                .WithMany(a => a.CreatedArticles)
                .HasForeignKey(n => n.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NewsArticle>()
                .HasOne(n => n.UpdatedBy)
                .WithMany()
                .HasForeignKey(n => n.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
