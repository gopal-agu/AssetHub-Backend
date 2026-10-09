namespace AssetHub.Core.Entities;

/// <summary>DB-backed counter, one row per key (e.g. "asset_LAP", "grn_2026"), mirroring the
/// frontend's localStorage sequence store in utils/idGenerator.js so generated codes never collide.</summary>
public class Sequence
{
    public string Key { get; set; } = string.Empty;
    public long Value { get; set; }
}
