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
                .HasMany(x => x.OrderItems)
                .WithOne()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        protected override string TableName()
            => "orders";
    }
}
