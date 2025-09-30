using Catalog.Domain.Entities.Authorization;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configurations.Auth
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("application_users");

            builder.HasKey(x => x.Id);

            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder
                .Property(x => x.UserName)
                .IsRequired()
                .HasMaxLength(128);

            builder
                .Property(x => x.ExternalId)
                .HasMaxLength(128);

            builder
                .Property(x => x.Password)
                .IsRequired()
                .HasMaxLength(PasswordValue.MaxPasswordLength)
                .HasConversion(x => x.Value, x => PasswordValue.Create(x).Result);

            builder
                .Property(x => x.Email)
                .HasMaxLength(20);

            builder
                .HasOne(x => x.UserToken)
                .WithOne(x => x.User);

            builder
                .HasMany(x => x.Roles)
                .WithMany(x => x.ApplicationUsers)
                .UsingEntity(x => x.ToTable("user_role_items"));

            builder
                .HasMany(x => x.Messages)
                .WithOne(x => x.ApplicationUser)
                .HasForeignKey(x=> x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
