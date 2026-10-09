using AssetHub.Core.Dtos.Category;

namespace AssetHub.Core.Interfaces.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto> GetByIdAsync(string id);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
    Task<CategoryDto> UpdateAsync(string id, UpdateCategoryDto dto);
}
