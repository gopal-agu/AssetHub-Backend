using AssetHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetHub.Infrastructure.Data.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Assets");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasMaxLength(30);
        builder.Property(a => a.UnitCost).HasColumnType("decimal(18,2)");

        builder.Property(a => a.Configuration)
            .HasConversion(JsonValueConverters.StringDictConverter)
            .Metadata.SetValueComparer(JsonValueConverters.StringDictComparer);

        builder.HasOne(a => a.Vendor)
            .WithMany(v => v.Assets)
            .HasForeignKey(a => a.VendorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.AssignedEmployee)
            .WithMany(e => e.AssignedAssets)
            .HasForeignKey(a => a.AssignedTo)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.Category);
        builder.HasIndex(a => a.WarrantyExpiry);
    }
}
