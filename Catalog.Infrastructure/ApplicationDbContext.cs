using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Catalog.Domain.Entities.Autorization;
using Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Catalog.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionBuilder)
        {
            optionBuilder.AddInterceptors(new DateInterceptors());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            string AdministratorPassword = _configuration.GetSection("Authorization").GetSection("AdministratorPassword").Value;

            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            var administrator = ApplicationUser
                .Create(
                    id: Guid.NewGuid(),
                    userName: "Administrator",
                    password: AdministratorPassword
                );

            administrator.CreatedAt = DateTime.Now;
            administrator.UpdatedAt = default;

            modelBuilder.Entity<ApplicationUser>().HasData(administrator);
        }
    }
}