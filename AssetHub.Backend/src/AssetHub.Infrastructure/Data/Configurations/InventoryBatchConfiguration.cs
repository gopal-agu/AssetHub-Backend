//using AssetHub.Core.Entities;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;

//namespace AssetHub.Infrastructure.Data.Configurations;

//public class InventoryBatchConfiguration : IEntityTypeConfiguration<InventoryBatch>
//{
//    public void Configure(EntityTypeBuilder<InventoryBatch> builder)
//    {
//        builder.ToTable("InventoryBatches");
//        builder.HasKey(b => b.Id);
//        builder.Property(b => b.Id).HasMaxLength(30);
//        builder.Property(b => b.UnitCost).HasColumnType("decimal(18,2)");
//        builder.Property(b => b.TotalCost).HasColumnType("decimal(18,2)");
//        builder.Property(b => b.PurchaseCost).HasColumnType("decimal(18,2)");

//        builder.Property(b => b.Configuration)
//            .HasConversion(JsonValueConverters.StringDictConverter)
//            .Metadata.SetValueComparer(JsonValueConverters.StringDictComparer);

//        builder.HasOne(b => b.Vendor)
//            .WithMany(v => v.InventoryBatches)
//            .HasForeignKey(b => b.VendorId)
//            .OnDelete(DeleteBehavior.SetNull);

//        builder.HasMany(b => b.Items)
//            .WithOne(i => i.InventoryBatch)
//            .HasForeignKey(i => i.InventoryBatchId)
//            .OnDelete(DeleteBehavior.Cascade);

//        builder.HasMany(b => b.Assets)
//            .WithOne(a => a.Batch)
//            .HasForeignKey(a => a.BatchId)
//            .OnDelete(DeleteBehavior.SetNull);
//    }
//}
using AssetHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetHub.Infrastructure.Data.Configurations;

public class InventoryBatchConfiguration : IEntityTypeConfiguration<InventoryBatch>
{
  public void Configure(EntityTypeBuilder<InventoryBatch> builder)
  {
    builder.ToTable("InventoryBatches");
    builder.HasKey(b => b.Id);
    builder.Property(b => b.Id).HasMaxLength(30);
    builder.Property(b => b.UnitCost).HasColumnType("decimal(18,2)");
    builder.Property(b => b.TotalCost).HasColumnType("decimal(18,2)");
    builder.Property(b => b.PurchaseCost).HasColumnType("decimal(18,2)");

    builder.Property(b => b.Configuration)
        .HasConversion(JsonValueConverters.StringDictConverter)
        .Metadata.SetValueComparer(JsonValueConverters.StringDictComparer);

    builder.HasOne(b => b.Vendor)
        .WithMany(v => v.InventoryBatches)
        .HasForeignKey(b => b.VendorId)
        .OnDelete(DeleteBehavior.SetNull);

    builder.HasMany(b => b.Items)
        .WithOne(i => i.InventoryBatch)
        .HasForeignKey(i => i.InventoryBatchId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(b => b.Assets)
        .WithOne(a => a.Batch)
        .HasForeignKey(a => a.BatchId)
        .OnDelete(DeleteBehavior.SetNull);

    // Soft-delete: every normal query (GetAll, GetById, Query()) automatically excludes
    // IsDeleted rows. Use IgnoreQueryFilters() explicitly when an admin/audit view needs to
    // see deleted batches too (see InventoryService.GetAllAsync(includeDeleted: true)).
    builder.HasQueryFilter(b => !b.IsDeleted);
  }
}
