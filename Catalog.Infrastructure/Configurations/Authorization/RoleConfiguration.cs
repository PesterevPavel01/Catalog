using Catalog.Contracts.Entities.Authorization;
using Catalog.Infrastructure.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations.Authorization
{
    internal class RoleConfiguration : SimpleEntityConfiguration<Role>
    {
        protected override void AddBuilder(EntityTypeBuilder<Role> builder)
        {
            builder
                .HasMany(x => x.ApplicationUsers)
                .WithMany(x => x.Roles)
                .UsingEntity(x => x.ToTable("user_role_items"));
        }

        protected override string TableName()
            => "user_roles";
    }
}
