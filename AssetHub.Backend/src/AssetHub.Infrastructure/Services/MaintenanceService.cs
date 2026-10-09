using AssetHub.Core.Common;
using AssetHub.Core.Dtos.Maintenance;
using AssetHub.Core.Entities;
using AssetHub.Core.Interfaces.Repositories;
using AssetHub.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace AssetHub.Infrastructure.Services;

/// <summary>Raising a ticket also flips the Asset to "Under Repair"; closing it restores the asset
/// to Assigned/Available (or an explicit status), matching raiseMaintenance/closeMaintenance in DataContext.jsx.</summary>
public class MaintenanceService : IMaintenanceService
{
    private readonly IUnitOfWork _uow;
    private readonly ISequenceService _sequences;
    private readonly IActivityLogService _activity;

    public MaintenanceService(IUnitOfWork uow, ISequenceService sequences, IActivityLogService activity)
    {
        _uow = uow;
        _sequences = sequences;
        _activity = activity;
    }

    public async Task<List<MaintenanceRecordDto>> GetAllAsync(string? assetId = null, string? status = null)
    {
        var query = _uow.MaintenanceRecords.Query().Include(m => m.Vendor).AsQueryable();
        if (!string.IsNullOrWhiteSpace(assetId)) query = query.Where(m => m.AssetId == assetId);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(m => m.Status == status);

        var records = await query.AsNoTracking().OrderByDescending(m => m.OpenedOn).ToListAsync();
        return records.Select(Map).ToList();
    }

    public async Task<MaintenanceRecordDto> GetByIdAsync(string id)
    {
        var record = await _uow.MaintenanceRecords.Query().Include(m => m.Vendor).FirstOrDefaultAsync(m => m.Id == id)
            ?? throw ApiException.NotFound("Maintenance ticket", id);
        return Map(record);
    }

    public async Task<MaintenanceRecordDto> RaiseAsync(RaiseMaintenanceDto dto)
    {
        var asset = await _uow.Assets.GetByIdAsync(dto.AssetId)
            ?? await _uow.Assets.Query().FirstOrDefaultAsync(a => a.Id == dto.AssetId || a.SerialNumber == dto.AssetId)
            ?? throw ApiException.NotFound("Asset", dto.AssetId);

        var id = await _sequences.GenerateMaintenanceIdAsync();
        var openedOn = DateOnly.FromDateTime(DateTime.UtcNow);

        var record = new MaintenanceRecord
        {
            Id = id,
            AssetId = asset.Id,
            IssueType = dto.IssueType,
            Description = dto.Description,
            ReportedBy = string.IsNullOrWhiteSpace(dto.ReportedBy) ? (asset.AssignedToName ?? "IT Helpdesk") : dto.ReportedBy,
            OpenedOn = openedOn,
            ExpectedCompletion = openedOn,
            Status = MaintenanceStatuses.Open,
            Cost = 0,
            VendorId = asset.VendorId,
        };

        await _uow.MaintenanceRecords.AddAsync(record);

        asset.Status = AssetStatuses.UnderRepair;
        _uow.Assets.Update(asset);

        await _uow.SaveChangesAsync();
        await _activity.LogAsync($"Maintenance ticket {id} raised for {asset.Id} ({dto.IssueType}).");

        var saved = await _uow.MaintenanceRecords.Query().Include(m => m.Vendor).FirstAsync(m => m.Id == id);
        return Map(saved);
    }

    public async Task<MaintenanceRecordDto> CloseAsync(string id, CloseMaintenanceDto dto)
    {
        var record = await _uow.MaintenanceRecords.GetByIdAsync(id) ?? throw ApiException.NotFound("Maintenance ticket", id);

        record.Status = MaintenanceStatuses.Closed;
        record.CompletedOn = DateOnly.FromDateTime(DateTime.UtcNow);
        if (dto.Cost.HasValue) record.Cost = dto.Cost.Value;
        record.ResolutionNotes = dto.ResolutionNotes;
        _uow.MaintenanceRecords.Update(record);

        var asset = await _uow.Assets.GetByIdAsync(record.AssetId)
            ?? await _uow.Assets.Query().FirstOrDefaultAsync(a => a.Id == record.AssetId || a.SerialNumber == record.AssetId);
        if (asset != null)
        {
            asset.Status = !string.IsNullOrWhiteSpace(dto.RestoreStatus)
                ? dto.RestoreStatus
                : (asset.AssignedTo != null ? AssetStatuses.Assigned : AssetStatuses.Available);
            _uow.Assets.Update(asset);
        }

        await _uow.SaveChangesAsync();
        await _activity.LogAsync($"Maintenance ticket {id} closed for {record.AssetId}.");

        var saved = await _uow.MaintenanceRecords.Query().Include(m => m.Vendor).FirstAsync(m => m.Id == id);
        return Map(saved);
    }

    private static MaintenanceRecordDto Map(MaintenanceRecord m) => new()
    {
        Id = m.Id,
        AssetId = m.AssetId,
        IssueType = m.IssueType,
        Description = m.Description,
        ReportedBy = m.ReportedBy,
        OpenedOn = m.OpenedOn.ToString("yyyy-MM-dd"),
        ExpectedCompletion = m.ExpectedCompletion?.ToString("yyyy-MM-dd"),
        CompletedOn = m.CompletedOn?.ToString("yyyy-MM-dd"),
        Status = m.Status,
        Cost = m.Cost,
        ResolutionNotes = m.ResolutionNotes,
        VendorId = m.VendorId,
        VendorName = m.Vendor?.Name,
    };
}
