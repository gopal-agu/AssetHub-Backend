using AssetHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetHub.Infrastructure.Data.Configurations;

public class MaintenanceRecordConfiguration : IEntityTypeConfiguration<MaintenanceRecord>
{
    public void Configure(EntityTypeBuilder<MaintenanceRecord> builder)
    {
        builder.ToTable("MaintenanceRecords");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasMaxLength(30);
        builder.Property(m => m.Cost).HasColumnType("decimal(18,2)");

        builder.HasOne(m => m.Asset)
            .WithMany(a => a.MaintenanceRecords)
            .HasForeignKey(m => m.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Vendor)
            .WithMany(v => v.MaintenanceRecords)
            .HasForeignKey(m => m.VendorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(m => m.Status);
    }
}
