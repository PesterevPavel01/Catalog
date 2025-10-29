using Catalog.Contracts.Entities.Parameters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations.Parameters
{
    public sealed class ComponentTextParameterConfiguration : TextParameterConfiguration<ComponentTextParameter>
    {
        protected override void AddBuilder(EntityTypeBuilder<ComponentTextParameter> builder)
        {
            builder
                .HasOne(x => x.ParameterType)
                .WithMany(x => x.ComponentTextParameters)
                .HasForeignKey(x => x.ParameterTypeId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);
        }

        protected override string TableName()
            => "component_text_parameters";
    }
}
