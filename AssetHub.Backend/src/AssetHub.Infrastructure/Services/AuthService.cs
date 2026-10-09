using AssetHub.Core.Dtos.Auth;
using AssetHub.Core.Interfaces.Repositories;
using AssetHub.Core.Interfaces.Services;

namespace AssetHub.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtTokenService _tokenService;

    public AuthService(IUnitOfWork uow, IJwtTokenService tokenService)
    {
        _uow = uow;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _uow.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
        if (user == null) return null;

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return null;

        var (token, expiresAt) = _tokenService.GenerateToken(user);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserDto { Email = user.Email, Name = user.Name, Role = user.Role }
        };
    }
}
