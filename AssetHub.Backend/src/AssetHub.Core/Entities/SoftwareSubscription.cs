namespace AssetHub.Core.Entities;

public class SoftwareSubscription : BaseEntity
{
    public string AssetCode { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public string? VendorId { get; set; }
    public Vendor? Vendor { get; set; }
    public string? VendorName { get; set; }
    public string? VendorPortalUrl { get; set; }

    public string? ResponsiblePersonId { get; set; }
    public string ResponsiblePersonName { get; set; } = string.Empty;

    public int? SeatsCount { get; set; }

    public DateOnly? PurchaseDate { get; set; }
    public DateOnly NextRenewalDate { get; set; }
    public DateOnly ExpiryDate { get; set; }

    public string BillingType { get; set; } = "Monthly";
    public string Currency { get; set; } = "INR";
    public decimal Amount { get; set; }
    public bool AutoRenewal { get; set; } = true;
    public string? PaymentMethod { get; set; }

    public string Status { get; set; } = "Active";
    public string? AttachmentName { get; set; }
    public string? Description { get; set; }
}
