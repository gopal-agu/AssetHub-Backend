using AssetHub.Core.Common;
using AssetHub.Core.Interfaces.Repositories;
using AssetHub.Core.Interfaces.Services;
using AssetHub.Infrastructure.Data;
using AssetHub.Infrastructure.Repositories;
using AssetHub.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AssetHub.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Wires up EF Core + every repository/service. Connection string "UseInMemoryDb": true in
    /// appsettings switches to the EF Core InMemory provider for zero-setup local runs/demos;
    /// otherwise SQL Server is used with the "DefaultConnection" connection string.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var useInMemory = configuration.GetValue<bool>("UseInMemoryDb");

        services.AddDbContext<AppDbContext>(options =>
        {
            if (useInMemory)
            {
                options.UseInMemoryDatabase("AssetHubDb");
            }
            else
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
            }
        });

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.PostConfigure<EmailSettings>(options =>
        {
            // Support both JSON array [ "a@b.com", "c@d.com" ] and comma/semicolon-separated string "a@b.com, c@d.com"
            var mgmtSection = configuration.GetSection("EmailSettings:ManagementEmail");
            if (mgmtSection.Exists() && !string.IsNullOrWhiteSpace(mgmtSection.Value))
            {
                var split = mgmtSection.Value
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();
                if (split.Count > 0)
                {
                    options.ManagementEmail = split;
                }
            }
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddMemoryCache();
        services.AddSingleton<IDashboardCacheService, DashboardCacheService>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISequenceService, SequenceService>();
        services.AddScoped<IActivityLogService, ActivityLogService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IVendorService, VendorService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IAssetService, AssetService>();
        services.AddScoped<IMaintenanceService, MaintenanceService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ISoftwareService, SoftwareService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISoftwareNotificationService, SoftwareNotificationService>();

        return services;
    }
}
