namespace PhotoManager.Tests.Integration.Domain;

[TestFixture]
public class CatalogFolderPipelineTests
{
    private const int FileCount = 200;

    [Test]
    [TestCase(0, 1)]
    [TestCase(1, 0)]
    public async Task CatalogAsync_NoFilesOrSuccessfulAssetLimit_ReturnsZero(int maxSuccessfulAssets, int fileNameCount)
    {
        CatalogFolderPipeline pipeline = CreatePipeline();
        string[] fileNames = [.. Enumerable.Range(0, fileNameCount).Select(index => $"image-{index}.jpg")];

        int cataloguedCount = await pipeline.CatalogAsync(
                TestContext.CurrentContext.TestDirectory,
                fileNames,
                false,
                maxSuccessfulAssets,
                CatalogAssetPipelineOperation.Create,
                new(),
                (_, _) => ValueTask.FromResult(true),
                CancellationToken.None)
            .WaitAsync(TimeSpan.FromSeconds(5));

        Assert.That(cataloguedCount, Is.Zero);
    }

    [Test]
    public void CatalogAsync_CallbackThrows_PropagatesExceptionWithoutDeadlock()
    {
        CatalogFolderPipeline pipeline = CreatePipeline();
        string directory = TestContext.CurrentContext.TestDirectory;
        string[] fileNames = CreateFileNames();
        InvalidOperationException expectedException = new("Callback failed.");

        InvalidOperationException? exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await pipeline.CatalogAsync(
                directory,
                fileNames,
                false,
                fileNames.Length,
                CatalogAssetPipelineOperation.Create,
                new(),
                (_, _) => throw expectedException,
                CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(5)));

        Assert.That(exception, Is.SameAs(expectedException));
    }

    [Test]
    public void CatalogAsync_PersistenceThrows_PropagatesExceptionWithoutDeadlock()
    {
        InvalidOperationException expectedException = new("Persistence failed.");
        CatalogFolderPipeline pipeline = CreatePipeline(expectedException);
        string directory = TestContext.CurrentContext.TestDirectory;
        string[] fileNames = CreateFileNames();

        InvalidOperationException? exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await pipeline.CatalogAsync(
                directory,
                fileNames,
                false,
                fileNames.Length,
                CatalogAssetPipelineOperation.Create,
                new(),
                (_, _) => ValueTask.FromResult(true),
                CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(5)));

        Assert.That(exception, Is.SameAs(expectedException));
    }

    [Test]
    public void CatalogAsync_FileReadCanceled_PropagatesOperationCanceledExceptionWithoutDeadlock()
    {
        // An OperationCanceledException is not reported as a failed item: it escapes the per-file handler,
        // faults the whole read stage and completes the channel with the error
        OperationCanceledException expectedException = new("Read canceled.");
        CatalogFolderPipeline pipeline = CreatePipeline(fileBytesFactory: _ => throw expectedException);
        List<CatalogPipelineResult> results = [];
        string directory = TestContext.CurrentContext.TestDirectory;

        OperationCanceledException? exception = Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await pipeline.CatalogAsync(
                directory,
                ["first.jpg"],
                false,
                1,
                CatalogAssetPipelineOperation.Create,
                new(),
                (result, _) =>
                {
                    results.Add(result);

                    return ValueTask.FromResult(true);
                },
                CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(5)));

        Assert.That(exception, Is.SameAs(expectedException));
        Assert.That(results, Is.Empty);
    }

    [Test]
    public async Task CatalogAsync_CorruptFileBeforeSuccessLimit_AttemptsLaterValidFile()
    {
        CatalogFolderPipeline pipeline = CreatePipeline(assetFactory: (directory, fileName) =>
            fileName == "corrupt.jpg" ? null : new(CreateAsset(directory, fileName), [1, 2, 3]));
        List<CatalogPipelineResult> results = [];
        string directory = TestContext.CurrentContext.TestDirectory;

        int cataloguedCount = await pipeline.CatalogAsync(
                directory,
                ["corrupt.jpg", "valid.jpg"],
                false,
                1,
                CatalogAssetPipelineOperation.Create,
                new(),
                (result, _) =>
                {
                    results.Add(result);

                    return ValueTask.FromResult(result.Asset != null);
                },
                CancellationToken.None)
            .WaitAsync(TimeSpan.FromSeconds(5));

        Assert.That(cataloguedCount, Is.EqualTo(1));
        Assert.That(results, Has.Count.EqualTo(2));
        Assert.That(results[0].FileName, Is.EqualTo("corrupt.jpg"));
        Assert.That(results[0].Asset, Is.Null);
        Assert.That(results[1].FileName, Is.EqualTo("valid.jpg"));
        Assert.That(results[1].Asset, Is.Not.Null);
    }

    [Test]
    public async Task CatalogAsync_FileReadThrows_ReportsNullAssetAndAttemptsLaterValidFile()
    {
        IOException expectedException = new("Read failed.");
        CatalogFolderPipeline pipeline = CreatePipeline(fileBytesFactory: path =>
            Path.GetFileName(path) == "broken.jpg" ? throw expectedException : [1, 2, 3]);
        List<CatalogPipelineResult> results = [];
        string directory = TestContext.CurrentContext.TestDirectory;

        int cataloguedCount = await pipeline.CatalogAsync(
                directory,
                ["broken.jpg", "valid.jpg"],
                false,
                1,
                CatalogAssetPipelineOperation.Create,
                new(),
                (result, _) =>
                {
                    results.Add(result);

                    return ValueTask.FromResult(result.Asset != null);
                },
                CancellationToken.None)
            .WaitAsync(TimeSpan.FromSeconds(5));

        Assert.That(cataloguedCount, Is.EqualTo(1));
        Assert.That(results, Has.Count.EqualTo(2));
        Assert.That(results[0].FileName, Is.EqualTo("broken.jpg"));
        Assert.That(results[0].Asset, Is.Null);
        Assert.That(results[1].FileName, Is.EqualTo("valid.jpg"));
        Assert.That(results[1].Asset, Is.Not.Null);
    }

    [Test]
    public void EffectiveProcessConcurrency_ProcessConcurrencyIsPositive_ReturnsConfiguredValue()
    {
        CatalogPipelineOptions options = new(ProcessConcurrency: 2);

        Assert.That(options.EffectiveProcessConcurrency, Is.EqualTo(2));
    }

    [Test]
    public void CatalogPipelineResult_IsVideoProvided_ReturnsValue()
    {
        CatalogPipelineResult result = new(
            0,
            TestContext.CurrentContext.TestDirectory,
            "video.mp4",
            true,
            CatalogAssetPipelineOperation.Create,
            null);

        Assert.That(result.IsVideo, Is.True);
    }

    [Test]
    public void CatalogAsync_CallbackAwaitsCancellationToken_ThrowsWithoutDeadlock()
    {
        CatalogFolderPipeline pipeline = CreatePipeline();
        CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        Action cancel = cancellationTokenSource.Cancel;
        string directory = TestContext.CurrentContext.TestDirectory;

        try
        {
            OperationCanceledException? exception = Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await pipeline.CatalogAsync(
                        directory,
                        ["image.jpg"],
                        false,
                        1,
                        CatalogAssetPipelineOperation.Create,
                        new(),
                        OnResultAsync,
                        cancellationToken)
                    .WaitAsync(TimeSpan.FromSeconds(5)));

            Assert.That(exception?.Message, Is.EqualTo("The operation was canceled."));
        }
        finally
        {
            cancellationTokenSource.Dispose();
        }

        async ValueTask<bool> OnResultAsync(CatalogPipelineResult _, CancellationToken token)
        {
            cancel();
            await Task.Delay(Timeout.InfiniteTimeSpan, token);

            return true;
        }
    }

    [Test]
    public void CatalogAsync_CallbackCancelsToken_StopsInvokingFurtherCallbacks()
    {
        CatalogFolderPipeline pipeline = CreatePipeline();
        CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        Action cancel = cancellationTokenSource.Cancel;
        List<CatalogPipelineResult> results = [];
        string directory = TestContext.CurrentContext.TestDirectory;

        try
        {
            OperationCanceledException? exception = Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await pipeline.CatalogAsync(
                        directory,
                        ["first.jpg", "second.jpg"],
                        false,
                        2,
                        CatalogAssetPipelineOperation.Create,
                        new(),
                        OnResultAsync,
                        cancellationToken)
                    .WaitAsync(TimeSpan.FromSeconds(5)));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exception?.Message, Is.EqualTo("The operation was canceled."));

                Assert.That(results, Has.Count.EqualTo(1));
                Assert.That(results[0].FileName, Is.EqualTo("first.jpg"));
            }
        }
        finally
        {
            cancellationTokenSource.Dispose();
        }

        ValueTask<bool> OnResultAsync(CatalogPipelineResult result, CancellationToken _)
        {
            results.Add(result);
            cancel();

            return ValueTask.FromResult(true);
        }
    }

    private static CatalogFolderPipeline CreatePipeline(
        Exception? persistenceException = null,
        Func<string, string, AssetWithThumbnail?>? assetFactory = null,
        Func<string, byte[]>? fileBytesFactory = null)
    {
        IFileOperationsService fileOperationsService = Substitute.For<IFileOperationsService>();
        IAssetCreationService assetCreationService = Substitute.For<IAssetCreationService>();
        IAssetRepository assetRepository = Substitute.For<IAssetRepository>();
        Func<string, string, AssetWithThumbnail?> createAssetWithThumbnail =
            assetFactory ?? ((directory, fileName) => new(CreateAsset(directory, fileName), [1, 2, 3]));

        fileOperationsService.GetFileBytes(Arg.Any<string>())
            .Returns(call => fileBytesFactory?.Invoke(call.ArgAt<string>(0)) ?? [1, 2, 3]);
        assetCreationService.CreateAssetWithThumbnail(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<byte[]>(), false,
                true)
            .Returns(call =>
            {
                string directory = call.ArgAt<string>(0);
                string fileName = call.ArgAt<string>(1);

                return createAssetWithThumbnail(directory, fileName);
            });

        if (persistenceException == null)
        {
            assetRepository.AddAssets(Arg.Any<IReadOnlyList<AssetWithThumbnail>>())
                .Returns(call => call.ArgAt<IReadOnlyList<AssetWithThumbnail>>(0).Count);
        }
        else
        {
            assetRepository.AddAssets(Arg.Any<IReadOnlyList<AssetWithThumbnail>>()).Throws(persistenceException);
        }

        return new(fileOperationsService, assetCreationService, assetRepository);
    }

    private static string[] CreateFileNames()
    {
        return [.. Enumerable.Range(0, FileCount).Select(index => $"image-{index}.jpg")];
    }

    private static Asset CreateAsset(string directory, string fileName)
    {
        return AssetBuilder.Create()
            .WithFolderPath(directory, Guid.NewGuid())
            .WithFileName(fileName)
            .WithFileProperties(1, DateTime.UnixEpoch, DateTime.UnixEpoch)
            .WithPixels(0, 0, 0, 0)
            .WithRotation(ImageRotation.Rotate0)
            .WithHash(fileName)
            .WithThumbnailCreationDateTime(DateTime.UnixEpoch)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
    }
}
