using System.Reflection;
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
            string AdministratorPassword = _configuration.GetSection("Authorization").GetSection("AdministratorPassword").Value ?? "DefaultPassword";

            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            var administratorResult = ApplicationUser
                .Create(
                    id: Guid.NewGuid(),
                    userName: "Administrator",
                    password: AdministratorPassword
                );

            if (!administratorResult.Ok)
                throw new DbUpdateException(administratorResult.Error);

            var administrator = administratorResult.Result;
            administrator.CreatedAt = DateTime.Now;
            administrator.UpdatedAt = default;

            modelBuilder.Entity<ApplicationUser>().HasData(administrator);
        }
    }
}