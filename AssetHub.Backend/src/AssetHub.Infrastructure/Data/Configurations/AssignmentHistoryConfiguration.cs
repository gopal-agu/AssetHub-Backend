using AssetHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetHub.Infrastructure.Data.Configurations;

public class AssignmentHistoryConfiguration : IEntityTypeConfiguration<AssignmentHistory>
{
    public void Configure(EntityTypeBuilder<AssignmentHistory> builder)
    {
        builder.ToTable("AssignmentHistories");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasMaxLength(40);

        builder.HasOne(h => h.Asset)
            .WithMany(a => a.AssignmentHistories)
            .HasForeignKey(h => h.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.Employee)
            .WithMany(e => e.AssignmentHistories)
            .HasForeignKey(h => h.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
