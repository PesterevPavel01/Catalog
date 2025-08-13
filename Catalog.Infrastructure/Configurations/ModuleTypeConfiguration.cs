using Catalog.Domain.Entities;
using Catalog.Infrastructure.Configurations.Base;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations
{
    internal class ModuleTypeConfiguration : SimpleEntityConfiguration<ModuleType>
    {
        protected override void AddBuilder(EntityTypeBuilder<ModuleType> builder)
        {
            /*builder
                .HasMany(x => x.Components)
                .WithOne()
                .HasForeignKey(x => x.ComponentTypeId)
                .OnDelete(DeleteBehavior.Restrict);*/
        }

        protected override string TableName()
            => "module_types";
    }
}