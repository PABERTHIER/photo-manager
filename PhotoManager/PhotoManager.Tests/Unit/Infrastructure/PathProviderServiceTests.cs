namespace PhotoManager.Tests.Unit.Infrastructure;

[TestFixture]
public class PathProviderServiceTests
{
    [Test]
    public void ResolveDatabaseDirectory_ReturnsPathRelativeToBaseDirectory()
    {
        PathProviderService pathProviderService = new();

        string result = pathProviderService.ResolveDatabaseDirectory();

        string expected = Path.Combine(AppContext.BaseDirectory, "Database");
        Assert.That(string.IsNullOrWhiteSpace(result), Is.False);
        Assert.That(result, Is.EqualTo(expected));
    }
}
