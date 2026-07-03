namespace ChadGreen.Management.Api.Options;

public sealed class ManagementOptions
{
    public string? SiteRoot { get; set; }

    public string ArchiveFolderName { get; set; } = ".archive";

    public int ArchiveRetentionDays { get; set; } = 90;

    public string[] ClientOrigins { get; set; } = ["http://localhost:5506", "https://localhost:7506"];

    public ManagementFeatureOptions Features { get; set; } = new();
}

public sealed class ManagementFeatureOptions
{
    public bool GitIntegration { get; set; }
}
