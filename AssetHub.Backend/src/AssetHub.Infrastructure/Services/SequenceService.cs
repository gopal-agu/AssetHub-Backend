////using AssetHub.Core.Common;
////using AssetHub.Core.Entities;
////using AssetHub.Core.Interfaces.Services;
////using AssetHub.Infrastructure.Data;
////using Microsoft.EntityFrameworkCore;

////namespace AssetHub.Infrastructure.Services;

/////// <summary>
/////// Ports utils/idGenerator.js to a DB-backed counter table (Sequences: Key/Value) instead of
/////// localStorage, so ids stay unique and monotonic across every server instance / restart.
/////// </summary>
////public class SequenceService : ISequenceService
////{
////    private readonly AppDbContext _context;

////    public SequenceService(AppDbContext context)
////    {
////        _context = context;
////    }

////    private async Task<long> NextAsync(string key, long start = 1)
////    {
////        // Simple, portable read-increment-save. For very high write concurrency this could be
////        // swapped for a provider-specific atomic UPDATE ... SET Value = Value + 1 OUTPUT.
////        var seq = await _context.Sequences.FirstOrDefaultAsync(s => s.Key == key);
////        if (seq == null)
////        {
////            seq = new Sequence { Key = key, Value = start };
////            _context.Sequences.Add(seq);
////        }
////        else
////        {
////            seq.Value += 1;
////        }
////        await _context.SaveChangesAsync();
////        return seq.Value;
////    }

////    public async Task<string> GenerateAssetTagAsync(string category, string? explicitPrefix = null)
////    {
////        var prefix = CategoryPrefixes.Resolve(category, explicitPrefix);
////        var n = await NextAsync($"asset_{prefix}");
////        return $"{prefix}-{n:D5}";
////    }

////    public async Task<string> GenerateInternalSerialAsync(string category, string? explicitPrefix = null)
////    {
////        var prefix = CategoryPrefixes.Resolve(category, explicitPrefix);
////        var n = await NextAsync($"serial_{prefix}", 100000);
////        var year = DateTime.UtcNow.ToString("yy");
////        return $"SN{year}{prefix}{n}";
////    }

////    public async Task<string> GenerateCategoryIdAsync()
////    {
////        var n = await NextAsync("cat", 100);
////        return $"CAT-{n}";
////    }

////    public async Task<string> GenerateBatchIdAsync()
////    {
////        var year = DateTime.UtcNow.Year;
////        var n = await NextAsync($"grn_{year}");
////        return $"GRN-{year}-{n:D6}";
////    }

////    public async Task<string> GeneratePoNumberAsync()
////    {
////        var year = DateTime.UtcNow.Year;
////        var n = await NextAsync($"po_{year}");
////        return $"PO-{year}-{n:D4}";
////    }

////    public async Task<string> GenerateEmployeeIdAsync()
////    {
////        var n = await NextAsync("emp", 1000);
////        return $"EMP-{n}";
////    }

////    public async Task<string> GenerateVendorIdAsync()
////    {
////        var n = await NextAsync("ven", 100);
////        return $"VEN-{n}";
////    }

////    public async Task<string> GenerateMaintenanceIdAsync()
////    {
////        var year = DateTime.UtcNow.Year;
////        var n = await NextAsync($"mt_{year}");
////        return $"MT-{year}-{n:D5}";
////    }

////    public async Task SeedSequenceFloorAsync(string key, long floor)
////    {
////        var seq = await _context.Sequences.FirstOrDefaultAsync(s => s.Key == key);
////        if (seq == null)
////        {
////            _context.Sequences.Add(new Sequence { Key = key, Value = floor });
////        }
////        else if (seq.Value < floor)
////        {
////            seq.Value = floor;
////        }
////        await _context.SaveChangesAsync();
////    }
////}
//using AssetHub.Core.Common;
//using AssetHub.Core.Entities;
//using AssetHub.Core.Interfaces.Services;
//using AssetHub.Infrastructure.Data;
//using Microsoft.EntityFrameworkCore;

//namespace AssetHub.Infrastructure.Services;

///// <summary>
///// Handles unique asset tags and sequence generation using DB values.
///// </summary>
//public class SequenceService : ISequenceService
//{
//  private readonly AppDbContext _context;

//  public SequenceService(AppDbContext context)
//  {
//    _context = context;
//  }

//  private async Task<long> NextAsync(string key, long start = 1)
//  {
//    var seq = await _context.Sequences.FirstOrDefaultAsync(s => s.Key == key);
//    if (seq == null)
//    {
//      seq = new Sequence { Key = key, Value = start };
//      _context.Sequences.Add(seq);
//    }
//    else
//    {
//      seq.Value += 1;
//    }
//    await _context.SaveChangesAsync();
//    return seq.Value;
//  }

//  public async Task<string> GenerateAssetTagAsync(string category, string? explicitPrefix = null)
//  {
//    var categoryName = (category ?? "Other").Trim();
//    string? prefix = explicitPrefix?.Trim();

//    // 1. Fetch exact prefix from Categories DB table if explicitPrefix is not passed
//    if (string.IsNullOrWhiteSpace(prefix))
//    {
//      var catEntity = await _context.Categories
//          .FirstOrDefaultAsync(c => c.Name.ToLower() == categoryName.ToLower());
//      prefix = catEntity?.Prefix?.Trim();
//    }

//    // 2. Fallback to CategoryPrefixes helper if DB record not found
//    if (string.IsNullOrWhiteSpace(prefix))
//    {
//      prefix = CategoryPrefixes.Resolve(categoryName, explicitPrefix);
//    }

//    // 3. Count existing assets in DB for this category
//    var currentCount = await _context.Assets
//        .CountAsync(a => a.Category.ToLower() == categoryName.ToLower());

//    var n = currentCount + 1;

//    // 4. Return formatted tag (e.g. LAP-00011, ALC-00001)
//    return $"{prefix}-{n:D5}";
//  }

//  public async Task<string> GenerateInternalSerialAsync(string category, string? explicitPrefix = null)
//  {
//    var categoryName = (category ?? "Other").Trim();
//    string? prefix = explicitPrefix?.Trim();

//    if (string.IsNullOrWhiteSpace(prefix))
//    {
//      var catEntity = await _context.Categories
//          .FirstOrDefaultAsync(c => c.Name.ToLower() == categoryName.ToLower());
//      prefix = catEntity?.Prefix?.Trim();
//    }

//    if (string.IsNullOrWhiteSpace(prefix))
//    {
//      prefix = CategoryPrefixes.Resolve(categoryName, explicitPrefix);
//    }

//    var n = await NextAsync($"serial_{prefix}", 100000);
//    var year = DateTime.UtcNow.ToString("yy");
//    return $"SN{year}{prefix}{n}";
//  }

//  public async Task<string> GenerateCategoryIdAsync()
//  {
//    var n = await NextAsync("cat", 100);
//    return $"CAT-{n}";
//  }

//  public async Task<string> GenerateBatchIdAsync()
//  {
//    var year = DateTime.UtcNow.Year;
//    var n = await NextAsync($"grn_{year}");
//    return $"GRN-{year}-{n:D6}";
//  }

//  public async Task<string> GeneratePoNumberAsync()
//  {
//    var year = DateTime.UtcNow.Year;
//    var n = await NextAsync($"po_{year}");
//    return $"PO-{year}-{n:D4}";
//  }

//  public async Task<string> GenerateEmployeeIdAsync()
//  {
//    var n = await NextAsync("emp", 1000);
//    return $"EMP-{n}";
//  }

//  public async Task<string> GenerateVendorIdAsync()
//  {
//    var n = await NextAsync("ven", 100);
//    return $"VEN-{n}";
//  }

//  public async Task<string> GenerateMaintenanceIdAsync()
//  {
//    var year = DateTime.UtcNow.Year;
//    var n = await NextAsync($"mt_{year}");
//    return $"MT-{year}-{n:D5}";
//  }

//  public async Task SeedSequenceFloorAsync(string key, long floor)
//  {
//    var seq = await _context.Sequences.FirstOrDefaultAsync(s => s.Key == key);
//    if (seq == null)
//    {
//      _context.Sequences.Add(new Sequence { Key = key, Value = floor });
//    }
//    else if (seq.Value < floor)
//    {
//      seq.Value = floor;
//    }
//    await _context.SaveChangesAsync();
//  }
//}

using AssetHub.Core.Common;
using AssetHub.Core.Entities;
using AssetHub.Core.Interfaces.Services;
using AssetHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetHub.Infrastructure.Services;

/// <summary>
/// Handles unique asset tags and sequence generation using DB values.
/// </summary>
public class SequenceService : ISequenceService
{
    private readonly AppDbContext _context;

    public SequenceService(AppDbContext context)
    {
        _context = context;
    }

    private async Task<long> NextAsync(string key, long start = 1)
    {
        var seq = await _context.Sequences.FirstOrDefaultAsync(s => s.Key == key);
        if (seq == null)
        {
            seq = new Sequence { Key = key, Value = start };
            _context.Sequences.Add(seq);
        }
        else
        {
            seq.Value += 1;
        }
        await _context.SaveChangesAsync();
        return seq.Value;
    }

    public async Task<string> GenerateAssetTagAsync(string category, string? explicitPrefix = null)
    {
        var categoryName = (category ?? "Other").Trim();
        string? prefix = explicitPrefix?.Trim();

        // 1. Fetch exact prefix from Categories DB table if explicitPrefix is not passed
        if (string.IsNullOrWhiteSpace(prefix))
        {
            var catEntity = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == categoryName.ToLower());
            prefix = catEntity?.Prefix?.Trim();
        }

        // 2. Fallback to CategoryPrefixes helper if DB record not found
        if (string.IsNullOrWhiteSpace(prefix))
        {
            prefix = CategoryPrefixes.Resolve(categoryName, explicitPrefix);
        }

        // 3. Fetch all existing IDs for this prefix from the database
        var tagPrefix = $"{prefix}-";
        var existingTags = await _context.Assets
            .Where(a => a.Id.StartsWith(tagPrefix) || a.Category.ToLower() == categoryName.ToLower())
            .Select(a => a.Id)
            .ToListAsync();

        var existingNumbers = new HashSet<int>();
        foreach (var tag in existingTags)
        {
            var parts = tag.Split('-');
            if (parts.Length >= 2 && int.TryParse(parts[parts.Length - 1], out var num))
            {
                existingNumbers.Add(num);
            }
        }

        // 4. Always start scanning from 1 to find the lowest available sequence number
        int n = 1;
        while (existingNumbers.Contains(n))
        {
            n++;
        }

        // 5. Return formatted tag (e.g. MOB-00001, LAP-00001, etc.)
        return $"{prefix}-{n:D5}";
    }

    public async Task<string> GenerateInternalSerialAsync(string category, string? explicitPrefix = null)
    {
        var categoryName = (category ?? "Other").Trim();
        string? prefix = explicitPrefix?.Trim();

        if (string.IsNullOrWhiteSpace(prefix))
        {
            var catEntity = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == categoryName.ToLower());
            prefix = catEntity?.Prefix?.Trim();
        }

        if (string.IsNullOrWhiteSpace(prefix))
        {
            prefix = CategoryPrefixes.Resolve(categoryName, explicitPrefix);
        }

        var n = await NextAsync($"serial_{prefix}", 100000);
        var year = DateTime.UtcNow.ToString("yy");
        return $"SN{year}{prefix}{n}";
    }

    public async Task<string> GenerateCategoryIdAsync()
    {
        var n = await NextAsync("cat", 100);
        return $"CAT-{n}";
    }

    public async Task<string> GenerateBatchIdAsync()
    {
        var year = DateTime.UtcNow.Year;
        var n = await NextAsync($"grn_{year}");
        return $"GRN-{year}-{n:D6}";
    }

    public async Task<string> GeneratePoNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var n = await NextAsync($"po_{year}");
        return $"PO-{year}-{n:D4}";
    }

    public async Task<string> GenerateEmployeeIdAsync()
    {
        var n = await NextAsync("emp", 1000);
        return $"EMP-{n}";
    }

    public async Task<string> GenerateVendorIdAsync()
    {
        var n = await NextAsync("ven", 100);
        return $"VEN-{n}";
    }

    public async Task<string> GenerateMaintenanceIdAsync()
    {
        var year = DateTime.UtcNow.Year;
        var n = await NextAsync($"mt_{year}");
        return $"MT-{year}-{n:D5}";
    }

    public async Task SeedSequenceFloorAsync(string key, long floor)
    {
        var seq = await _context.Sequences.FirstOrDefaultAsync(s => s.Key == key);
        if (seq == null)
        {
            _context.Sequences.Add(new Sequence { Key = key, Value = floor });
        }
        else if (seq.Value < floor)
        {
            seq.Value = floor;
        }
        await _context.SaveChangesAsync();
    }
}

