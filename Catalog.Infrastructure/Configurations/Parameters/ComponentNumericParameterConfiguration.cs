using Catalog.Contracts.Entities.Parameters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations.Parameters
{
    public sealed class ComponentNumericParameterConfiguration : NumericParameterConfiguration<ComponentNumericParameter>
    {
        protected override void AddBuilder(EntityTypeBuilder<ComponentNumericParameter> builder)
        {
            builder
                .HasOne(x => x.ParameterType)
                .WithMany(x => x.ComponentNumericParameters)
                .HasForeignKey(x => x.ParameterTypeId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }

        protected override string TableName()
            => "component_numeric_parameters";
    }
}
