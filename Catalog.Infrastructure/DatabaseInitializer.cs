using Catalog.Contracts.Entities.Authorization;
using Catalog.Contracts.Entities.Parameters.Base;
using Catalog.Domain.Entities;
using Catalog.Domain.Entities.Authorization;
using Catalog.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Infrastructure
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IServiceProvider service)
        {
            using var scope = service.CreateScope();
            var services = scope.ServiceProvider;

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var context = services.GetRequiredService<ApplicationDbContext>();
            await CreateDefaultUserAsync(context, configuration);

        }

        private static async Task CreateDefaultUserAsync(ApplicationDbContext context, IConfiguration configuration)
        {

            var administrator = await context.Set<ApplicationUser>().FirstOrDefaultAsync(x => x.UserName == "Administrator");

            if (administrator is not null)
                return;

            string administratorPassword = configuration.GetSection("Authorization").GetSection("AdministratorPassword").Value ?? "DefaultPassword";
            string constructorPassword = configuration.GetSection("Authorization").GetSection("ConstructorPassword").Value ?? "DefaultPassword";

            var roleCreationResult = Role.Create(Guid.NewGuid(), "Administrator", "Administrator");
            if (!roleCreationResult.Ok)
                throw new DbUpdateException(roleCreationResult.Error);

            var administratorRole = roleCreationResult.Result;
            administratorRole.CreatedAt = DateTime.Now;
            administratorRole.UpdatedAt = default;

            await context.Set<Role>().AddAsync(administratorRole);

            roleCreationResult = Role.Create(Guid.NewGuid(), "CONSTRUCTOR", "CONSTRUCTOR");
            if (!roleCreationResult.Ok)
                throw new DbUpdateException(roleCreationResult.Error);

            var constructorRole = roleCreationResult.Result;
            constructorRole.CreatedAt = DateTime.Now;
            constructorRole.UpdatedAt = default;

            await context.Set<Role>().AddAsync(constructorRole);

            var administratorResult = ApplicationUser
                .Create(
                    id: Guid.NewGuid(),
                    userName: "Administrator",
                    password: administratorPassword
                );

            if (!administratorResult.Ok)
                throw new DbUpdateException(administratorResult.Error);

            administratorResult = administratorResult.Result.AddRole(administratorRole);
            administratorResult = administratorResult.Result.AddRole(constructorRole);

            if (!administratorResult.Ok)
                throw new DbUpdateException(administratorResult.Error);

            administrator = administratorResult.Result;
            administrator.CreatedAt = DateTime.Now;
            administrator.UpdatedAt = default;

            await context.Set<ApplicationUser>().AddAsync(administrator);

            var constructorResult = ApplicationUser
               .Create(
                   id: Guid.NewGuid(),
                   userName: "CONSTRUCTOR",
                   password: constructorPassword
               );

            if (!constructorResult.Ok)
                throw new DbUpdateException(constructorResult.Error);

            constructorResult = constructorResult.Result.AddRole(constructorRole);

            if (!constructorResult.Ok)
                throw new DbUpdateException(constructorResult.Error);

            var constructor = constructorResult.Result;
            constructor.CreatedAt = DateTime.Now;
            constructor.UpdatedAt = default;

            await context.Set<ApplicationUser>().AddAsync(constructor);

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

            await context.Set<ApplicationUser>().AddAsync(technicalUser);

            var moduleType = ModuleType.Create("Фасад", "00000000FSD", Guid.NewGuid());

            await context.Set<ModuleType>().AddAsync(moduleType.Result);

            var parameterType = ParameterType.Create(
                        title: "CUSTOM COMPONENT",
                        code: "0000000CSTM",
                        ParameterValueType.Text,
                        Guid.NewGuid());

            await context.Set<ParameterType>().AddAsync(parameterType.Result);

            await context.SaveChangesAsync();
        }
    }
}
