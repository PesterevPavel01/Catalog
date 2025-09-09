using System.Reflection;
using Catalog.Contracts.Entities.Parameters.Base;
using Catalog.Domain.Entities;
using Catalog.Domain.Entities.Autorization;
using Catalog.Domain.Enum;
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

            var technicalUserResult = ApplicationUser
                .Create(
                    id: Guid.NewGuid(),
                    userName: "TECHNICAL_USER"
                );

            if (!technicalUserResult.Ok)
                throw new DbUpdateException(technicalUserResult.Error);

            var technicalUser = technicalUserResult.Result;
            technicalUser.CreatedAt = DateTime.Now;
            technicalUser.UpdatedAt = default;

            modelBuilder.Entity<ApplicationUser>().HasData(technicalUser);


            var moduleType = ModuleType.Create("Фасад", "00000000FSD",Guid.NewGuid());

            modelBuilder.Entity<ModuleType>().HasData(moduleType.Result);

            var parameterType = ParameterType.Create(
                        title: "CUSTOM COMPONENT",
                        code: "0000000CSTM",
                        ParameterValueType.Text,
                        Guid.NewGuid());

            modelBuilder
                .Entity<ParameterType>()
                .HasData(parameterType.Result);


        }
    }
}