using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Catalog.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Catalog.Domain.ValueObjects;

namespace Catalog.Infrastructure.Configurations.Base
{
    public abstract class SimpleEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
     where TEntity : SimpleEntity
    {
        public void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.ToTable(TableName());

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
                .HasMaxLength(CodeValue.MaxTitleLength)
                .HasConversion(x => x.Value, x => CodeValue.Create(x).Result);

            builder.HasIndex(x => x.Title);
            builder
                .HasIndex(x => x.Code)
                .IsUnique();

            AddBuilder(builder);
        }

        protected abstract void AddBuilder(EntityTypeBuilder<TEntity> builder);

        protected abstract string TableName();
    }
}
