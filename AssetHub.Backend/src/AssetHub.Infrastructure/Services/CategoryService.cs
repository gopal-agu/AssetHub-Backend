using AssetHub.Core.Common;
using AssetHub.Core.Dtos.Category;
using AssetHub.Core.Entities;
using AssetHub.Core.Interfaces.Repositories;
using AssetHub.Core.Interfaces.Services;

namespace AssetHub.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _uow;
    private readonly ISequenceService _sequences;
    private readonly IActivityLogService _activity;

    public CategoryService(IUnitOfWork uow, ISequenceService sequences, IActivityLogService activity)
    {
        _uow = uow;
        _sequences = sequences;
        _activity = activity;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var categories = await _uow.Categories.GetAllAsync();
        var assets = await _uow.Assets.GetAllAsync();
        var counts = assets.GroupBy(a => a.Category).ToDictionary(g => g.Key, g => g.Count());

        return categories.Select(c => Map(c, counts.GetValueOrDefault(c.Name))).ToList();
    }

    public async Task<CategoryDto> GetByIdAsync(string id)
    {
        var category = await _uow.Categories.GetByIdAsync(id)
            ?? throw ApiException.NotFound("Category", id);
        var count = await _uow.Assets.CountAsync(a => a.Category == category.Name);
        return Map(category, count);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        if (await _uow.Categories.ExistsAsync(c => c.Name == dto.Name))
            throw new ApiException($"Category '{dto.Name}' already exists.", 409);

        var category = new Category
        {
            Id = await _sequences.GenerateCategoryIdAsync(),
            Name = dto.Name,
            Prefix = dto.Prefix.ToUpperInvariant(),
            Description = dto.Description,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? RecordStatuses.Active : dto.Status,
        };

        await _uow.Categories.AddAsync(category);
        await _uow.SaveChangesAsync();
        await _activity.LogAsync($"New category added: {category.Name} ({category.Prefix}).");

        return Map(category, 0);
    }

    public async Task<CategoryDto> UpdateAsync(string id, UpdateCategoryDto dto)
    {
        var category = await _uow.Categories.GetByIdAsync(id)
            ?? throw ApiException.NotFound("Category", id);

        if (!string.IsNullOrWhiteSpace(dto.Name)) category.Name = dto.Name;
        if (!string.IsNullOrWhiteSpace(dto.Prefix)) category.Prefix = dto.Prefix.ToUpperInvariant();
        if (dto.Description != null) category.Description = dto.Description;
        if (!string.IsNullOrWhiteSpace(dto.Status)) category.Status = dto.Status;
        category.UpdatedAt = DateTime.UtcNow;

        _uow.Categories.Update(category);
        await _uow.SaveChangesAsync();
        await _activity.LogAsync($"Category {category.Id} updated.");

        var count = await _uow.Assets.CountAsync(a => a.Category == category.Name);
        return Map(category, count);
    }

    private static CategoryDto Map(Category c, int assetCount) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Prefix = c.Prefix,
        Description = c.Description,
        Status = c.Status,
        AssetCount = assetCount,
    };
}
