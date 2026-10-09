namespace AssetHub.Core.Dtos.Category;

public class CategoryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int AssetCount { get; set; }
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}

public class UpdateCategoryDto
{
    public string? Name { get; set; }
    public string? Prefix { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
}
