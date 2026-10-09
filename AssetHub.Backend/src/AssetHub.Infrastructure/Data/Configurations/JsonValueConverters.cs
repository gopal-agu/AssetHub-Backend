using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AssetHub.Infrastructure.Data.Configurations;

/// <summary>Shared converters so List&lt;string&gt; / Dictionary&lt;string,string&gt; properties
/// (Vendor.Categories, Asset.Configuration, InventoryBatch.Configuration) persist as JSON columns
/// instead of needing separate join tables.</summary>
public static class JsonValueConverters
{
    public static readonly ValueConverter<List<string>, string> StringListConverter = new(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
        v => string.IsNullOrWhiteSpace(v)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

    public static readonly ValueComparer<List<string>> StringListComparer = new(
        (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
        v => v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
        v => v.ToList());

    public static readonly ValueConverter<Dictionary<string, string>, string> StringDictConverter = new(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
        v => string.IsNullOrWhiteSpace(v)
            ? new Dictionary<string, string>()
            : JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>());

    public static readonly ValueComparer<Dictionary<string, string>> StringDictComparer = new(
        (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
        v => v.Aggregate(0, (h, kv) => HashCode.Combine(h, kv.Key.GetHashCode(), kv.Value.GetHashCode())),
        v => v.ToDictionary(kv => kv.Key, kv => kv.Value));
}
