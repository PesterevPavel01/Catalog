using Catalog.Contracts.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations
{
    public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.ToTable("messages");

            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.ApplicationUserId);
            builder.HasIndex(x => x.OrderItemId);

            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder
                .HasOne(x => x.OrderItem)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.OrderItemId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.ApplicationUser)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.ApplicationUserId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
