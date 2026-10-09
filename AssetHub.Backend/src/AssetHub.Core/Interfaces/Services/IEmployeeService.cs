using AssetHub.Core.Dtos.Employee;

namespace AssetHub.Core.Interfaces.Services;

public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetAllAsync();
    Task<EmployeeDto> GetByIdAsync(string id);
    Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto);
}
