using System.Globalization;
using System.IO;
using ConstantsPath = PhotoManager.Constants.Path;

namespace PhotoManager.Tests.Helpers;

public class StorageServiceHelper
{
    public static string ResolveDataDirectory(double storageVersion)
    {
        return Path.Combine(ConstantsPath.PathBackUpTests, "v" + storageVersion.ToString("0.0", new CultureInfo("en-US")));
    }
}
