using PhotoManager.Application;
using System.ComponentModel;

namespace PhotoManager.Tests.Unit.UI.ViewModels;

[TestFixture]
[NonParallelizable]
public class ConvertAssetsViewModelTests
{
    private IApplication? _application;
    private ConvertAssetsViewModel? _convertAssetsViewModel;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        AvaloniaTestSetup.EnsureInitialized();
    }

    [SetUp]
    public void SetUp()
    {
        _application = Substitute.For<IApplication>();
        _convertAssetsViewModel = new(_application);
    }

    [Test]
    public void SetSelectedFilePaths_SelectedFiles_NotifiesAndUpdatesSelection()
    {
        string[] expectedFilePaths = [@"C:\Photos\Image.png", @"C:\Photos\Image.jpg"];

        (List<string> notifyPropertyChangedEvents, List<ConvertAssetsViewModel> convertAssetsViewModelInstances)
            = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _convertAssetsViewModel!.SetSelectedFilePaths(expectedFilePaths);

        CheckAfterChanges(
            _convertAssetsViewModel,
            expectedFilePaths,
            "JPG",
            false,
            0d,
            string.Empty,
            "2 image(s) selected.",
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo(nameof(ConvertAssetsViewModel.SelectedFilePaths)));

        CheckInstance(
            convertAssetsViewModelInstances,
            expectedFilePaths,
            "JPG",
            false,
            0d,
            string.Empty,
            "2 image(s) selected.",
            true);
    }

    [Test]
    public void SelectedTargetFormat_ChangedAndSameValue_NotifiesOnlyChangedValue()
    {
        (List<string> notifyPropertyChangedEvents, List<ConvertAssetsViewModel> convertAssetsViewModelInstances)
            = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _convertAssetsViewModel!.SelectedTargetFormat = "JPG";
        _convertAssetsViewModel.SelectedTargetFormat = "PNG";

        CheckAfterChanges(
            _convertAssetsViewModel,
            [],
            "PNG",
            false,
            0d,
            string.Empty,
            string.Empty,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo(nameof(ConvertAssetsViewModel.SelectedTargetFormat)));

        CheckInstance(
            convertAssetsViewModelInstances,
            [],
            "PNG",
            false,
            0d,
            string.Empty,
            string.Empty,
            false);
    }

    [Test]
    public void SetSelectedFilePaths_SameImageCountTwice_NotifiesSummaryMessageOnce()
    {
        (List<string> notifyPropertyChangedEvents, List<ConvertAssetsViewModel> convertAssetsViewModelInstances)
            = NotifyPropertyChangedEvents();

        string[] expectedFilePaths1 = [@"C:\Photos\Image1.png"];
        string[] expectedFilePaths2 = [@"C:\Photos\Image2.png"];

        CheckBeforeChanges();

        _convertAssetsViewModel!.SetSelectedFilePaths(expectedFilePaths1);
        _convertAssetsViewModel.SetSelectedFilePaths(expectedFilePaths2);

        CheckAfterChanges(
            _convertAssetsViewModel,
            expectedFilePaths2,
            "JPG",
            false,
            0d,
            string.Empty,
            "1 image(s) selected.",
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo(nameof(ConvertAssetsViewModel.SelectedFilePaths)));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo(nameof(ConvertAssetsViewModel.SelectedFilePaths)));

        CheckInstance(
            convertAssetsViewModelInstances,
            expectedFilePaths2,
            "JPG",
            false,
            0d,
            string.Empty,
            "1 image(s) selected.",
            true);
    }

    [Test]
    public async Task ConvertAsync_NoSelectedFiles_ReturnsNullAndDoesNotConvert()
    {
        (List<string> notifyPropertyChangedEvents, List<ConvertAssetsViewModel> convertAssetsViewModelInstances)
            = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        AssetConversionResult? result = await _convertAssetsViewModel!.ConvertAsync();

        Assert.That(result, Is.Null);

        await _application!.DidNotReceiveWithAnyArgs().ConvertAssetsAsync(null!, null!, CancellationToken.None);

        CheckAfterChanges(
            _convertAssetsViewModel,
            [],
            "JPG",
            false,
            0d,
            string.Empty,
            "Select at least one image before converting.",
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));

        CheckInstance(
            convertAssetsViewModelInstances,
            [],
            "JPG",
            false,
            0d,
            string.Empty,
            "Select at least one image before converting.",
            false);
    }

    [Test]
    public async Task ConvertAsync_SelectedFiles_ConvertsAndUpdatesProgressFromUiThread()
    {
        string[] expectedFilePaths = [@"C:\Photos\Image.png", @"C:\Photos\Image.gif"];

        (List<string> notifyPropertyChangedEvents, List<ConvertAssetsViewModel> convertAssetsViewModelInstances)
            = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _convertAssetsViewModel!.SetSelectedFilePaths(expectedFilePaths);

        AssetConversionResult conversionResult = new(
        [
            new(@"C:\Photos\Image.png", @"C:\Photos\Converted\Image.jpg", true, "Image.png converted."),
            new(@"C:\Photos\Image.gif", null, false, "Image.gif cannot be converted to .jpg.")
        ]);
        AssetConversionRequest? capturedRequest = null;

        _application!.ConvertAssetsAsync(
                Arg.Any<AssetConversionRequest>(),
                Arg.Any<AssetConversionProgressCallback>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(() =>
            {
                capturedRequest = callInfo.ArgAt<AssetConversionRequest>(0);
                AssetConversionProgressCallback callback = callInfo.ArgAt<AssetConversionProgressCallback>(1);
                callback(new()
                {
                    CompletedCount = 1,
                    TotalCount = 2,
                    Message = "Image.png converted."
                });
                callback(new()
                {
                    CompletedCount = 2,
                    TotalCount = 2,
                    Message = "Conversion finished."
                });

                return conversionResult;
            }));

        AssetConversionResult? result =
            await AvaloniaTestSetup.RunOnUiThreadAsync(() => _convertAssetsViewModel.ConvertAsync());

        Assert.That(result, Is.SameAs(conversionResult));

        Assert.That(capturedRequest, Is.Not.Null);
        Assert.That(capturedRequest!.TargetFormat, Is.EqualTo(AssetConversionTargetFormat.Jpeg));
        Assert.That(capturedRequest.FilePaths, Is.EqualTo(expectedFilePaths));

        CheckAfterChanges(
            _convertAssetsViewModel,
            expectedFilePaths,
            "JPG",
            false,
            100d,
            "Conversion finished.",
            "Converted: 1. Failed: 1.",
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(14));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo(nameof(ConvertAssetsViewModel.SelectedFilePaths)));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo(nameof(ConvertAssetsViewModel.IsConverting)));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressPercentage)));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressPercentage)));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo(nameof(ConvertAssetsViewModel.IsConverting)));
        Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));

        CheckInstance(
            convertAssetsViewModelInstances,
            expectedFilePaths,
            "JPG",
            false,
            100d,
            "Conversion finished.",
            "Converted: 1. Failed: 1.",
            true);
    }

    [Test]
    public async Task ConvertAsync_SelectedPngTarget_UsesPngTargetFormat()
    {
        string[] expectedFilePaths = [@"C:\Photos\Image.jpg"];

        (List<string> notifyPropertyChangedEvents, List<ConvertAssetsViewModel> convertAssetsViewModelInstances)
            = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _convertAssetsViewModel!.SetSelectedFilePaths(expectedFilePaths);
        _convertAssetsViewModel.SelectedTargetFormat = "PNG";

        AssetConversionResult conversionResult =
            new([new(@"C:\Photos\Image.jpg", @"C:\Photos\Converted\Image.png", true, "Image.jpg converted.")]);
        AssetConversionRequest? capturedRequest = null;

        _application!.ConvertAssetsAsync(
                Arg.Any<AssetConversionRequest>(),
                Arg.Any<AssetConversionProgressCallback>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                capturedRequest = callInfo.ArgAt<AssetConversionRequest>(0);
                return Task.FromResult(conversionResult);
            });

        AssetConversionResult? result = await _convertAssetsViewModel.ConvertAsync();

        Assert.That(result, Is.SameAs(conversionResult));

        Assert.That(capturedRequest!.TargetFormat, Is.EqualTo(AssetConversionTargetFormat.Png));
        Assert.That(capturedRequest.FilePaths, Is.EqualTo(expectedFilePaths));

        // Because it's a unit test and _application is mocked, the ProgressPercentage and ProgressMessage are not updated
        CheckAfterChanges(
            _convertAssetsViewModel,
            expectedFilePaths,
            "PNG",
            false,
            0d,
            "Starting conversion...",
            "Converted: 1. Failed: 0.",
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(11));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo(nameof(ConvertAssetsViewModel.SelectedFilePaths)));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo(nameof(ConvertAssetsViewModel.SelectedTargetFormat)));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo(nameof(ConvertAssetsViewModel.IsConverting)));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo(nameof(ConvertAssetsViewModel.IsConverting)));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));

        CheckInstance(
            convertAssetsViewModelInstances,
            expectedFilePaths,
            "PNG",
            false,
            0d,
            "Starting conversion...",
            "Converted: 1. Failed: 0.",
            true);
    }

    [Test]
    public void ConvertAsync_ApplicationThrows_ResetsConvertingState()
    {
        string[] expectedFilePaths = [@"C:\Photos\Image.jpg"];

        (List<string> notifyPropertyChangedEvents, List<ConvertAssetsViewModel> convertAssetsViewModelInstances)
            = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _convertAssetsViewModel!.SetSelectedFilePaths(expectedFilePaths);
        _application!.ConvertAssetsAsync(
                Arg.Any<AssetConversionRequest>(),
                Arg.Any<AssetConversionProgressCallback>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Conversion failed."));

        InvalidOperationException? exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _convertAssetsViewModel.ConvertAsync());

        Assert.That(exception?.Message, Is.EqualTo("Conversion failed."));

        // Because it's a unit test and _application is mocked, the ProgressPercentage and ProgressMessage are not updated
        CheckAfterChanges(
            _convertAssetsViewModel,
            expectedFilePaths,
            "JPG",
            false,
            0d,
            "Starting conversion...",
            string.Empty,
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(9));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo(nameof(ConvertAssetsViewModel.SelectedFilePaths)));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo(nameof(ConvertAssetsViewModel.IsConverting)));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo(nameof(ConvertAssetsViewModel.IsConverting)));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));

        CheckInstance(
            convertAssetsViewModelInstances,
            expectedFilePaths,
            "JPG",
            false,
            0d,
            "Starting conversion...",
            string.Empty,
            true);
    }

    [Test]
    public async Task ConvertAsync_RepeatedProgressMessage_NotifiesProgressMessageOnce()
    {
        string[] expectedFilePaths = [@"C:\Photos\Image.png"];

        (List<string> notifyPropertyChangedEvents, List<ConvertAssetsViewModel> convertAssetsViewModelInstances)
            = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _convertAssetsViewModel!.SetSelectedFilePaths(expectedFilePaths);

        AssetConversionResult conversionResult =
            new([new(@"C:\Photos\Image.png", @"C:\Photos\Converted\Image.jpg", true, "Image.png converted.")]);

        _application!.ConvertAssetsAsync(
                Arg.Any<AssetConversionRequest>(),
                Arg.Any<AssetConversionProgressCallback>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(() =>
            {
                AssetConversionProgressCallback callback = callInfo.ArgAt<AssetConversionProgressCallback>(1);
                callback(new()
                {
                    CompletedCount = 1,
                    TotalCount = 2,
                    Message = "Converting images..."
                });
                callback(new()
                {
                    CompletedCount = 2,
                    TotalCount = 2,
                    Message = "Converting images..."
                });

                return conversionResult;
            }));

        await AvaloniaTestSetup.RunOnUiThreadAsync(() => _convertAssetsViewModel.ConvertAsync());

        // Because it's a unit test and _application is mocked, the ProgressPercentage and ProgressMessage are not updated
        CheckAfterChanges(
            _convertAssetsViewModel,
            expectedFilePaths,
            "JPG",
            false,
            100d,
            "Converting images...",
            "Converted: 1. Failed: 0.",
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(13));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo(nameof(ConvertAssetsViewModel.SelectedFilePaths)));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo(nameof(ConvertAssetsViewModel.IsConverting)));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressPercentage)));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressPercentage)));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo(nameof(ConvertAssetsViewModel.IsConverting)));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));

        CheckInstance(
            convertAssetsViewModelInstances,
            expectedFilePaths,
            "JPG",
            false,
            100d,
            "Converting images...",
            "Converted: 1. Failed: 0.",
            true);
    }

    [Test]
    public async Task ConvertAsync_ReentrantWhileConverting_NotifiesIsConvertingOncePerTransition()
    {
        string[] expectedFilePaths = [@"C:\Photos\Image.png"];

        (List<string> notifyPropertyChangedEvents, List<ConvertAssetsViewModel> convertAssetsViewModelInstances)
            = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _convertAssetsViewModel!.SetSelectedFilePaths(expectedFilePaths);

        AssetConversionResult conversionResult =
            new([new(@"C:\Photos\Image.png", @"C:\Photos\Converted\Image.jpg", true, "Image.png converted.")]);

        int applicationCallCount = 0;

        _application!.ConvertAssetsAsync(
                Arg.Any<AssetConversionRequest>(),
                Arg.Any<AssetConversionProgressCallback>(),
                Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                applicationCallCount++;

                if (applicationCallCount == 1)
                {
                    // Re-enter while the first conversion is still in progress: IsConverting is already true,
                    // so the nested set (and the outer finally's redundant reset) hit the equality guard.
                    _convertAssetsViewModel.ConvertAsync().GetAwaiter().GetResult();
                }

                return Task.FromResult(conversionResult);
            });

        await _convertAssetsViewModel.ConvertAsync();

        Assert.That(applicationCallCount, Is.EqualTo(2));

        // Because it's a unit test and _application is mocked, the ProgressPercentage and ProgressMessage are not updated
        CheckAfterChanges(
            _convertAssetsViewModel,
            expectedFilePaths,
            "JPG",
            false,
            0d,
            "Starting conversion...",
            "Converted: 1. Failed: 0.",
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(10));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo(nameof(ConvertAssetsViewModel.SelectedFilePaths)));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo(nameof(ConvertAssetsViewModel.IsConverting)));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo(nameof(ConvertAssetsViewModel.IsConverting)));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));

        CheckInstance(
            convertAssetsViewModelInstances,
            expectedFilePaths,
            "JPG",
            false,
            0d,
            "Starting conversion...",
            "Converted: 1. Failed: 0.",
            true);
    }

    private (List<string> notifyPropertyChangedEvents, List<ConvertAssetsViewModel> convertAssetsViewModelInstances)
        NotifyPropertyChangedEvents()
    {
        List<string> notifyPropertyChangedEvents = [];
        List<ConvertAssetsViewModel> convertAssetsViewModelInstances = [];
        object eventsLock = new();

        _convertAssetsViewModel!.PropertyChanged += delegate (object? sender, PropertyChangedEventArgs e)
        {
            lock (eventsLock)
            {
                notifyPropertyChangedEvents.Add(e.PropertyName!);
                convertAssetsViewModelInstances.Add((ConvertAssetsViewModel)sender!);
            }
        };

        return (notifyPropertyChangedEvents, convertAssetsViewModelInstances);
    }

    private void CheckBeforeChanges()
    {
        string[] expectedTargetFormats = ["JPG", "PNG"];

        Assert.That(_convertAssetsViewModel!.SelectedFilePaths, Is.Empty);
        Assert.That(_convertAssetsViewModel.TargetFormats, Is.EqualTo(expectedTargetFormats));
        Assert.That(_convertAssetsViewModel.SelectedTargetFormat, Is.EqualTo("JPG"));
        Assert.That(_convertAssetsViewModel.IsConverting, Is.False);
        Assert.That(_convertAssetsViewModel.ProgressPercentage, Is.Zero);
        Assert.That(_convertAssetsViewModel.ProgressMessage, Is.EqualTo(string.Empty));
        Assert.That(_convertAssetsViewModel.SummaryMessage, Is.EqualTo(string.Empty));
        Assert.That(_convertAssetsViewModel.CanConvert, Is.False);
    }

    private static void CheckAfterChanges(
        ConvertAssetsViewModel convertAssetsViewModelInstance,
        string[] selectedFilePaths,
        string selectedTargetFormat,
        bool isConverting,
        double progressPercentage,
        string progressMessage,
        string summaryMessage,
        bool canConvert)
    {
        string[] expectedTargetFormats = ["JPG", "PNG"];

        Assert.That(convertAssetsViewModelInstance.SelectedFilePaths, Is.EqualTo(selectedFilePaths));
        Assert.That(convertAssetsViewModelInstance.TargetFormats, Is.EqualTo(expectedTargetFormats));
        Assert.That(convertAssetsViewModelInstance.SelectedTargetFormat, Is.EqualTo(selectedTargetFormat));
        Assert.That(convertAssetsViewModelInstance.IsConverting, Is.EqualTo(isConverting));
        Assert.That(convertAssetsViewModelInstance.ProgressPercentage, Is.EqualTo(progressPercentage));
        Assert.That(convertAssetsViewModelInstance.ProgressMessage, Is.EqualTo(progressMessage));
        Assert.That(convertAssetsViewModelInstance.SummaryMessage, Is.EqualTo(summaryMessage));
        Assert.That(convertAssetsViewModelInstance.CanConvert, Is.EqualTo(canConvert));
    }

    private static void CheckInstance(
        List<ConvertAssetsViewModel> convertAssetsViewModelInstances,
        string[] selectedFilePaths,
        string selectedTargetFormat,
        bool isConverting,
        double progressPercentage,
        string progressMessage,
        string summaryMessage,
        bool canConvert)
    {
        int convertAssetsViewModelInstancesCount = convertAssetsViewModelInstances.Count;

        if (convertAssetsViewModelInstancesCount > 1)
        {
            Assert.That(convertAssetsViewModelInstances[convertAssetsViewModelInstancesCount - 2],
                Is.EqualTo(convertAssetsViewModelInstances[0]));
            // No need to go deeper, same instance because ref updated each time
            Assert.That(convertAssetsViewModelInstances[convertAssetsViewModelInstancesCount - 1],
                Is.EqualTo(convertAssetsViewModelInstances[convertAssetsViewModelInstancesCount - 2]));
        }

        if (convertAssetsViewModelInstancesCount > 0)
        {
            CheckAfterChanges(
                convertAssetsViewModelInstances[0],
                selectedFilePaths,
                selectedTargetFormat,
                isConverting,
                progressPercentage,
                progressMessage,
                summaryMessage,
                canConvert);
        }
    }
}
