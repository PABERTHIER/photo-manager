namespace PhotoManager.Domain;

public class AboutInformation
{
    public required string Product { get; set; }
    // public string Author { get; set; } // TODO: appsettings.json -> Owner -> set up in UserConfigurationService
    public required string Version { get; set; }
}
