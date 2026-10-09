namespace AssetHub.Core.Dtos.Software;

public class SoftwareSubscriptionDto
{
    public string Id { get; set; } = string.Empty;
    public string AssetCode { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public string? VendorId { get; set; }
    public string? VendorName { get; set; }
    public string? VendorPortalUrl { get; set; }

    public string? ResponsiblePersonId { get; set; }
    public string ResponsiblePersonName { get; set; } = string.Empty;

    public int? SeatsCount { get; set; }

    public string? PurchaseDate { get; set; }
    public string NextRenewalDate { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;

    public string BillingType { get; set; } = "Monthly";
    public string Currency { get; set; } = "INR";
    public decimal Amount { get; set; }
    public bool AutoRenewal { get; set; }
    public string? PaymentMethod { get; set; }

    public string Status { get; set; } = "Active";
    public string? AttachmentName { get; set; }
    public string? Description { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

public class CreateSoftwareSubscriptionDto
{
    public string? AssetCode { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public string? VendorId { get; set; }
    public string? VendorName { get; set; }
    public string? VendorPortalUrl { get; set; }

    public string? ResponsiblePersonId { get; set; }
    public string ResponsiblePersonName { get; set; } = string.Empty;

    public int? SeatsCount { get; set; }

    public string? PurchaseDate { get; set; }
    public string NextRenewalDate { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;

    public string BillingType { get; set; } = "Monthly";
    public string Currency { get; set; } = "INR";
    public decimal Amount { get; set; }
    public bool AutoRenewal { get; set; } = true;
    public string? PaymentMethod { get; set; }

    public string Status { get; set; } = "Active";
    public string? AttachmentName { get; set; }
    public string? Description { get; set; }
}

public class UpdateSoftwareSubscriptionDto : CreateSoftwareSubscriptionDto
{
}
