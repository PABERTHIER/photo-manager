namespace PhotoManager.Tests.Integration;

public static class Constants
{
    public const string DATABASE_END_PATH = "v1.0";
    public const string DATABASE_BACKUP_END_PATH = "v1.0_Backups";
    public const string VERSION = "v1.0.0";

    public static class Directories
    {
        public const string BLOBS = "Blobs";
        public const string TABLES = "Tables";
        public const string TEST_FILES = "TestFiles";
        public const string TEST_FOLDER = "TestFolder";
        public const string TEST_FOLDER_1 = "TestFolder1";
        public const string TEST_FOLDER_2 = "TestFolder2";
        public const string TEST_SUB_FOLDER_1 = "TestSubFolder1";
        public const string TEST_SUB_FOLDER_2 = "TestSubFolder2";
        public const string TEST_SUB_FOLDER_3 = "TestSubFolder3";
        public const string TEST_HIDDEN_SUB_FOLDER = "TestHiddenSubFolder";
        public const string TEMP_ASSETS_DIRECTORY = "TempAssetsDirectory";
        public const string TEMP_EMPTY_FOLDER = "TempEmptyFolder";
        public const string TEMP_FOLDER = "TempFolder";
        public const string DESTINATION_TO_COPY = "DestinationToCopy";
        public const string DESTINATION_TO_MOVE = "DestinationToMove";
        public const string DESTINATION_TO_SYNC = "DestinationToSync";
        public const string DESTINATION_TO_SYNC_1 = "DestinationToSync1";
        public const string DESTINATION_TO_SYNC_2 = "DestinationToSync2";
        public const string FINAL_DESTINATION = "FinalDestination";
        public const string NO_COPY_DIRECTORY = "NoCopyDirectory";
        public const string NO_MOVE_DIRECTORY = "NoMoveDirectory";
        public const string SOURCE = "Source";
        public const string SOURCE_TO_MOVE = "SourceToMove";
        public const string SOURCE_TO_SYNC = "SourceToSync";
        public const string TEST_DIRECTORY = "TestDirectory";
        public const string ASSETS_TESTS = "AssetsTests";
        public const string CORRUPTED_IMAGE = "CorruptedImage";
        public const string IMAGE_CONVERTED = "ImageConverted";
        public const string INVALID_IMAGE = "InvalidImage";
        public const string FOLDER_TO_DELETE = "FolderToDelete";
        public const string FOLDER_IMAGE_DELETED = "FolderImageDeleted";
        public const string FOLDER_IMAGE_UPDATED = "FolderImageUpdated";
        public const string FOLDER_SUB_DIR = "FolderSubDir";
        public const string FOLDER_SUB_SUB_DIR = "FolderSubSubDir";
        public const string Z_FOLDER_SUB_DIR = "ZFolderSubDir";
        public const string SUB_DIRECTORY_1 = "SubDirectory1";
        public const string SUB_DIRECTORY_2 = "SubDirectory2";
        public const string SUB_DIRECTORY_3 = "SubDirectory3";
        public const string SUB_DIRECTORY_4 = "SubDirectory4";
        public const string SUB_DIRECTORY_5 = "SubDirectory5";
        public const string DEFAULT_ASSETS = "Path";
        public const string OUTPUT_VIDEO_FIRST_FRAME = "OutputVideoFirstFrame";
        public const string DUPLICATES = "Duplicates";
        public const string FOLDER_1 = "Folder1";
        public const string FOLDER_2 = "Folder2";
        public const string NEW_FOLDER = "NewFolder";
        public const string NEW_FOLDER_1 = "NewFolder1";
        public const string NEW_FOLDER_2 = "NewFolder2";
        public const string NEW_FOLDER_3 = "NewFolder3";
        public const string NEW_FOLDER_4 = "NewFolder4";
        public const string NEW_FOLDER_5 = "NewFolder5";
        public const string NEW_FOLDER_6 = "NewFolder6";
        public const string NOT_DUPLICATE = "NotDuplicate";
        public const string PART = "Part";
        public const string RESOLUTION = "Resolution";
        public const string SAMPLE_1 = "Sample1";
        public const string SAMPLE_2 = "Sample2";
        public const string SAMPLE_3 = "Sample3";
        public const string THUMBNAIL = "Thumbnail";
        public const string DATABASE_TESTS = "DatabaseTests";
        public const string DATABASE_TESTS_BACKUPS = "DatabaseTests_Backups";
        public const string TEST_BACKUP = "TestBackup";
        public const string FAKE = "fake";

        public const string NON_EXISTENT_FOLDER = "NonExistentFolder";
        public const string NON_EXISTENT_FOLDER_1 = "NonExistentFolder1";
    }

    public static class Tables
    {
        public const string ASSETS_DB = "assets.db";
        public const string FOLDERS_DB = "folders.db";
        public const string RECENT_TARGET_PATHS_DB = "recenttargetpaths.db";
        public const string SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB = "syncassetsdirectoriesdefinitions.db";
    }

    public static class FileNames
    {
        public const string HOMER_GIF = "Homer.gif";
        public const string HOMER_JPG = "Homer.jpg";
        public const string HOMER_DUPLICATED_JPG = "HomerDuplicated.jpg";
        public const string HOMER_MP4 = "Homer.mp4";
        public const string HOMER_DUPLICATED_MP4 = "HomerDuplicated.mp4";
        public const string HOMER_1_S_MP4 = "Homer1s.mp4";
        public const string HOMER_1_S_JPG = "Homer1s.jpg";
        public const string IMAGE_JPG = "Image.jpg";
        public const string IMAGE_1_JPG = "Image 1.jpg";
        public const string IMAGE_1_90_DEG_JPG = "Image 1_90_deg.jpg";
        public const string IMAGE_1_180_DEG_JPG = "Image 1_180_deg.jpg";
        public const string IMAGE_1_270_DEG_JPG = "Image 1_270_deg.jpg";
        public const string IMAGE_1_DUPLICATE_JPG = "Image 1_duplicate.jpg";
        public const string IMAGE_1_DUPLICATE_COPIED_JPG = "Image 1_duplicate_copied.jpg";
        public const string IMAGE_1_TEMP_JPG = "Image 1_Temp.jpg";
        public const string IMAGE_2_JPG = "Image 2.jpg";
        public const string IMAGE_2_DUPLICATED_JPG = "Image 2 duplicated.jpg";
        public const string IMAGE_2_PNG = "Image2.png";
        public const string IMAGE_3_JPG = "Image 3.jpg";
        public const string IMAGE_4_JPG = "Image 4.jpg";
        public const string IMAGE_5_JPG = "Image 5.jpg";
        public const string IMAGE_6_JPG = "Image 6.jpg";
        public const string IMAGE_7_JPG = "Image 7.jpg";
        public const string IMAGE_8_JPEG = "Image 8.jpeg";
        public const string IMAGE_9_PNG = "Image 9.png";
        public const string IMAGE_9_DUPLICATE_PNG = "Image 9_duplicate.png";
        public const string IMAGE_10_PORTRAIT_PNG = "Image 10 portrait.png";
        public const string IMAGE_11_HEIC = "Image_11.heic";
        public const string IMAGE_11_90_DEG_HEIC = "Image_11_90.heic";
        public const string IMAGE_11_180_DEG_HEIC = "Image_11_180.heic";
        public const string IMAGE_11_270_DEG_HEIC = "Image_11_270.heic";
        public const string IMAGE_WITH_UPPERCASE_NAME_JPG = "IMAGE_WITH_UPPERCASE_NAME.JPG";
        public const string _1336_JPG = "1336.JPG";
        public const string _1336_ORIGINAL_JPG = "1336_Original.JPG";
        public const string _1336_1_K_JPG = "1336_1K.JPG";
        public const string _1336_2_K_JPG = "1336_2K.JPG";
        public const string _1336_3_K_JPG = "1336_3K.JPG";
        public const string _1336_4_K_ORIGINAL_JPG = "1336_4K_Original.JPG";
        public const string _1336_8_K_JPG = "1336_8K.JPG";
        public const string _1336_THUMBNAIL_JPG = "1336_Thumbnail.JPG";
        public const string IMAGE_1336_ORIGINAL_JPG = "Image_1336_Original.JPG";
        public const string IMAGE_1336_MINI_JPG = "Image_1336_Mini.JPG";
        public const string IMAGE_1336_SHIT_QUALITY_JPG = "Image_1336_ShitQuality.JPG";
        public const string IMAGE_1336_SMALL_JPG = "Image_1336_Small.JPG";
        public const string _1336_BOTTOM_LEFT_PART_JPG = "1336_BottomLeftPart.JPG";
        public const string _1336_BOTTOM_PART_JPG = "1336_BottomPart.JPG";
        public const string _1336_BOTTOM_RIGHT_PART_JPG = "1336_BottomRightPart.JPG";
        public const string _1336_LEFT_PART_JPG = "1336_LeftPart.JPG";
        public const string _1336_RIGHT_PART_JPG = "1336_RightPart.JPG";
        public const string _1336_TOP_LEFT_PART_JPG = "1336_TopLeftPart.JPG";
        public const string _1336_TOP_PART_JPG = "1336_TopPart.JPG";
        public const string _1336_TOP_RIGHT_PART_JPG = "1336_TopRightPart.JPG";
        public const string _1337_JPG = "1337.JPG";
        public const string _1349_JPG = "1349.JPG";
        public const string _1350_JPG = "1350.JPG";
        public const string _1413_JPG = "1413.JPG";
        public const string _1414_JPG = "1414.JPG";
        public const string _1415_JPG = "1415.JPG";

        public const string IMAGE_CONVERTED_GIF = "image_converted.gif";
        public const string IMAGE_CONVERTED_JPEG = "image_converted.jpeg";
        public const string IMAGE_CONVERTED_PNG = "image_converted.png";
        public const string IMAGE_TO_DELETE_JPG = "ImageToDelete.jpg";

        public const string NON_EXISTENT_FILE_JPG = "NonExistentFile.jpg";
        public const string NON_EXISTENT_IMAGE_JPG = "ImageDoesNotExist.jpg";
        public const string NON_EXISTENT_IMAGE_PNG = "ImageDoesNotExist.png";
        public const string NON_EXISTENT_VIDEO_MP4 = "VideoDoesNotExist.mp4";
    }

    public static class PixelWidthAsset
    {
        public const int HOMER_GIF = 320;
        public const int HOMER_JPG = 320;
        public const int HOMER_DUPLICATED_JPG = 320;
        public const int IMAGE_1_JPG = 1280;
        public const int IMAGE_1_90_DEG_JPG = 720;
        public const int IMAGE_1_180_DEG_JPG = 1280;
        public const int IMAGE_1_270_DEG_JPG = 720;
        public const int IMAGE_1_DUPLICATE_JPG = 1280;
        public const int IMAGE_1_DUPLICATE_COPIED_JPG = 1280;
        public const int IMAGE_2_JPG = 1280;
        public const int IMAGE_2_DUPLICATED_JPG = 1280;
        public const int IMAGE_3_JPG = 1280;
        public const int IMAGE_4_JPG = 1280;
        public const int IMAGE_5_JPG = 1280;
        public const int IMAGE_6_JPG = 1280;
        public const int IMAGE_7_JPG = 1280;
        public const int IMAGE_8_JPEG = 1280;
        public const int IMAGE_9_PNG = 1280;
        public const int IMAGE_9_DUPLICATE_PNG = 1280;
        public const int IMAGE_10_PORTRAIT_PNG = 720;
        public const int IMAGE_11_HEIC = 3024;
        public const int IMAGE_11_90_DEG_HEIC = 4032;
        public const int IMAGE_11_180_DEG_HEIC = 3024;
        public const int IMAGE_11_270_DEG_HEIC = 4032;
        public const int IMAGE_WITH_UPPERCASE_NAME_JPG = 1280;
        public const int _1336_JPG = 4032;
        public const int _1336_ORIGINAL_JPG = 4032;
        public const int _1336_1_K_JPG = 1024;
        public const int _1336_2_K_JPG = 2048;
        public const int _1336_3_K_JPG = 2880;
        public const int _1336_4_K_ORIGINAL_JPG = 4032;
        public const int _1336_8_K_JPG = 7680;
        public const int _1336_THUMBNAIL_JPG = 202;
        public const int IMAGE_1336_ORIGINAL_JPG = 4032;
        public const int IMAGE_1336_MINI_JPG = 43;
        public const int IMAGE_1336_SHIT_QUALITY_JPG = 4032;
        public const int IMAGE_1336_SMALL_JPG = 591;
        public const int _1336_BOTTOM_LEFT_PART_JPG = 2016;
        public const int _1336_BOTTOM_PART_JPG = 4032;
        public const int _1336_BOTTOM_RIGHT_PART_JPG = 2016;
        public const int _1336_LEFT_PART_JPG = 2016;
        public const int _1336_RIGHT_PART_JPG = 2016;
        public const int _1336_TOP_LEFT_PART_JPG = 2016;
        public const int _1336_TOP_PART_JPG = 4032;
        public const int _1336_TOP_RIGHT_PART_JPG = 2016;
        public const int _1337_JPG = 4032;
        public const int _1349_JPG = 4032;
        public const int _1350_JPG = 4032;
        public const int _1413_JPG = 4032;
        public const int _1414_JPG = 4032;
        public const int _1415_JPG = 3024;

        public const int NON_EXISTENT_FILE_JPG = 0;
        public const int NON_EXISTENT_IMAGE_JPG = 1280;
    }

    public static class PixelHeightAsset
    {
        public const int HOMER_GIF = 320;
        public const int HOMER_JPG = 180;
        public const int HOMER_DUPLICATED_JPG = 180;
        public const int IMAGE_1_JPG = 720;
        public const int IMAGE_1_90_DEG_JPG = 1280;
        public const int IMAGE_1_180_DEG_JPG = 720;
        public const int IMAGE_1_270_DEG_JPG = 1280;
        public const int IMAGE_1_DUPLICATE_JPG = 720;
        public const int IMAGE_1_DUPLICATE_COPIED_JPG = 720;
        public const int IMAGE_2_JPG = 720;
        public const int IMAGE_2_DUPLICATED_JPG = 720;
        public const int IMAGE_3_JPG = 720;
        public const int IMAGE_4_JPG = 720;
        public const int IMAGE_5_JPG = 720;
        public const int IMAGE_6_JPG = 720;
        public const int IMAGE_7_JPG = 720;
        public const int IMAGE_8_JPEG = 720;
        public const int IMAGE_9_PNG = 720;
        public const int IMAGE_9_DUPLICATE_PNG = 720;
        public const int IMAGE_10_PORTRAIT_PNG = 1280;
        public const int IMAGE_11_HEIC = 4032;
        public const int IMAGE_11_90_DEG_HEIC = 3024;
        public const int IMAGE_11_180_DEG_HEIC = 4032;
        public const int IMAGE_11_270_DEG_HEIC = 3024;
        public const int IMAGE_WITH_UPPERCASE_NAME_JPG = 720;
        public const int _1336_JPG = 3024;
        public const int _1336_ORIGINAL_JPG = 3024;
        public const int _1336_1_K_JPG = 768;
        public const int _1336_2_K_JPG = 1536;
        public const int _1336_3_K_JPG = 2160;
        public const int _1336_4_K_ORIGINAL_JPG = 3024;
        public const int _1336_8_K_JPG = 5760;
        public const int _1336_THUMBNAIL_JPG = 152;
        public const int IMAGE_1336_ORIGINAL_JPG = 3024;
        public const int IMAGE_1336_MINI_JPG = 32;
        public const int IMAGE_1336_SHIT_QUALITY_JPG = 3024;
        public const int IMAGE_1336_SMALL_JPG = 443;
        public const int _1336_BOTTOM_LEFT_PART_JPG = 1512;
        public const int _1336_BOTTOM_PART_JPG = 1512;
        public const int _1336_BOTTOM_RIGHT_PART_JPG = 1512;
        public const int _1336_LEFT_PART_JPG = 3024;
        public const int _1336_RIGHT_PART_JPG = 3024;
        public const int _1336_TOP_LEFT_PART_JPG = 1512;
        public const int _1336_TOP_PART_JPG = 1512;
        public const int _1336_TOP_RIGHT_PART_JPG = 1512;
        public const int _1337_JPG = 3024;
        public const int _1349_JPG = 3024;
        public const int _1350_JPG = 3024;
        public const int _1413_JPG = 3024;
        public const int _1414_JPG = 3024;
        public const int _1415_JPG = 4032;

        public const int NON_EXISTENT_FILE_JPG = 0;
        public const int NON_EXISTENT_IMAGE_JPG = 720;
    }

    public static class ThumbnailWidthAsset
    {
        public const int HOMER_GIF = 150;
        public const int HOMER_JPG = 200;
        public const int HOMER_DUPLICATED_JPG = 200;
        public const int IMAGE_1_JPG = 200;
        public const int IMAGE_1_90_DEG_JPG = 84;
        public const int IMAGE_1_180_DEG_JPG = 200;
        public const int IMAGE_1_270_DEG_JPG = 84;
        public const int IMAGE_1_DUPLICATE_JPG = 200;
        public const int IMAGE_1_DUPLICATE_COPIED_JPG = 200;
        public const int IMAGE_2_JPG = 200;
        public const int IMAGE_2_DUPLICATED_JPG = 200;
        public const int IMAGE_3_JPG = 200;
        public const int IMAGE_4_JPG = 200;
        public const int IMAGE_5_JPG = 200;
        public const int IMAGE_6_JPG = 200;
        public const int IMAGE_7_JPG = 200;
        public const int IMAGE_8_JPEG = 200;
        public const int IMAGE_9_PNG = 200;
        public const int IMAGE_9_DUPLICATE_PNG = 200;
        public const int IMAGE_10_PORTRAIT_PNG = 84;
        public const int IMAGE_11_HEIC = 112;
        public const int IMAGE_11_90_DEG_HEIC = 200;
        public const int IMAGE_11_180_DEG_HEIC = 112;
        public const int IMAGE_11_270_DEG_HEIC = 200;
        public const int IMAGE_WITH_UPPERCASE_NAME_JPG = 200;
        public const int _1336_JPG = 200;
        public const int _1336_ORIGINAL_JPG = 200;
        public const int _1336_1_K_JPG = 200;
        public const int _1336_2_K_JPG = 200;
        public const int _1336_3_K_JPG = 200;
        public const int _1336_4_K_ORIGINAL_JPG = 200;
        public const int _1336_8_K_JPG = 200;
        public const int _1336_THUMBNAIL_JPG = 200;
        public const int IMAGE_1336_ORIGINAL_JPG = 200;
        public const int IMAGE_1336_MINI_JPG = 200;
        public const int IMAGE_1336_SHIT_QUALITY_JPG = 200;
        public const int IMAGE_1336_SMALL_JPG = 200;
        public const int _1336_BOTTOM_LEFT_PART_JPG = 200;
        public const int _1336_BOTTOM_PART_JPG = 200;
        public const int _1336_BOTTOM_RIGHT_PART_JPG = 200;
        public const int _1336_LEFT_PART_JPG = 100;
        public const int _1336_RIGHT_PART_JPG = 100;
        public const int _1336_TOP_LEFT_PART_JPG = 200;
        public const int _1336_TOP_PART_JPG = 200;
        public const int _1336_TOP_RIGHT_PART_JPG = 200;
        public const int _1337_JPG = 200;
        public const int _1349_JPG = 200;
        public const int _1350_JPG = 200;
        public const int _1413_JPG = 200;
        public const int _1414_JPG = 200;
        public const int _1415_JPG = 112;

        public const int NON_EXISTENT_FILE_JPG = 0;
        public const int NON_EXISTENT_IMAGE_JPG = 200;
    }

    public static class ThumbnailHeightAsset
    {
        public const int HOMER_GIF = 150;
        public const int HOMER_JPG = 112;
        public const int HOMER_DUPLICATED_JPG = 112;
        public const int IMAGE_1_JPG = 112;
        public const int IMAGE_1_90_DEG_JPG = 150;
        public const int IMAGE_1_180_DEG_JPG = 112;
        public const int IMAGE_1_270_DEG_JPG = 150;
        public const int IMAGE_1_DUPLICATE_JPG = 112;
        public const int IMAGE_1_DUPLICATE_COPIED_JPG = 112;
        public const int IMAGE_2_JPG = 112;
        public const int IMAGE_2_DUPLICATED_JPG = 112;
        public const int IMAGE_3_JPG = 112;
        public const int IMAGE_4_JPG = 112;
        public const int IMAGE_5_JPG = 112;
        public const int IMAGE_6_JPG = 112;
        public const int IMAGE_7_JPG = 112;
        public const int IMAGE_8_JPEG = 112;
        public const int IMAGE_9_PNG = 112;
        public const int IMAGE_9_DUPLICATE_PNG = 112;
        public const int IMAGE_10_PORTRAIT_PNG = 150;
        public const int IMAGE_11_HEIC = 150;
        public const int IMAGE_11_90_DEG_HEIC = 150;
        public const int IMAGE_11_180_DEG_HEIC = 150;
        public const int IMAGE_11_270_DEG_HEIC = 150;
        public const int IMAGE_WITH_UPPERCASE_NAME_JPG = 112;
        public const int _1336_JPG = 150;
        public const int _1336_ORIGINAL_JPG = 150;
        public const int _1336_1_K_JPG = 150;
        public const int _1336_2_K_JPG = 150;
        public const int _1336_3_K_JPG = 150;
        public const int _1336_4_K_ORIGINAL_JPG = 150;
        public const int _1336_8_K_JPG = 150;
        public const int _1336_THUMBNAIL_JPG = 150;
        public const int IMAGE_1336_ORIGINAL_JPG = 150;
        public const int IMAGE_1336_MINI_JPG = 149;
        public const int IMAGE_1336_SHIT_QUALITY_JPG = 150;
        public const int IMAGE_1336_SMALL_JPG = 150;
        public const int _1336_BOTTOM_LEFT_PART_JPG = 150;
        public const int _1336_BOTTOM_PART_JPG = 75;
        public const int _1336_BOTTOM_RIGHT_PART_JPG = 150;
        public const int _1336_LEFT_PART_JPG = 150;
        public const int _1336_RIGHT_PART_JPG = 150;
        public const int _1336_TOP_LEFT_PART_JPG = 150;
        public const int _1336_TOP_PART_JPG = 75;
        public const int _1336_TOP_RIGHT_PART_JPG = 150;
        public const int _1337_JPG = 150;
        public const int _1349_JPG = 150;
        public const int _1350_JPG = 150;
        public const int _1413_JPG = 150;
        public const int _1414_JPG = 150;
        public const int _1415_JPG = 150;

        public const int NON_EXISTENT_FILE_JPG = 0;
        public const int NON_EXISTENT_IMAGE_JPG = 112;
    }

    public static class FileSize
    {
        public const long HOMER_GIF = 64123;
        public const long HOMER_JPG = 6599;
        public const long HOMER_DUPLICATED_JPG = 6599;
        public const long IMAGE_1_JPG = 29857;
        public const long IMAGE_1_90_DEG_JPG = 31535;
        public const long IMAGE_1_180_DEG_JPG = 31535;
        public const long IMAGE_1_270_DEG_JPG = 31535;
        public const long IMAGE_1_DUPLICATE_JPG = 29857;
        public const long IMAGE_1_DUPLICATE_COPIED_JPG = 29857;
        public const long IMAGE_2_JPG = 30197;
        public const long IMAGE_2_DUPLICATED_JPG = 30197;
        public const long IMAGE_3_JPG = 30276;
        public const long IMAGE_4_JPG = 30115;
        public const long IMAGE_5_JPG = 30117;
        public const long IMAGE_6_JPG = 30366;
        public const long IMAGE_7_JPG = 29974;
        public const long IMAGE_8_JPEG = 30514;
        public const long IMAGE_9_PNG = 126277;
        public const long IMAGE_9_DUPLICATE_PNG = 126277;
        public const long IMAGE_10_PORTRAIT_PNG = 153318;
        public const long IMAGE_11_HEIC = 1411940;
        public const long IMAGE_11_90_DEG_HEIC = 1411940;
        public const long IMAGE_11_180_DEG_HEIC = 1423616;
        public const long IMAGE_11_270_DEG_HEIC = 1423616;
        public const long IMAGE_WITH_UPPERCASE_NAME_JPG = 40937;
        public const long _1336_JPG = 4526710;
        public const long _1336_ORIGINAL_JPG = 4526710;
        public const long _1336_1_K_JPG = 311067;
        public const long _1336_2_K_JPG = 1154116;
        public const long _1336_3_K_JPG = 2098339;
        public const long _1336_4_K_ORIGINAL_JPG = 4526710;
        public const long _1336_8_K_JPG = 8855421;
        public const long _1336_THUMBNAIL_JPG = 19472;
        public const long IMAGE_1336_ORIGINAL_JPG = 4526710;
        public const long IMAGE_1336_MINI_JPG = 1586;
        public const long IMAGE_1336_SHIT_QUALITY_JPG = 370819;
        public const long IMAGE_1336_SMALL_JPG = 53227;
        public const long _1336_BOTTOM_LEFT_PART_JPG = 1119873;
        public const long _1336_BOTTOM_PART_JPG = 2172866;
        public const long _1336_BOTTOM_RIGHT_PART_JPG = 1061673;
        public const long _1336_LEFT_PART_JPG = 1475892;
        public const long _1336_RIGHT_PART_JPG = 1437903;
        public const long _1336_TOP_LEFT_PART_JPG = 362012;
        public const long _1336_TOP_PART_JPG = 737269;
        public const long _1336_TOP_RIGHT_PART_JPG = 383508;
        public const long _1337_JPG = 4003090;
        public const long _1349_JPG = 2670045;
        public const long _1350_JPG = 2959497;
        public const long _1413_JPG = 3633142;
        public const long _1414_JPG = 3700191;
        public const long _1415_JPG = 2264917;

        public const long NON_EXISTENT_IMAGE_JPG = 0;
    }

    public static class ImageByteSizes
    {
        public const int HOMER_GIF = 8594;
        public const int HOMER_JPG = 4819;
        public const int HOMER_DUPLICATED_JPG = 4819;
        public const int IMAGE_1_JPG = 2100;
        public const int IMAGE_1_90_DEG_JPG = 1737;
        public const int IMAGE_1_180_DEG_JPG = 2102;
        public const int IMAGE_1_270_DEG_JPG = 1716;
        public const int IMAGE_1_DUPLICATE_JPG = 2100;
        public const int IMAGE_1_DUPLICATE_COPIED_JPG = 2100;
        public const int IMAGE_2_JPG = 2109;
        public const int IMAGE_2_DUPLICATED_JPG = 2109;
        public const int IMAGE_3_JPG = 2111;
        public const int IMAGE_4_JPG = 2108;
        public const int IMAGE_5_JPG = 2107;
        public const int IMAGE_6_JPG = 2110;
        public const int IMAGE_7_JPG = 2105;
        public const int IMAGE_8_JPEG = 2115;
        public const int IMAGE_9_PNG = 10583;
        public const int IMAGE_9_DUPLICATE_PNG = 10583;
        public const int IMAGE_10_PORTRAIT_PNG = 5852;
        public const int IMAGE_11_HEIC = 5851;
        public const int IMAGE_11_90_DEG_HEIC = 5921;
        public const int IMAGE_11_180_DEG_HEIC = 5713;
        public const int IMAGE_11_270_DEG_HEIC = 6070;
        public const int IMAGE_WITH_UPPERCASE_NAME_JPG = 3019;
        public const int _1336_ORIGINAL_JPG = 10753;
        public const int _1336_1_K_JPG = 10312;
        public const int _1336_2_K_JPG = 10226;
        public const int _1336_3_K_JPG = 10576;
        public const int _1336_4_K_ORIGINAL_JPG = 10753;
        public const int _1336_8_K_JPG = 10836;
        public const int _1336_THUMBNAIL_JPG = 10880;
        public const int IMAGE_1336_ORIGINAL_JPG = 10753;
        public const int IMAGE_1336_MINI_JPG = 4769;
        public const int IMAGE_1336_SHIT_QUALITY_JPG = 11304;
        public const int IMAGE_1336_SMALL_JPG = 10592;
        public const int _1336_BOTTOM_LEFT_PART_JPG = 13128;
        public const int _1336_BOTTOM_PART_JPG = 7563;
        public const int _1336_BOTTOM_RIGHT_PART_JPG = 13254;
        public const int _1336_LEFT_PART_JPG = 5817;
        public const int _1336_RIGHT_PART_JPG = 5911;
        public const int _1336_TOP_LEFT_PART_JPG = 5752;
        public const int _1336_TOP_PART_JPG = 4147;
        public const int _1336_TOP_RIGHT_PART_JPG = 6045;
        public const int _1337_JPG = 8467;
        public const int _1349_JPG = 7834;
        public const int _1350_JPG = 8921;
        public const int _1413_JPG = 8733;
        public const int _1414_JPG = 8567;
        public const int _1415_JPG = 5055;
    }

    public static class ModificationDate
    {
        public static readonly DateTime Default = new (2024, 06, 07, 08, 54, 37);
    }

    public static class Hashes
    {
        public const string HOMER_GIF = "c48b1f61f3a3a004f425d8493d30a50ae14408ed4c5354bf4d0ca40069f91951381a7df32ee7455a6edef0996c95571557a9993021331ff2dfbc3ccc7f0c8ff1";
        public const string HOMER_JPG = "59742f2cd31c0997be96f9e758799d975f5918f7732f351d66280a708681ea74ccbfa1b61a327835a3f1dbb5ea5f9989484764a10f56f7dd6f32f7b24e286d66";
        public const string HOMER_DUPLICATED_JPG = "59742f2cd31c0997be96f9e758799d975f5918f7732f351d66280a708681ea74ccbfa1b61a327835a3f1dbb5ea5f9989484764a10f56f7dd6f32f7b24e286d66";
        public const string IMAGE_1_JPG =             "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
        public const string IMAGE_1_90_DEG_JPG =             "0092081bd0a83e9253f4eb319b4cfab197a09ed3b28b5697c31f17bf2646638dec95bf28d4f454dda8ff3dcdaab33fa002774f13dfe6d7aee82f1a960e3e3aee";
        public const string IMAGE_1_180_DEG_JPG = "399383d82d4b7ed8233f714a1d14272b276b80505d7b3a2935b6ec962fd5cf280ccedb3e9380df1a11ee65e5f5fa66278dd2f407b7b6bdfa3093319164c446dd";
        public const string IMAGE_1_270_DEG_JPG = "43950e06cecd509509d868a2064a828aeb6ff6a9a30f296ec9a3621b269742e75dacb40300aa6927c4c8f0879268e1ab3d7eac228b38cade35cc9b7c6adf3449";
        public const string IMAGE_1_DUPLICATE_JPG = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
        public const string IMAGE_1_DUPLICATE_COPIED_JPG = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
        public const string IMAGE_2_JPG = "0b6d010f85544871c307bb3a96028402f55fa29094908cdd0f74a8ec8d3fc3d4fbec995d98b89aafef3dcf5581c018fbb50481e33c7e45aef552d66c922f4078";
        public const string IMAGE_2_DUPLICATED_JPG = "0b6d010f85544871c307bb3a96028402f55fa29094908cdd0f74a8ec8d3fc3d4fbec995d98b89aafef3dcf5581c018fbb50481e33c7e45aef552d66c922f4078";
        public const string IMAGE_3_JPG = "a0918addfe12765fc9a2e437b452a6bf44561a97a568c9c58ffaa6b22b94a9934a6d760739c7c1b70e2dc83f6686e34ee13f520a5e9e33380a6b88be4f866ab0";
        public const string IMAGE_4_JPG = "73cbdcfc710f5310b275c22519116b1c1e108a1540bf3c43ae539116d1026d3f55277a83785a798f37701cf1fe757d5c0f56cc6019e1b574d578f9f2db45aa4f";
        public const string IMAGE_5_JPG = "d372752a168f353d8360915f7d157d81a077581e4a391c446cb5897902ce6b5627025befa3fea2672167f1d44c2fb8b1008ab262c7bdb6f26ab974434084e609";
        public const string IMAGE_6_JPG = "495f3967cd1255f09b630f9c9a9660373aeefedc1a1c4937034865fc3f1e09e6fa637027e8d9f7ab19ce7f0725b826fbf4d45f0bbeea747ab4b701a506960b35";
        public const string IMAGE_7_JPG = "e2e5b1008e4ee77111e955083c5dc6be2f7c870e5a6457f02108e3b83041dc5efc1e1d8d901ddb55baa7ca482dd44fa79ad71539c6b2f75ffc494c5617352bdc";
        public const string IMAGE_8_JPEG = "156f81ceb3db1bfdf27618efe4b89575f17d4708053c1a9ce6c5dfb7f36a6b958a5c3f45147728de28cf538345336e7b81d01b3f5908d4677ce76e1661e97ac3";
        public const string IMAGE_9_PNG = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";
        public const string IMAGE_9_DUPLICATE_PNG = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";
        public const string IMAGE_10_PORTRAIT_PNG = "7ad01e9fe639957a9e8eaddc7fd864068f4a03f9981fd480f310740a43a0a4f2b8fa7a80d9c83039c46fcfbb63a5e465adaf07d33191369590adcda1586b1c94";
        public const string IMAGE_11_HEIC = "f52bd860f5ad7f81a92919e5fb5769d3e86778b2ade74832fbd3029435c85e59cb64b3c2ce425445a49917953e6e913c72b81e48976041a4439cb65e92baf18d";
        public const string IMAGE_11_90_DEG_HEIC = "b75df4942de1efa7d4326fedd721e99cc43959a1c22098a8cfcf6861aea099741d9c98fa1615e54d53838c22b9c6ff3b328c4ee3c0875d2b4ac7f7dee8d61bcf";
        public const string IMAGE_11_180_DEG_HEIC = "a2ad3ac9d51f74230b9f1d554172781d9cc214d01f8344354e2e56dc3daecb5a0021d4d2814eba0cde6fd681f3a8e8b8b55fbfad7184740e3518190e05dca77f";
        public const string IMAGE_11_270_DEG_HEIC = "d4d7342a783f0f1b5e10ad9b493b71dd688dbc31b8ddd5534c9fff9c5fdbfa8bee32ef4e4258f7514a1b57f9a12b571454c8b66172b1a66bf1b53953b66227d5";
        public const string IMAGE_WITH_UPPERCASE_NAME_JPG = "0465fc2af396e003a3f45bf71e951b905ca7d19c21c36d47cd7b2524190862e38cb4c0da6fdd1dbd89b5b7050d837be664bc45693657ec7681e4d9b648044c91";
        public const string _1336_JPG = "fb0bb3887c28d022113c5c52a65f9da14eab0806842f902b618806dedeb7c53da1ed26ae5bcaf9a358e81efead71c4807ea38054e44f426857f8bf2c1fd1c7aa";
        public const string _1336_ORIGINAL_JPG = "fb0bb3887c28d022113c5c52a65f9da14eab0806842f902b618806dedeb7c53da1ed26ae5bcaf9a358e81efead71c4807ea38054e44f426857f8bf2c1fd1c7aa";
        public const string _1336_1_K_JPG = "4dcbbf63585dc9fff056a0370ee33efc3d7c57b556bce917f89c5012fbacbc1ee2929f7d3a11ee2f8af426b183212d67167d87e48b80707ab5a1fb4e766d2544";
        public const string _1336_2_K_JPG = "76a8f0b611fdd32b73bff92a186449a7e4bf0b84d618fe8b9666bbf2b022ff6bf259a4a98be42db7d9043ff3c3ff20ae720c5067966c40d16ad3fc67769f514b";
        public const string _1336_3_K_JPG = "98044e6674187219ab629c901dfdf5a72f2535104d83cd167d1bdd9039ded91b7a585b74c5c89860f2e0fab85af9e2181265cba48b908028a42b8ec042265f03";
        public const string _1336_4_K_ORIGINAL_JPG = "fb0bb3887c28d022113c5c52a65f9da14eab0806842f902b618806dedeb7c53da1ed26ae5bcaf9a358e81efead71c4807ea38054e44f426857f8bf2c1fd1c7aa";
        public const string _1336_8_K_JPG = "18289324fb22b8b337b1f4a7d627c605056e04fd308699bc831af0579cbc2d6a5959811dbecce09b26f82c341ea82d02d5c99c1580f50c24106d19c88bd6eba2";
        public const string _1336_THUMBNAIL_JPG = "0bbabdb2fe28d4ef53b78a917eb89938fe564d758ebb9cb011e5a675dbee0a8e92bc38b8fe0762e279d48e95e7d17f1bb392fd96e8db7006e9e0bb60c5d2ed31";
        public const string IMAGE_1336_ORIGINAL_JPG = "fb0bb3887c28d022113c5c52a65f9da14eab0806842f902b618806dedeb7c53da1ed26ae5bcaf9a358e81efead71c4807ea38054e44f426857f8bf2c1fd1c7aa";
        public const string IMAGE_1336_MINI_JPG = "768d0fd37e280f1ef8d2a17702626ca1b290521167c22b0aecadd3cc2f858a88ed59a72f7a4b36c123f487da69842e55e4407de0dd248076947ce7df53e92be4";
        public const string IMAGE_1336_SHIT_QUALITY_JPG = "0861602a84f5bf56e5f4acf49c1d98da8129542e871208a9c9f25eb497cdc65a75ccf4941874c2be7f4e36309053fe34d61a42b4d1b266910918a0b68f19cfc2";
        public const string IMAGE_1336_SMALL_JPG = "787146674307ab1e867c0961968dfea28aafc7d45e6cfd83f1562fbd201d029812289cd692c36a4f537c576b25af03c9f6f3cc996a8651f3c69f8240ecb3d5f4";
        public const string _1336_BOTTOM_LEFT_PART_JPG = "c50e17cee9a52c0235afb410c361d094daf7330c20e4b3f3cb3e3e18683793355e903f84d49bf1db4af7e094980204cc005aa3c503829d14c9e6cd19826e284f";
        public const string _1336_BOTTOM_PART_JPG = "aef59f88b3c54a5fd1bfc098acac1d662bb3799e83220049ee7c3ae5def487a1464112cce7b7384428ee9b69813448d509413a1fc1abcddc272ec2e044c5f876";
        public const string _1336_BOTTOM_RIGHT_PART_JPG = "966370f38d7864499453b99934bb51f1654e567033e637b68ded1ec12c338eaa03d42bb4dc74621551c5dfd922180a291f7c58980bf6c94ae114e4d296a484e4";
        public const string _1336_LEFT_PART_JPG = "8afa3f673b047d5096ed2a2528b37c7222ab5b44cf780a7920cea35228e93323709b09b1a2ffe1bd2f789061be9bef8ebcb642fc47a2f3b6573a68a8cd5045ee";
        public const string _1336_RIGHT_PART_JPG = "0e0e12ea70894ad521c8d3da14b92bbd4e7c7c03010502d779f2e485ea30e917ad6a9bc0435520f70c3448fd96cd9ccf88cf95d2a75a6da669b0f41731aabbe7";
        public const string _1336_TOP_LEFT_PART_JPG = "f59b3913bdd53b6cb485a2c04770d1153350df2caf6dabf38b8b8311fb3fb583becbf7d368a1e0987d3584251a64bca72042d334b6e6c25edb7dfaead2db383d";
        public const string _1336_TOP_PART_JPG = "93d6ea1575ea8fd4f540b88ae3ce727d28c657a11388c9b6cf927572abffdcc467d3e9b43e622b34158c18ed680c841d43e17a80c7b544261f106d887b51d2d2";
        public const string _1336_TOP_RIGHT_PART_JPG = "3fde05f0e85d71a909fa38351411f3d45305857ea7a920ad22bcffaf4cf50136b4a55acb128bdc9cd9d7db600332f83e4157926a511939b0b57f2e50c020e3d7";
        public const string _1337_JPG = "4435b989c27e16be1bf583fb6bb4b3474bd55735447a52e663482703553c5ce38947b17f88a2bfd9662956b24575e7578f7b7d921fdba2273d6046ee765eec36";
        public const string _1349_JPG = "715188ad52e6bd3dc9dfb7c0469038d92638dae94b7818f6326da093505f18b60d277831d451e259809af7dad3149cc2babf7442d1956ceca911f75180365cec";
        public const string _1350_JPG = "a4e02e2ac49b00c7b1b5f53d76d397b88a01148b16d920be66f8f6b7ebe6cb199e964a5129ee41a80db67b699bc3ffa04c6d87e8a2d89cdf46a8130287d11e7d";
        public const string _1413_JPG = "7c2961e6e6842b31a1dea11bfb655c5a2afba42b15c66c2dc155e6b86f2e510c0e2196149fa6d5e94e1c799c8a58503235f6f5f09b9cf618402c927991b6ae4c";
        public const string _1414_JPG = "4d4e53fa11a8e2ffde7e02fa774c959a87c24b9f70ab3dbb8ff6c3384808bfc9a822adb85ae77bef690f93469b2d575bc995317e2af5a63ae3151a04c5df57c7";
        public const string _1415_JPG = "5f122c429a160e0eb1df1a6b77cea6ee4c13b731ebff02c79646314d5bf57070d4fae13a1a8821927d7954f83ae706c937affb452ed32a38dc800e00ef0f6176";

        public const string EMPTY_IMAGE = "cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e";

        public const int LENGTH = 128; // SHA-512 generates a 128-character long hash in hexadecimal representation
    }
    
    public static class DHashes
    {
        public const string HOMER_GIF = "00000000000000";
        public const string HOMER_JPG = "00000000000000";
        public const string IMAGE_1_JPG = "54aaa1554aaa14";
        public const string IMAGE_1_90_DEG_JPG = "54aaa1554aaa14";
        public const string IMAGE_1_180_DEG_JPG = "54aaa1554aaa14";
        public const string IMAGE_1_270_DEG_JPG = "54aaa1554aaa14";
        public const string IMAGE_1_DUPLICATE_JPG = "54aaa1554aaa14";
        public const string IMAGE_2_JPG = "54aaa1554aaa14";
        public const string IMAGE_2_DUPLICATED_JPG = "54aaa1554aaa14";
        public const string IMAGE_3_JPG = "54aaa1554aaa14";
        public const string IMAGE_4_JPG = "54aaa1554aaa14";
        public const string IMAGE_5_JPG = "54aaa1554aaa14";
        public const string IMAGE_6_JPG = "54aaa1554aaa14";
        public const string IMAGE_7_JPG = "54aaa1554aaa14";
        public const string IMAGE_8_JPEG = "54aaa1554aaa14";
        public const string IMAGE_9_PNG = "00088044010900";
        public const string IMAGE_9_DUPLICATE_PNG = "00088044010900";
        public const string IMAGE_10_PORTRAIT_PNG = "80500d40555402";
        public const string IMAGE_11_HEIC = "00000000000000";
        public const string IMAGE_11_90_DEG_HEIC = "347636785c99d1";
        public const string IMAGE_11_180_DEG_HEIC = "bd7b746d992246";
        public const string IMAGE_11_270_DEG_HEIC = "00000000000000";
        public const string IMAGE_WITH_UPPERCASE_NAME_JPG = "54aaa1554aaa14";
        public const string _1336_JPG = "00000041040892";
        public const string _1336_ORIGINAL_JPG = "00000041040892";
        public const string _1336_1_K_JPG = "04108000800000";
        public const string _1336_2_K_JPG = "0e0986000080a0";
        public const string _1336_3_K_JPG = "c187020002060c";
        public const string _1336_4_K_ORIGINAL_JPG = "00000041040892";
        public const string _1336_8_K_JPG = "00000010410409";
        public const string _1336_THUMBNAIL_JPG = "00000000408242";
        public const string IMAGE_1336_ORIGINAL_JPG = "00000041040892";
        public const string IMAGE_1336_MINI_JPG = "d9f3e47d581798";
        public const string IMAGE_1336_SHIT_QUALITY_JPG = "00000000000000";
        public const string IMAGE_1336_SMALL_JPG = "00000000000000";
        public const string _1336_BOTTOM_LEFT_PART_JPG = "60808000000285";
        public const string _1336_BOTTOM_PART_JPG = "60808000000285";
        public const string _1336_BOTTOM_RIGHT_PART_JPG = "ddeddbb1f3b66c";
        public const string _1336_LEFT_PART_JPG = "20410021024914";
        public const string _1336_RIGHT_PART_JPG = "1870c1041830e0";
        public const string _1336_TOP_LEFT_PART_JPG = "10408c410408a2";
        public const string _1336_TOP_PART_JPG = "20410021024914";
        public const string _1336_TOP_RIGHT_PART_JPG = "1870c1041830e0";
        public const string _1337_JPG = "00000000000000";
        public const string _1349_JPG = "01000000004104";
        public const string _1350_JPG = "c20000808101c3";
        public const string _1413_JPG = "df8c811878c307";
        public const string _1414_JPG = "ceccd9b1d3089b";
        public const string _1415_JPG = "20e0e79e2613b6";

        public const int LENGTH = 14; // The DHash is a 14-hex digits
    }

    public static class MD5Hashes
    {
        public const string HOMER_GIF = "a409ce713de9334117791b15a586dd0e";
        public const string IMAGE_1_JPG = "a886f237e1cf6da03fb793588fd79b64";
        public const string IMAGE_1_90_DEG_JPG = "e740c5b99b0c87d86b9b1b3f322dec7e";
        public const string IMAGE_1_180_DEG_JPG = "bc43ad1ebd118a2f9443b3199d4127c8";
        public const string IMAGE_1_270_DEG_JPG = "f8f724518021547c26ef9262f29f6b9a";
        public const string IMAGE_1_DUPLICATE_JPG = "a886f237e1cf6da03fb793588fd79b64";
        public const string IMAGE_2_JPG = "27007861db67443498b6aa0d00808b41";
        public const string IMAGE_2_DUPLICATED_JPG = "27007861db67443498b6aa0d00808b41";
        public const string IMAGE_3_JPG = "adb03c1eda4dc85e7e67ba0b69c10b74";
        public const string IMAGE_4_JPG = "e243d9680908721b4ba66d59f62aac5b";
        public const string IMAGE_5_JPG = "8b3a1061da05bad5463ea53c387f952b";
        public const string IMAGE_6_JPG = "84203a93ca05b45de6bd6ef5bbf184c9";
        public const string IMAGE_7_JPG = "148817a75c57bc5ae960df3354e04327";
        public const string IMAGE_8_JPEG = "4194e88c247b5bbc7a1c6294cc795466";
        public const string IMAGE_9_PNG = "05d29088572bd5a5970fc370c0ca8751";
        public const string IMAGE_9_DUPLICATE_PNG = "05d29088572bd5a5970fc370c0ca8751";
        public const string IMAGE_10_PORTRAIT_PNG = "02b09a63d382bc1a1f88afa125f3adb3";
        public const string IMAGE_11_HEIC = "d9aa281b55316e10c767877dee265732";
        public const string IMAGE_11_90_DEG_HEIC = "e3aeca5fc781bd52470f9186653d3a3f";
        public const string IMAGE_11_180_DEG_HEIC = "9bf6a1a37331ba6f391b885b6dcc67a1";
        public const string IMAGE_11_270_DEG_HEIC = "35d24aaf7f6fc8b9872f069a8c28ab78";
        public const string IMAGE_WITH_UPPERCASE_NAME_JPG = "b70782c592e3e88f16291acf31d1b133";
        public const string _1336_JPG = "bed3d05d409001724e5f6095c2acaa95";
        public const string _1336_ORIGINAL_JPG = "bed3d05d409001724e5f6095c2acaa95";
        public const string _1336_1_K_JPG = "80bbaf9483ef57ae043863fa954615c6";
        public const string _1336_2_K_JPG = "83355feec0abe189e3b3f1bcff375151";
        public const string _1336_3_K_JPG = "02433546c8d0b8a9b76f861077924f85";
        public const string _1336_4_K_ORIGINAL_JPG = "bed3d05d409001724e5f6095c2acaa95";
        public const string _1336_8_K_JPG = "961b08d1e4b7d9ed07fd9c5b83d23edb";
        public const string _1336_THUMBNAIL_JPG = "dd33165a8c3329683ca1ce2bb90ed1b2";
        public const string IMAGE_1336_ORIGINAL_JPG = "bed3d05d409001724e5f6095c2acaa95";
        public const string IMAGE_1336_MINI_JPG = "051a784180ba8c0fce93ce0b81add76c";
        public const string IMAGE_1336_SHIT_QUALITY_JPG = "5ec22ad598e3518bf7a5b78344ee8543";
        public const string IMAGE_1336_SMALL_JPG = "1f060a67916d271bd37c366d5e1287e8";
        public const string _1336_BOTTOM_LEFT_PART_JPG = "1691eb898f26c38b8b5d66b5d5ed71c6";
        public const string _1336_BOTTOM_PART_JPG = "bac5ead516448cc2a657ea75251ae88d";
        public const string _1336_BOTTOM_RIGHT_PART_JPG = "25830c1fb500800d426c8832d7fabb9e";
        public const string _1336_LEFT_PART_JPG = "55370d738498beff0963693512d2d94f";
        public const string _1336_RIGHT_PART_JPG = "01b35414fd99b583a5261ff83260b698";
        public const string _1336_TOP_LEFT_PART_JPG = "58695d954982417e68b68975beff02c4";
        public const string _1336_TOP_PART_JPG = "216eaa3b37a658bd0d288d858022c7e1";
        public const string _1336_TOP_RIGHT_PART_JPG = "1ec7dfdec1b6e9aa8ad5f50aae638e0e";
        public const string _1337_JPG = "49f629639e6a86f43cc6863da60edc16";
        public const string _1349_JPG = "40b609ff0eafe7c6a767e05daed14850";
        public const string _1350_JPG = "a0501328ef7e513beadd169965353a0b";
        public const string _1413_JPG = "c77db382d58e7cecca32bf39c966c8b9";
        public const string _1414_JPG = "62ae52fd23a004cb2b70fdbf631712bf";
        public const string _1415_JPG = "8a81b304ab3be092d2e46085fe818782";

        public const string EMPTY_IMAGE = "d41d8cd98f00b204e9800998ecf8427e";

        public const int LENGTH = 32; // The MD5Hash is a 32-character hexadecimal string
    }

    public static class PHashes
    {
        public const string HOMER_GIF = "a6aed62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a738962ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0abca685e0f877208957c62ee062ee062ee0af783881028a11d8ad6962ee062ee062ee0";
        public const string IMAGE_1_JPG = "a505b8264162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a58f7827fa62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0add4f8456d8d85a8b4e062ee062ee062d57afbaa849d28fc2b8dc1262ee062ee062ee0";
        public const string IMAGE_1_90_DEG_JPG = "a505b8264162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a58f7827fa62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0add4f8456d8d85a8b4e062ee062ee062d57afbaa849d28fc2b8dc1262ee062ee062ee0";
        public const string IMAGE_1_180_DEG_JPG = "a505b8264162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a58f7827fa62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0add4f8456d8d85a8b4e062ee062ee062d57afbaa849d28fc2b8dc1262ee062ee062ee0";
        public const string IMAGE_1_270_DEG_JPG = "a505b8264162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a58f7827fa62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0add4f8456d8d85a8b4e062ee062ee062d57afbaa849d28fc2b8dc1262ee062ee062ee0";
        public const string IMAGE_1_DUPLICATE_JPG = "a505b8264162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a58f7827fa62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0add4f8456d8d85a8b4e062ee062ee062d57afbaa849d28fc2b8dc1262ee062ee062ee0";
        public const string IMAGE_2_JPG = "a505b8264162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a58f7827fa62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0add3d8456a8d85d8b4df62ee062ee062e57afba2849d18fc388dc1662ee062ee062ee0";
        public const string IMAGE_2_DUPLICATED_JPG = "a505b8264162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a58f7827fa62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0add3d8456a8d85d8b4df62ee062ee062e57afba2849d18fc388dc1662ee062ee062ee0";
        public const string IMAGE_3_JPG = "a505b8264162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a58f7827fa62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0add34845688d8638b4e062ee062ee062ee0afb9e849d08fc428dc1a62ee062ee062ee0";
        public const string IMAGE_4_JPG = "a505b8264162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a58f7827fa62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0add2d845678d8648b4df62ee062ee062ee0afb9b849cf8fc488dc1b62ee062ee062ee0";
        public const string IMAGE_5_JPG = "a505b8264162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a58f7827fa62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0add35845688d8668b4e162ee062ee062ee0afb9e849d08fc458dc1a62ee062ee062ee0";
        public const string IMAGE_6_JPG = "a505b8264162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a58f7827fa62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0add1c845638d8638b4dc62ee062ee062ee0afb93849ce8fc508dc1d62ee062ee062ee0";
        public const string IMAGE_7_JPG = "a505b8264162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a58f7827fa62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0add538456e8d8738b4eb62ee062ee062d43afbac849d38fc408dc1b62ee062ee062ee0";
        public const string IMAGE_8_JPEG = "a505b8264162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a58f7827fa62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0add078455f8d8688b4db62ee062ee062ee0afb8a849cc8fc608dc2362ee062ee062ee0";
        public const string IMAGE_9_PNG = "a505b8264162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a58f7827fa62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0adc32845338d8e28b4a762ee062ee062ee0afb2d849b98fd038dc1062ee062ee062ee0";
        public const string IMAGE_9_DUPLICATE_PNG = "a505b8264162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a58f7827fa62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0adc32845338d8e28b4a762ee062ee062ee0afb2d849b98fd038dc1062ee062ee062ee0";
        public const string IMAGE_10_PORTRAIT_PNG = "a505b8264162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a58f7827fa62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0adbc58451f8d8dd8b49462ee062ee062ee0afaff849b08fd348dc1f62ee062ee062ee0";
        public const string IMAGE_11_HEIC = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0b1ae2820bc83d0c84a5388e0462ee08d539a929b84a9b881408946061cba8d60062ee0";
        public const string IMAGE_11_90_DEG_HEIC = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0b1adb820bf83d0a84a6188e1762ee08d8c2a929e84a9d881418946b61cbc8d62a62ee0";
        public const string IMAGE_11_180_DEG_HEIC = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0b1ade820bd83d0f84a5688e0962ee08d635a929d84a9b881438946161cbb8d60362ee0";
        public const string IMAGE_11_270_DEG_HEIC = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0b1ad8820c083d0e84a6388e1b62ee08d9c0a92a084a9d881458946c61cbd8d62b62ee0";
        public const string IMAGE_WITH_UPPERCASE_NAME_JPG = "a505b8264162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a58f7827fa62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0ad810844d58d60b8b35e62ee062ee062a61af946849858fab58dc6b62ee062ee062ee0";
        public const string _1336_JPG = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a572f82804892c286dad8f28562ee08f53dabf9e847058af2e892da61f5062ee061f71";
        public const string _1336_ORIGINAL_JPG = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a572f82804892c286dad8f28562ee08f53dabf9e847058af2e892da61f5062ee061f71";
        public const string _1336_1_K_JPG = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a560c827bc8927a86d5d8f1ff62ee08f485abf0a846dd8aef48929a61f4562ee061f62";
        public const string _1336_2_K_JPG = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a560e827bd8927486d5e8f1f962ee08f487abf0b846dd8aef18929961f4562ee061f62";
        public const string _1336_3_K_JPG = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a54f1827778922e86d108f17062ee08f3ddabe7a846b68aeb78925961f3a62ee061f54";
        public const string _1336_4_K_ORIGINAL_JPG = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a572f82804892c286dad8f28562ee08f53dabf9e847058af2e892da61f5062ee061f71";
        public const string _1336_8_K_JPG = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a56c6827ec8928a86d8e8f24b62ee08f4deabf6a846f88af0c892c361f4d62ee061f6a";
        public const string _1336_THUMBNAIL_JPG = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a5617827be8925d86d628f21562ee08f458abf0f846de8aeee8929b61f4762ee061f60";
        public const string IMAGE_1336_ORIGINAL_JPG = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a572f82804892c286dad8f28562ee08f53dabf9e847058af2e892da61f5062ee061f71";
        public const string IMAGE_1336_MINI_JPG = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a5267826b58933d86cb98f0ab62ee08f50eabd4f8464c8af0d8926261f3762ee061f63";
        public const string IMAGE_1336_SHIT_QUALITY_JPG = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a527e826b28935486d038f13e62ee08f561abd4c846458af188927261f3862ee061f69";
        public const string IMAGE_1336_SMALL_JPG = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a5286826b4891a986d208f16662ee08f388abd60846478ae848929b61f4562ee061f58";
        public const string _1336_BOTTOM_LEFT_PART_JPG = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0d312e8281883b2483b64876d862ee08971ba939484b35885058815f61a1f62ee061c45";
        public const string _1336_BOTTOM_PART_JPG = "a23edaa75562ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a2c89ab88d62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0b7607b98008216381bde83ee5ae14762ee0a3cb9ad75a86e60869ba8da5c876de62ee0";
        public const string _1336_BOTTOM_RIGHT_PART_JPG = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0b3c948219381ee081fb562ee062ee062ee0a757a847f2873ca86abd62ee08a10a8e58f";
        public const string _1336_LEFT_PART_JPG = "a5c5e831b162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a64fa8336a62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a7347850ef85302856d98abcb87f5d62ee0ac9c684f6c8797587bc88f6668a37d62ee0";
        public const string _1336_RIGHT_PART_JPG = "a5c5e831b162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a64fa8336a62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a747a857ab847f784cdd8984987a4762ee0ac49784f3886c9286e8b8dcb98966462ee0";
        public const string _1336_TOP_LEFT_PART_JPG = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a860983dff890f188d7761cef62ee061d32ade578542a8bf548bb8b6261a62ee062685";
        public const string _1336_TOP_PART_JPG = "a23edaa75562ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a2c89ab88d62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a4309ad6ef88013881ba61a8088da262ee0a9d838286c8b5668b5f3625668cbfa62ee0";
        public const string _1336_TOP_RIGHT_PART_JPG = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a90b183f0189676896d662ee062ee061e28ae3968548a8c4158c2f562b5262ee06270e";
        public const string _1337_JPG = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a46fb824c38613386f8562ee062ee08d912aba21844698994389c3062ee062ee061fa3";
        public const string _1349_JPG = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a45a48230087223877b48eca362ee062ee0ab6ba84445890c1895e961dea62ee062ee0";
        public const string _1350_JPG = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a53dd82ae985d8b863678c4e662ee08d657abc9584a4e88a8388f3e61c8162ee061e58";
        public const string _1413_JPG = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0b3ae3a6c078366e82a5e62ee062ee062ee0a66038379386f52862528d84e62ee062ee0";
        public const string _1414_JPG = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0b420fa61b5834ce8279562ee062ee062ee0a610d836af86c76860278de5262ee062ee0";
        public const string _1415_JPG = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0db53884cb781b71ad09d82dea841d762ee0a74048491d84b8c84917893a186db662ee0";

        public const int LENGTH = 210; // The PHash is a 210-character hexadecimal string
    }
}
