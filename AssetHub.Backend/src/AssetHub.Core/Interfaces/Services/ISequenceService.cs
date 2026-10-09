namespace AssetHub.Core.Interfaces.Services;

/// <summary>
/// DB-backed equivalent of the frontend's utils/idGenerator.js. Produces the exact same code
/// formats (AST-LAP-00047, GRN-2026-000012, PO-2026-0341, EMP-1042, VEN-118, MT-2026-00231, CAT-101)
/// from an atomic, per-key counter so codes stay unique even under concurrent requests.
/// </summary>
public interface ISequenceService
{
    Task<string> GenerateAssetTagAsync(string category, string? explicitPrefix = null);
    Task<string> GenerateInternalSerialAsync(string category, string? explicitPrefix = null);
    Task<string> GenerateCategoryIdAsync();
    Task<string> GenerateBatchIdAsync();
    Task<string> GeneratePoNumberAsync();
    Task<string> GenerateEmployeeIdAsync();
    Task<string> GenerateVendorIdAsync();
    Task<string> GenerateMaintenanceIdAsync();
    Task SeedSequenceFloorAsync(string key, long floor);
}
