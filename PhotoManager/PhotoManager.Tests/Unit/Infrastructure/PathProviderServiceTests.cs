namespace PhotoManager.Tests.Unit.Infrastructure;

[TestFixture]
public class PathProviderServiceTests
{
    [Test]
    public void ResolveDatabaseDirectory_ReturnsPathInLocalApplicationData()
    {
        ApplicationDataPathProvider applicationDataPathProvider = new();
        PathProviderService pathProviderService = new(applicationDataPathProvider);   

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

    [Test]
    public void ResolveDatabaseDirectory_ApplicationDataPathUnavailable_UsesTemporaryPath()
    {
        IApplicationDataPathProvider applicationDataPathProvider = Substitute.For<IApplicationDataPathProvider>();
        applicationDataPathProvider.GetPath().Returns(string.Empty);

        PathProviderService pathProviderService = new(applicationDataPathProvider);

        string result = pathProviderService.ResolveDatabaseDirectory();

        string expected = Path.Combine(Path.GetTempPath(), "PhotoManager", "Database");

        Assert.That(result, Is.EqualTo(expected));
    }
}
