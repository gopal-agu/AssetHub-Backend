namespace AssetHub.Core.Dtos.Vendor;

public class VendorDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Categories { get; set; } = new();
    public string ContactPerson { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string GstNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Rating { get; set; }
    public string Status { get; set; } = string.Empty;
    public string OnboardedOn { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = string.Empty;
    public int AssetCount { get; set; }
}

public class CreateVendorDto
{
    public string Name { get; set; } = string.Empty;
    public List<string> Categories { get; set; } = new();
    public string ContactPerson { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string GstNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = "Net 30";
}

public class UpdateVendorDto
{
  public string? Name { get; set; }
  public List<string>? Categories { get; set; }
  public string? ContactPerson { get; set; }
  public string? Email { get; set; }
  public string? Phone { get; set; }
  public string? City { get; set; }
  public string? GstNumber { get; set; }
  public string? Address { get; set; }
  public string? Status { get; set; }
  public string? PaymentTerms { get; set; }
  public double? Rating { get; set; }
}
