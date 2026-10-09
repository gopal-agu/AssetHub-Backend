
//using AssetHub.Core.Common;
//using AssetHub.Core.Dtos.Asset;
//using AssetHub.Core.Entities;
//using AssetHub.Core.Interfaces.Repositories;
//using AssetHub.Core.Interfaces.Services;
//using Microsoft.EntityFrameworkCore;

//namespace AssetHub.Infrastructure.Services;

//public class AssetService : IAssetService
//{
//  private readonly IUnitOfWork _uow;
//  private readonly ISequenceService _sequences;
//  private readonly IActivityLogService _activity;

//  public AssetService(IUnitOfWork uow, ISequenceService sequences, IActivityLogService activity)
//  {
//    _uow = uow;
//    _sequences = sequences;
//    _activity = activity;
//  }

//  public async Task<List<AssetDto>> GetAllAsync(string? category = null, string? status = null, string? location = null, string? department = null)
//  {
//    var query = _uow.Assets.Query().Include(a => a.Vendor).Include(a => a.AssignedEmployee).AsQueryable();

//    if (!string.IsNullOrWhiteSpace(category)) query = query.Where(a => a.Category == category);
//    if (!string.IsNullOrWhiteSpace(status)) query = query.Where(a => a.Status == status);
//    if (!string.IsNullOrWhiteSpace(location)) query = query.Where(a => a.Location == location);
//    if (!string.IsNullOrWhiteSpace(department)) query = query.Where(a => a.AssignedDept == department);

//    var assets = await query.AsNoTracking().OrderByDescending(a => a.CreatedAt).ToListAsync();

//    // AUTO-RELEASE FEATURE: If assigned employee is Inactive or Resigned in DB, automatically show asset as Available
//    //foreach (var asset in assets)
//    //{
//    //  if (asset.Status == "Assigned" && asset.AssignedEmployee != null && (asset.AssignedEmployee.Status == "Inactive" || asset.AssignedEmployee.Status == "Resigned"))
//    //  {
//    //    asset.Status = "Available";
//    //    asset.AssignedTo = null;
//    //    asset.AssignedToName = null;
//    //    asset.AssignedDept = null;
//    //    asset.AssignedOn = null;
//    //  }
//    //}

//    return assets.Select(Map).ToList();
//  }

//  public async Task<AssetDto> GetByIdAsync(string id)

//    var asset = await _uow.Assets.GetByIdWithRelationsAsync(id)
//        ?? throw ApiException.NotFound("Asset", id);

//    // AUTO-RELEASE CHECK FOR SINGLE ASSET:
//    //if (asset.Status == "Assigned" && asset.AssignedEmployee != null && (asset.AssignedEmployee.Status == "Inactive" || asset.AssignedEmployee.Status == "Resigned"))
//    //{
//    //  asset.Status = "Available";
//    //  asset.AssignedTo = null;
//    //  asset.AssignedToName = null;
//    //  asset.AssignedDept = null;
//    //  asset.AssignedOn = null;
//    //}

//    return Map(asset);
//  }

//  public async Task<List<EmployeeAssetDto>> GetByEmployeeIdAsync(string employeeId)
//  {
//    if (string.IsNullOrWhiteSpace(employeeId))
//      return new List<EmployeeAssetDto>();

//    var cleanEmpId = employeeId.Trim().ToLower();

//    var query = _uow.Assets.Query()
//        .Include(a => a.Vendor)
//        .Include(a => a.AssignedEmployee)
//        .Where(a => a.AssignedTo != null && a.Status == "Assigned" &&
//                    (a.AssignedTo.ToLower() == cleanEmpId ||
//                     a.AssignedTo.ToLower().Contains(cleanEmpId) ||
//                     (a.AssignedToName != null && a.AssignedToName.ToLower().Contains(cleanEmpId))));

//    var assets = await query.AsNoTracking().OrderByDescending(a => a.CreatedAt).ToListAsync();

//    //var activeAssets = new List<Asset>();
//    //foreach (var asset in assets)
//    //{
//    //  if (asset.AssignedEmployee != null &&
//    //     (asset.AssignedEmployee.Status == "Inactive" || asset.AssignedEmployee.Status == "Resigned"))
//    //  {
//    //    continue;
//    //  }
//    //  activeAssets.Add(asset);
//    //}

//    return activeAssets.Select(a => new EmployeeAssetDto
//    {
//      AssetId = a.Id,
//      AssetName = a.Category,
//      Category = a.Category,
//      SerialNumber = a.SerialNumber,
//      ConfigSummary = a.ConfigSummary,
//      Configuration = a.Configuration,
//      AssignedOn = a.AssignedOn?.ToString("yyyy-MM-dd"),
//      Status = a.Status,
//      Location = a.Location
//    }).ToList();
//  }



//  public async Task<AssetDto> CreateAsync(CreateAssetDto dto)
//  {
//    var tag = await _sequences.GenerateAssetTagAsync(dto.Category);
//    var serial = string.IsNullOrWhiteSpace(dto.SerialNumber) ? await _sequences.GenerateInternalSerialAsync(dto.Category) : dto.SerialNumber;

//    var purchaseDate = DateOnly.TryParse(dto.PurchaseDate, out var pd) ? pd : DateOnly.FromDateTime(DateTime.UtcNow);
//    var warrantyExpiry = dto.WarrantyMonths > 0 ? purchaseDate.AddMonths(dto.WarrantyMonths) : (DateOnly?)null;
//    var configSummary = string.Join(" · ", dto.Configuration.Values.Where(v => !string.IsNullOrWhiteSpace(v)));

//    var asset = new Asset
//    {
//      Id = tag,
//      SerialNumber = serial,
//      Category = dto.Category,
//      Configuration = dto.Configuration,
//      ConfigSummary = configSummary,
//      BatchId = dto.BatchId,
//      VendorId = dto.VendorId,
//      PurchaseDate = purchaseDate,
//      UnitCost = dto.UnitCost,
//      PoNumber = dto.PoNumber,
//      InvoiceNo = dto.InvoiceNo,
//      WarrantyMonths = dto.WarrantyMonths,
//      WarrantyExpiry = warrantyExpiry,
//      Location = dto.Location,
//      Condition = dto.Condition,
//      Status = string.IsNullOrWhiteSpace(dto.Status) ? "Available" : dto.Status,
//    };

//    await _uow.Assets.AddAsync(asset);
//    await _uow.SaveChangesAsync();
//    await _activity.LogAsync($"Asset {asset.Id} added to inventory.");

//    var saved = await _uow.Assets.GetByIdWithRelationsAsync(asset.Id) ?? asset;
//    return Map(saved);
//  }

//  public async Task<AssetDto> UpdateAsync(string id, UpdateAssetDto dto)
//  {
//    var asset = await _uow.Assets.GetByIdAsync(id) ?? throw ApiException.NotFound("Asset", id);

//    if (!string.IsNullOrWhiteSpace(dto.SerialNumber)) asset.SerialNumber = dto.SerialNumber;
//    if (dto.Configuration != null)
//    {
//      asset.Configuration = dto.Configuration;
//      asset.ConfigSummary = string.Join(" · ", dto.Configuration.Values.Where(v => !string.IsNullOrWhiteSpace(v)));
//    }
//    if (!string.IsNullOrWhiteSpace(dto.Location)) asset.Location = dto.Location;
//    if (!string.IsNullOrWhiteSpace(dto.Condition)) asset.Condition = dto.Condition;
//    if (dto.Notes != null) asset.Notes = dto.Notes;
//    if (!string.IsNullOrWhiteSpace(dto.Status)) asset.Status = dto.Status;

//    if (!string.IsNullOrWhiteSpace(dto.AssignedTo)) asset.AssignedTo = dto.AssignedTo;
//    if (!string.IsNullOrWhiteSpace(dto.AssignedToName)) asset.AssignedToName = dto.AssignedToName;
//    if (!string.IsNullOrWhiteSpace(dto.AssignedDept)) asset.AssignedDept = dto.AssignedDept;
//    if (!string.IsNullOrWhiteSpace(dto.AssignedOn) && DateOnly.TryParse(dto.AssignedOn, out var parsedOn))
//    {
//      asset.AssignedOn = parsedOn;
//    }

//    asset.UpdatedAt = DateTime.UtcNow;

//    _uow.Assets.Update(asset);
//    await _uow.SaveChangesAsync();
//    await _activity.LogAsync($"{asset.Id} details corrected.");

//    var saved = await _uow.Assets.GetByIdWithRelationsAsync(asset.Id) ?? asset;
//    return Map(saved);
//  }

//  public async Task<AssetDto> AssignAsync(string assetId, AssignAssetDto dto)
//  {
//    var asset = await _uow.Assets.GetByIdAsync(assetId) ?? throw ApiException.NotFound("Asset", assetId);

//    // 1. Extract real employee details sent from frontend (sourced from 3rd party SDaemon API)
//    string empId = !string.IsNullOrWhiteSpace(dto.AssignedTo) ? dto.AssignedTo : (!string.IsNullOrWhiteSpace(dto.EmployeeId) ? dto.EmployeeId : "UNKNOWN");
//    string empName = !string.IsNullOrWhiteSpace(dto.AssignedToName) ? dto.AssignedToName : (!string.IsNullOrWhiteSpace(dto.EmployeeName) ? dto.EmployeeName : empId);
//    string empDept = !string.IsNullOrWhiteSpace(dto.AssignedDept) ? dto.AssignedDept : "General";

//    // 2. Check if employee exists in dbo.Employees; if not, INSERT new Employee record into dbo.Employees table
//    var employee = await _uow.Employees.GetByIdAsync(empId);
//    if (employee == null)
//    {
//      employee = new Employee
//      {
//        Id = empId,
//        Name = empName,
//        Department = empDept,
//        Email = $"{empId.ToLower()}@company.com",
//        Status = "Active",
//        CreatedAt = DateTime.UtcNow
//      };
//      await _uow.Employees.AddAsync(employee);
//    }
//    else
//    {
//      // Update name, department, and ensure status is Active
//      employee.Name = empName;
//      employee.Department = empDept;
//      employee.Status = "Active";
//      _uow.Employees.Update(employee);
//    }

//    if (AssetStatuses.NonAssignable.Contains(asset.Status))
//    {
//      await _activity.LogAsync($"{assetId} assignment blocked because asset status is {asset.Status}.");
//      throw new ApiException($"Asset '{assetId}' cannot be assigned while status is '{asset.Status}'.", 409);
//    }

//    var assignedOn = DateOnly.FromDateTime(DateTime.UtcNow);
//    if (!string.IsNullOrWhiteSpace(dto.AssignedOn) && DateOnly.TryParse(dto.AssignedOn, out var parsedDate))
//    {
//      assignedOn = parsedDate;
//    }

//    // 3. Update asset record
//    asset.Status = AssetStatuses.Assigned;
//    asset.AssignedTo = employee.Id;
//    asset.AssignedToName = employee.Name;
//    asset.AssignedDept = employee.Department;
//    asset.AssignedOn = assignedOn;
//    _uow.Assets.Update(asset);

//    // 4. Create assignment history record
//    var historyCount = await _uow.AssignmentHistories.CountAsync(h => h.AssetId == assetId);
//    await _uow.AssignmentHistories.AddAsync(new AssignmentHistory
//    {
//      Id = $"{assetId}-A{historyCount + 1}",
//      AssetId = assetId,
//      EmployeeId = employee.Id,
//      EmployeeName = employee.Name,
//      Department = employee.Department,
//      Action = AssignmentActions.Assigned,
//      Date = assignedOn,
//      Remarks = string.IsNullOrWhiteSpace(dto.Remarks) ? "Assigned via Assets module" : dto.Remarks,
//      PerformedBy = string.IsNullOrWhiteSpace(dto.PerformedBy) ? "IT Asset Desk" : dto.PerformedBy,
//    });

//    // 5. Atomic transaction saves BOTH employee into dbo.Employees AND asset into dbo.Assets!
//    await _uow.SaveChangesAsync();
//    await _activity.LogAsync($"{assetId} assigned to {employee.Name} ({employee.Department}).");

//    var saved = await _uow.Assets.GetByIdWithRelationsAsync(assetId) ?? asset;
//    return Map(saved);
//  }

//  public async Task<AssetDto> UnassignAsync(string assetId, UnassignAssetDto dto)
//  {
//    var asset = await _uow.Assets.GetByIdAsync(assetId) ?? throw ApiException.NotFound("Asset", assetId);
//    var date = DateOnly.FromDateTime(DateTime.UtcNow);
//    var previousEmployeeName = asset.AssignedToName;

//    var historyCount = await _uow.AssignmentHistories.CountAsync(h => h.AssetId == assetId);
//    await _uow.AssignmentHistories.AddAsync(new AssignmentHistory
//    {
//      Id = $"{assetId}-R{historyCount + 1}",
//      AssetId = assetId,
//      EmployeeId = asset.AssignedTo,
//      EmployeeName = asset.AssignedToName,
//      Department = asset.AssignedDept,
//      Action = AssignmentActions.Returned,
//      Date = date,
//      Remarks = string.IsNullOrWhiteSpace(dto.Remarks) ? "Returned to inventory" : dto.Remarks,
//      PerformedBy = string.IsNullOrWhiteSpace(dto.PerformedBy) ? "IT Asset Desk" : dto.PerformedBy,
//    });

//    asset.Status = AssetStatuses.Available;
//    asset.AssignedTo = null;
//    asset.AssignedToName = null;
//    asset.AssignedDept = null;
//    asset.AssignedOn = null;
//    _uow.Assets.Update(asset);

//    await _uow.SaveChangesAsync();
//    await _activity.LogAsync($"{assetId} returned to stock from {previousEmployeeName ?? "employee"}.");

//    var saved = await _uow.Assets.GetByIdWithRelationsAsync(assetId) ?? asset;
//    return Map(saved);
//  }

//  public async Task<AssetDto> TransferAsync(string assetId, TransferAssetDto dto)
//  {
//    var asset = await _uow.Assets.GetByIdAsync(assetId) ?? throw ApiException.NotFound("Asset", assetId);

//    // 1. Extract real employee details sent from frontend (sourced from 3rd party SDaemon API)
//    string targetEmpId = !string.IsNullOrWhiteSpace(dto.AssignedTo) ? dto.AssignedTo : (!string.IsNullOrWhiteSpace(dto.NewEmployeeId) ? dto.NewEmployeeId : "UNKNOWN");
//    string targetEmpName = !string.IsNullOrWhiteSpace(dto.AssignedToName) ? dto.AssignedToName : (!string.IsNullOrWhiteSpace(dto.NewEmployeeName) ? dto.NewEmployeeName : targetEmpId);
//    string targetEmpDept = !string.IsNullOrWhiteSpace(dto.AssignedDept) ? dto.AssignedDept : "General";

//    // 2. Check if new employee exists in dbo.Employees; if not, INSERT new Employee record into dbo.Employees table
//    var newEmployee = await _uow.Employees.GetByIdAsync(targetEmpId);
//    if (newEmployee == null)
//    {
//      newEmployee = new Employee
//      {
//        Id = targetEmpId,
//        Name = targetEmpName,
//        Department = targetEmpDept,
//        Email = $"{targetEmpId.ToLower()}@company.com",
//        Status = "Active",
//        CreatedAt = DateTime.UtcNow
//      };
//      await _uow.Employees.AddAsync(newEmployee);
//    }
//    else
//    {
//      newEmployee.Name = targetEmpName;
//      newEmployee.Department = targetEmpDept;
//      newEmployee.Status = "Active";
//      _uow.Employees.Update(newEmployee);
//    }

//    var date = DateOnly.FromDateTime(DateTime.UtcNow);
//    if (!string.IsNullOrWhiteSpace(dto.AssignedOn) && DateOnly.TryParse(dto.AssignedOn, out var parsedDate))
//    {
//      date = parsedDate;
//    }

//    var fromName = asset.AssignedToName;
//    var fromDept = asset.AssignedDept;

//    // 3. Update asset record
//    asset.Status = AssetStatuses.Assigned;
//    asset.AssignedTo = newEmployee.Id;
//    asset.AssignedToName = newEmployee.Name;
//    asset.AssignedDept = newEmployee.Department;
//    asset.AssignedOn = date;
//    _uow.Assets.Update(asset);

//    // 4. Create assignment history record
//    var historyCount = await _uow.AssignmentHistories.CountAsync(h => h.AssetId == assetId);
//    await _uow.AssignmentHistories.AddAsync(new AssignmentHistory
//    {
//      Id = $"{assetId}-T{historyCount + 1}",
//      AssetId = assetId,
//      EmployeeId = newEmployee.Id,
//      EmployeeName = newEmployee.Name,
//      Department = newEmployee.Department,
//      Action = AssignmentActions.Transferred,
//      Date = date,
//      Remarks = string.IsNullOrWhiteSpace(dto.Remarks) ? $"Transferred from {fromName ?? "previous holder"} ({fromDept ?? "—"})" : dto.Remarks,
//      PerformedBy = string.IsNullOrWhiteSpace(dto.PerformedBy) ? "IT Asset Desk" : dto.PerformedBy,
//    });

//    // 5. Atomic transaction saves BOTH employee into dbo.Employees AND asset into dbo.Assets!
//    await _uow.SaveChangesAsync();
//    await _activity.LogAsync($"{assetId} transferred from {fromName ?? "unassigned"} to {newEmployee.Name}.");

//    var saved = await _uow.Assets.GetByIdWithRelationsAsync(assetId) ?? asset;
//    return Map(saved);
//  }

//  public async Task<AssetDto> ChangeStatusAsync(string assetId, ChangeAssetStatusDto dto)
//  {
//    var asset = await _uow.Assets.GetByIdAsync(assetId) ?? throw ApiException.NotFound("Asset", assetId);
//    if (!AssetStatuses.All.Contains(dto.Status))
//      throw new ApiException($"'{dto.Status}' is not a valid asset status.", 400);

//    var date = DateOnly.FromDateTime(DateTime.UtcNow);

//    if (asset.Status == AssetStatuses.Assigned && asset.AssignedTo != null)
//    {
//      var historyCount = await _uow.AssignmentHistories.CountAsync(h => h.AssetId == assetId);
//      await _uow.AssignmentHistories.AddAsync(new AssignmentHistory
//      {
//        Id = $"{assetId}-R{historyCount + 1}",
//        AssetId = assetId,
//        EmployeeId = asset.AssignedTo,
//        EmployeeName = asset.AssignedToName,
//        Department = asset.AssignedDept,
//        Action = AssignmentActions.Returned,
//        Date = date,
//        Remarks = $"Auto-returned — status changed to {dto.Status}.",
//        PerformedBy = string.IsNullOrWhiteSpace(dto.PerformedBy) ? "IT Asset Desk" : dto.PerformedBy,
//      });
//    }

//    asset.Status = dto.Status;
//    asset.AssignedTo = null;
//    asset.AssignedToName = null;
//    asset.AssignedDept = null;
//    asset.AssignedOn = null;
//    if (!string.IsNullOrWhiteSpace(dto.Remarks)) asset.Notes = dto.Remarks;
//    _uow.Assets.Update(asset);

//    await _uow.SaveChangesAsync();
//    var by = string.IsNullOrWhiteSpace(dto.PerformedBy) ? "" : $" by {dto.PerformedBy}";
//    await _activity.LogAsync($"{assetId} status changed to {dto.Status}{by}.");

//    var saved = await _uow.Assets.GetByIdWithRelationsAsync(assetId) ?? asset;
//    return Map(saved);
//  }

//  public async Task<AssetDto> RetireAsync(string assetId, RetireAssetDto dto)
//  {
//    var asset = await _uow.Assets.GetByIdAsync(assetId) ?? throw ApiException.NotFound("Asset", assetId);

//    asset.Status = AssetStatuses.Retired;
//    asset.AssignedTo = null;
//    asset.AssignedToName = null;
//    asset.AssignedDept = null;
//    asset.Notes = string.IsNullOrWhiteSpace(dto.Reason) ? "Retired" : dto.Reason;
//    _uow.Assets.Update(asset);

//    await _uow.SaveChangesAsync();
//    await _activity.LogAsync($"{assetId} marked as Retired.");

//    var saved = await _uow.Assets.GetByIdWithRelationsAsync(asset.Id) ?? asset;
//    return Map(saved);
//  }

//  public async Task<List<AssignmentHistoryDto>> GetAssignmentHistoryAsync(string? assetId = null)
//  {
//    var query = _uow.AssignmentHistories.Query().AsQueryable();
//    if (!string.IsNullOrWhiteSpace(assetId)) query = query.Where(h => h.AssetId == assetId);

//    var history = await query.AsNoTracking().OrderByDescending(h => h.Date).ToListAsync();
//    return history.Select(h => new AssignmentHistoryDto
//    {
//      Id = h.Id,
//      AssetId = h.AssetId,
//      EmployeeId = h.EmployeeId,
//      EmployeeName = h.EmployeeName,
//      Department = h.Department,
//      Action = h.Action,
//      Date = h.Date.ToString("yyyy-MM-dd"),
//      Remarks = h.Remarks,
//      PerformedBy = h.PerformedBy,
//    }).ToList();
//  }

//  private static AssetDto Map(Asset a) => new()
//  {
//    Id = a.Id,
//    SerialNumber = a.SerialNumber,
//    Category = a.Category,
//    Configuration = a.Configuration,
//    ConfigSummary = a.ConfigSummary,
//    BatchId = a.BatchId,
//    VendorId = a.VendorId,
//    VendorName = a.Vendor?.Name ?? "—",
//    PurchaseDate = a.PurchaseDate.ToString("yyyy-MM-dd"),
//    UnitCost = a.UnitCost,
//    PoNumber = a.PoNumber,
//    InvoiceNo = a.InvoiceNo,
//    WarrantyMonths = a.WarrantyMonths,
//    WarrantyExpiry = a.WarrantyExpiry?.ToString("yyyy-MM-dd"),
//    Location = a.Location,
//    Condition = a.Condition,
//    Status = a.Status,
//    AssignedTo = a.AssignedTo,
//    AssignedToName = a.AssignedToName,
//    AssignedDept = a.AssignedDept,
//    AssignedOn = a.AssignedOn?.ToString("yyyy-MM-dd"),
//    Notes = a.Notes,
//  };
//}
using AssetHub.Core.Common;
using AssetHub.Core.Dtos.Asset;
using AssetHub.Core.Entities;
using AssetHub.Core.Interfaces.Repositories;
using AssetHub.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace AssetHub.Infrastructure.Services;

public class AssetService : IAssetService
{
  private readonly IUnitOfWork _uow;
  private readonly ISequenceService _sequences;
  private readonly IActivityLogService _activity;

  public AssetService(IUnitOfWork uow, ISequenceService sequences, IActivityLogService activity)
  {
    _uow = uow;
    _sequences = sequences;
    _activity = activity;
  }

  public async Task<List<AssetDto>> GetAllAsync(string? category = null, string? status = null, string? location = null, string? department = null)
  {
    var query = _uow.Assets.Query().Include(a => a.Vendor).Include(a => a.AssignedEmployee).AsQueryable();

    if (!string.IsNullOrWhiteSpace(category)) query = query.Where(a => a.Category == category);
    if (!string.IsNullOrWhiteSpace(status)) query = query.Where(a => a.Status == status);
    if (!string.IsNullOrWhiteSpace(location)) query = query.Where(a => a.Location == location);
    if (!string.IsNullOrWhiteSpace(department)) query = query.Where(a => a.AssignedDept == department);

    var assets = await query.AsNoTracking().OrderByDescending(a => a.CreatedAt).ToListAsync();

    // AUTO-RELEASE FEATURE: Disabled (Assets only show available when explicitly returned)
    /*
    foreach (var asset in assets)
    {
      if (asset.Status == "Assigned" && asset.AssignedEmployee != null && (asset.AssignedEmployee.Status == "Inactive" || asset.AssignedEmployee.Status == "Resigned"))
      {
        asset.Status = "Available";
        asset.AssignedTo = null;
        asset.AssignedToName = null;
        asset.AssignedDept = null;
        asset.AssignedOn = null;
      }
    }
    */

    return assets.Select(Map).ToList();
  }

  public async Task<AssetDto> GetByIdAsync(string id)
  {
    var asset = await _uow.Assets.GetByIdWithRelationsAsync(id)
        ?? throw ApiException.NotFound("Asset", id);

    // AUTO-RELEASE CHECK FOR SINGLE ASSET: Disabled (Assets only show available when explicitly returned)
    /*
    if (asset.Status == "Assigned" && asset.AssignedEmployee != null && (asset.AssignedEmployee.Status == "Inactive" || asset.AssignedEmployee.Status == "Resigned"))
    {
      asset.Status = "Available";
      asset.AssignedTo = null;
      asset.AssignedToName = null;
      asset.AssignedDept = null;
      asset.AssignedOn = null;
    }
    */

    return Map(asset);
  }

  public async Task<List<EmployeeAssetDto>> GetByEmployeeIdAsync(string employeeId)
  {
    if (string.IsNullOrWhiteSpace(employeeId))
      return new List<EmployeeAssetDto>();

    var cleanEmpId = employeeId.Trim().ToLower();

    // OLD LOGIC (COMMENTED OUT):
    /*
    var query = _uow.Assets.Query()
        .Include(a => a.Vendor)
        .Include(a => a.AssignedEmployee)
        .Where(a => a.AssignedTo != null && a.Status == "Assigned" &&
                    (a.AssignedTo.ToLower() == cleanEmpId ||
                     a.AssignedTo.ToLower().Contains(cleanEmpId) ||
                     (a.AssignedToName != null && a.AssignedToName.ToLower().Contains(cleanEmpId))));

    var assets = await query.AsNoTracking().OrderByDescending(a => a.CreatedAt).ToListAsync();
    */

    // NEW LOGIC: Verify Employee existence in Employees master list first, then perform exact match
    var empExists = await _uow.Employees.Query()
        .AsNoTracking()
        .AnyAsync(e => e.Id.ToLower() == cleanEmpId);

    if (!empExists)
    {
        return new List<EmployeeAssetDto>();
    }

    var query = _uow.Assets.Query()
        .Include(a => a.Vendor)
        .Include(a => a.AssignedEmployee)
        .Where(a => a.AssignedTo != null && 
                    a.Status == "Assigned" &&
                    a.AssignedTo.ToLower() == cleanEmpId);

    var assets = await query.AsNoTracking().OrderByDescending(a => a.CreatedAt).ToListAsync();

    // AUTO-RELEASE CHECK: Disabled (Include all assigned assets regardless of employee status)
    /*
    var activeAssets = new List<Asset>();
    foreach (var asset in assets)
    {
      if (asset.AssignedEmployee != null &&
         (asset.AssignedEmployee.Status == "Inactive" || asset.AssignedEmployee.Status == "Resigned"))
      {
        continue;
      }
      activeAssets.Add(asset);
    }
    */

    return assets.Select(a => new EmployeeAssetDto
    {
      AssetId = a.Id,
      AssetName = a.Category,
      Category = a.Category,
      SerialNumber = a.SerialNumber,
      ConfigSummary = a.ConfigSummary,
      Configuration = a.Configuration,
      AssignedOn = a.AssignedOn?.ToString("yyyy-MM-dd"),
      Status = a.Status,
      Location = a.Location
    }).ToList();
  }

  public async Task<AssetDto> CreateAsync(CreateAssetDto dto)
  {
    var tag = await _sequences.GenerateAssetTagAsync(dto.Category);
    var serial = string.IsNullOrWhiteSpace(dto.SerialNumber) ? await _sequences.GenerateInternalSerialAsync(dto.Category) : dto.SerialNumber;

    var purchaseDate = DateOnly.TryParse(dto.PurchaseDate, out var pd) ? pd : DateOnly.FromDateTime(DateTime.UtcNow);
    var warrantyExpiry = dto.WarrantyMonths > 0 ? purchaseDate.AddMonths(dto.WarrantyMonths) : (DateOnly?)null;
    var configSummary = string.Join(" · ", dto.Configuration.Values.Where(v => !string.IsNullOrWhiteSpace(v)));

    var asset = new Asset
    {
      Id = tag,
      SerialNumber = serial,
      Category = dto.Category,
      Configuration = dto.Configuration,
      ConfigSummary = configSummary,
      BatchId = dto.BatchId,
      VendorId = dto.VendorId,
      PurchaseDate = purchaseDate,
      UnitCost = dto.UnitCost,
      PoNumber = dto.PoNumber,
      InvoiceNo = dto.InvoiceNo,
      WarrantyMonths = dto.WarrantyMonths,
      WarrantyExpiry = warrantyExpiry,
      Location = dto.Location,
      Condition = dto.Condition,
      Status = string.IsNullOrWhiteSpace(dto.Status) ? "Available" : dto.Status,
    };

    await _uow.Assets.AddAsync(asset);
    await _uow.SaveChangesAsync();
    await _activity.LogAsync($"Asset {asset.Id} added to inventory.");

    var saved = await _uow.Assets.GetByIdWithRelationsAsync(asset.Id) ?? asset;
    return Map(saved);
  }

  public async Task<AssetDto> UpdateAsync(string id, UpdateAssetDto dto)
  {
    var asset = await _uow.Assets.GetByIdAsync(id) ?? throw ApiException.NotFound("Asset", id);

    if (!string.IsNullOrWhiteSpace(dto.SerialNumber)) asset.SerialNumber = dto.SerialNumber;
    if (dto.Configuration != null)
    {
      asset.Configuration = dto.Configuration;
      asset.ConfigSummary = string.Join(" · ", dto.Configuration.Values.Where(v => !string.IsNullOrWhiteSpace(v)));
    }
    if (!string.IsNullOrWhiteSpace(dto.Location)) asset.Location = dto.Location;
    if (!string.IsNullOrWhiteSpace(dto.Condition)) asset.Condition = dto.Condition;
    if (dto.Notes != null) asset.Notes = dto.Notes;
    if (!string.IsNullOrWhiteSpace(dto.Status)) asset.Status = dto.Status;

    if (!string.IsNullOrWhiteSpace(dto.AssignedTo)) asset.AssignedTo = dto.AssignedTo;
    if (!string.IsNullOrWhiteSpace(dto.AssignedToName)) asset.AssignedToName = dto.AssignedToName;
    if (!string.IsNullOrWhiteSpace(dto.AssignedDept)) asset.AssignedDept = dto.AssignedDept;
    if (!string.IsNullOrWhiteSpace(dto.AssignedOn) && DateOnly.TryParse(dto.AssignedOn, out var parsedOn))
    {
      asset.AssignedOn = parsedOn;
    }

    asset.UpdatedAt = DateTime.UtcNow;

    _uow.Assets.Update(asset);
    await _uow.SaveChangesAsync();
    await _activity.LogAsync($"{asset.Id} details corrected.");

    var saved = await _uow.Assets.GetByIdWithRelationsAsync(asset.Id) ?? asset;
    return Map(saved);
  }

  public async Task<AssetDto> AssignAsync(string assetId, AssignAssetDto dto)
  {
    var asset = await _uow.Assets.GetByIdAsync(assetId) ?? throw ApiException.NotFound("Asset", assetId);

    // 1. Extract real employee details sent from frontend (sourced from 3rd party SDaemon API)
    string empId = !string.IsNullOrWhiteSpace(dto.AssignedTo) ? dto.AssignedTo : (!string.IsNullOrWhiteSpace(dto.EmployeeId) ? dto.EmployeeId : "UNKNOWN");
    string empName = !string.IsNullOrWhiteSpace(dto.AssignedToName) ? dto.AssignedToName : (!string.IsNullOrWhiteSpace(dto.EmployeeName) ? dto.EmployeeName : empId);
    string empDept = !string.IsNullOrWhiteSpace(dto.AssignedDept) ? dto.AssignedDept : "General";

    // 2. Check if employee exists in dbo.Employees; if not, INSERT new Employee record into dbo.Employees table
    var employee = await _uow.Employees.GetByIdAsync(empId);
    if (employee == null)
    {
      employee = new Employee
      {
        Id = empId,
        Name = empName,
        Department = empDept,
        Email = $"{empId.ToLower()}@company.com",
        Status = "Active",
        CreatedAt = DateTime.UtcNow
      };
      await _uow.Employees.AddAsync(employee);
    }
    else
    {
      // Update name, department, and ensure status is Active
      employee.Name = empName;
      employee.Department = empDept;
      employee.Status = "Active";
      _uow.Employees.Update(employee);
    }

    if (AssetStatuses.NonAssignable.Contains(asset.Status))
    {
      await _activity.LogAsync($"{assetId} assignment blocked because asset status is {asset.Status}.");
      throw new ApiException($"Asset '{assetId}' cannot be assigned while status is '{asset.Status}'.", 409);
    }

    var assignedOn = DateOnly.FromDateTime(DateTime.UtcNow);
    if (!string.IsNullOrWhiteSpace(dto.AssignedOn) && DateOnly.TryParse(dto.AssignedOn, out var parsedDate))
    {
      assignedOn = parsedDate;
    }

    // 3. Update asset record
    asset.Status = AssetStatuses.Assigned;
    asset.AssignedTo = employee.Id;
    asset.AssignedToName = employee.Name;
    asset.AssignedDept = employee.Department;
    asset.AssignedOn = assignedOn;
    _uow.Assets.Update(asset);

    // 4. Create assignment history record
    var historyCount = await _uow.AssignmentHistories.CountAsync(h => h.AssetId == assetId);
    await _uow.AssignmentHistories.AddAsync(new AssignmentHistory
    {
      Id = $"{assetId}-A{historyCount + 1}",
      AssetId = assetId,
      EmployeeId = employee.Id,
      EmployeeName = employee.Name,
      Department = employee.Department,
      Action = AssignmentActions.Assigned,
      Date = assignedOn,
      Remarks = string.IsNullOrWhiteSpace(dto.Remarks) ? "Assigned via Assets module" : dto.Remarks,
      PerformedBy = string.IsNullOrWhiteSpace(dto.PerformedBy) ? "IT Asset Desk" : dto.PerformedBy,
    });

    // 5. Atomic transaction saves BOTH employee into dbo.Employees AND asset into dbo.Assets!
    await _uow.SaveChangesAsync();
    await _activity.LogAsync($"{assetId} assigned to {employee.Name} ({employee.Department}).");

    var saved = await _uow.Assets.GetByIdWithRelationsAsync(assetId) ?? asset;
    return Map(saved);
  }

  public async Task<AssetDto> UnassignAsync(string assetId, UnassignAssetDto dto)
  {
    var asset = await _uow.Assets.GetByIdAsync(assetId) ?? throw ApiException.NotFound("Asset", assetId);
    var date = DateOnly.FromDateTime(DateTime.UtcNow);
    var previousEmployeeName = asset.AssignedToName;

    var historyCount = await _uow.AssignmentHistories.CountAsync(h => h.AssetId == assetId);
    await _uow.AssignmentHistories.AddAsync(new AssignmentHistory
    {
      Id = $"{assetId}-R{historyCount + 1}",
      AssetId = assetId,
      EmployeeId = asset.AssignedTo,
      EmployeeName = asset.AssignedToName,
      Department = asset.AssignedDept,
      Action = AssignmentActions.Returned,
      Date = date,
      Remarks = string.IsNullOrWhiteSpace(dto.Remarks) ? "Returned to inventory" : dto.Remarks,
      PerformedBy = string.IsNullOrWhiteSpace(dto.PerformedBy) ? "IT Asset Desk" : dto.PerformedBy,
    });

    asset.Status = AssetStatuses.Available;
    asset.AssignedTo = null;
    asset.AssignedToName = null;
    asset.AssignedDept = null;
    asset.AssignedOn = null;
    _uow.Assets.Update(asset);

    await _uow.SaveChangesAsync();
    await _activity.LogAsync($"{assetId} returned to stock from {previousEmployeeName ?? "employee"}.");

    var saved = await _uow.Assets.GetByIdWithRelationsAsync(assetId) ?? asset;
    return Map(saved);
  }

  public async Task<AssetDto> TransferAsync(string assetId, TransferAssetDto dto)
  {
    var asset = await _uow.Assets.GetByIdAsync(assetId) ?? throw ApiException.NotFound("Asset", assetId);

    // 1. Extract real employee details sent from frontend (sourced from 3rd party SDaemon API)
    string targetEmpId = !string.IsNullOrWhiteSpace(dto.AssignedTo) ? dto.AssignedTo : (!string.IsNullOrWhiteSpace(dto.NewEmployeeId) ? dto.NewEmployeeId : "UNKNOWN");
    string targetEmpName = !string.IsNullOrWhiteSpace(dto.AssignedToName) ? dto.AssignedToName : (!string.IsNullOrWhiteSpace(dto.NewEmployeeName) ? dto.NewEmployeeName : targetEmpId);
    string targetEmpDept = !string.IsNullOrWhiteSpace(dto.AssignedDept) ? dto.AssignedDept : "General";

    // 2. Check if new employee exists in dbo.Employees; if not, INSERT new Employee record into dbo.Employees table
    var newEmployee = await _uow.Employees.GetByIdAsync(targetEmpId);
    if (newEmployee == null)
    {
      newEmployee = new Employee
      {
        Id = targetEmpId,
        Name = targetEmpName,
        Department = targetEmpDept,
        Email = $"{targetEmpId.ToLower()}@company.com",
        Status = "Active",
        CreatedAt = DateTime.UtcNow
      };
      await _uow.Employees.AddAsync(newEmployee);
    }
    else
    {
      newEmployee.Name = targetEmpName;
      newEmployee.Department = targetEmpDept;
      newEmployee.Status = "Active";
      _uow.Employees.Update(newEmployee);
    }

    var date = DateOnly.FromDateTime(DateTime.UtcNow);
    if (!string.IsNullOrWhiteSpace(dto.AssignedOn) && DateOnly.TryParse(dto.AssignedOn, out var parsedDate))
    {
      date = parsedDate;
    }

    var fromName = asset.AssignedToName;
    var fromDept = asset.AssignedDept;

    // 3. Update asset record
    asset.Status = AssetStatuses.Assigned;
    asset.AssignedTo = newEmployee.Id;
    asset.AssignedToName = newEmployee.Name;
    asset.AssignedDept = newEmployee.Department;
    asset.AssignedOn = date;
    _uow.Assets.Update(asset);

    // 4. Create assignment history record
    var historyCount = await _uow.AssignmentHistories.CountAsync(h => h.AssetId == assetId);
    await _uow.AssignmentHistories.AddAsync(new AssignmentHistory
    {
      Id = $"{assetId}-T{historyCount + 1}",
      AssetId = assetId,
      EmployeeId = newEmployee.Id,
      EmployeeName = newEmployee.Name,
      Department = newEmployee.Department,
      Action = AssignmentActions.Transferred,
      Date = date,
      Remarks = string.IsNullOrWhiteSpace(dto.Remarks) ? $"Transferred from {fromName ?? "previous holder"} ({fromDept ?? "—"})" : dto.Remarks,
      PerformedBy = string.IsNullOrWhiteSpace(dto.PerformedBy) ? "IT Asset Desk" : dto.PerformedBy,
    });

    // 5. Atomic transaction saves BOTH employee into dbo.Employees AND asset into dbo.Assets!
    await _uow.SaveChangesAsync();
    await _activity.LogAsync($"{assetId} transferred from {fromName ?? "unassigned"} to {newEmployee.Name}.");

    var saved = await _uow.Assets.GetByIdWithRelationsAsync(assetId) ?? asset;
    return Map(saved);
  }

  public async Task<AssetDto> ChangeStatusAsync(string assetId, ChangeAssetStatusDto dto)
  {
    var asset = await _uow.Assets.GetByIdAsync(assetId) ?? throw ApiException.NotFound("Asset", assetId);
    if (!AssetStatuses.All.Contains(dto.Status))
      throw new ApiException($"'{dto.Status}' is not a valid asset status.", 400);

    var date = DateOnly.FromDateTime(DateTime.UtcNow);

    if (asset.Status == AssetStatuses.Assigned && asset.AssignedTo != null)
    {
      var historyCount = await _uow.AssignmentHistories.CountAsync(h => h.AssetId == assetId);
      await _uow.AssignmentHistories.AddAsync(new AssignmentHistory
      {
        Id = $"{assetId}-R{historyCount + 1}",
        AssetId = assetId,
        EmployeeId = asset.AssignedTo,
        EmployeeName = asset.AssignedToName,
        Department = asset.AssignedDept,
        Action = AssignmentActions.Returned,
        Date = date,
        Remarks = $"Auto-returned — status changed to {dto.Status}.",
        PerformedBy = string.IsNullOrWhiteSpace(dto.PerformedBy) ? "IT Asset Desk" : dto.PerformedBy,
      });
    }

    asset.Status = dto.Status;
    asset.AssignedTo = null;
    asset.AssignedToName = null;
    asset.AssignedDept = null;
    asset.AssignedOn = null;
    if (!string.IsNullOrWhiteSpace(dto.Remarks)) asset.Notes = dto.Remarks;
    _uow.Assets.Update(asset);

    await _uow.SaveChangesAsync();
    var by = string.IsNullOrWhiteSpace(dto.PerformedBy) ? "" : $" by {dto.PerformedBy}";
    await _activity.LogAsync($"{assetId} status changed to {dto.Status}{by}.");

    var saved = await _uow.Assets.GetByIdWithRelationsAsync(assetId) ?? asset;
    return Map(saved);
  }

  public async Task<AssetDto> RetireAsync(string assetId, RetireAssetDto dto)
  {
    var asset = await _uow.Assets.GetByIdAsync(assetId) ?? throw ApiException.NotFound("Asset", assetId);

    asset.Status = AssetStatuses.Retired;
    asset.AssignedTo = null;
    asset.AssignedToName = null;
    asset.AssignedDept = null;
    asset.Notes = string.IsNullOrWhiteSpace(dto.Reason) ? "Retired" : dto.Reason;
    _uow.Assets.Update(asset);

    await _uow.SaveChangesAsync();
    await _activity.LogAsync($"{assetId} marked as Retired.");

    var saved = await _uow.Assets.GetByIdWithRelationsAsync(asset.Id) ?? asset;
    return Map(saved);
  }

  public async Task<List<AssignmentHistoryDto>> GetAssignmentHistoryAsync(string? assetId = null)
  {
    var query = _uow.AssignmentHistories.Query().AsQueryable();
    if (!string.IsNullOrWhiteSpace(assetId)) query = query.Where(h => h.AssetId == assetId);

    var history = await query.AsNoTracking().OrderByDescending(h => h.Date).ToListAsync();
    return history.Select(h => new AssignmentHistoryDto
    {
      Id = h.Id,
      AssetId = h.AssetId,
      EmployeeId = h.EmployeeId,
      EmployeeName = h.EmployeeName,
      Department = h.Department,
      Action = h.Action,
      Date = h.Date.ToString("yyyy-MM-dd"),
      Remarks = h.Remarks,
      PerformedBy = h.PerformedBy,
    }).ToList();
  }

  private static AssetDto Map(Asset a) => new()
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
}
