using Catalog.Contracts.Entities;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations
{
    public class OrderEventConfiguration : IEntityTypeConfiguration<OrderEvent>
    {
        public void Configure(EntityTypeBuilder<OrderEvent> builder)
        {
            builder.ToTable("order_events");

            builder.HasKey(x => x.Id);

            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder
               .Property(x => x.Title)
               .IsRequired()
               .HasMaxLength(TitleValue.MaxTitleLength)
               .HasConversion(x => x.Value, x => TitleValue.Create(x).Result);

            builder
                .Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.Title);

            builder
                .HasIndex(x => x.Code)
                .IsUnique();

            builder.Property(x => x.Type).HasMaxLength(50);

            builder
                .HasOne(x => x.Order)
                .WithMany(x => x.OrderHistory)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
