using AssetHub.Core.Entities;
using AssetHub.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AssetHub.Infrastructure.Data.Seed;

/// <summary>
/// Ports the frontend's src/data/mockData.js + demo users from AuthContext.jsx into real rows, so the
/// UI has the same look (same vendors, employee names, department spread, a mix of asset statuses,
/// some open maintenance tickets) the moment the API comes up against an empty database.
/// Idempotent: skips entirely if Categories already has rows.
/// </summary>
public static class DbSeeder
{
  private static readonly string[] Departments =
  {
        "Engineering", "Quality Assurance", "DevOps", "IT Infrastructure", "Human Resources",
        "Finance", "Sales", "Customer Success", "Design", "Product"
    };

  private static readonly string[] Locations =
  {
        "Pune - Kharadi HQ", "Pune - Hinjewadi Annex", "Bengaluru - Whitefield", "Mumbai - BKC", "Remote / WFH"
    };

  private static readonly (string First, string Last)[] Names =
  {
        ("Aarav","Sharma"),("Vivaan","Verma"),("Ishaan","Iyer"),("Kabir","Nair"),("Rohan","Kulkarni"),
        ("Ananya","Deshpande"),("Priya","Reddy"),("Sneha","Patel"),("Meera","Chatterjee"),("Kavya","Bose"),
        ("Aditi","Menon"),("Rahul","Rao"),("Neha","Joshi"),("Arjun","Gupta"),("Sanya","Malhotra"),
        ("Devika","Kapoor"),("Karan","Bhatt"),("Tanya","Pillai"),("Nikhil","Mehta"),("Pooja","Agarwal"),
    };

  public static async Task SeedAsync(IServiceProvider services)
  {
    using var scope = services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var sequences = scope.ServiceProvider.GetRequiredService<ISequenceService>();

    if (context.Database.IsRelational())
    {
      await context.Database.MigrateAsync();
    }
    else
    {
      await context.Database.EnsureCreatedAsync();
    }

    if (await context.Categories.AnyAsync()) return; // already seeded

    // ---- Users (matches AuthContext.jsx DEMO_USERS) ----------------------------------------
    context.Users.AddRange(
        new User { Email = "admin@assethub.io", Name = "Ritika Shah", Role = "Administrator", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123") },
        new User { Email = "itmanager@assethub.io", Name = "Omkar Deshmukh", Role = "IT Manager", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123") }
    );

    // ---- Categories --------------------------------------------------------------------------
    var categoryPrefixes = new (string Name, string Prefix)[]
    {
            ("Laptop","LAP"), ("Desktop","DSK"), ("Monitor","MON"), ("Server","SRV"), ("Networking","NET"),
            ("Mobile","MOB"), ("Printer","PRN"), ("Peripheral","PER"), ("Software","SFT"),
    };
    var categories = new List<Category>();
    var catSeq = 100;
    foreach (var (name, prefix) in categoryPrefixes)
    {
      catSeq++;
      categories.Add(new Category { Id = $"CAT-{catSeq}", Name = name, Prefix = prefix, Description = "", Status = "Active" });
    }
    context.Categories.AddRange(categories);
    await sequences.SeedSequenceFloorAsync("cat", catSeq);

    // ---- Vendors -------------------------------------------------------------------------------
    var vendorSeed = new[]
    {
            new { Name = "Redington India Ltd", Cats = new[]{"Laptop","Desktop","Monitor"}, City="Mumbai", Gst="27AACCR1234F1Z5", Contact="Sunil Kamath", Email="sunil.kamath@redington.co.in", Phone="+91 98200 11223", Rating=4.6 },
            new { Name = "Ingram Micro India", Cats = new[]{"Laptop","Desktop","Peripheral","Software"}, City="Bengaluru", Gst="29AACCI5678K1Z2", Contact="Radhika Menon", Email="radhika.menon@ingrammicro.com", Phone="+91 98450 33221", Rating=4.4 },
            new { Name = "Dell Technologies India", Cats = new[]{"Laptop","Desktop","Server","Monitor"}, City="Bengaluru", Gst="29AAACD4567M1ZR", Contact="Vikram Suresh", Email="vikram.suresh@dell.com", Phone="+91 80 4172 5000", Rating=4.8 },
            new { Name = "HP India Sales Pvt Ltd", Cats = new[]{"Laptop","Printer","Desktop"}, City="Gurugram", Gst="06AAACH0165M1ZY", Contact="Anjali Bhatia", Email="anjali.bhatia@hp.com", Phone="+91 124 383 6000", Rating=4.5 },
            new { Name = "Cisco Systems India", Cats = new[]{"Networking"}, City="Bengaluru", Gst="29AAACC5678N1ZQ", Contact="Arvind Nambiar", Email="arvind.nambiar@cisco.com", Phone="+91 80 4358 4000", Rating=4.7 },
            new { Name = "Rashi Peripherals Ltd", Cats = new[]{"Peripheral","Monitor"}, City="Mumbai", Gst="27AAACR2345P1Z8", Contact="Farhan Sheikh", Email="farhan.sheikh@rptechindia.com", Phone="+91 98200 55667", Rating=4.2 },
            new { Name = "Samsung India Electronics", Cats = new[]{"Mobile","Monitor"}, City="Noida", Gst="09AAACS3210E1ZH", Contact="Preeti Rawal", Email="preeti.rawal@samsung.com", Phone="+91 120 685 5000", Rating=4.5 },
            new { Name = "Lenovo India Pvt Ltd", Cats = new[]{"Laptop","Desktop","Server"}, City="Bengaluru", Gst="29AAACL1230Z1ZJ", Contact="Manoj Pillai", Email="manoj.pillai@lenovo.com", Phone="+91 80 6654 8000", Rating=4.3 },
            new { Name = "Microsoft India Pvt Ltd", Cats = new[]{"Software"}, City="Hyderabad", Gst="36AAACM4433N1Z6", Contact="Shalini Rao", Email="shalini.rao@microsoft.com", Phone="+91 40 6672 1000", Rating=4.9 },
            new { Name = "Canon India Pvt Ltd", Cats = new[]{"Printer"}, City="Gurugram", Gst="06AAACC6789L1Z3", Contact="Deepak Oberoi", Email="deepak.oberoi@canon.co.in", Phone="+91 124 456 4000", Rating=4.1 },
        };

    var vendors = new List<Vendor>();
    var venSeq = 100;
    foreach (var v in vendorSeed)
    {
      venSeq++;
      vendors.Add(new Vendor
      {
        Id = $"VEN-{venSeq}",
        Name = v.Name,
        Categories = v.Cats.ToList(),
        ContactPerson = v.Contact,
        Email = v.Email,
        Phone = v.Phone,
        City = v.City,
        GstNumber = v.Gst,
        Address = $"{200 + venSeq * 4}, {v.City} Business Park, {v.City}, India",
        Rating = v.Rating,
        Status = "Active",
        OnboardedOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-(400 + venSeq))),
        PaymentTerms = "Net 30",
      });
    }
    context.Vendors.AddRange(vendors);
    await sequences.SeedSequenceFloorAsync("ven", venSeq);

    // ---- Employees ----------------------------------------------------------------------------
    var employees = new List<Employee>();
    var empSeq = 1000;
    for (var i = 0; i < Names.Length; i++)
    {
      empSeq++;
      var (first, last) = Names[i];
      employees.Add(new Employee
      {
        Id = $"EMP-{empSeq}",
        Name = $"{first} {last}",
        Email = $"{first.ToLowerInvariant()}.{last.ToLowerInvariant()}@itfirm.com",
        Department = Departments[i % Departments.Length],
        Designation = i % 4 == 0 ? "Team Lead" : "Software Engineer",
        Location = Locations[i % Locations.Length],
        JoinDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-(700 - i * 20))),
        Status = "Active",
        ReportingManager = i < 3 ? "—" : $"{Names[i - 3].First} {Names[i - 3].Last}",
        Phone = $"+91 9{700000000 + i * 137}",
      });
    }
    context.Employees.AddRange(employees);
    await sequences.SeedSequenceFloorAsync("emp", empSeq);

    await context.SaveChangesAsync();

    // ---- Inventory batches + generated assets --------------------------------------------------
    var batchPlan = new[]
    {
            new { Category = "Laptop", VendorIdx = 2, Qty = 8, Cost = 78500m, WarrantyMonths = 36, DaysAgo = 640,
                  Config = new Dictionary<string,string>{{"brandModel","Dell Latitude 5440"},{"cpu","Intel Core i7-1355U"},{"ram","16 GB"},{"storage","512 GB SSD"},{"os","Windows 11 Pro"}} },
            new { Category = "Laptop", VendorIdx = 7, Qty = 6, Cost = 71200m, WarrantyMonths = 24, DaysAgo = 420,
                  Config = new Dictionary<string,string>{{"brandModel","Lenovo ThinkPad T14 Gen 4"},{"cpu","AMD Ryzen 7 PRO 7840U"},{"ram","16 GB"},{"storage","512 GB SSD"},{"os","Windows 11 Pro"}} },
            new { Category = "Desktop", VendorIdx = 3, Qty = 5, Cost = 54300m, WarrantyMonths = 36, DaysAgo = 580,
                  Config = new Dictionary<string,string>{{"brandModel","HP EliteDesk 800 G9"},{"cpu","Intel Core i5-13500"},{"ram","16 GB"},{"storage","512 GB SSD"},{"os","Windows 11 Pro"}} },
            new { Category = "Monitor", VendorIdx = 5, Qty = 10, Cost = 16800m, WarrantyMonths = 36, DaysAgo = 500,
                  Config = new Dictionary<string,string>{{"brandModel","Dell UltraSharp U2422H"},{"size","23.8\""},{"resolution","1920x1080"},{"panel","IPS"}} },
            new { Category = "Server", VendorIdx = 2, Qty = 2, Cost = 612000m, WarrantyMonths = 60, DaysAgo = 720,
                  Config = new Dictionary<string,string>{{"brandModel","Dell PowerEdge R750"},{"cpu","2x Intel Xeon Gold 6338"},{"ram","256 GB"},{"storage","4x 2TB SAS RAID10"},{"formFactor","Rack 2U"}} },
            new { Category = "Networking", VendorIdx = 4, Qty = 4, Cost = 187000m, WarrantyMonths = 60, DaysAgo = 610,
                  Config = new Dictionary<string,string>{{"brandModel","Cisco Catalyst 9300-48P"},{"deviceType","Switch"},{"ports","48x 1GbE PoE+"}} },
            new { Category = "Mobile", VendorIdx = 6, Qty = 6, Cost = 62900m, WarrantyMonths = 12, DaysAgo = 110,
                  Config = new Dictionary<string,string>{{"brandModel","Samsung Galaxy S24"},{"storage","256 GB"},{"os","Android 14"}} },
            new { Category = "Printer", VendorIdx = 9, Qty = 3, Cost = 34500m, WarrantyMonths = 24, DaysAgo = 380,
                  Config = new Dictionary<string,string>{{"brandModel","Canon imageCLASS MF445dw"},{"type","Multifunction"},{"connectivity","Ethernet, USB, Wi-Fi"}} },
            new { Category = "Peripheral", VendorIdx = 5, Qty = 12, Cost = 6200m, WarrantyMonths = 12, DaysAgo = 300,
                  Config = new Dictionary<string,string>{{"brandModel","Logitech MX Keys Combo"},{"type","Keyboard"}} },
            new { Category = "Software", VendorIdx = 8, Qty = 20, Cost = 10500m, WarrantyMonths = 12, DaysAgo = 30,
                  Config = new Dictionary<string,string>{{"brandModel","Microsoft 365 E3"},{"licenseType","Annual Subscription"},{"seats","1"}} },
        };

    var statusCycle = new[] { "Assigned", "Assigned", "Assigned", "Available", "Available", "Under Repair" };
    var conditions = new[] { "Excellent", "Good", "Fair" };
    var rand = new Random(42);
    var empCursor = 0;
    var assignmentHistories = new List<AssignmentHistory>();
    var maintenanceRecords = new List<MaintenanceRecord>();
    var allAssets = new List<Asset>();
    var batches = new List<InventoryBatch>();

    var poSeq = 0;
    var grnSeq = 0;
    var mtSeq = 0;
    var assetSeqByPrefix = new Dictionary<string, int>();
    var serialSeqByPrefix = new Dictionary<string, int>();
    var year = DateTime.UtcNow.Year;

    string NextTag(string prefix)
    {
      assetSeqByPrefix[prefix] = assetSeqByPrefix.GetValueOrDefault(prefix, 0) + 1;
      return $"AST-{prefix}-{assetSeqByPrefix[prefix]:D5}";
    }
    string NextSerial(string prefix)
    {
      serialSeqByPrefix[prefix] = serialSeqByPrefix.GetValueOrDefault(prefix, 100000) + 1;
      return $"SN{DateTime.UtcNow:yy}{prefix}{serialSeqByPrefix[prefix]}";
    }

    var categoryPrefixMap = categoryPrefixes.ToDictionary(c => c.Name, c => c.Prefix);

    foreach (var plan in batchPlan)
    {
      var vendor = vendors[plan.VendorIdx];
      var purchaseDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-plan.DaysAgo));
      var warrantyExpiry = purchaseDate.AddMonths(plan.WarrantyMonths);
      grnSeq++;
      poSeq++;
      var batchId = $"GRN-{year}-{grnSeq:D6}";
      var poNumber = $"PO-{year}-{poSeq:D4}";
      var configSummary = string.Join(" · ", plan.Config.Values);

      var batch = new InventoryBatch
      {
        Id = batchId,
        Category = plan.Category,
        Department = "Engineering",
        Description = $"{plan.Category} procurement batch",
        VendorId = vendor.Id,
        Quantity = plan.Qty,
        UnitCost = plan.Cost,
        TotalCost = plan.Qty * plan.Cost,
        PurchaseCost = plan.Cost,
        PurchaseDate = purchaseDate,
        WarrantyStartDate = purchaseDate,
        WarrantyEndDate = warrantyExpiry,
        Location = "Pune - Kharadi HQ",
        Condition = "New",
        Status = "Available",
        InvoiceNo = $"INV-{venSeq}-{1000 + grnSeq * 7}",
        PoNumber = poNumber,
        ReceivedBy = "IT Asset Desk",
        Configuration = plan.Config,
        ConfigSummary = configSummary,
        AssetsGenerated = plan.Qty,
      };
      batches.Add(batch);

      var prefix = categoryPrefixMap.GetValueOrDefault(plan.Category, "AST");

      for (var i = 0; i < plan.Qty; i++)
      {
        var tag = NextTag(prefix);
        var serial = NextSerial(prefix);
        var status = plan.Category == "Software" ? (i < plan.Qty - 2 ? "Assigned" : "Available") : statusCycle[rand.Next(statusCycle.Length)];

        var asset = new Asset
        {
          Id = tag,
          SerialNumber = serial,
          Category = plan.Category,
          Configuration = plan.Config,
          ConfigSummary = configSummary,
          BatchId = batch.Id,
          VendorId = vendor.Id,
          PurchaseDate = batch.PurchaseDate,
          UnitCost = plan.Cost,
          PoNumber = poNumber,
          InvoiceNo = batch.InvoiceNo,
          WarrantyMonths = plan.WarrantyMonths,
          WarrantyExpiry = warrantyExpiry,
          Location = Locations[rand.Next(Locations.Length)],
          Condition = conditions[rand.Next(conditions.Length)],
          Status = status,
          Notes = "",
        };

        if (status is "Assigned" or "Under Repair")
        {
          var emp = employees[empCursor % employees.Count];
          empCursor++;
          var assignedOn = purchaseDate.AddDays(3 + rand.Next(20));
          asset.AssignedTo = emp.Id;
          asset.AssignedToName = emp.Name;
          asset.AssignedDept = emp.Department;
          asset.AssignedOn = assignedOn;

          assignmentHistories.Add(new AssignmentHistory
          {
            Id = $"{tag}-A1",
            AssetId = tag,
            EmployeeId = emp.Id,
            EmployeeName = emp.Name,
            Department = emp.Department,
            Action = "Assigned",
            Date = assignedOn,
            Remarks = "Issued at onboarding / requisition",
            PerformedBy = "IT Asset Desk",
          });
        }

        if (status == "Under Repair")
        {
          mtSeq++;
          var openedOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-rand.Next(1, 20)));
          maintenanceRecords.Add(new MaintenanceRecord
          {
            Id = $"MT-{year}-{mtSeq:D5}",
            AssetId = tag,
            IssueType = "Hardware Fault",
            Description = "Device not powering on, sent for diagnostics.",
            ReportedBy = asset.AssignedToName ?? "IT Helpdesk",
            OpenedOn = openedOn,
            ExpectedCompletion = openedOn.AddDays(7),
            Status = "Open",
            Cost = 500 + rand.Next(3500),
            VendorId = vendor.Id,
          });
        }

        allAssets.Add(asset);
      }
    }

    context.InventoryBatches.AddRange(batches);
    context.Assets.AddRange(allAssets);
    context.AssignmentHistories.AddRange(assignmentHistories);
    context.MaintenanceRecords.AddRange(maintenanceRecords);

    context.ActivityLogs.Add(new ActivityLog { Text = "System initialized with historical procurement data.", Ts = DateTime.UtcNow });

    await sequences.SeedSequenceFloorAsync($"po_{year}", poSeq);
    await sequences.SeedSequenceFloorAsync($"grn_{year}", grnSeq);
    await sequences.SeedSequenceFloorAsync($"mt_{year}", mtSeq);
    foreach (var kv in assetSeqByPrefix) await sequences.SeedSequenceFloorAsync($"asset_{kv.Key}", kv.Value);
    foreach (var kv in serialSeqByPrefix) await sequences.SeedSequenceFloorAsync($"serial_{kv.Key}", kv.Value);

    await context.SaveChangesAsync();
  }
}
