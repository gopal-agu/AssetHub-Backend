using AssetHub.Core.Dtos.Vendor;

namespace AssetHub.Core.Interfaces.Services;

public interface IVendorService
{
  Task<List<VendorDto>> GetAllAsync();
  Task<VendorDto> GetByIdAsync(string id);
  Task<VendorDto> CreateAsync(CreateVendorDto dto);
  Task<VendorDto> UpdateAsync(string id, UpdateVendorDto dto);
  Task DeleteAsync(string id);
}
