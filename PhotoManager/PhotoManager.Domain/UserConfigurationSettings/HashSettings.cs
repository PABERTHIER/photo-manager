namespace PhotoManager.Domain.UserConfigurationSettings;

public record class HashSettings(ushort PHashThreshold, bool UsingDHash, bool UsingMD5Hash, bool UsingPHash);
