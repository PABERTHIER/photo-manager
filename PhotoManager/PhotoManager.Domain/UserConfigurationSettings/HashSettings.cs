namespace PhotoManager.Domain.UserConfigurationSettings;

public record HashSettings(ushort PHashThreshold, bool UsingDHash, bool UsingMD5Hash, bool UsingPHash);
