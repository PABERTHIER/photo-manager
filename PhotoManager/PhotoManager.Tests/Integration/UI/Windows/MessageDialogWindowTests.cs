using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PhotoManager.UI.Windows;

namespace PhotoManager.Tests.Integration.UI.Windows;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public class MessageDialogWindowTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        AvaloniaTestSetup.EnsureInitialized();
    }

    [Test]
    [TestCase("No duplicates have been found.", "Information")]
    [TestCase("The operation failed.", "Conversion failed")]
    [TestCase("Folder selection failed because access is denied.", "Folder selection failed")]
    public Task Constructor_MessageAndTitle_SetsTitleMessageAndFixedSize(string message, string title)
    {
        return AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            MessageDialogWindow? window = null;

            try
            {
                window = new(message, title);

                TextBlock messageTextBlock = window.FindControl<TextBlock>("MessageTextBlock")
                    ?? throw new InvalidOperationException("MessageTextBlock was not found.");

                Assert.That(messageTextBlock.Text, Is.EqualTo(message));

                Assert.That(window.Title, Is.EqualTo(title));
                Assert.That(window.Width, Is.EqualTo(420));
                Assert.That(window.Height, Is.EqualTo(180));
                Assert.That(window.CanResize, Is.False);
            }
            finally
            {
                window?.Close();
            }
        });
    }

    [Test]
    public Task OkButton_Click_ClosesWindow()
    {
        return AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            MessageDialogWindow window = new("Done.", "Information");
            bool closed = false;
            window.Closed += (_, _) => closed = true;

            try
            {
                window.Show();

                Button okButton = window.FindControl<Button>("OkButton")
                    ?? throw new InvalidOperationException("OkButton was not found.");

                // Triggers the real OkButton_Click handler wired in XAML, which closes the window.
                okButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Dispatcher.UIThread.RunJobs();

                Assert.That(closed, Is.True);
            }
            finally
            {
                if (!closed)
                {
                    window.Close();
                }
            }
        });
    }
}
