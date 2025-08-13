using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations
{
    public class TextParameterConfiguration : IEntityTypeConfiguration<TextParameter>
    {
        public void Configure(EntityTypeBuilder<TextParameter> builder)
        {
            builder.ToTable("text_parameters");
            builder.HasKey(x => x.Id);

            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder
                .Property(x => x.Value)
                .IsRequired()
                .HasConversion(x => x.Value, x => TextParameterValue.Create(x).Result);

            builder
                .HasOne(x => x.ParameterType)
                .WithMany(x => x.TextParameters)
                .HasForeignKey(x => x.ParameterTypeId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
