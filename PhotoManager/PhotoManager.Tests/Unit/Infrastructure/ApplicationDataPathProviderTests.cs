namespace PhotoManager.Tests.Unit.Infrastructure;

[TestFixture]
public class ApplicationDataPathProviderTests
{
    [Test]
    public void GetPath_ReturnsLocalApplicationDataPath()
    {
        ApplicationDataPathProvider applicationDataPathProvider = new();

        string result = applicationDataPathProvider.GetPath();

        string expected = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        Assert.That(result, Is.EqualTo(expected));
    }
}
