using Catalog.Contracts.Entities.Parameters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations.Parameters
{
    internal class ModuleTextParameterConfiguration : TextParameterConfiguration<ModuleTextParameter>
    {
        protected override void AddBuilder(EntityTypeBuilder<ModuleTextParameter> builder)
        {
            builder
                .HasOne(x => x.ParameterType)
                .WithMany(x => x.ModuleTextParameters)
                .HasForeignKey(x => x.ParameterTypeId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);
        }

        protected override string TableName()
            => "module_text_parameters";
    }
}