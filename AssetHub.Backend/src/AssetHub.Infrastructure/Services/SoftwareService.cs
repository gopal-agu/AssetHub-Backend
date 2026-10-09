using AssetHub.Core.Common;
using AssetHub.Core.Dtos.Software;
using AssetHub.Core.Entities;
using AssetHub.Core.Interfaces.Repositories;
using AssetHub.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace AssetHub.Infrastructure.Services;

public class SoftwareService : ISoftwareService
{
    private readonly IUnitOfWork _uow;
    private readonly ISequenceService _sequences;
    private readonly IActivityLogService _activity;

    public SoftwareService(IUnitOfWork uow, ISequenceService sequences, IActivityLogService activity)
    {
        _uow = uow;
        _sequences = sequences;
        _activity = activity;
    }

    public async Task<List<SoftwareSubscriptionDto>> GetAllAsync(string? category = null, string? status = null)
    {
        var query = _uow.SoftwareSubscriptions.Query().Include(s => s.Vendor).AsQueryable();
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(s => s.Category == category);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(s => s.Status == status);

        var list = await query.AsNoTracking().OrderByDescending(s => s.CreatedAt).ToListAsync();
        return list.Select(Map).ToList();
    }

    public async Task<SoftwareSubscriptionDto> GetByIdAsync(string id)
    {
        var item = await _uow.SoftwareSubscriptions.Query().Include(s => s.Vendor).FirstOrDefaultAsync(s => s.Id == id)
            ?? throw ApiException.NotFound("Software Subscription", id);
        return Map(item);
    }

    public async Task<SoftwareSubscriptionDto> CreateAsync(CreateSoftwareSubscriptionDto dto)
    {
        var count = await _uow.SoftwareSubscriptions.Query().CountAsync();
        var code = !string.IsNullOrWhiteSpace(dto.AssetCode)
            ? dto.AssetCode
            : $"SW-{count + 1:D5}";

        var id = $"SFT-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        var item = new SoftwareSubscription
        {
            Id = id,
            AssetCode = code,
            ServiceName = dto.ServiceName,
            Category = dto.Category,
            VendorId = dto.VendorId,
            VendorName = dto.VendorName,
            VendorPortalUrl = dto.VendorPortalUrl,
            ResponsiblePersonId = dto.ResponsiblePersonId,
            ResponsiblePersonName = dto.ResponsiblePersonName,
            SeatsCount = dto.SeatsCount,
            PurchaseDate = DateOnly.TryParse(dto.PurchaseDate, out var pd) ? pd : null,
            NextRenewalDate = DateOnly.TryParse(dto.NextRenewalDate, out var rd) ? rd : DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)),
            ExpiryDate = DateOnly.TryParse(dto.ExpiryDate, out var ed) ? ed : DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            BillingType = dto.BillingType,
            Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "INR" : dto.Currency,
            Amount = dto.Amount,
            AutoRenewal = dto.AutoRenewal,
            PaymentMethod = dto.PaymentMethod,
            Status = dto.Status,
            AttachmentName = dto.AttachmentName,
            Description = dto.Description,
        };

        await _uow.SoftwareSubscriptions.AddAsync(item);
        await _uow.SaveChangesAsync();
        await _activity.LogAsync($"Software subscription {code} ({dto.ServiceName}) added.");

        return Map(item);
    }

    public async Task<SoftwareSubscriptionDto> UpdateAsync(string id, UpdateSoftwareSubscriptionDto dto)
    {
        var item = await _uow.SoftwareSubscriptions.GetByIdAsync(id)
            ?? throw ApiException.NotFound("Software Subscription", id);

        item.AssetCode = !string.IsNullOrWhiteSpace(dto.AssetCode) ? dto.AssetCode : item.AssetCode;
        item.ServiceName = dto.ServiceName;
        item.Category = dto.Category;
        item.VendorId = dto.VendorId;
        item.VendorName = dto.VendorName;
        item.VendorPortalUrl = dto.VendorPortalUrl;
        item.ResponsiblePersonId = dto.ResponsiblePersonId;
        item.ResponsiblePersonName = dto.ResponsiblePersonName;
        item.SeatsCount = dto.SeatsCount;
        item.PurchaseDate = DateOnly.TryParse(dto.PurchaseDate, out var pd) ? pd : item.PurchaseDate;
        item.NextRenewalDate = DateOnly.TryParse(dto.NextRenewalDate, out var rd) ? rd : item.NextRenewalDate;
        item.ExpiryDate = DateOnly.TryParse(dto.ExpiryDate, out var ed) ? ed : item.ExpiryDate;
        item.BillingType = dto.BillingType;
        item.Currency = string.IsNullOrWhiteSpace(dto.Currency) ? item.Currency : dto.Currency;
        item.Amount = dto.Amount;
        item.AutoRenewal = dto.AutoRenewal;
        item.PaymentMethod = dto.PaymentMethod;
        item.Status = dto.Status;
        item.AttachmentName = dto.AttachmentName;
        item.Description = dto.Description;

        _uow.SoftwareSubscriptions.Update(item);
        await _uow.SaveChangesAsync();
        await _activity.LogAsync($"Software subscription {item.AssetCode} ({dto.ServiceName}) updated.");

        return Map(item);
    }

    public async Task DeleteAsync(string id)
    {
        var item = await _uow.SoftwareSubscriptions.GetByIdAsync(id)
            ?? throw ApiException.NotFound("Software Subscription", id);

        _uow.SoftwareSubscriptions.Remove(item);
        await _uow.SaveChangesAsync();
        await _activity.LogAsync($"Software subscription {item.AssetCode} deleted.");
    }

    private static SoftwareSubscriptionDto Map(SoftwareSubscription s) => new()
    {
        Id = s.Id,
        AssetCode = s.AssetCode,
        ServiceName = s.ServiceName,
        Category = s.Category,
        VendorId = s.VendorId,
        VendorName = s.VendorName ?? s.Vendor?.Name,
        VendorPortalUrl = s.VendorPortalUrl,
        ResponsiblePersonId = s.ResponsiblePersonId,
        ResponsiblePersonName = s.ResponsiblePersonName,
        SeatsCount = s.SeatsCount,
        PurchaseDate = s.PurchaseDate?.ToString("yyyy-MM-dd"),
        NextRenewalDate = s.NextRenewalDate.ToString("yyyy-MM-dd"),
        ExpiryDate = s.ExpiryDate.ToString("yyyy-MM-dd"),
        BillingType = s.BillingType,
        Currency = s.Currency,
        Amount = s.Amount,
        AutoRenewal = s.AutoRenewal,
        PaymentMethod = s.PaymentMethod,
        Status = s.Status,
        AttachmentName = s.AttachmentName,
        Description = s.Description,
        CreatedAt = s.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
    };
}
