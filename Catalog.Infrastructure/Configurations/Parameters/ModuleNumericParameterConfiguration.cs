using Catalog.Contracts.Entities.Parameters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations.Parameters
{
    public sealed class ModuleNumericParameterConfiguration : NumericParameterConfiguration<ModuleNumericParameter>
    {
        protected override void AddBuilder(EntityTypeBuilder<ModuleNumericParameter> builder)
        {
            builder
                .HasOne(x => x.ParameterType)
                .WithMany(x => x.ModuleNumericParameters)
                .HasForeignKey(x => x.ParameterTypeId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);
        }

        protected override string TableName()
            => "module_numeric_parameters";
    }
}
