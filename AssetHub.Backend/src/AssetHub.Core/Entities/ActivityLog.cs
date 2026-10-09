namespace AssetHub.Core.Entities;

/// <summary>Rolling feed of recent actions shown on the Dashboard (kept to the latest 40, like the frontend).</summary>
public class ActivityLog
{
    public int Id { get; set; }
    public DateTime Ts { get; set; } = DateTime.UtcNow;
    public string Text { get; set; } = string.Empty;
}
