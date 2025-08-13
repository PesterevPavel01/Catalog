using Catalog.Domain.Entities;
using Catalog.Infrastructure.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations
{
    internal class ComponentTypeConfiguration : SimpleEntityConfiguration<ComponentType>
    {
        protected override void AddBuilder(EntityTypeBuilder<ComponentType> builder)
        {
            /*builder
                .HasMany(x => x.Components)
                .WithOne()
                .HasForeignKey(x => x.ComponentTypeId)
                .OnDelete(DeleteBehavior.Restrict);*/
        }

        protected override string TableName()
            => "component_types";
    }
}
