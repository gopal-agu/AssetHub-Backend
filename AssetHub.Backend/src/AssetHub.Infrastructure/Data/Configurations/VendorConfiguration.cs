using AssetHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetHub.Infrastructure.Data.Configurations;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendors");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasMaxLength(30);
        builder.Property(v => v.Name).IsRequired().HasMaxLength(200);
        builder.Property(v => v.Email).HasMaxLength(200);
        builder.Property(v => v.GstNumber).HasMaxLength(30);

        builder.Property(v => v.Categories)
            .HasConversion(JsonValueConverters.StringListConverter)
            .Metadata.SetValueComparer(JsonValueConverters.StringListComparer);
    }
}
