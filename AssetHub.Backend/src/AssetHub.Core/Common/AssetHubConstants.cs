namespace AssetHub.Core.Common;

/// <summary>Allowed values that mirror the frontend's fixed option lists (mockData.js / categoryConfig.js).</summary>
public static class AssetStatuses
{
    public const string Available = "Available";
    public const string Assigned = "Assigned";
    public const string UnderRepair = "Under Repair";
    public const string Lost = "Lost";
    public const string Damaged = "Damaged";
    public const string Retired = "Retired";
    public const string Disposed = "Disposed";

    public static readonly string[] All =
    {
        Available, Assigned, UnderRepair, Lost, Damaged, Retired, Disposed
    };

    public static readonly string[] NonAssignable = { Damaged, Disposed, UnderRepair, Retired };
}

public static class MaintenanceStatuses
{
    public const string Open = "Open";
    public const string InProgress = "In Progress";
    public const string AwaitingVendorParts = "Awaiting Vendor Parts";
    public const string Closed = "Closed";
}

public static class AssignmentActions
{
    public const string Assigned = "Assigned";
    public const string Returned = "Returned";
    public const string Transferred = "Transferred";
}

public static class RecordStatuses
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";
}

public static class CategoryPrefixes
{
    public static readonly Dictionary<string, string> Map = new()
    {
        ["Laptop"] = "LAP",
        ["Desktop"] = "DSK",
        ["Monitor"] = "MON",
        ["Server"] = "SRV",
        ["Networking"] = "NET",
        ["Mobile"] = "MOB",
        ["Printer"] = "PRN",
        ["Peripheral"] = "PER",
        ["Software"] = "SFT",
        ["Other"] = "AST",
    };

    public static string Resolve(string? category, string? explicitPrefix)
    {
        if (!string.IsNullOrWhiteSpace(explicitPrefix)) return explicitPrefix.ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(category) && Map.TryGetValue(category, out var p)) return p;
        var letters = new string((category ?? "AST").Where(char.IsLetter).ToArray());
        return string.IsNullOrEmpty(letters) ? "AST" : letters.ToUpperInvariant()[..Math.Min(3, letters.Length)];
    }
}
