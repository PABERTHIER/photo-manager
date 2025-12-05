namespace PhotoManager.Tests.Unit;

public static class Constants
{
    public const string DATABASE_END_PATH = "v1.0";
    public const char SEMICOLON_SEPARATOR = ';';
    public const string VERSION = "v1.0.0";

    public static class Directories
    {
        public const string BLOBS = "Blobs";
        public const string TABLES = "Tables";
        public const string TEST_FILES = "TestFiles";
        public const string TEST_FOLDER = "TestFolder";
        public const string IMAGE_CONVERTED = "ImageConverted";
        public const string OUTPUT_VIDEO_FIRST_FRAME = "OutputVideoFirstFrame";
        public const string DUPLICATES = "Duplicates";
        public const string FOLDER_1 = "Folder1";
        public const string FOLDER_2 = "Folder2";
        public const string NEW_FOLDER_2 = "NewFolder2";
        public const string DATABASE_TESTS = "DatabaseTests";
        public const string DATABASE_TESTS_BACKUPS = "DatabaseTests_Backups";
        public const string TEST_BACKUP = "TestBackup";

        public const string NON_EXISTENT_FOLDER = "NonExistentFolder";
    }

    public static class FileNames
    {
        public const string HOMER_GIF = "Homer.gif";
        public const string HOMER_MP4 = "Homer.mp4";
        public const string IMAGE_JPG = "Image.jpg";
        public const string IMAGE_1_JPG = "Image 1.jpg";
        public const string IMAGE_1_90_DEG_JPG = "Image 1_90_deg.jpg";
        public const string IMAGE_1_180_DEG_JPG = "Image 1_180_deg.jpg";
        public const string IMAGE_1_270_DEG_JPG = "Image 1_270_deg.jpg";
        public const string IMAGE1_JPG = "Image1.jpg";
        public const string IMAGE_2_JPG = "Image 2.jpg";
        public const string IMAGE_2_PNG = "Image2.png";
        public const string IMAGE_3_JPG = "Image 3.jpg";
        public const string IMAGE_4_JPG = "Image 4.jpg";
        public const string IMAGE_5_JPG = "Image 5.jpg";
        public const string IMAGE_6_JPG = "Image 6.jpg";
        public const string IMAGE_8_JPEG = "Image 8.jpeg";
        public const string IMAGE_9_PNG = "Image 9.png";
        public const string IMAGE_10_PORTRAIT_PNG = "Image 10 portrait.png";
        public const string IMAGE_11_HEIC = "Image_11.heic";
        public const string IMAGE_11_90_DEG_HEIC = "Image_11_90.heic";
        public const string IMAGE_11_180_DEG_HEIC = "Image_11_180.heic";
        public const string IMAGE_11_270_DEG_HEIC = "Image_11_270.heic";

        public const string IMAGE_CONVERTED_GIF = "image_converted.gif";
        public const string IMAGE_CONVERTED_JPEG = "image_converted.jpeg";
        public const string IMAGE_CONVERTED_PNG = "image_converted.png";
        public const string NEW_IMAGE_JPG = "NewImage.jpg";

        public const string NON_EXISTENT_IMAGE_JPG = "ImageDoesNotExist.jpg";
        public const string NON_EXISTENT_IMAGE_PNG = "ImageDoesNotExist.png";
        public const string NON_EXISTENT_IMAGE_HEIC = "ImageDoesNotExist.heic";
    }

    public static class PixelWidthAsset
    {
        public const int HOMER_GIF = 320;
        public const int IMAGE_JPG = 1280;
        public const int IMAGE_1_JPG = 1280;
        public const int IMAGE_2_JPG = 1280;
        public const int IMAGE_3_JPG = 1280;
        public const int IMAGE_4_JPG = 1280;
        public const int IMAGE_5_JPG = 1280;
        public const int IMAGE_6_JPG = 1280;
        public const int IMAGE_9_PNG = 1280;
        public const int IMAGE_11_HEIC = 3024;
        public const int IMAGE_11_90_DEG_HEIC = 4032;
        public const int IMAGE_11_180_DEG_HEIC = 3024;
        public const int IMAGE_11_270_DEG_HEIC = 4032;

        public const int NEW_IMAGE_JPG = 1280;
        public const int NON_EXISTENT_IMAGE_JPG = 1000;
        public const int NON_EXISTENT_IMAGE_HEIC = 1000;
    }

    public static class PixelHeightAsset
    {
        public const int HOMER_GIF = 320;
        public const int IMAGE_JPG = 720;
        public const int IMAGE_1_JPG = 720;
        public const int IMAGE_2_JPG = 720;
        public const int IMAGE_3_JPG = 720;
        public const int IMAGE_4_JPG = 720;
        public const int IMAGE_5_JPG = 720;
        public const int IMAGE_6_JPG = 720;
        public const int IMAGE_9_PNG = 700;
        public const int IMAGE_11_HEIC = 4032;
        public const int IMAGE_11_90_DEG_HEIC = 3024;
        public const int IMAGE_11_180_DEG_HEIC = 4032;
        public const int IMAGE_11_270_DEG_HEIC = 3024;

        public const int NEW_IMAGE_JPG = 720;
        public const int NON_EXISTENT_IMAGE_JPG = 1000;
        public const int NON_EXISTENT_IMAGE_HEIC = 1000;
    }

    public static class ThumbnailWidthAsset
    {
        public const int HOMER_GIF = 150;
        public const int IMAGE_JPG = 200;
        public const int IMAGE_1_JPG = 200;
        public const int IMAGE_2_JPG = 200;
        public const int IMAGE_3_JPG = 200;
        public const int IMAGE_4_JPG = 200;
        public const int IMAGE_5_JPG = 200;
        public const int IMAGE_6_JPG = 200;
        public const int IMAGE_9_PNG = 147;
        public const int IMAGE_11_HEIC = 112;
        public const int IMAGE_11_90_DEG_HEIC = 200;
        public const int IMAGE_11_180_DEG_HEIC = 112;
        public const int IMAGE_11_270_DEG_HEIC = 200;

        public const int NEW_IMAGE_JPG = 200;
        public const int NON_EXISTENT_IMAGE_JPG = 200;
        public const int NON_EXISTENT_IMAGE_HEIC = 112;
    }

    public static class ThumbnailHeightAsset
    {
        public const int HOMER_GIF = 150;
        public const int IMAGE_JPG = 112;
        public const int IMAGE_1_JPG = 112;
        public const int IMAGE_2_JPG = 112;
        public const int IMAGE_3_JPG = 112;
        public const int IMAGE_4_JPG = 112;
        public const int IMAGE_5_JPG = 112;
        public const int IMAGE_6_JPG = 112;
        public const int IMAGE_9_PNG = 150;
        public const int IMAGE_11_HEIC = 150;
        public const int IMAGE_11_90_DEG_HEIC = 150;
        public const int IMAGE_11_180_DEG_HEIC = 150;
        public const int IMAGE_11_270_DEG_HEIC = 150;

        public const int NEW_IMAGE_JPG = 112;
        public const int NON_EXISTENT_IMAGE_JPG = 112;
        public const int NON_EXISTENT_IMAGE_HEIC = 150;
    }

    public static class FileSize
    {
        public const long HOMER_GIF = 64123;
        public const long IMAGE_1_JPG = 29857;
        public const long IMAGE_2_JPG = 29857;
        public const long IMAGE_9_PNG = 4602393;
        public const long IMAGE_11_HEIC = 1411940;
        public const long IMAGE_11_90_DEG_HEIC = 1411940;
        public const long IMAGE_11_180_DEG_HEIC = 1423616;
        public const long IMAGE_11_270_DEG_HEIC = 1423616;

        public const long NON_EXISTENT_IMAGE_JPG = 29857;
        public const long NON_EXISTENT_IMAGE_HEIC = 1411940;
    }

    public static class ModificationDate
    {
        public static readonly DateTime Default = new (2024, 06, 07, 08, 54, 37);
    }

    public static class Hashes
    {
        public const string HOMER_GIF = "c48b1f61f3a3a004f425d8493d30a50ae14408ed4c5354bf4d0ca40069f91951381a7df32ee7455a6edef0996c95571557a9993021331ff2dfbc3ccc7f0c8ff1";
        public const string IMAGE_1_JPG =             "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
        public const string IMAGE_1_90_DEG_JPG =             "0092081bd0a83e9253f4eb319b4cfab197a09ed3b28b5697c31f17bf2646638dec95bf28d4f454dda8ff3dcdaab33fa002774f13dfe6d7aee82f1a960e3e3aee";
        public const string IMAGE_1_180_DEG_JPG = "399383d82d4b7ed8233f714a1d14272b276b80505d7b3a2935b6ec962fd5cf280ccedb3e9380df1a11ee65e5f5fa66278dd2f407b7b6bdfa3093319164c446dd";
        public const string IMAGE_1_270_DEG_JPG = "43950e06cecd509509d868a2064a828aeb6ff6a9a30f296ec9a3621b269742e75dacb40300aa6927c4c8f0879268e1ab3d7eac228b38cade35cc9b7c6adf3449";
        public const string IMAGE_2_JPG = "0b6d010f85544871c307bb3a96028402f55fa29094908cdd0f74a8ec8d3fc3d4fbec995d98b89aafef3dcf5581c018fbb50481e33c7e45aef552d66c922f4078";
        public const string IMAGE_3_JPG = "1cc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";
        public const string IMAGE_4_JPG = "2cc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";
        public const string IMAGE_5_JPG = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4";
        public const string IMAGE_8_JPEG = "156f81ceb3db1bfdf27618efe4b89575f17d4708053c1a9ce6c5dfb7f36a6b958a5c3f45147728de28cf538345336e7b81d01b3f5908d4677ce76e1661e97ac3";
        public const string IMAGE_9_PNG = "f8d5cf6deda198be0f181dd7cabfe74cb14c43426c867f0ae855d9e844651e2d7ce4833c178912d5bc7be600cfdd18d5ba19f45988a0c6943b4476a90295e960";
        public const string IMAGE_9_DUPLICATE_PNG = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";
        public const string IMAGE_10_PORTRAIT_PNG = "7ad01e9fe639957a9e8eaddc7fd864068f4a03f9981fd480f310740a43a0a4f2b8fa7a80d9c83039c46fcfbb63a5e465adaf07d33191369590adcda1586b1c94";
        public const string IMAGE_11_HEIC = "f52bd860f5ad7f81a92919e5fb5769d3e86778b2ade74832fbd3029435c85e59cb64b3c2ce425445a49917953e6e913c72b81e48976041a4439cb65e92baf18d";
        public const string IMAGE_11_90_DEG_HEIC = "b75df4942de1efa7d4326fedd721e99cc43959a1c22098a8cfcf6861aea099741d9c98fa1615e54d53838c22b9c6ff3b328c4ee3c0875d2b4ac7f7dee8d61bcf";
        public const string IMAGE_11_180_DEG_HEIC = "a2ad3ac9d51f74230b9f1d554172781d9cc214d01f8344354e2e56dc3daecb5a0021d4d2814eba0cde6fd681f3a8e8b8b55fbfad7184740e3518190e05dca77f";
        public const string IMAGE_11_270_DEG_HEIC = "d4d7342a783f0f1b5e10ad9b493b71dd688dbc31b8ddd5534c9fff9c5fdbfa8bee32ef4e4258f7514a1b57f9a12b571454c8b66172b1a66bf1b53953b66227d5";

        public const string NON_EXISTENT_IMAGE_JPG = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
        public const string NON_EXISTENT_IMAGE_HEIC = "f52bd860f5ad7f81a92919e5fb5769d3e86778b2ade74832fbd3029435c85e59cb64b3c2ce425445a49917953e6e913c72b81e48976041a4439cb65e92baf18d";
        public const string EMPTY_IMAGE = "cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e";

        public const int LENGTH = 128; // SHA-512 generates a 128-character long hash in hexadecimal representation
    }
    
    public static class DHashes
    {
        public const string HOMER_GIF = "00000000000000";
        public const string IMAGE_1_JPG = "54aaa1554aaa14";
        public const string IMAGE_1_90_DEG_JPG = "54aaa1554aaa14";
        public const string IMAGE_1_180_DEG_JPG = "54aaa1554aaa14";
        public const string IMAGE_1_270_DEG_JPG = "54aaa1554aaa14";
        public const string IMAGE_8_JPEG = "54aaa1554aaa14";
        public const string IMAGE_10_PORTRAIT_PNG = "80500d40555402";
        public const string IMAGE_11_HEIC = "00000000000000";
        public const string IMAGE_11_90_DEG_HEIC = "347636785c99d1";

        public const int LENGTH = 14; // The DHash is a 14-hex digits
    }

    public static class MD5Hashes
    {
        public const string HOMER_GIF = "a409ce713de9334117791b15a586dd0e";
        public const string IMAGE_1_JPG = "a886f237e1cf6da03fb793588fd79b64";
        public const string IMAGE_1_90_DEG_JPG = "e740c5b99b0c87d86b9b1b3f322dec7e";
        public const string IMAGE_1_180_DEG_JPG = "bc43ad1ebd118a2f9443b3199d4127c8";
        public const string IMAGE_1_270_DEG_JPG = "f8f724518021547c26ef9262f29f6b9a";
        public const string IMAGE_8_JPEG = "4194e88c247b5bbc7a1c6294cc795466";
        public const string IMAGE_10_PORTRAIT_PNG = "02b09a63d382bc1a1f88afa125f3adb3";
        public const string IMAGE_11_90_DEG_HEIC = "e3aeca5fc781bd52470f9186653d3a3f";

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
        public const string IMAGE_8_JPEG = "a505b8264162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a58f7827fa62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0add078455f8d8688b4db62ee062ee062ee0afb8a849cc8fc608dc2362ee062ee062ee0";
        public const string IMAGE_10_PORTRAIT_PNG = "a505b8264162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a58f7827fa62ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0adbc58451f8d8dd8b49462ee062ee062ee0afaff849b08fd348dc1f62ee062ee062ee0";
        public const string IMAGE_11_90_DEG_HEIC = "a640c83f2862ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0a6ca8840e162ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee062ee0b1adb820bf83d0a84a6188e1762ee08d8c2a929e84a9d881418946b61cbc8d62a62ee0";

        public const int LENGTH = 210; // The PHash is a 210-character hexadecimal string
    }
}
