using PhotoManager.Infrastructure;
using System.Globalization;
using System.IO;

namespace PhotoManager.Tests.Helpers;

public class StorageServiceHelper
{
    public static string ResolveDataDirectory(double storageVersion)
    {
        return Path.Combine(Constants.PathBackUpTests, "v" + storageVersion.ToString("0.0", new CultureInfo("en-US")));
    }
}
