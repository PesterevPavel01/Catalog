using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations
{
    public class NumericParameterConfiguration : IEntityTypeConfiguration<NumericParameter>
    {
        public void Configure(EntityTypeBuilder<NumericParameter> builder)
        {
            builder.ToTable("numeric_parameters");
            builder.HasKey(x => x.Id);

            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder
                .HasOne(x => x.ParameterType)
                .WithMany(x => x.NumericParameters)
                .HasForeignKey(x => x.ParameterTypeId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}