using AssetHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetHub.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasMaxLength(30);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Prefix).IsRequired().HasMaxLength(10);
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.Status).HasMaxLength(20);
        builder.HasIndex(c => c.Name).IsUnique();
    }
}
