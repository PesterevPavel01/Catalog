using Catalog.Domain.Entities;
using Catalog.Infrastructure.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations
{
    public class ComponentConfiguration : SimpleEntityConfiguration<Component>
    {
        protected override void AddBuilder(EntityTypeBuilder<Component> builder)
        {
            builder
                .HasMany(x => x.TextParameters)
                .WithOne()
                .HasForeignKey(x => x.ComponentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(x => x.NumericParameters)
                .WithOne()
                .HasForeignKey(x => x.ComponentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.ComponentType)
                .WithMany(x => x.Components)
                .HasForeignKey(x => x.ComponentTypeId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(x => x.Modules)
                .WithMany(x => x.Components)
                .UsingEntity(x => x.ToTable("ModuleItems"));
        }

        protected override string TableName()
            => "components";
    }
}
