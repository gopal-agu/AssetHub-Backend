namespace AssetHub.Core.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
