using Microsoft.Extensions.Logging.Abstractions;
using PhotoManager.UI.Windows;

namespace PhotoManager.Tests.Integration.UI.Windows;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public class ShortcutsWindowTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        AvaloniaTestSetup.EnsureInitialized();
    }

    [Test]
    public async Task Constructor_ShortcutsViewModel_SetsMediumWindowAndDataContext()
    {
        await AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            ShortcutsWindow? window = null;

            try
            {
                ShortcutsViewModel viewModel = new();
                window = new(viewModel, NullLogger<ShortcutsWindow>.Instance);

                Assert.That(window.Title, Is.EqualTo("Shortcuts"));
                Assert.That(window.Width, Is.EqualTo(860));
                Assert.That(window.Height, Is.EqualTo(600));
                Assert.That(window.DataContext, Is.SameAs(viewModel));
            }
            finally
            {
                window?.Close();
            }
        });
    }
}
