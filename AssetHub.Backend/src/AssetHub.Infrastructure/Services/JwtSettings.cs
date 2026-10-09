namespace AssetHub.Infrastructure.Services;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "AssetHub.API";
    public string Audience { get; set; } = "AssetHub.Client";
    public int ExpiryMinutes { get; set; } = 480;
}
