namespace PhotoManager.Tests.Unit.Infrastructure;

[TestFixture]
public class PathProviderServiceTests
{
    [Test]
    public void ResolveDatabaseDirectory_ReturnsPathInLocalApplicationData()
    {
        PathProviderService pathProviderService = new();

        string result = pathProviderService.ResolveDatabaseDirectory();

        string applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (string.IsNullOrWhiteSpace(applicationDataPath))
        {
            applicationDataPath = Path.GetTempPath();
        }

        string expected = Path.Combine(applicationDataPath, "PhotoManager", "Database");

        Assert.That(string.IsNullOrWhiteSpace(result), Is.False);
        Assert.That(result, Is.EqualTo(expected));
    }
}
