using Catalog.Domain.Entities;
using Catalog.Infrastructure.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations
{
    internal class ModuleConfiguration : SimpleEntityConfiguration<Module>
    {
        protected override void AddBuilder(EntityTypeBuilder<Module> builder)
        {
            builder
                .HasOne(x => x.ModuleType)
                .WithMany(x => x.Modules)
                .HasForeignKey(x => x.ModuleTypeId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(x => x.Components)
                .WithMany(x => x.Modules)
                .UsingEntity(x => x.ToTable("module_items"));

            builder
                .HasMany(x => x.ModuleTextParameters)
                .WithOne()
                .HasForeignKey(x => x.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(x => x.ModuleNumericParameters)
                .WithOne()
                .HasForeignKey(x => x.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        protected override string TableName()
            => "modules";
    }
}
