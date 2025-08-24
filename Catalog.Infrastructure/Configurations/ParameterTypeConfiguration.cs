using Catalog.Contracts.Entities.Parameters.Base;
using Catalog.Domain.Enum;
using Catalog.Infrastructure.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations
{
    public class ParameterTypeConfiguration : SimpleEntityConfiguration<ParameterType>
    {
        protected override void AddBuilder(EntityTypeBuilder<ParameterType> builder)
        {
            builder
                .Property(x => x.Type)
                    .HasMaxLength(100)
                    .IsRequired()
                    .HasConversion(
                        v => v.ToString(),  // При сохранении в БД: enum -> string
                        v => (ParameterValueType)Enum.Parse(typeof(ParameterValueType), v));

            builder
                .HasMany(x => x.ComponentTextParameters)
                .WithOne(x => x.ParameterType)
                .HasForeignKey(x => x.ParameterTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(x => x.ModuleTextParameters)
                .WithOne(x => x.ParameterType)
                .HasForeignKey(x => x.ParameterTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(x => x.ComponentNumericParameters)
                .WithOne(x => x.ParameterType)
                .HasForeignKey(x => x.ParameterTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        protected override string TableName()
            => "parameter_types";
    }
}