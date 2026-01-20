using Catalog.Contracts.ApplicationEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations.Exchange
{
    internal class ExportedEntityConfiguration : IEntityTypeConfiguration<ExportedEntity>
    {
        public void Configure(EntityTypeBuilder<ExportedEntity> builder)
        {
            builder.ToTable("exported_entity");

            builder.HasKey(x => x.Id);

            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder
                .Property(x => x.EntityType)
                .IsRequired()
                .HasMaxLength(50);

            builder
                .Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder
                .HasOne(x => x.ExchangeEvent)
                .WithMany(x => x.Entities)
                .HasForeignKey(x => x.ExchangeEventId)
                .HasPrincipalKey(x => x.Id);
        }
    }
}
