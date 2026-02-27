using Catalog.Contracts.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations
{
    internal class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.HasKey(x => x.Id);

            builder.ToTable("outbox_messages");

            builder.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(4096);

            builder.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(512);

            builder.Property(x => x.Error)
                .HasMaxLength(8192);
        }
    }
}
