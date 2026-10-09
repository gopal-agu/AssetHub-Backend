using AssetHub.Core.Common;
using AssetHub.Core.Dtos.Vendor;
using AssetHub.Core.Entities;
using AssetHub.Core.Interfaces.Repositories;
using AssetHub.Core.Interfaces.Services;

namespace AssetHub.Infrastructure.Services;

public class VendorService : IVendorService
{
  private readonly IUnitOfWork _uow;
  private readonly ISequenceService _sequences;
  private readonly IActivityLogService _activity;

  public VendorService(IUnitOfWork uow, ISequenceService sequences, IActivityLogService activity)
  {
    _uow = uow;
    _sequences = sequences;
    _activity = activity;
  }

  public async Task<List<VendorDto>> GetAllAsync()
  {
    var vendors = await _uow.Vendors.GetAllAsync();
    var assets = await _uow.Assets.GetAllAsync();
    var counts = assets.Where(a => a.VendorId != null)
        .GroupBy(a => a.VendorId!)
        .ToDictionary(g => g.Key, g => g.Count());

    return vendors.Select(v => Map(v, counts.GetValueOrDefault(v.Id))).ToList();
  }

  public async Task<VendorDto> GetByIdAsync(string id)
  {
    var vendor = await _uow.Vendors.GetByIdAsync(id)
        ?? throw ApiException.NotFound("Vendor", id);
    var count = await _uow.Assets.CountAsync(a => a.VendorId == id);
    return Map(vendor, count);
  }

  public async Task<VendorDto> CreateAsync(CreateVendorDto dto)
  {
    var vendor = new Vendor
    {
      Id = await _sequences.GenerateVendorIdAsync(),
      Name = dto.Name,
      Categories = dto.Categories ?? new List<string>(),
      ContactPerson = dto.ContactPerson,
      Email = dto.Email,
      Phone = dto.Phone,
      City = dto.City,
      GstNumber = dto.GstNumber,
      Address = dto.Address,
      Rating = 4.0,
      Status = RecordStatuses.Active,
      OnboardedOn = DateOnly.FromDateTime(DateTime.UtcNow),
      PaymentTerms = string.IsNullOrWhiteSpace(dto.PaymentTerms) ? "Net 30" : dto.PaymentTerms,
    };

    await _uow.Vendors.AddAsync(vendor);
    await _uow.SaveChangesAsync();
    await _activity.LogAsync($"New vendor onboarded: {vendor.Name}.");

    return Map(vendor, 0);
  }

  public async Task<VendorDto> UpdateAsync(string id, UpdateVendorDto dto)
  {
    var vendor = await _uow.Vendors.GetByIdAsync(id)
        ?? throw ApiException.NotFound("Vendor", id);

    if (!string.IsNullOrWhiteSpace(dto.Name)) vendor.Name = dto.Name;
    if (dto.Categories != null) vendor.Categories = dto.Categories;
    if (dto.ContactPerson != null) vendor.ContactPerson = dto.ContactPerson;
    if (dto.Email != null) vendor.Email = dto.Email;
    if (dto.Phone != null) vendor.Phone = dto.Phone;
    if (dto.City != null) vendor.City = dto.City;
    if (dto.GstNumber != null) vendor.GstNumber = dto.GstNumber;
    if (dto.Address != null) vendor.Address = dto.Address;
    if (!string.IsNullOrWhiteSpace(dto.Status)) vendor.Status = dto.Status;
    if (!string.IsNullOrWhiteSpace(dto.PaymentTerms)) vendor.PaymentTerms = dto.PaymentTerms;
    if (dto.Rating.HasValue) vendor.Rating = dto.Rating.Value;
    vendor.UpdatedAt = DateTime.UtcNow;

    _uow.Vendors.Update(vendor);
    await _uow.SaveChangesAsync();
    await _activity.LogAsync($"Vendor {vendor.Id} updated.");

    var count = await _uow.Assets.CountAsync(a => a.VendorId == id);
    return Map(vendor, count);
  }

  public async Task DeleteAsync(string id)
  {
    var vendor = await _uow.Vendors.GetByIdAsync(id)
        ?? throw ApiException.NotFound("Vendor", id);
    _uow.Vendors.Remove(vendor);
    await _uow.SaveChangesAsync();
    await _activity.LogAsync($"Vendor {id} deleted.");
  }

  private static VendorDto Map(Vendor v, int assetCount) => new()
  {
    Id = v.Id,
    Name = v.Name,
    Categories = v.Categories,
    ContactPerson = v.ContactPerson,
    Email = v.Email,
    Phone = v.Phone,
    City = v.City,
    GstNumber = v.GstNumber,
    Address = v.Address,
    Rating = v.Rating,
    Status = v.Status,
    OnboardedOn = v.OnboardedOn.ToString("yyyy-MM-dd"),
    PaymentTerms = v.PaymentTerms,
    AssetCount = assetCount,
  };
}
