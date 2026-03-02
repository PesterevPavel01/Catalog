using Catalog.Domain.Entities;
using Catalog.Infrastructure.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations
{
    public sealed class OrderConfiguration : SimpleEntityConfiguration<Order>
    {
        protected override void AddBuilder(EntityTypeBuilder<Order> builder)
        {
            builder
                .Property(x => x.Status)
                .IsRequired();

            builder
                .HasIndex(x => x.Status);

            builder
                .HasMany(x => x.OrderItems)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(x => x.OrderHistory)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(x => x.ApplicationUser)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.ApplicationUserId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);
        }

        protected override string TableName()
            => "orders";
    }
}
