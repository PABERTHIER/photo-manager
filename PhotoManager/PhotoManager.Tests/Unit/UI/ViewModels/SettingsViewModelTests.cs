using PhotoManager.Application;
using System.ComponentModel;

namespace PhotoManager.Tests.Unit.UI.ViewModels;

[TestFixture]
public class SettingsViewModelTests
{
    private IApplication? _application;
    private SettingsViewModel? _settingsViewModel;

    [SetUp]
    public void SetUp()
    {
        _application = Substitute.For<IApplication>();
        _application.GetEditableConfiguration().Returns(CreateEditableConfiguration());
        _settingsViewModel = new(_application);
    }

    [Test]
    public void EditableStringProperties_ChangedAndSameValues_NotifiesOnlyChangedValues()
    {
        (List<string> notifyPropertyChangedEvents, List<SettingsViewModel> settingsViewModelInstances)
            = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _settingsViewModel!.AssetsDirectory = PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets");
        _settingsViewModel.AssetsDirectory = PathHelper.ToPlatformAbsolutePath("D:\\Photos");
        _settingsViewModel.ExemptedFolderPath = PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets\\Exempted");
        _settingsViewModel.ExemptedFolderPath = PathHelper.ToPlatformAbsolutePath("D:\\Photos\\Exempted");
        _settingsViewModel.FirstFrameVideosFolderName = "Frames";
        _settingsViewModel.FirstFrameVideosFolderName = "VideoFrames";
        _settingsViewModel.ThemeMode = "Dark";
        _settingsViewModel.ThemeMode = "Light";

        CheckAfterChanges(
            _settingsViewModel,
            true,
            "Corrupted",
            "Rotated",
            42,
            3,
            999,
            1,
            true,
            true,
            320,
            640,
            6,
            true,
            true,
            true,
            5,
            1,
            10,
            20,
            30,
            25,
            4,
            12,
            PathHelper.ToPlatformAbsolutePath("D:\\Photos"),
            PathHelper.ToPlatformAbsolutePath("D:\\Photos\\Exempted"),
            "VideoFrames",
            "Light",
            string.Empty);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(4));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo(nameof(SettingsViewModel.AssetsDirectory)));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo(nameof(SettingsViewModel.ExemptedFolderPath)));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo(nameof(SettingsViewModel.FirstFrameVideosFolderName)));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo(nameof(SettingsViewModel.ThemeMode)));

        CheckInstance(
            settingsViewModelInstances,
            true,
            "Corrupted",
            "Rotated",
            42,
            3,
            999,
            1,
            true,
            true,
            320,
            640,
            6,
            true,
            true,
            true,
            5,
            1,
            10,
            20,
            30,
            25,
            4,
            12,
            PathHelper.ToPlatformAbsolutePath("D:\\Photos"),
            PathHelper.ToPlatformAbsolutePath("D:\\Photos\\Exempted"),
            "VideoFrames",
            "Light",
            string.Empty);
    }

    [Test]
    public void Save_ValidSettings_SavesConfigurationAndReturnsTrue()
    {
        (List<string> notifyPropertyChangedEvents, List<SettingsViewModel> settingsViewModelInstances)
            = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _settingsViewModel!.AssetsDirectory = PathHelper.ToPlatformAbsolutePath("D:\\Photos");
        _settingsViewModel.ExemptedFolderPath = PathHelper.ToPlatformAbsolutePath("D:\\Photos\\Exempted");
        _settingsViewModel.FirstFrameVideosFolderName = "VideoFrames";
        _settingsViewModel.ThemeMode = "Light";

        bool saved = _settingsViewModel.Save();

        Assert.That(saved, Is.True);

        _application!.Received(1).SaveEditableConfiguration(Arg.Is<EditableUserConfiguration>(configuration =>
            configuration.PathSettings.AssetsDirectory == PathHelper.ToPlatformAbsolutePath("D:\\Photos")
            && configuration.PathSettings.ExemptedFolderPath ==
            PathHelper.ToPlatformAbsolutePath("D:\\Photos\\Exempted")
            && configuration.PathSettings.FirstFrameVideosFolderName == "VideoFrames"
            && configuration.UiSettings.ThemeMode == "Light"));

        CheckAfterChanges(
            _settingsViewModel,
            true,
            "Corrupted",
            "Rotated",
            42,
            3,
            999,
            1,
            true,
            true,
            320,
            640,
            6,
            true,
            true,
            true,
            5,
            1,
            10,
            20,
            30,
            25,
            4,
            12,
            PathHelper.ToPlatformAbsolutePath("D:\\Photos"),
            PathHelper.ToPlatformAbsolutePath("D:\\Photos\\Exempted"),
            "VideoFrames",
            "Light",
            "Settings saved in the database. They now override appsettings.json values.");

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo(nameof(SettingsViewModel.AssetsDirectory)));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo(nameof(SettingsViewModel.ExemptedFolderPath)));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo(nameof(SettingsViewModel.FirstFrameVideosFolderName)));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo(nameof(SettingsViewModel.ThemeMode)));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo(nameof(SettingsViewModel.StatusMessage)));

        CheckInstance(
            settingsViewModelInstances,
            true,
            "Corrupted",
            "Rotated",
            42,
            3,
            999,
            1,
            true,
            true,
            320,
            640,
            6,
            true,
            true,
            true,
            5,
            1,
            10,
            20,
            30,
            25,
            4,
            12,
            PathHelper.ToPlatformAbsolutePath("D:\\Photos"),
            PathHelper.ToPlatformAbsolutePath("D:\\Photos\\Exempted"),
            "VideoFrames",
            "Light",
            "Settings saved in the database. They now override appsettings.json values.");
    }

    [Test]
    [TestCaseSource(nameof(InvalidSettingsCases))]
    public void Save_InvalidSettings_ReturnsFalseAndSetsStatusMessage(Action<SettingsViewModel> updateSettings,
        string expectedStatusMessage)
    {
        (List<string> notifyPropertyChangedEvents, List<SettingsViewModel> _)
            = NotifyPropertyChangedEvents();

        updateSettings(_settingsViewModel!);

        bool saved = _settingsViewModel!.Save();

        Assert.That(saved, Is.False);

        _application!.DidNotReceiveWithAnyArgs().SaveEditableConfiguration(null!);

        Assert.That(_settingsViewModel.StatusMessage, Is.EqualTo(expectedStatusMessage));

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1).Or.Count.EqualTo(2));
    }

    [Test]
    public void Save_InvalidSettingsTwiceWithSameError_NotifiesStatusMessageOnce()
    {
        (List<string> notifyPropertyChangedEvents, List<SettingsViewModel> settingsViewModelInstances)
            = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _settingsViewModel!.AssetsDirectory = string.Empty;

        bool firstSave = _settingsViewModel.Save();
        bool secondSave = _settingsViewModel.Save();

        Assert.That(firstSave, Is.False);
        Assert.That(secondSave, Is.False);

        _application!.DidNotReceiveWithAnyArgs().SaveEditableConfiguration(null!);

        CheckAfterChanges(
            _settingsViewModel,
            true,
            "Corrupted",
            "Rotated",
            42,
            3,
            999,
            1,
            true,
            true,
            320,
            640,
            6,
            true,
            true,
            true,
            5,
            1,
            10,
            20,
            30,
            25,
            4,
            12,
            string.Empty,
            PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets\\Exempted"),
            "Frames",
            "Dark",
            "Assets directory is required.");

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo(nameof(SettingsViewModel.AssetsDirectory)));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo(nameof(SettingsViewModel.StatusMessage)));

        CheckInstance(
            settingsViewModelInstances,
            true,
            "Corrupted",
            "Rotated",
            42,
            3,
            999,
            1,
            true,
            true,
            320,
            640,
            6,
            true,
            true,
            true,
            5,
            1,
            10,
            20,
            30,
            25,
            4,
            12,
            string.Empty,
            PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets\\Exempted"),
            "Frames",
            "Dark",
            "Assets directory is required.");
    }

    private static EditableUserConfiguration CreateEditableConfiguration()
    {
        return new(
            new(true, "Corrupted", "Rotated", 42, 3, 999, 1, true, true, 320, 640),
            new(6, true, true, true),
            new(PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets"),
                PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets\\Exempted"),
                "Frames"),
            new(5, 1, 10, 20, 30, 25),
            new(4, 12),
            new("Dark"));
    }

    private static IEnumerable<TestCaseData> InvalidSettingsCases()
    {
        yield return InvalidSettingsCase(static viewModel => viewModel.AssetsDirectory = string.Empty,
            "Assets directory is required.");
        yield return InvalidSettingsCase(static viewModel => viewModel.ExemptedFolderPath = " ",
            "Exempted folder path is required.");
        yield return InvalidSettingsCase(static viewModel => viewModel.FirstFrameVideosFolderName = "",
            "First frame videos folder name is required.");
        yield return InvalidSettingsCase(static viewModel => viewModel.CatalogBatchSize = 0,
            "Catalog batch size must be greater than 0.");
        yield return InvalidSettingsCase(static viewModel => viewModel.CatalogCooldownMinutes = 0,
            "Catalog cooldown minutes must be greater than 0.");
        yield return InvalidSettingsCase(static viewModel => viewModel.CorruptedImageOrientation = 0,
            "Corrupted image orientation must be greater than 0.");
        yield return InvalidSettingsCase(static viewModel => viewModel.DefaultExifOrientation = 0,
            "Default EXIF orientation must be greater than 0.");
        yield return InvalidSettingsCase(static viewModel => viewModel.ThumbnailMaxHeight = 0,
            "Thumbnail max height must be greater than 0.");
        yield return InvalidSettingsCase(static viewModel => viewModel.ThumbnailMaxWidth = 0,
            "Thumbnail max width must be greater than 0.");
        yield return InvalidSettingsCase(static viewModel => viewModel.ReadConcurrency = 0,
            "Read concurrency must be greater than 0.");
        yield return InvalidSettingsCase(static viewModel => viewModel.ProcessConcurrency = -1,
            "Process concurrency cannot be negative.");
        yield return InvalidSettingsCase(static viewModel => viewModel.ReadBufferSize = 0,
            "Read buffer size must be greater than 0.");
        yield return InvalidSettingsCase(static viewModel => viewModel.ProcessBufferSize = 0,
            "Process buffer size must be greater than 0.");
        yield return InvalidSettingsCase(static viewModel => viewModel.PersistBufferSize = 0,
            "Persist buffer size must be greater than 0.");
        yield return InvalidSettingsCase(static viewModel => viewModel.PersistBatchSize = 0,
            "Persist batch size must be greater than 0.");
        yield return InvalidSettingsCase(static viewModel => viewModel.BackupsToKeep = 0,
            "Backups to keep must be greater than 0.");
        yield return InvalidSettingsCase(static viewModel => viewModel.ThumbnailsDictionaryEntriesToKeep = 0,
            "Thumbnails dictionary entries to keep must be greater than 0.");
        yield return InvalidSettingsCase(static viewModel => viewModel.ThemeMode = "Blue",
            "Theme mode must be System, Light, or Dark.");
    }

    private static TestCaseData InvalidSettingsCase(Action<SettingsViewModel> updateSettings, string statusMessage)
    {
        return new(updateSettings, statusMessage);
    }

    private (List<string> notifyPropertyChangedEvents, List<SettingsViewModel> settingsViewModelInstances)
        NotifyPropertyChangedEvents()
    {
        List<string> notifyPropertyChangedEvents = [];
        List<SettingsViewModel> settingsViewModelInstances = [];
        object eventsLock = new();

        _settingsViewModel!.PropertyChanged += delegate (object? sender, PropertyChangedEventArgs e)
        {
            lock (eventsLock)
            {
                notifyPropertyChangedEvents.Add(e.PropertyName!);
                settingsViewModelInstances.Add((SettingsViewModel)sender!);
            }
        };

        return (notifyPropertyChangedEvents, settingsViewModelInstances);
    }

    private void CheckBeforeChanges()
    {
        string[] expectedThemeModes = ["System", "Light", "Dark"];

        Assert.That(_settingsViewModel!.ThemeModes, Is.EqualTo(expectedThemeModes));
        Assert.That(_settingsViewModel.AnalyseVideos, Is.True);
        Assert.That(_settingsViewModel.CorruptedMessage, Is.EqualTo("Corrupted"));
        Assert.That(_settingsViewModel.RotatedMessage, Is.EqualTo("Rotated"));
        Assert.That(_settingsViewModel.CatalogBatchSize, Is.EqualTo(42));
        Assert.That(_settingsViewModel.CatalogCooldownMinutes, Is.EqualTo(3));
        Assert.That(_settingsViewModel.CorruptedImageOrientation, Is.EqualTo(999));
        Assert.That(_settingsViewModel.DefaultExifOrientation, Is.EqualTo(1));
        Assert.That(_settingsViewModel.DetectThumbnails, Is.True);
        Assert.That(_settingsViewModel.SyncAssetsEveryXMinutes, Is.True);
        Assert.That(_settingsViewModel.ThumbnailMaxHeight, Is.EqualTo(320));
        Assert.That(_settingsViewModel.ThumbnailMaxWidth, Is.EqualTo(640));
        Assert.That(_settingsViewModel.PHashThreshold, Is.EqualTo(6));
        Assert.That(_settingsViewModel.UsingDHash, Is.True);
        Assert.That(_settingsViewModel.UsingMD5Hash, Is.True);
        Assert.That(_settingsViewModel.UsingPHash, Is.True);
        Assert.That(_settingsViewModel.ReadConcurrency, Is.EqualTo(5));
        Assert.That(_settingsViewModel.ProcessConcurrency, Is.EqualTo(1));
        Assert.That(_settingsViewModel.ReadBufferSize, Is.EqualTo(10));
        Assert.That(_settingsViewModel.ProcessBufferSize, Is.EqualTo(20));
        Assert.That(_settingsViewModel.PersistBufferSize, Is.EqualTo(30));
        Assert.That(_settingsViewModel.PersistBatchSize, Is.EqualTo(25));
        Assert.That(_settingsViewModel.BackupsToKeep, Is.EqualTo(4));
        Assert.That(_settingsViewModel.ThumbnailsDictionaryEntriesToKeep, Is.EqualTo(12));
        Assert.That(_settingsViewModel.AssetsDirectory, Is.EqualTo(
            PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets")));
        Assert.That(_settingsViewModel.ExemptedFolderPath, Is.EqualTo(
            PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets\\Exempted")));
        Assert.That(_settingsViewModel.FirstFrameVideosFolderName, Is.EqualTo("Frames"));
        Assert.That(_settingsViewModel.ThemeMode, Is.EqualTo("Dark"));
        Assert.That(_settingsViewModel.StatusMessage, Is.EqualTo(string.Empty));
    }

    private static void CheckAfterChanges(
        SettingsViewModel settingsViewModelInstance,
        bool analyseVideos,
        string corruptedMessage,
        string rotatedMessage,
        int catalogBatchSize,
        ushort catalogCooldownMinutes,
        ushort corruptedImageOrientation,
        ushort defaultExifOrientation,
        bool detectThumbnails,
        bool syncAssetsEveryXMinutes,
        int thumbnailMaxHeight,
        int thumbnailMaxWidth,
        ushort pHashThreshold,
        bool usingDHash,
        bool usingMD5Hash,
        bool usingPHash,
        int readConcurrency,
        int processConcurrency,
        int readBufferSize,
        int processBufferSize,
        int persistBufferSize,
        int persistBatchSize,
        ushort backupsToKeep,
        ushort thumbnailsDictionaryEntriesToKeep,
        string assetsDirectory,
        string exemptedFolderPath,
        string firstFrameVideosFolderName,
        string themeMode,
        string statusMessage)
    {
        string[] expectedThemeModes = ["System", "Light", "Dark"];

        Assert.That(settingsViewModelInstance.ThemeModes, Is.EqualTo(expectedThemeModes));
        Assert.That(settingsViewModelInstance.AnalyseVideos, Is.EqualTo(analyseVideos));
        Assert.That(settingsViewModelInstance.CorruptedMessage, Is.EqualTo(corruptedMessage));
        Assert.That(settingsViewModelInstance.RotatedMessage, Is.EqualTo(rotatedMessage));
        Assert.That(settingsViewModelInstance.CatalogBatchSize, Is.EqualTo(catalogBatchSize));
        Assert.That(settingsViewModelInstance.CatalogCooldownMinutes, Is.EqualTo(catalogCooldownMinutes));
        Assert.That(settingsViewModelInstance.CorruptedImageOrientation, Is.EqualTo(corruptedImageOrientation));
        Assert.That(settingsViewModelInstance.DefaultExifOrientation, Is.EqualTo(defaultExifOrientation));
        Assert.That(settingsViewModelInstance.DetectThumbnails, Is.EqualTo(detectThumbnails));
        Assert.That(settingsViewModelInstance.SyncAssetsEveryXMinutes, Is.EqualTo(syncAssetsEveryXMinutes));
        Assert.That(settingsViewModelInstance.ThumbnailMaxHeight, Is.EqualTo(thumbnailMaxHeight));
        Assert.That(settingsViewModelInstance.ThumbnailMaxWidth, Is.EqualTo(thumbnailMaxWidth));
        Assert.That(settingsViewModelInstance.PHashThreshold, Is.EqualTo(pHashThreshold));
        Assert.That(settingsViewModelInstance.UsingDHash, Is.EqualTo(usingDHash));
        Assert.That(settingsViewModelInstance.UsingMD5Hash, Is.EqualTo(usingMD5Hash));
        Assert.That(settingsViewModelInstance.UsingPHash, Is.EqualTo(usingPHash));
        Assert.That(settingsViewModelInstance.ReadConcurrency, Is.EqualTo(readConcurrency));
        Assert.That(settingsViewModelInstance.ProcessConcurrency, Is.EqualTo(processConcurrency));
        Assert.That(settingsViewModelInstance.ReadBufferSize, Is.EqualTo(readBufferSize));
        Assert.That(settingsViewModelInstance.ProcessBufferSize, Is.EqualTo(processBufferSize));
        Assert.That(settingsViewModelInstance.PersistBufferSize, Is.EqualTo(persistBufferSize));
        Assert.That(settingsViewModelInstance.PersistBatchSize, Is.EqualTo(persistBatchSize));
        Assert.That(settingsViewModelInstance.BackupsToKeep, Is.EqualTo(backupsToKeep));
        Assert.That(settingsViewModelInstance.ThumbnailsDictionaryEntriesToKeep, Is.EqualTo(
            thumbnailsDictionaryEntriesToKeep));
        Assert.That(settingsViewModelInstance.AssetsDirectory,
            Is.EqualTo(PathHelper.ToPlatformAbsolutePath(assetsDirectory)));
        Assert.That(settingsViewModelInstance.ExemptedFolderPath,
            Is.EqualTo(PathHelper.ToPlatformAbsolutePath(exemptedFolderPath)));
        Assert.That(settingsViewModelInstance.FirstFrameVideosFolderName, Is.EqualTo(firstFrameVideosFolderName));
        Assert.That(settingsViewModelInstance.ThemeMode, Is.EqualTo(themeMode));
        Assert.That(settingsViewModelInstance.StatusMessage, Is.EqualTo(statusMessage));
    }

    private static void CheckInstance(
        List<SettingsViewModel> settingsViewModelInstances,
        bool analyseVideos,
        string corruptedMessage,
        string rotatedMessage,
        int catalogBatchSize,
        ushort catalogCooldownMinutes,
        ushort corruptedImageOrientation,
        ushort defaultExifOrientation,
        bool detectThumbnails,
        bool syncAssetsEveryXMinutes,
        int thumbnailMaxHeight,
        int thumbnailMaxWidth,
        ushort pHashThreshold,
        bool usingDHash,
        bool usingMD5Hash,
        bool usingPHash,
        int readConcurrency,
        int processConcurrency,
        int readBufferSize,
        int processBufferSize,
        int persistBufferSize,
        int persistBatchSize,
        ushort backupsToKeep,
        ushort thumbnailsDictionaryEntriesToKeep,
        string assetsDirectory,
        string exemptedFolderPath,
        string firstFrameVideosFolderName,
        string themeMode,
        string statusMessage)
    {
        int settingsViewModelInstancesCount = settingsViewModelInstances.Count;

        if (settingsViewModelInstancesCount > 1)
        {
            Assert.That(settingsViewModelInstances[settingsViewModelInstancesCount - 2],
                Is.EqualTo(settingsViewModelInstances[0]));
            // No need to go deeper, same instance because ref updated each time
            Assert.That(settingsViewModelInstances[settingsViewModelInstancesCount - 1],
                Is.EqualTo(settingsViewModelInstances[settingsViewModelInstancesCount - 2]));
        }

        if (settingsViewModelInstancesCount > 0)
        {
            CheckAfterChanges(settingsViewModelInstances[0],
                analyseVideos,
                corruptedMessage,
                rotatedMessage,
                catalogBatchSize,
                catalogCooldownMinutes,
                corruptedImageOrientation,
                defaultExifOrientation,
                detectThumbnails,
                syncAssetsEveryXMinutes,
                thumbnailMaxHeight,
                thumbnailMaxWidth,
                pHashThreshold,
                usingDHash,
                usingMD5Hash,
                usingPHash,
                readConcurrency,
                processConcurrency,
                readBufferSize,
                processBufferSize,
                persistBufferSize,
                persistBatchSize,
                backupsToKeep,
                thumbnailsDictionaryEntriesToKeep,
                assetsDirectory,
                exemptedFolderPath,
                firstFrameVideosFolderName,
                themeMode,
                statusMessage);
        }
    }
}
