namespace AssetHub.Core.Common;

public class EmailSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SenderName { get; set; } = "AssetHub Notifications";
    public string SenderEmail { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    // CURRENT LOGIC (COMMENTED OUT):
    // public string ManagementEmail { get; set; } = string.Empty;

    // NEW LOGIC: Supports multiple email IDs (array or comma-separated list)
    public List<string> ManagementEmail { get; set; } = new();
}
