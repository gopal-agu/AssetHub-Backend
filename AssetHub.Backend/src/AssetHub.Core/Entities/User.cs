namespace AssetHub.Core.Entities;

/// <summary>Login account. Seeded with the same two demo users the frontend's AuthContext hard-codes.</summary>
public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
