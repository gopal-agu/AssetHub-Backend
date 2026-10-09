//using AssetHub.Core.Common;
//using AssetHub.Core.Dtos.Asset;
//using AssetHub.Core.Dtos.Inventory;
//using AssetHub.Core.Entities;
//using AssetHub.Core.Interfaces.Repositories;
//using AssetHub.Core.Interfaces.Services;

//namespace AssetHub.Infrastructure.Services;

///// <summary>
///// Business logic behind the Inventory ("goods intake") module. Mirrors DataContext.jsx's
///// addInventoryBatch/updateInventoryBatch: one intake call writes the batch header, its free-form
///// child items, and — when category configuration values are supplied — bulk-generates the
///// individual Asset rows (tag + serial + inherited vendor/warranty/config) in the same transaction.
///// </summary>
//public class InventoryService : IInventoryService
//{
//    private readonly IUnitOfWork _uow;
//    private readonly ISequenceService _sequences;
//    private readonly IActivityLogService _activity;

//    public InventoryService(IUnitOfWork uow, ISequenceService sequences, IActivityLogService activity)
//    {
//        _uow = uow;
//        _sequences = sequences;
//        _activity = activity;
//    }

//    public async Task<List<InventoryBatchDto>> GetAllAsync()
//    {
//        var batches = await _uow.InventoryBatches.GetAllWithItemsAsync();
//        return batches.Select(Map).ToList();
//    }

//    public async Task<InventoryBatchDto> GetByIdAsync(string id)
//    {
//        var batch = await _uow.InventoryBatches.GetByIdWithItemsAsync(id)
//            ?? throw ApiException.NotFound("Inventory batch", id);
//        return Map(batch);
//    }

//    public async Task<InventoryBatchDto> CreateAsync(CreateInventoryBatchDto dto)
//    {
//        var batchId = !string.IsNullOrWhiteSpace(dto.InventoryId) ? dto.InventoryId! : await _sequences.GenerateBatchIdAsync();

//        if (!string.IsNullOrWhiteSpace(dto.VendorId) && !await _uow.Vendors.ExistsAsync(v => v.Id == dto.VendorId))
//            throw ApiException.NotFound("Vendor", dto.VendorId);

//        var cost = dto.PurchaseCost ?? dto.UnitCost ?? dto.TotalCost ?? 0m;
//        var purchaseDate = ParseDate(dto.PurchaseDate) ?? DateOnly.FromDateTime(DateTime.UtcNow);
//        var warrantyStart = ParseDate(dto.WarrantyStartDate) ?? purchaseDate;
//        var warrantyEnd = ParseDate(dto.WarrantyEndDate) ?? warrantyStart.AddMonths(12);

//        var childItems = dto.Items.Select((it, idx) => new InventoryItem
//        {
//            Id = $"{batchId}-{idx + 1}",
//            InventoryBatchId = batchId,
//            Category = it.Category ?? dto.Category ?? string.Empty,
//            Item = it.Item ?? it.Category ?? dto.Category ?? string.Empty,
//            Brand = it.Brand ?? string.Empty,
//            Configuration = it.Configuration ?? string.Empty,
//            SerialNo = it.SerialNo ?? string.Empty,
//        }).ToList();

//        var batchCategory = !string.IsNullOrWhiteSpace(dto.Category)
//            ? dto.Category
//            : (childItems.Select(c => c.Category).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().DefaultIfEmpty("—").Aggregate((a, b) => $"{a}, {b}"));

//        var batchConfig = dto.Configuration ?? new Dictionary<string, string>();
//        var batchConfigSummary = string.Join(" · ", batchConfig.Values.Where(v => !string.IsNullOrWhiteSpace(v)));

//        if (batchConfig.Count == 0 && childItems.Any(c => !string.IsNullOrWhiteSpace(c.Configuration)))
//        {
//            var summaries = childItems
//                .Select(c => c.Configuration)
//                .Where(c => !string.IsNullOrWhiteSpace(c))
//                .Distinct()
//                .ToList();
//            batchConfigSummary = string.Join(" · ", summaries);

//            var firstWithConfig = childItems.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.Configuration));
//            if (firstWithConfig != null)
//            {
//                var (parsedDict, _) = ParseConfiguration(firstWithConfig.Configuration, firstWithConfig.Brand, firstWithConfig.Item);
//                batchConfig = parsedDict;
//            }
//        }

//        var batch = new InventoryBatch
//        {
//            Id = batchId,
//            Category = batchCategory,
//            Department = string.IsNullOrWhiteSpace(dto.Department) ? "Engineering" : dto.Department,
//            Description = dto.Description,
//            VendorId = dto.VendorId,
//            Quantity = dto.Quantity > 0 ? dto.Quantity : Math.Max(childItems.Count, 1),
//            UnitCost = cost,
//            TotalCost = cost,
//            PurchaseCost = cost,
//            PurchaseDate = purchaseDate,
//            WarrantyStartDate = warrantyStart,
//            WarrantyEndDate = warrantyEnd,
//            Location = string.IsNullOrWhiteSpace(dto.Location) ? "Pune - Kharadi HQ" : dto.Location,
//            Condition = string.IsNullOrWhiteSpace(dto.Condition) ? "New" : dto.Condition,
//            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Available" : dto.Status,
//            InvoiceNo = dto.InvoiceNo,
//            PoNumber = dto.PoNumber,
//            Remarks = dto.Remarks,
//            AttachmentName = dto.AttachmentName,
//            ReceivedBy = "IT Asset Desk",
//            Configuration = batchConfig,
//            ConfigSummary = batchConfigSummary,
//            AssetsGenerated = childItems.Count > 0 ? childItems.Count : 1,
//        };

//        await _uow.InventoryBatches.AddAsync(batch);
//        await _uow.InventoryItems.AddRangeAsync(childItems);

//        // Auto-generate individual Asset records from child items (or bulk quantity)
//        var generatedAssets = new List<Asset>();
//        var totalAssetsToGenerate = Math.Max(batch.Quantity, childItems.Count);
//        if (totalAssetsToGenerate > 0 && !string.IsNullOrWhiteSpace(batchCategory))
//        {
//            for (var i = 0; i < totalAssetsToGenerate; i++)
//            {
//                var child = i < childItems.Count ? childItems[i] : null;
//                var assetCategory = !string.IsNullOrWhiteSpace(child?.Category)
//                    ? child.Category
//                    : (!string.IsNullOrWhiteSpace(dto.Category) ? dto.Category : batchCategory);

//                var tag = await _sequences.GenerateAssetTagAsync(assetCategory);
//                var serial = !string.IsNullOrWhiteSpace(child?.SerialNo)
//                    ? child.SerialNo
//                    : await _sequences.GenerateInternalSerialAsync(assetCategory);

//                var (assetConfigDict, assetConfigSummary) = ParseConfiguration(
//                    child?.Configuration,
//                    child?.Brand,
//                    child?.Item,
//                    batchConfig);

//                var asset = new Asset
//                {
//                    Id = tag,
//                    SerialNumber = serial,
//                    Category = assetCategory,
//                    Configuration = assetConfigDict,
//                    ConfigSummary = assetConfigSummary,
//                    BatchId = batch.Id,
//                    VendorId = batch.VendorId,
//                    PurchaseDate = batch.PurchaseDate,
//                    UnitCost = cost,
//                    PoNumber = batch.PoNumber,
//                    InvoiceNo = batch.InvoiceNo,
//                    WarrantyMonths = ((warrantyEnd.Year - warrantyStart.Year) * 12) + warrantyEnd.Month - warrantyStart.Month,
//                    WarrantyExpiry = warrantyEnd,
//                    Location = batch.Location,
//                    Condition = batch.Condition == "New" ? "Excellent" : batch.Condition,
//                    Status = "Available",
//                    Notes = string.Empty,
//                };
//                generatedAssets.Add(asset);
//            }
//            await _uow.Assets.AddRangeAsync(generatedAssets);
//            batch.AssetsGenerated = generatedAssets.Count;
//        }

//        await _uow.SaveChangesAsync();
//        await _activity.LogAsync($"Inventory item {batch.Id} recorded with {batch.Quantity} unit(s) ({batchCategory} · {batch.Department}).");

//        var saved = await _uow.InventoryBatches.GetByIdWithItemsAsync(batch.Id) ?? batch;
//        return Map(saved, generatedAssets);
//    }

//    public async Task<InventoryBatchDto> UpdateAsync(string id, UpdateInventoryBatchDto dto)
//    {
//        var batch = await _uow.InventoryBatches.GetByIdWithItemsAsync(id)
//            ?? throw ApiException.NotFound("Inventory batch", id);

//        if (!string.IsNullOrWhiteSpace(dto.Category)) batch.Category = dto.Category;
//        if (!string.IsNullOrWhiteSpace(dto.Department)) batch.Department = dto.Department;
//        if (dto.Description != null) batch.Description = dto.Description;
//        if (!string.IsNullOrWhiteSpace(dto.VendorId)) batch.VendorId = dto.VendorId;
//        if (dto.Quantity.HasValue) batch.Quantity = dto.Quantity.Value;
//        if (dto.PurchaseCost.HasValue)
//        {
//            batch.PurchaseCost = dto.PurchaseCost.Value;
//            batch.UnitCost = dto.PurchaseCost.Value;
//            batch.TotalCost = dto.PurchaseCost.Value;
//        }
//        var purchaseDate = ParseDate(dto.PurchaseDate);
//        if (purchaseDate.HasValue) batch.PurchaseDate = purchaseDate.Value;
//        var warrantyStart = ParseDate(dto.WarrantyStartDate);
//        if (warrantyStart.HasValue) batch.WarrantyStartDate = warrantyStart.Value;
//        var warrantyEnd = ParseDate(dto.WarrantyEndDate);
//        if (warrantyEnd.HasValue) batch.WarrantyEndDate = warrantyEnd.Value;
//        if (!string.IsNullOrWhiteSpace(dto.Location)) batch.Location = dto.Location;
//        if (!string.IsNullOrWhiteSpace(dto.Condition)) batch.Condition = dto.Condition;
//        if (!string.IsNullOrWhiteSpace(dto.Status)) batch.Status = dto.Status;
//        if (dto.InvoiceNo != null) batch.InvoiceNo = dto.InvoiceNo;
//        if (dto.PoNumber != null) batch.PoNumber = dto.PoNumber;
//        if (dto.Remarks != null) batch.Remarks = dto.Remarks;
//        if (dto.AttachmentName != null) batch.AttachmentName = dto.AttachmentName;
//        batch.UpdatedAt = DateTime.UtcNow;

//        _uow.InventoryBatches.Update(batch);
//        await _uow.SaveChangesAsync();
//        await _activity.LogAsync($"Inventory item {batch.Id} updated.");

//        return Map(batch);
//    }

//    private static DateOnly? ParseDate(string? value) =>
//        !string.IsNullOrWhiteSpace(value) && DateOnly.TryParse(value, out var d) ? d : null;

//    private static (Dictionary<string, string> ConfigDict, string ConfigSummary) ParseConfiguration(
//        string? rawConfig,
//        string? brand = null,
//        string? item = null,
//        Dictionary<string, string>? fallbackDict = null)
//    {
//        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

//        if (fallbackDict != null)
//        {
//            foreach (var (k, v) in fallbackDict)
//            {
//                if (!string.IsNullOrWhiteSpace(k) && !string.IsNullOrWhiteSpace(v))
//                    dict[k] = v;
//            }
//        }

//        var brandModel = !string.IsNullOrWhiteSpace(item) && !string.IsNullOrWhiteSpace(brand) && !item.Contains(brand, StringComparison.OrdinalIgnoreCase)
//            ? $"{brand} {item}".Trim()
//            : (!string.IsNullOrWhiteSpace(item) ? item : brand);

//        if (!string.IsNullOrWhiteSpace(brandModel) && !dict.ContainsKey("brandModel"))
//        {
//            dict["brandModel"] = brandModel;
//        }

//        if (!string.IsNullOrWhiteSpace(rawConfig))
//        {
//            var trimmed = rawConfig.Trim();
//            if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
//            {
//                try
//                {
//                    var parsed = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(trimmed);
//                    if (parsed != null)
//                    {
//                        foreach (var (k, v) in parsed)
//                        {
//                            if (!string.IsNullOrWhiteSpace(k) && !string.IsNullOrWhiteSpace(v))
//                                dict[k] = v;
//                        }
//                    }
//                }
//                catch
//                {
//                    ParseTextIntoDict(trimmed, dict);
//                }
//            }
//            else
//            {
//                ParseTextIntoDict(trimmed, dict);
//            }
//        }

//        string summary;
//        if (!string.IsNullOrWhiteSpace(rawConfig) && !(rawConfig.Trim().StartsWith("{") && rawConfig.Trim().EndsWith("}")))
//        {
//            summary = rawConfig.Trim();
//        }
//        else
//        {
//            var summaryParts = dict
//                .Where(kv => !kv.Key.Equals("brandModel", StringComparison.OrdinalIgnoreCase))
//                .Select(kv => kv.Value)
//                .Where(v => !string.IsNullOrWhiteSpace(v));
//            summary = string.Join(" · ", summaryParts);
//            if (string.IsNullOrWhiteSpace(summary) && dict.ContainsKey("brandModel"))
//            {
//                summary = dict["brandModel"];
//            }
//        }

//        return (dict, summary);
//    }

//    private static void ParseTextIntoDict(string text, Dictionary<string, string> dict)
//    {
//        var delimiters = new[] { ',', '·', ';', '\n', '\r' };
//        var parts = text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

//        var unnamedIndex = 1;
//        foreach (var part in parts)
//        {
//            var colonIdx = part.IndexOf(':');
//            var eqIdx = part.IndexOf('=');
//            var splitIdx = colonIdx >= 0 ? colonIdx : eqIdx;

//            if (splitIdx > 0 && splitIdx < part.Length - 1)
//            {
//                var key = part[..splitIdx].Trim();
//                var val = part[(splitIdx + 1)..].Trim();
//                if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(val))
//                {
//                    dict[key] = val;
//                    continue;
//                }
//            }

//            var lower = part.ToLowerInvariant();
//            if (lower.Contains("ram") || (lower.EndsWith("gb") && !lower.Contains("ssd") && !lower.Contains("hdd")))
//            {
//                dict["ram"] = part;
//            }
//            else if (lower.Contains("i3") || lower.Contains("i5") || lower.Contains("i7") || lower.Contains("i9") || lower.Contains("ryzen") || lower.Contains("cpu") || lower.Contains("m1") || lower.Contains("m2") || lower.Contains("m3"))
//            {
//                dict["cpu"] = part;
//            }
//            else if (lower.Contains("ssd") || lower.Contains("hdd") || lower.Contains("storage") || lower.Contains("tb"))
//            {
//                dict["storage"] = part;
//            }
//            else
//            {
//                dict[$"spec_{unnamedIndex++}"] = part;
//            }
//        }

//        if (!dict.Any(k => !k.Key.Equals("brandModel", StringComparison.OrdinalIgnoreCase)))
//        {
//            dict["specification"] = text;
//        }
//    }

//    private static AssetDto MapAsset(Asset a) => new()
//    {
//        Id = a.Id,
//        SerialNumber = a.SerialNumber,
//        Category = a.Category,
//        Configuration = a.Configuration,
//        ConfigSummary = a.ConfigSummary,
//        BatchId = a.BatchId,
//        VendorId = a.VendorId,
//        VendorName = a.Vendor?.Name ?? "—",
//        PurchaseDate = a.PurchaseDate.ToString("yyyy-MM-dd"),
//        UnitCost = a.UnitCost,
//        PoNumber = a.PoNumber,
//        InvoiceNo = a.InvoiceNo,
//        WarrantyMonths = a.WarrantyMonths,
//        WarrantyExpiry = a.WarrantyExpiry?.ToString("yyyy-MM-dd"),
//        Location = a.Location,
//        Condition = a.Condition,
//        Status = a.Status,
//        AssignedTo = a.AssignedTo,
//        AssignedToName = a.AssignedToName,
//        AssignedDept = a.AssignedDept,
//        AssignedOn = a.AssignedOn?.ToString("yyyy-MM-dd"),
//        Notes = a.Notes,
//    };

//    private static InventoryBatchDto Map(InventoryBatch b) => Map(b, null);

//    private static InventoryBatchDto Map(InventoryBatch b, List<Asset>? assets) => new()
//    {
//        Id = b.Id,
//        InventoryId = b.Id,
//        Category = b.Category,
//        Department = b.Department,
//        Description = b.Description,
//        VendorId = b.VendorId,
//        VendorName = b.Vendor?.Name ?? "—",
//        Quantity = b.Quantity,
//        UnitCost = b.UnitCost,
//        TotalCost = b.TotalCost,
//        PurchaseCost = b.PurchaseCost,
//        PurchaseDate = b.PurchaseDate.ToString("yyyy-MM-dd"),
//        WarrantyStartDate = b.WarrantyStartDate.ToString("yyyy-MM-dd"),
//        WarrantyEndDate = b.WarrantyEndDate?.ToString("yyyy-MM-dd"),
//        WarrantyExpiry = b.WarrantyEndDate?.ToString("yyyy-MM-dd"),
//        Location = b.Location,
//        Condition = b.Condition,
//        Status = b.Status,
//        InvoiceNo = b.InvoiceNo,
//        PoNumber = b.PoNumber,
//        Remarks = b.Remarks,
//        AttachmentName = b.AttachmentName,
//        ReceivedBy = b.ReceivedBy,
//        CreatedAt = b.CreatedAt.ToString("yyyy-MM-dd"),
//        AssetsGenerated = b.AssetsGenerated,
//        Configuration = b.Configuration,
//        ConfigSummary = b.ConfigSummary,
//        Items = b.Items.Select(i => new InventoryItemDto
//        {
//            Id = i.Id,
//            InventoryId = i.InventoryBatchId,
//            Category = i.Category,
//            Item = i.Item,
//            Brand = i.Brand,
//            Configuration = i.Configuration,
//            SerialNo = i.SerialNo,
//        }).ToList(),
//        Assets = (assets ?? b.Assets?.ToList() ?? new List<Asset>()).Select(MapAsset).ToList(),
//    };
//}

using AssetHub.Core.Common;
using AssetHub.Core.Dtos.Asset;
using AssetHub.Core.Dtos.Inventory;
using AssetHub.Core.Entities;
using AssetHub.Core.Interfaces.Repositories;
using AssetHub.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace AssetHub.Infrastructure.Services;

/// <summary>
/// Business logic behind the Inventory ("goods intake") module. Mirrors DataContext.jsx's
/// addInventoryBatch/updateInventoryBatch: one intake call writes the batch header, its free-form
/// child items, and — when category configuration values are supplied — bulk-generates the
/// individual Asset rows (tag + serial + inherited vendor/warranty/config) in the same transaction.
/// </summary>
public class InventoryService : IInventoryService
{
  private readonly IUnitOfWork _uow;
  private readonly ISequenceService _sequences;
  private readonly IActivityLogService _activity;

  public InventoryService(IUnitOfWork uow, ISequenceService sequences, IActivityLogService activity)
  {
    _uow = uow;
    _sequences = sequences;
    _activity = activity;
  }

  public async Task<List<InventoryBatchDto>> GetAllAsync(bool includeDeleted = false)
  {
    if (!includeDeleted)
    {
      var batches = await _uow.InventoryBatches.GetAllWithItemsAsync();
      return batches.Select(b => Map(b)).ToList();
    }

    // Admin/audit view: bypass the IsDeleted query filter to also show soft-deleted batches.
    var all = await _uow.InventoryBatches.Query()
        .IgnoreQueryFilters()
        .Include(b => b.Items)
        .Include(b => b.Vendor)
        .OrderByDescending(b => b.CreatedAt)
        .AsNoTracking()
        .ToListAsync();

    return all.Select(b => Map(b)).ToList();
  }

  public async Task<InventoryBatchDto> GetByIdAsync(string id)
  {
    var batch = await _uow.InventoryBatches.GetByIdWithItemsAsync(id)
        ?? throw ApiException.NotFound("Inventory batch", id);
    return Map(batch);
  }

  public async Task<InventoryBatchDto> CreateAsync(CreateInventoryBatchDto dto)
  {
    var batchId = !string.IsNullOrWhiteSpace(dto.InventoryId) ? dto.InventoryId! : await _sequences.GenerateBatchIdAsync();

    if (!string.IsNullOrWhiteSpace(dto.VendorId) && !await _uow.Vendors.ExistsAsync(v => v.Id == dto.VendorId))
      throw ApiException.NotFound("Vendor", dto.VendorId);

    var cost = dto.PurchaseCost ?? dto.UnitCost ?? dto.TotalCost ?? 0m;
    var purchaseDate = ParseDate(dto.PurchaseDate) ?? DateOnly.FromDateTime(DateTime.UtcNow);
    var warrantyStart = ParseDate(dto.WarrantyStartDate) ?? purchaseDate;
    var warrantyEnd = ParseDate(dto.WarrantyEndDate) ?? warrantyStart.AddMonths(12);

    var childItems = dto.Items.Select((it, idx) => new InventoryItem
    {
      Id = $"{batchId}-{idx + 1}",
      InventoryBatchId = batchId,
      Category = it.Category ?? dto.Category ?? string.Empty,
      Item = it.Item ?? it.Category ?? dto.Category ?? string.Empty,
      Brand = it.Brand ?? string.Empty,
      Configuration = it.Configuration ?? string.Empty,
      SerialNo = it.SerialNo ?? string.Empty,
    }).ToList();

    var batchCategory = !string.IsNullOrWhiteSpace(dto.Category)
        ? dto.Category
        : (childItems.Select(c => c.Category).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().DefaultIfEmpty("—").Aggregate((a, b) => $"{a}, {b}"));

    var batchConfig = dto.Configuration ?? new Dictionary<string, string>();
    var batchConfigSummary = string.Join(" · ", batchConfig.Values.Where(v => !string.IsNullOrWhiteSpace(v)));

    if (batchConfig.Count == 0 && childItems.Any(c => !string.IsNullOrWhiteSpace(c.Configuration)))
    {
      var summaries = childItems
          .Select(c => c.Configuration)
          .Where(c => !string.IsNullOrWhiteSpace(c))
          .Distinct()
          .ToList();
      batchConfigSummary = string.Join(" · ", summaries);

      var firstWithConfig = childItems.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.Configuration));
      if (firstWithConfig != null)
      {
        var (parsedDict, _) = ParseConfiguration(firstWithConfig.Configuration, firstWithConfig.Brand, firstWithConfig.Item);
        batchConfig = parsedDict;
      }
    }

    var batch = new InventoryBatch
    {
      Id = batchId,
      Category = batchCategory,
      Department = string.IsNullOrWhiteSpace(dto.Department) ? "Engineering" : dto.Department,
      Description = dto.Description,
      VendorId = dto.VendorId,
      Quantity = dto.Quantity > 0 ? dto.Quantity : Math.Max(childItems.Count, 1),
      UnitCost = cost,
      TotalCost = cost,
      PurchaseCost = cost,
      PurchaseDate = purchaseDate,
      WarrantyStartDate = warrantyStart,
      WarrantyEndDate = warrantyEnd,
      Location = string.IsNullOrWhiteSpace(dto.Location) ? "Pune - Kharadi HQ" : dto.Location,
      Condition = string.IsNullOrWhiteSpace(dto.Condition) ? "New" : dto.Condition,
      Status = string.IsNullOrWhiteSpace(dto.Status) ? "Available" : dto.Status,
      InvoiceNo = dto.InvoiceNo,
      PoNumber = dto.PoNumber,
      Remarks = dto.Remarks,
      AttachmentName = dto.AttachmentName,
      ReceivedBy = "IT Asset Desk",
      Configuration = batchConfig,
      ConfigSummary = batchConfigSummary,
      AssetsGenerated = childItems.Count > 0 ? childItems.Count : 1,
    };

    await _uow.InventoryBatches.AddAsync(batch);
    await _uow.InventoryItems.AddRangeAsync(childItems);

    // Auto-generate individual Asset records from child items (or bulk quantity)
    var generatedAssets = new List<Asset>();
    var totalAssetsToGenerate = Math.Max(batch.Quantity, childItems.Count);
    if (totalAssetsToGenerate > 0 && !string.IsNullOrWhiteSpace(batchCategory))
    {
      for (var i = 0; i < totalAssetsToGenerate; i++)
      {
        var child = i < childItems.Count ? childItems[i] : null;
        var assetCategory = !string.IsNullOrWhiteSpace(child?.Category)
            ? child.Category
            : (!string.IsNullOrWhiteSpace(dto.Category) ? dto.Category : batchCategory);

        var tag = await _sequences.GenerateAssetTagAsync(assetCategory);
        var serial = !string.IsNullOrWhiteSpace(child?.SerialNo)
            ? child.SerialNo
            : await _sequences.GenerateInternalSerialAsync(assetCategory);

        var (assetConfigDict, assetConfigSummary) = ParseConfiguration(
            child?.Configuration,
            child?.Brand,
            child?.Item,
            batchConfig);

        var asset = new Asset
        {
          Id = tag,
          SerialNumber = serial,
          Category = assetCategory,
          Configuration = assetConfigDict,
          ConfigSummary = assetConfigSummary,
          BatchId = batch.Id,
          VendorId = batch.VendorId,
          PurchaseDate = batch.PurchaseDate,
          UnitCost = cost,
          PoNumber = batch.PoNumber,
          InvoiceNo = batch.InvoiceNo,
          WarrantyMonths = ((warrantyEnd.Year - warrantyStart.Year) * 12) + warrantyEnd.Month - warrantyStart.Month,
          WarrantyExpiry = warrantyEnd,
          Location = batch.Location,
          Condition = batch.Condition == "New" ? "Excellent" : batch.Condition,
          Status = "Available",
          Notes = string.Empty,
        };
        generatedAssets.Add(asset);
      }
      await _uow.Assets.AddRangeAsync(generatedAssets);
      batch.AssetsGenerated = generatedAssets.Count;
    }

    await _uow.SaveChangesAsync();
    await _activity.LogAsync($"Inventory item {batch.Id} recorded with {batch.Quantity} unit(s) ({batchCategory} · {batch.Department}).");

    var saved = await _uow.InventoryBatches.GetByIdWithItemsAsync(batch.Id) ?? batch;
    return Map(saved, generatedAssets);
  }

  public async Task<InventoryBatchDto> UpdateAsync(string id, UpdateInventoryBatchDto dto)
  {
    var batch = await _uow.InventoryBatches.GetByIdWithItemsAsync(id)
        ?? throw ApiException.NotFound("Inventory batch", id);

    if (!string.IsNullOrWhiteSpace(dto.Category)) batch.Category = dto.Category;
    if (!string.IsNullOrWhiteSpace(dto.Department)) batch.Department = dto.Department;
    if (dto.Description != null) batch.Description = dto.Description;
    if (!string.IsNullOrWhiteSpace(dto.VendorId)) batch.VendorId = dto.VendorId;
    if (dto.Quantity.HasValue) batch.Quantity = dto.Quantity.Value;
    if (dto.PurchaseCost.HasValue)
    {
      batch.PurchaseCost = dto.PurchaseCost.Value;
      batch.UnitCost = dto.PurchaseCost.Value;
      batch.TotalCost = dto.PurchaseCost.Value;
    }
    var purchaseDate = ParseDate(dto.PurchaseDate);
    if (purchaseDate.HasValue) batch.PurchaseDate = purchaseDate.Value;
    var warrantyStart = ParseDate(dto.WarrantyStartDate);
    if (warrantyStart.HasValue) batch.WarrantyStartDate = warrantyStart.Value;
    var warrantyEnd = ParseDate(dto.WarrantyEndDate);
    if (warrantyEnd.HasValue) batch.WarrantyEndDate = warrantyEnd.Value;
    if (!string.IsNullOrWhiteSpace(dto.Location)) batch.Location = dto.Location;
    if (!string.IsNullOrWhiteSpace(dto.Condition)) batch.Condition = dto.Condition;
    if (!string.IsNullOrWhiteSpace(dto.Status)) batch.Status = dto.Status;
    if (dto.InvoiceNo != null) batch.InvoiceNo = dto.InvoiceNo;
    if (dto.PoNumber != null) batch.PoNumber = dto.PoNumber;
    if (dto.Remarks != null) batch.Remarks = dto.Remarks;
    if (dto.AttachmentName != null) batch.AttachmentName = dto.AttachmentName;
    batch.UpdatedAt = DateTime.UtcNow;

    _uow.InventoryBatches.Update(batch);
    await _uow.SaveChangesAsync();
    await _activity.LogAsync($"Inventory item {batch.Id} updated.");

    return Map(batch);
  }

  public async Task DeleteAsync(string id)
  {
    var batch = await _uow.InventoryBatches.GetByIdAsync(id) ?? throw ApiException.NotFound("Inventory batch", id);
    batch.IsDeleted = true;
    batch.DeletedAt = DateTime.UtcNow;
    _uow.InventoryBatches.Update(batch);
    await _uow.SaveChangesAsync();
    await _activity.LogAsync($"Inventory item {id} deleted.");
  }

  public async Task<InventoryBatchDto> RestoreAsync(string id)
  {
    var batch = await _uow.InventoryBatches.Query()
        .IgnoreQueryFilters()
        .Include(b => b.Items)
        .Include(b => b.Vendor)
        .FirstOrDefaultAsync(b => b.Id == id)
        ?? throw ApiException.NotFound("Inventory batch", id);

    if (!batch.IsDeleted) return Map(batch); // already active, nothing to do

    batch.IsDeleted = false;
    batch.DeletedAt = null;
    _uow.InventoryBatches.Update(batch);
    await _uow.SaveChangesAsync();
    await _activity.LogAsync($"Inventory item {id} restored.");

    return Map(batch);
  }

  private static DateOnly? ParseDate(string? value) =>
      !string.IsNullOrWhiteSpace(value) && DateOnly.TryParse(value, out var d) ? d : null;

  private static (Dictionary<string, string> ConfigDict, string ConfigSummary) ParseConfiguration(
      string? rawConfig,
      string? brand = null,
      string? item = null,
      Dictionary<string, string>? fallbackDict = null)
  {
    var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    if (fallbackDict != null)
    {
      foreach (var (k, v) in fallbackDict)
      {
        if (!string.IsNullOrWhiteSpace(k) && !string.IsNullOrWhiteSpace(v))
          dict[k] = v;
      }
    }

    var brandModel = !string.IsNullOrWhiteSpace(item) && !string.IsNullOrWhiteSpace(brand) && !item.Contains(brand, StringComparison.OrdinalIgnoreCase)
        ? $"{brand} {item}".Trim()
        : (!string.IsNullOrWhiteSpace(item) ? item : brand);

    if (!string.IsNullOrWhiteSpace(brandModel) && !dict.ContainsKey("brandModel"))
    {
      dict["brandModel"] = brandModel;
    }

    if (!string.IsNullOrWhiteSpace(rawConfig))
    {
      var trimmed = rawConfig.Trim();
      if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
      {
        try
        {
          var parsed = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(trimmed);
          if (parsed != null)
          {
            foreach (var (k, v) in parsed)
            {
              if (!string.IsNullOrWhiteSpace(k) && !string.IsNullOrWhiteSpace(v))
                dict[k] = v;
            }
          }
        }
        catch
        {
          ParseTextIntoDict(trimmed, dict);
        }
      }
      else
      {
        ParseTextIntoDict(trimmed, dict);
      }
    }

    string summary;
    if (!string.IsNullOrWhiteSpace(rawConfig) && !(rawConfig.Trim().StartsWith("{") && rawConfig.Trim().EndsWith("}")))
    {
      summary = rawConfig.Trim();
    }
    else
    {
      var summaryParts = dict
          .Where(kv => !kv.Key.Equals("brandModel", StringComparison.OrdinalIgnoreCase))
          .Select(kv => kv.Value)
          .Where(v => !string.IsNullOrWhiteSpace(v));
      summary = string.Join(" · ", summaryParts);
      if (string.IsNullOrWhiteSpace(summary) && dict.ContainsKey("brandModel"))
      {
        summary = dict["brandModel"];
      }
    }

    return (dict, summary);
  }

  private static void ParseTextIntoDict(string text, Dictionary<string, string> dict)
  {
    var delimiters = new[] { ',', '·', ';', '\n', '\r' };
    var parts = text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    var unnamedIndex = 1;
    foreach (var part in parts)
    {
      var colonIdx = part.IndexOf(':');
      var eqIdx = part.IndexOf('=');
      var splitIdx = colonIdx >= 0 ? colonIdx : eqIdx;

      if (splitIdx > 0 && splitIdx < part.Length - 1)
      {
        var key = part[..splitIdx].Trim();
        var val = part[(splitIdx + 1)..].Trim();
        if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(val))
        {
          dict[key] = val;
          continue;
        }
      }

      var lower = part.ToLowerInvariant();
      if (lower.Contains("ram") || (lower.EndsWith("gb") && !lower.Contains("ssd") && !lower.Contains("hdd")))
      {
        dict["ram"] = part;
      }
      else if (lower.Contains("i3") || lower.Contains("i5") || lower.Contains("i7") || lower.Contains("i9") || lower.Contains("ryzen") || lower.Contains("cpu") || lower.Contains("m1") || lower.Contains("m2") || lower.Contains("m3"))
      {
        dict["cpu"] = part;
      }
      else if (lower.Contains("ssd") || lower.Contains("hdd") || lower.Contains("storage") || lower.Contains("tb"))
      {
        dict["storage"] = part;
      }
      else
      {
        dict[$"spec_{unnamedIndex++}"] = part;
      }
    }

    if (!dict.Any(k => !k.Key.Equals("brandModel", StringComparison.OrdinalIgnoreCase)))
    {
      dict["specification"] = text;
    }
  }

  private static AssetDto MapAsset(Asset a) => new()
  {
    Id = a.Id,
    SerialNumber = a.SerialNumber,
    Category = a.Category,
    Configuration = a.Configuration,
    ConfigSummary = a.ConfigSummary,
    BatchId = a.BatchId,
    VendorId = a.VendorId,
    VendorName = a.Vendor?.Name ?? "—",
    PurchaseDate = a.PurchaseDate.ToString("yyyy-MM-dd"),
    UnitCost = a.UnitCost,
    PoNumber = a.PoNumber,
    InvoiceNo = a.InvoiceNo,
    WarrantyMonths = a.WarrantyMonths,
    WarrantyExpiry = a.WarrantyExpiry?.ToString("yyyy-MM-dd"),
    Location = a.Location,
    Condition = a.Condition,
    Status = a.Status,
    AssignedTo = a.AssignedTo,
    AssignedToName = a.AssignedToName,
    AssignedDept = a.AssignedDept,
    AssignedOn = a.AssignedOn?.ToString("yyyy-MM-dd"),
    Notes = a.Notes,
  };

  private static InventoryBatchDto Map(InventoryBatch b) => Map(b, null);

  private static InventoryBatchDto Map(InventoryBatch b, List<Asset>? assets) => new()
  {
    Id = b.Id,
    InventoryId = b.Id,
    Category = b.Category,
    Department = b.Department,
    Description = b.Description,
    VendorId = b.VendorId,
    VendorName = b.Vendor?.Name ?? "—",
    Quantity = b.Quantity,
    UnitCost = b.UnitCost,
    TotalCost = b.TotalCost,
    PurchaseCost = b.PurchaseCost,
    PurchaseDate = b.PurchaseDate.ToString("yyyy-MM-dd"),
    WarrantyStartDate = b.WarrantyStartDate.ToString("yyyy-MM-dd"),
    WarrantyEndDate = b.WarrantyEndDate?.ToString("yyyy-MM-dd"),
    WarrantyExpiry = b.WarrantyEndDate?.ToString("yyyy-MM-dd"),
    Location = b.Location,
    Condition = b.Condition,
    Status = b.Status,
    InvoiceNo = b.InvoiceNo,
    PoNumber = b.PoNumber,
    Remarks = b.Remarks,
    AttachmentName = b.AttachmentName,
    ReceivedBy = b.ReceivedBy,
    CreatedAt = b.CreatedAt.ToString("yyyy-MM-dd"),
    AssetsGenerated = b.AssetsGenerated,
    Configuration = b.Configuration,
    ConfigSummary = b.ConfigSummary,
    Items = b.Items.Select(i => new InventoryItemDto
    {
      Id = i.Id,
      InventoryId = i.InventoryBatchId,
      Category = i.Category,
      Item = i.Item,
      Brand = i.Brand,
      Configuration = i.Configuration,
      SerialNo = i.SerialNo,
    }).ToList(),
    Assets = (assets ?? b.Assets?.ToList() ?? new List<Asset>()).Select(MapAsset).ToList(),
  };
}
