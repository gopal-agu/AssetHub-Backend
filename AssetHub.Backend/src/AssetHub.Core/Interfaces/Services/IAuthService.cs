using AssetHub.Core.Dtos.Auth;

namespace AssetHub.Core.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
}
