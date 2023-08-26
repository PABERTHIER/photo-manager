namespace PhotoManager.UI.Tests.Unit;

public class TestFixture : IDisposable
{
    public TestFixture()
    {
        // Use _fixture to access data and resources
        Folder folder = new() { FolderId = "1", Path = "C:\\Inexistent Folder" };
    }

    public void Dispose()
    {
        // Clean up data and resources here
    }
}
