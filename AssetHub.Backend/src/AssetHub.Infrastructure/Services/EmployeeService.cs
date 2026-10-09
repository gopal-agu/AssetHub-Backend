using AssetHub.Core.Common;
using AssetHub.Core.Dtos.Employee;
using AssetHub.Core.Entities;
using AssetHub.Core.Interfaces.Repositories;
using AssetHub.Core.Interfaces.Services;

namespace AssetHub.Infrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _uow;
    private readonly ISequenceService _sequences;
    private readonly IActivityLogService _activity;

    public EmployeeService(IUnitOfWork uow, ISequenceService sequences, IActivityLogService activity)
    {
        _uow = uow;
        _sequences = sequences;
        _activity = activity;
    }

    public async Task<List<EmployeeDto>> GetAllAsync()
    {
        var employees = await _uow.Employees.GetAllAsync();
        var assets = await _uow.Assets.GetAllAsync();
        var counts = assets.Where(a => a.AssignedTo != null)
            .GroupBy(a => a.AssignedTo!)
            .ToDictionary(g => g.Key, g => g.Count());

        return employees.Select(e => Map(e, counts.GetValueOrDefault(e.Id))).ToList();
    }

    public async Task<EmployeeDto> GetByIdAsync(string id)
    {
        var employee = await _uow.Employees.GetByIdAsync(id)
            ?? throw ApiException.NotFound("Employee", id);
        var count = await _uow.Assets.CountAsync(a => a.AssignedTo == id);
        return Map(employee, count);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto)
    {
        if (await _uow.Employees.ExistsAsync(e => e.Email == dto.Email))
            throw new ApiException($"Employee with email '{dto.Email}' already exists.", 409);

        var employee = new Employee
        {
            Id = await _sequences.GenerateEmployeeIdAsync(),
            Name = dto.Name,
            Email = dto.Email,
            Department = dto.Department,
            Designation = dto.Designation,
            Location = dto.Location,
            JoinDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Status = "Active",
            ReportingManager = string.IsNullOrWhiteSpace(dto.ReportingManager) ? "—" : dto.ReportingManager,
            Phone = dto.Phone,
        };

        await _uow.Employees.AddAsync(employee);
        await _uow.SaveChangesAsync();
        await _activity.LogAsync($"New employee added: {employee.Name} ({employee.Department}).");

        return Map(employee, 0);
    }

    private static EmployeeDto Map(Employee e, int assetCount) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Email = e.Email,
        Department = e.Department,
        Designation = e.Designation,
        Location = e.Location,
        JoinDate = e.JoinDate.ToString("yyyy-MM-dd"),
        Status = e.Status,
        ReportingManager = e.ReportingManager,
        Phone = e.Phone,
        AssetCount = assetCount,
    };
}
