using Microsoft.Data.Sqlite;
using System.IO.Compression;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using ModificationDate = PhotoManager.Tests.Integration.Constants.ModificationDate;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.Persistence;

[TestFixture]
public class SqliteBackupServiceTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;
    private string? _backupsDirectory;

    private SqliteConnectionFactory? _factory;
    private SqliteBackupService? _backupService;
    private SqlitePersistenceContext? _sqlitePersistenceContext;
    private TestLogger<SqlitePersistenceContext> _testLogger = new();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);
        _backupsDirectory = Path.Combine(_databaseDirectory, Constants.DATABASE_BACKUP_END_PATH);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
        _factory = new(new TestLogger<SqliteConnectionFactory>());
        _backupService = new(_factory);
        _sqlitePersistenceContext = new(_factory, _backupService, _testLogger);
        _sqlitePersistenceContext.Initialize(_databaseDirectory!);
    }

    [TearDown]
    public void TearDown()
    {
        _sqlitePersistenceContext!.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
        _testLogger.LoggingAssertTearDown();
    }

    [Test]
    public void WriteBackup_ValidPath_CreatesZipFile()
    {
        string backupFilePath = Path.Combine(_backupsDirectory!, "20240101.zip");

        bool result = _backupService!.WriteBackup(backupFilePath);

        Assert.That(result, Is.True);
        Assert.That(File.Exists(backupFilePath), Is.True);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void WriteBackup_ValidPath_ZipContainsPhotomanagerDb()
    {
        string backupFilePath = Path.Combine(_backupsDirectory!, "20240102.zip");

        _backupService!.WriteBackup(backupFilePath);

        using (ZipArchive archive = ZipFile.OpenRead(backupFilePath))
        {
            Assert.That(archive.Entries, Has.Count.EqualTo(1));
            Assert.That(archive.Entries[0].Name, Is.EqualTo("photomanager.db"));
            Assert.That(archive.Entries[0].Length, Is.GreaterThan(0));
        }

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void WriteBackup_ExistingBackupFile_OverwritesPrevious()
    {
        string backupFilePath = Path.Combine(_backupsDirectory!, "20240103.zip");

        _backupService!.WriteBackup(backupFilePath);

        long firstSize = new FileInfo(backupFilePath).Length;

        _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\One");
        _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Two");
        _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Three");

        _backupService!.WriteBackup(backupFilePath);

        long secondSize = new FileInfo(backupFilePath).Length;

        Assert.That(secondSize, Is.GreaterThan(firstSize));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void WriteBackup_DirectoryDoesNotExist_CreatesDirectory()
    {
        string newDirectory = Path.Combine(_databaseDirectory!, "NewBackupDir");
        string backupFilePath = Path.Combine(newDirectory, "20240104.zip");

        Assert.That(Directory.Exists(newDirectory), Is.False);

        _backupService!.WriteBackup(backupFilePath);

        Assert.That(Directory.Exists(newDirectory), Is.True);
        Assert.That(File.Exists(backupFilePath), Is.True);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void WriteBackup_CleansUpTmpFile()
    {
        string backupFilePath = Path.Combine(_backupsDirectory!, "20240105.zip");

        _backupService!.WriteBackup(backupFilePath);

        string snapshotPath = backupFilePath + ".tmp.db";

        Assert.That(File.Exists(snapshotPath), Is.False);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void WriteBackup_BackupContainsValidSqliteDatabase()
    {
        // ─── Folders ───────────────────────────────────────────────────────────
        Folder vacationFolder = _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Vacation");
        Folder archiveFolder = _sqlitePersistenceContext.Folders.Insert(@"C:\Photos\Archive");
        Folder tempFolder = _sqlitePersistenceContext.Folders.Insert(@"C:\Photos\Temp");
        _sqlitePersistenceContext.Folders.Delete(tempFolder.Id);

        // ─── Assets ────────────────────────────────────────────────────────────
        // JPG: insert, then update via upsert (simulate rotation correction)
        _sqlitePersistenceContext.Assets.Upsert(CreateAsset(
            vacationFolder.Id, FileNames.IMAGE_1_JPG, Hashes.IMAGE_1_JPG,
            PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
            ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG,
            ImageRotation.Rotate0, false, null, false, null));
        _sqlitePersistenceContext.Assets.Upsert(CreateAsset(
            vacationFolder.Id, FileNames.IMAGE_1_JPG, Hashes.IMAGE_1_90_DEG_JPG,
            PixelWidthAsset.IMAGE_1_90_DEG_JPG, PixelHeightAsset.IMAGE_1_90_DEG_JPG,
            ThumbnailWidthAsset.IMAGE_1_90_DEG_JPG, ThumbnailHeightAsset.IMAGE_1_90_DEG_JPG,
            ImageRotation.Rotate90, false, null, true, "The asset has been rotated"));

        // PNG in Vacation folder
        _sqlitePersistenceContext.Assets.Upsert(CreateAsset(
            vacationFolder.Id, FileNames.IMAGE_9_PNG, Hashes.IMAGE_9_PNG,
            PixelWidthAsset.IMAGE_9_PNG, PixelHeightAsset.IMAGE_9_PNG,
            ThumbnailWidthAsset.IMAGE_9_PNG, ThumbnailHeightAsset.IMAGE_9_PNG,
            ImageRotation.Rotate0, false, null, false, null));

        // GIF in Vacation folder
        _sqlitePersistenceContext.Assets.Upsert(CreateAsset(
            vacationFolder.Id, FileNames.HOMER_GIF, Hashes.HOMER_GIF,
            PixelWidthAsset.HOMER_GIF, PixelHeightAsset.HOMER_GIF,
            ThumbnailWidthAsset.HOMER_GIF, ThumbnailHeightAsset.HOMER_GIF,
            ImageRotation.Rotate0, false, null, false, null));

        // HEIC in Archive folder
        _sqlitePersistenceContext.Assets.Upsert(CreateAsset(
            archiveFolder.Id, FileNames.IMAGE_11_HEIC, Hashes.IMAGE_11_HEIC,
            PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC,
            ThumbnailWidthAsset.IMAGE_11_HEIC, ThumbnailHeightAsset.IMAGE_11_HEIC,
            ImageRotation.Rotate0, false, null, false, null));

        // ─── Thumbnails ────────────────────────────────────────────────────────
        byte[] jpgThumbnailData = File.ReadAllBytes(Path.Combine(_assetsDirectory!, FileNames.IMAGE_1_JPG));
        byte[] pngThumbnailData = File.ReadAllBytes(Path.Combine(_assetsDirectory!, FileNames.IMAGE_9_PNG));
        byte[] gifThumbnailData = File.ReadAllBytes(Path.Combine(_assetsDirectory!, FileNames.HOMER_GIF));
        byte[] heicThumbnailData = File.ReadAllBytes(Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC));

        // Upsert one thumbnail then replace all Vacation thumbnails (simulates a full folder sync)
        _sqlitePersistenceContext.Thumbnails.Upsert(vacationFolder.Id, FileNames.IMAGE_1_JPG, jpgThumbnailData);
        _sqlitePersistenceContext.Thumbnails.ReplaceForFolder(vacationFolder.Id, new Dictionary<string, byte[]>
        {
            [FileNames.IMAGE_1_JPG] = jpgThumbnailData,
            [FileNames.IMAGE_9_PNG] = pngThumbnailData,
            [FileNames.HOMER_GIF] = gifThumbnailData
        });

        // HEIC thumbnail in Archive folder
        _sqlitePersistenceContext.Thumbnails.Upsert(archiveFolder.Id, FileNames.IMAGE_11_HEIC, heicThumbnailData);

        // ─── Recent Paths ──────────────────────────────────────────────────────
        _sqlitePersistenceContext.RecentPaths.Replace([@"C:\Photos\Old1", @"C:\Photos\Old2"]);
        _sqlitePersistenceContext.RecentPaths.Replace([@"C:\Photos\Vacation", @"C:\Photos\Archive"]);

        // ─── Sync Definitions ──────────────────────────────────────────────────
        _sqlitePersistenceContext.SyncDefinitions.Replace(
        [
            new()
            {
                SourceDirectory = @"C:\Source",
                DestinationDirectory = @"D:\OldBackup",
                IncludeSubFolders = false,
                DeleteAssetsNotInSource = false
            }
        ]);
        _sqlitePersistenceContext.SyncDefinitions.Replace(
        [
            new()
            {
                SourceDirectory = @"C:\Photos\Vacation",
                DestinationDirectory = @"D:\Backup\Vacation",
                IncludeSubFolders = true,
                DeleteAssetsNotInSource = false
            },
            new()
            {
                SourceDirectory = @"C:\Photos\Archive",
                DestinationDirectory = @"D:\Backup\Archive",
                IncludeSubFolders = false,
                DeleteAssetsNotInSource = true
            }
        ]);

        // ─── Backup ────────────────────────────────────────────────────────────
        string backupFilePath = Path.Combine(_backupsDirectory!, "20240106.zip");
        _backupService!.WriteBackup(backupFilePath);

        // ─── Verify extracted database ─────────────────────────────────────────
        string extractDirectory = Path.Combine(_databaseDirectory!, "extract_test");
        ZipFile.ExtractToDirectory(backupFilePath, extractDirectory);

        string extractedDbFile = Path.Combine(extractDirectory, "photomanager.db");
        Assert.That(File.Exists(extractedDbFile), Is.True);

        SqliteConnectionStringBuilder builder = new()
        {
            DataSource = extractedDbFile,
            Mode = SqliteOpenMode.ReadOnly,
            Pooling = false
        };

        using (SqliteConnection extractedConnection = new(builder.ConnectionString))
        {
            extractedConnection.Open();

            string vacationFolderId = vacationFolder.Id.ToString().ToUpperInvariant();
            string archiveFolderId = archiveFolder.Id.ToString().ToUpperInvariant();
            long expectedTicks = ModificationDate.Default.Ticks;

            // ─── Folders: 2 remain (Temp was deleted) ─────────────────────────
            using (SqliteCommand command = extractedConnection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Folders;";
                Assert.That((long)command.ExecuteScalar()!, Is.EqualTo(2));
            }

            using (SqliteCommand command = extractedConnection.CreateCommand())
            {
                command.CommandText = @"SELECT * FROM Folders WHERE Path = 'C:\Photos\Vacation';";

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    Assert.That(reader.Read(), Is.True);

                    Assert.That(reader["Id"].ToString(), Is.EqualTo(vacationFolderId));
                    Assert.That(reader["Path"].ToString(), Is.EqualTo(vacationFolder.Path));
                }
            }

            using (SqliteCommand command = extractedConnection.CreateCommand())
            {
                command.CommandText = @"SELECT * FROM Folders WHERE Path = 'C:\Photos\Archive';";

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    Assert.That(reader.Read(), Is.True);

                    Assert.That(reader["Id"].ToString(), Is.EqualTo(archiveFolderId));
                    Assert.That(reader["Path"].ToString(), Is.EqualTo(archiveFolder.Path));
                }
            }

            // ─── Assets: 4 total (JPG updated, PNG, GIF in Vacation; HEIC in Archive) ─────
            using (SqliteCommand command = extractedConnection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Assets;";
                Assert.That((long)command.ExecuteScalar()!, Is.EqualTo(4));
            }

            // JPG – updated to Rotate90 via second upsert
            using (SqliteCommand command = extractedConnection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Assets WHERE FileName = @fileName;";
                command.Parameters.AddWithValue("@fileName", FileNames.IMAGE_1_JPG);

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    Assert.That(reader.Read(), Is.True);

                    Assert.That(reader["FolderId"].ToString(), Is.EqualTo(vacationFolderId));
                    Assert.That(reader["FileName"].ToString(), Is.EqualTo(FileNames.IMAGE_1_JPG));
                    Assert.That((long)reader["ImageRotation"], Is.EqualTo((int)ImageRotation.Rotate90));
                    Assert.That((long)reader["PixelWidth"], Is.EqualTo(PixelWidthAsset.IMAGE_1_90_DEG_JPG));
                    Assert.That((long)reader["PixelHeight"], Is.EqualTo(PixelHeightAsset.IMAGE_1_90_DEG_JPG));
                    Assert.That((long)reader["ThumbnailPixelWidth"],
                        Is.EqualTo(ThumbnailWidthAsset.IMAGE_1_90_DEG_JPG));
                    Assert.That((long)reader["ThumbnailPixelHeight"],
                        Is.EqualTo(ThumbnailHeightAsset.IMAGE_1_90_DEG_JPG));
                    Assert.That((long)reader["ThumbnailCreationDateTime"], Is.EqualTo(expectedTicks));
                    Assert.That(reader["Hash"].ToString(), Is.EqualTo(Hashes.IMAGE_1_90_DEG_JPG));
                    Assert.That(reader["CorruptedMessage"], Is.EqualTo(DBNull.Value));
                    Assert.That((long)reader["IsCorrupted"], Is.Zero);
                    Assert.That(reader["RotatedMessage"].ToString(), Is.EqualTo("The asset has been rotated"));
                    Assert.That((long)reader["IsRotated"], Is.EqualTo(1));
                }
            }

            // PNG
            using (SqliteCommand command = extractedConnection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Assets WHERE FileName = @fileName;";
                command.Parameters.AddWithValue("@fileName", FileNames.IMAGE_9_PNG);

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    Assert.That(reader.Read(), Is.True);

                    Assert.That(reader["FolderId"].ToString(), Is.EqualTo(vacationFolderId));
                    Assert.That(reader["FileName"].ToString(), Is.EqualTo(FileNames.IMAGE_9_PNG));
                    Assert.That((long)reader["ImageRotation"], Is.EqualTo((int)ImageRotation.Rotate0));
                    Assert.That((long)reader["PixelWidth"], Is.EqualTo(PixelWidthAsset.IMAGE_9_PNG));
                    Assert.That((long)reader["PixelHeight"], Is.EqualTo(PixelHeightAsset.IMAGE_9_PNG));
                    Assert.That((long)reader["ThumbnailPixelWidth"], Is.EqualTo(ThumbnailWidthAsset.IMAGE_9_PNG));
                    Assert.That((long)reader["ThumbnailPixelHeight"], Is.EqualTo(ThumbnailHeightAsset.IMAGE_9_PNG));
                    Assert.That((long)reader["ThumbnailCreationDateTime"], Is.EqualTo(expectedTicks));
                    Assert.That(reader["Hash"].ToString(), Is.EqualTo(Hashes.IMAGE_9_PNG));
                    Assert.That(reader["CorruptedMessage"], Is.EqualTo(DBNull.Value));
                    Assert.That((long)reader["IsCorrupted"], Is.Zero);
                    Assert.That(reader["RotatedMessage"], Is.EqualTo(DBNull.Value));
                    Assert.That((long)reader["IsRotated"], Is.Zero);
                }
            }

            // GIF
            using (SqliteCommand command = extractedConnection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Assets WHERE FileName = @fileName;";
                command.Parameters.AddWithValue("@fileName", FileNames.HOMER_GIF);

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    Assert.That(reader.Read(), Is.True);

                    Assert.That(reader["FolderId"].ToString(), Is.EqualTo(vacationFolderId));
                    Assert.That(reader["FileName"].ToString(), Is.EqualTo(FileNames.HOMER_GIF));
                    Assert.That((long)reader["ImageRotation"], Is.EqualTo((int)ImageRotation.Rotate0));
                    Assert.That((long)reader["PixelWidth"], Is.EqualTo(PixelWidthAsset.HOMER_GIF));
                    Assert.That((long)reader["PixelHeight"], Is.EqualTo(PixelHeightAsset.HOMER_GIF));
                    Assert.That((long)reader["ThumbnailPixelWidth"], Is.EqualTo(ThumbnailWidthAsset.HOMER_GIF));
                    Assert.That((long)reader["ThumbnailPixelHeight"], Is.EqualTo(ThumbnailHeightAsset.HOMER_GIF));
                    Assert.That((long)reader["ThumbnailCreationDateTime"], Is.EqualTo(expectedTicks));
                    Assert.That(reader["Hash"].ToString(), Is.EqualTo(Hashes.HOMER_GIF));
                    Assert.That(reader["CorruptedMessage"], Is.EqualTo(DBNull.Value));
                    Assert.That((long)reader["IsCorrupted"], Is.Zero);
                    Assert.That(reader["RotatedMessage"], Is.EqualTo(DBNull.Value));
                    Assert.That((long)reader["IsRotated"], Is.Zero);
                }
            }

            // HEIC
            using (SqliteCommand command = extractedConnection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Assets WHERE FileName = @fileName;";
                command.Parameters.AddWithValue("@fileName", FileNames.IMAGE_11_HEIC);

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    Assert.That(reader.Read(), Is.True);

                    Assert.That(reader["FolderId"].ToString(), Is.EqualTo(archiveFolderId));
                    Assert.That(reader["FileName"].ToString(), Is.EqualTo(FileNames.IMAGE_11_HEIC));
                    Assert.That((long)reader["ImageRotation"], Is.EqualTo((int)ImageRotation.Rotate0));
                    Assert.That((long)reader["PixelWidth"], Is.EqualTo(PixelWidthAsset.IMAGE_11_HEIC));
                    Assert.That((long)reader["PixelHeight"], Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC));
                    Assert.That((long)reader["ThumbnailPixelWidth"], Is.EqualTo(ThumbnailWidthAsset.IMAGE_11_HEIC));
                    Assert.That((long)reader["ThumbnailPixelHeight"], Is.EqualTo(ThumbnailHeightAsset.IMAGE_11_HEIC));
                    Assert.That((long)reader["ThumbnailCreationDateTime"], Is.EqualTo(expectedTicks));
                    Assert.That(reader["Hash"].ToString(), Is.EqualTo(Hashes.IMAGE_11_HEIC));
                    Assert.That(reader["CorruptedMessage"], Is.EqualTo(DBNull.Value));
                    Assert.That((long)reader["IsCorrupted"], Is.Zero);
                    Assert.That(reader["RotatedMessage"], Is.EqualTo(DBNull.Value));
                    Assert.That((long)reader["IsRotated"], Is.Zero);
                }
            }

            // ─── Thumbnails: 4 total (3 in Vacation after ReplaceForFolder, 1 in Archive) ─
            using (SqliteCommand command = extractedConnection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Thumbnails;";
                Assert.That((long)command.ExecuteScalar()!, Is.EqualTo(4));
            }

            using (SqliteCommand command = extractedConnection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Thumbnails WHERE FileName = @fileName;";
                command.Parameters.AddWithValue("@fileName", FileNames.IMAGE_1_JPG);

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    Assert.That(reader.Read(), Is.True);

                    Assert.That(reader["FolderId"].ToString(), Is.EqualTo(vacationFolderId));
                    Assert.That(reader["FileName"].ToString(), Is.EqualTo(FileNames.IMAGE_1_JPG));

                    using (Stream stream = reader.GetStream(reader.GetOrdinal("Data")))
                    {
                        using (MemoryStream ms = new())
                        {
                            stream.CopyTo(ms);
                            Assert.That(ms.ToArray(), Is.EqualTo(jpgThumbnailData));
                        }
                    }
                }
            }

            using (SqliteCommand command = extractedConnection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Thumbnails WHERE FileName = @fileName;";
                command.Parameters.AddWithValue("@fileName", FileNames.IMAGE_9_PNG);

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    Assert.That(reader.Read(), Is.True);

                    Assert.That(reader["FolderId"].ToString(), Is.EqualTo(vacationFolderId));
                    Assert.That(reader["FileName"].ToString(), Is.EqualTo(FileNames.IMAGE_9_PNG));

                    using (Stream stream = reader.GetStream(reader.GetOrdinal("Data")))
                    {
                        using (MemoryStream ms = new())
                        {
                            stream.CopyTo(ms);
                            Assert.That(ms.ToArray(), Is.EqualTo(pngThumbnailData));
                        }
                    }
                }
            }

            using (SqliteCommand command = extractedConnection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Thumbnails WHERE FileName = @fileName;";
                command.Parameters.AddWithValue("@fileName", FileNames.HOMER_GIF);

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    Assert.That(reader.Read(), Is.True);

                    Assert.That(reader["FolderId"].ToString(), Is.EqualTo(vacationFolderId));
                    Assert.That(reader["FileName"].ToString(), Is.EqualTo(FileNames.HOMER_GIF));

                    using (Stream stream = reader.GetStream(reader.GetOrdinal("Data")))
                    {
                        using (MemoryStream ms = new())
                        {
                            stream.CopyTo(ms);
                            Assert.That(ms.ToArray(), Is.EqualTo(gifThumbnailData));
                        }
                    }
                }
            }

            using (SqliteCommand command = extractedConnection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Thumbnails WHERE FileName = @fileName;";
                command.Parameters.AddWithValue("@fileName", FileNames.IMAGE_11_HEIC);

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    Assert.That(reader.Read(), Is.True);

                    Assert.That(reader["FolderId"].ToString(), Is.EqualTo(archiveFolderId));
                    Assert.That(reader["FileName"].ToString(), Is.EqualTo(FileNames.IMAGE_11_HEIC));

                    using (Stream stream = reader.GetStream(reader.GetOrdinal("Data")))
                    {
                        using (MemoryStream ms = new())
                        {
                            stream.CopyTo(ms);
                            Assert.That(ms.ToArray(), Is.EqualTo(heicThumbnailData));
                        }
                    }
                }
            }

            // ─── Recent Paths: 2 final entries after the second Replace ───────
            using (SqliteCommand command = extractedConnection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM RecentPaths;";
                Assert.That((long)command.ExecuteScalar()!, Is.EqualTo(2));
            }

            using (SqliteCommand command = extractedConnection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM RecentPaths WHERE Position = 0;";

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    Assert.That(reader.Read(), Is.True);

                    Assert.That((long)reader["Position"], Is.Zero);
                    Assert.That(reader["Path"].ToString(), Is.EqualTo(@"C:\Photos\Vacation"));
                }
            }

            using (SqliteCommand command = extractedConnection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM RecentPaths WHERE Position = 1;";

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    Assert.That(reader.Read(), Is.True);

                    Assert.That((long)reader["Position"], Is.EqualTo(1));
                    Assert.That(reader["Path"].ToString(), Is.EqualTo(@"C:\Photos\Archive"));
                }
            }

            // ─── Sync Definitions: 2 final entries after the second Replace ───
            using (SqliteCommand command = extractedConnection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM SyncDefinitions;";
                Assert.That((long)command.ExecuteScalar()!, Is.EqualTo(2));
            }

            using (SqliteCommand command = extractedConnection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM SyncDefinitions WHERE Position = 0;";

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    Assert.That(reader.Read(), Is.True);

                    Assert.That((long)reader["Position"], Is.Zero);
                    Assert.That(reader["SourceDirectory"].ToString(), Is.EqualTo(@"C:\Photos\Vacation"));
                    Assert.That(reader["DestinationDirectory"].ToString(), Is.EqualTo(@"D:\Backup\Vacation"));
                    Assert.That((long)reader["IncludeSubFolders"], Is.EqualTo(1));
                    Assert.That((long)reader["DeleteAssetsNotInSource"], Is.Zero);
                }
            }

            using (SqliteCommand command = extractedConnection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM SyncDefinitions WHERE Position = 1;";

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    Assert.That(reader.Read(), Is.True);

                    Assert.That((long)reader["Position"], Is.EqualTo(1));
                    Assert.That(reader["SourceDirectory"].ToString(), Is.EqualTo(@"C:\Photos\Archive"));
                    Assert.That(reader["DestinationDirectory"].ToString(), Is.EqualTo(@"D:\Backup\Archive"));
                    Assert.That((long)reader["IncludeSubFolders"], Is.Zero);
                    Assert.That((long)reader["DeleteAssetsNotInSource"], Is.EqualTo(1));
                }
            }

            _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
        }
    }

    [Test]
    public void GetBackupFilesPaths_DirectoryWithZipFiles_ReturnsFilePaths()
    {
        _backupService!.WriteBackup(Path.Combine(_backupsDirectory!, "20240101.zip"));
        _backupService!.WriteBackup(Path.Combine(_backupsDirectory!, "20240102.zip"));
        _backupService!.WriteBackup(Path.Combine(_backupsDirectory!, "20240103.zip"));

        string[] paths = _backupService!.GetBackupFilesPaths(_backupsDirectory!);

        Assert.That(paths, Has.Length.EqualTo(3));
        Assert.That(paths.Any(p => p.Contains("20240101.zip")), Is.True);
        Assert.That(paths.Any(p => p.Contains("20240102.zip")), Is.True);
        Assert.That(paths.Any(p => p.Contains("20240103.zip")), Is.True);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetBackupFilesPaths_NonExistentDirectory_ReturnsEmptyArray()
    {
        string nonExistentDirectory = Path.Combine(_databaseDirectory!, "NonExistentDir");

        string[] paths = _backupService!.GetBackupFilesPaths(nonExistentDirectory);

        Assert.That(paths, Is.Empty);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetBackupFilesPaths_EmptyDirectory_ReturnsEmptyArray()
    {
        string emptyDirectory = Path.Combine(_databaseDirectory!, "EmptyBackupsDir");
        Directory.CreateDirectory(emptyDirectory);

        string[] paths = _backupService!.GetBackupFilesPaths(emptyDirectory);

        Assert.That(paths, Is.Empty);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetBackupFilesPaths_DirectoryWithNonZipFiles_ReturnsOnlyZipFiles()
    {
        _backupService!.WriteBackup(Path.Combine(_backupsDirectory!, "20240101.zip"));

        File.WriteAllText(Path.Combine(_backupsDirectory!, "notes.txt"), "Some notes");
        File.WriteAllText(Path.Combine(_backupsDirectory!, "data.db"), "Not a zip");

        string[] paths = _backupService!.GetBackupFilesPaths(_backupsDirectory!);

        Assert.That(paths, Has.Length.EqualTo(1));
        Assert.That(paths[0], Does.Contain("20240101.zip"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteBackupFile_ExistingFile_RemovesFile()
    {
        string backupFilePath = Path.Combine(_backupsDirectory!, "20240201.zip");

        _backupService!.WriteBackup(backupFilePath);

        Assert.That(File.Exists(backupFilePath), Is.True);

        _backupService!.DeleteBackupFile(backupFilePath);

        Assert.That(File.Exists(backupFilePath), Is.False);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteBackupFile_NonExistentFile_DoesNotThrow()
    {
        string nonExistentFile = Path.Combine(_backupsDirectory!, "nonexistent.zip");

        Assert.DoesNotThrow(() => _backupService!.DeleteBackupFile(nonExistentFile));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    private static Asset CreateAsset(Guid folderId, string fileName, string hash, int pixelWidth, int pixelHeight,
        int thumbnailWidth, int thumbnailHeight, ImageRotation rotation, bool isCorrupted, string? corruptedMessage,
        bool isRotated, string? rotatedMessage)
    {
        return AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = string.Empty })
            .WithFolderId(folderId)
            .WithFileName(fileName)
            .WithRotation(rotation)
            .WithPixels(pixelWidth, pixelHeight, thumbnailWidth, thumbnailHeight)
            .WithFileSize(0)
            .WithThumbnailCreationDateTime(ModificationDate.Default)
            .WithHash(hash)
            .WithCorrupted(isCorrupted, corruptedMessage)
            .WithRotated(isRotated, rotatedMessage)
            .Build();
    }
}
