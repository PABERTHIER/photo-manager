// using FluentAssertions;
// using PhotoManager.UI.Tests.Unit;
// using Xunit;

// namespace PhotoManager.Tests.Unit.UI.ViewModels;

// [Collection("Test Collection")]
// // TODO: Testing from file FindDuplicatedAssetsViewModel (it should be split into 2 files FindDuplicatedAssetsViewModel and DuplicatedSetViewModel)
// public class FindDuplicatedAssetsTests
// {
//     private readonly IApplication _application;
//     private readonly Folder _folder1;
//     private readonly Folder _folder2;
//     private readonly Folder _folder3;
//     private readonly List<DuplicatedSetViewModel> _duplicatedAssets;
//     private readonly DuplicatedSetViewModel _duplicatedSetViewModel1;
//     private readonly DuplicatedSetViewModel _duplicatedSetViewModel2;

//     private readonly DuplicatedAssetViewModel _duplicatedAssetViewModel1;
//     private readonly DuplicatedAssetViewModel _duplicatedAssetViewModel2;
//     private readonly DuplicatedAssetViewModel _duplicatedAssetViewModel3;
//     private readonly DuplicatedAssetViewModel _duplicatedAssetViewModel4;
//     private readonly DuplicatedAssetViewModel _duplicatedAssetViewModel5;
//     private readonly DuplicatedAssetViewModel _duplicatedAssetViewModel6;

//     private const string pathLocationToExemptTheFolder = "D:\\Inexistent Folder1";

//     public FindDuplicatedAssetsTests()
//     {
//         _folder1 = new() { FolderId = "1", Path = "D:\\Inexistent Folder1" };
//         _folder2 = new() { FolderId = "2", Path = "D:\\Inexistent Folder2" };
//         _folder3 = new() { FolderId = "3", Path = "D:\\Inexistent Folder3" };

//         _duplicatedAssets = new List<DuplicatedSetViewModel>();

//         _duplicatedSetViewModel1 = new DuplicatedSetViewModel();
//         _duplicatedSetViewModel2 = new DuplicatedSetViewModel();

//         _duplicatedAssetViewModel1 = new DuplicatedAssetViewModel(_application);
//         _duplicatedAssetViewModel2 = new DuplicatedAssetViewModel(_application);
//         _duplicatedAssetViewModel3 = new DuplicatedAssetViewModel(_application);
//         _duplicatedAssetViewModel4 = new DuplicatedAssetViewModel(_application);
//         _duplicatedAssetViewModel5 = new DuplicatedAssetViewModel(_application);
//         _duplicatedAssetViewModel6 = new DuplicatedAssetViewModel(_application);

//         _duplicatedAssetViewModel1.Asset = new Asset()
//         {
//             FileName = "FileName1",
//             FolderId = _folder1.FolderId,
//             Folder = _folder1,
//             Hash = "0b6d010f85544871c307bb3a96028402f55fa29094908cdd0f74a8ec8d3fc3d4fbec995d98b89aafef3dcf5581c018fbb50481e33c7e45aef552d66c922f4078"
//         };
//         _duplicatedAssetViewModel2.Asset = new Asset()
//         {
//             FileName = "FileName2",
//             FolderId = _folder2.FolderId,
//             Folder = _folder2,
//             Hash = "0b6d010f85544871c307bb3a96028402f55fa29094908cdd0f74a8ec8d3fc3d4fbec995d98b89aafef3dcf5581c018fbb50481e33c7e45aef552d66c922f4078"
//         };
//         _duplicatedAssetViewModel3.Asset = new Asset()
//         {
//             FileName = "FileName3",
//             FolderId = _folder1.FolderId,
//             Folder = _folder1,
//             Hash = "0b6d010f85544871c307bb3a96028402f55fa29094908cdd0f74a8ec8d3fc3d4fbec995d98b89aafef3dcf5581c018fbb50481e33c7e45aef552d66c922f4079"
//         };
//         _duplicatedAssetViewModel4.Asset = new Asset()
//         {
//             FileName = "FileName4",
//             FolderId = _folder2.FolderId,
//             Folder = _folder2,
//             Hash = "0b6d010f85544871c307bb3a96028402f55fa29094908cdd0f74a8ec8d3fc3d4fbec995d98b89aafef3dcf5581c018fbb50481e33c7e45aef552d66c922f4079"
//         };
//         _duplicatedAssetViewModel5.Asset = new Asset()
//         {
//             FileName = "FileName5",
//             FolderId = _folder2.FolderId,
//             Folder = _folder2,
//             Hash = "0b6d010f85544871c307bb3a96028402f55fa29094908cdd0f74a8ec8d3fc3d4fbec995d98b89aafef3dcf5581c018fbb50481e33c7e45aef552d66c922f4080"
//         };
//         _duplicatedAssetViewModel6.Asset = new Asset()
//         {
//             FileName = "FileName6",
//             FolderId = _folder3.FolderId,
//             Folder = _folder3,
//             Hash = "0b6d010f85544871c307bb3a96028402f55fa29094908cdd0f74a8ec8d3fc3d4fbec995d98b89aafef3dcf5581c018fbb50481e33c7e45aef552d66c922f4080"
//         };

//         _duplicatedSetViewModel1.Add(_duplicatedAssetViewModel1);
//         _duplicatedSetViewModel1.Add(_duplicatedAssetViewModel2);
//         _duplicatedSetViewModel2.Add(_duplicatedAssetViewModel3);
//         _duplicatedSetViewModel2.Add(_duplicatedAssetViewModel4);
//         _duplicatedSetViewModel2.Add(_duplicatedAssetViewModel5);
//         _duplicatedSetViewModel2.Add(_duplicatedAssetViewModel6);

//         _duplicatedAssets.Add(_duplicatedSetViewModel1);
//         _duplicatedAssets.Add(_duplicatedSetViewModel2);
//     }

//     [Test]
//     [Trait("Category", "DeleteAllDuplicatedAssetsByHash")]
//     public void SelectedAsset_Sould_Be_CurrentAsset()
//     {
//         Folder folder = new() { FolderId = "1", Path = "D:\\Inexistent Folder1" };

//         var currentAsset = new Asset()
//         {
//             FileName = "FileName1",
//             FolderId = _folder1.FolderId,
//             Folder = _folder1,
//             Hash = "0b6d010f85544871c307bb3a96028402f55fa29094908cdd0f74a8ec8d3fc3d4fbec995d98b89aafef3dcf5581c018fbb50481e33c7e45aef552d66c922f4078"
//         };

//         var currentRootFolderPath = currentAsset.Folder.Path;
//         var duplicatedAssetByHashList = _duplicatedAssets.Where(x => x.Any(y => y.Asset.Hash == currentAsset.Hash));
//         var duplicatedAssetByHash = duplicatedAssetByHashList.FirstOrDefault();

//         var assetSelected = duplicatedAssetByHash?.FirstOrDefault(x => x?.Asset.Folder.Path == currentRootFolderPath && x?.Asset.FileName == currentAsset.FileName);

//         assetSelected?.Asset.Should().Be(currentAsset);
//     }

//     [Test]
//     [Trait("Category", "DeleteAllDuplicatedAssetsByHash")]
//     public void An_Other_Asset_Sould_Not_Be_CurrentAsset()
//     {
//         Folder folder = new() { FolderId = "10", Path = "D:\\Inexistent Folder10" };

//         var currentAsset = new Asset()
//         {
//             FileName = "FileName10",
//             FolderId = _folder1.FolderId,
//             Folder = _folder1,
//             Hash = "0b6d010f85544871c307bb3a96028402f55fa29094908cdd0f74a8ec8d3fc3d4fbec995d98b89aafef3dcf5581c018fbb50481e33c7e45aef552d66c922f4010"
//         };

//         var currentRootFolderPath = currentAsset.Folder.Path;
//         var duplicatedAssetByHashList = _duplicatedAssets.Where(x => x.Any(y => y.Asset.Hash == currentAsset.Hash));
//         var duplicatedAssetByHash = duplicatedAssetByHashList.FirstOrDefault();

//         var assetSelected = duplicatedAssetByHash?.FirstOrDefault(x => x?.Asset.Folder.Path == currentRootFolderPath && x?.Asset.FileName == currentAsset.FileName);

//         assetSelected?.Asset.Should().NotBe(currentAsset);
//     }

//     [Test]
//     [Trait("Category", "DeleteAllDuplicatedAssetsByHash")]
//     public void AssetsToDelete_Should_BeEquivalentTo_The_DuplicatedAssets_Selected()
//     {
//         Folder folder = new() { FolderId = "10", Path = "D:\\Inexistent Folder10" };

//         var currentAsset = new Asset()
//         {
//             FileName = "FileName1",
//             FolderId = _folder1.FolderId,
//             Folder = _folder1,
//             Hash = "0b6d010f85544871c307bb3a96028402f55fa29094908cdd0f74a8ec8d3fc3d4fbec995d98b89aafef3dcf5581c018fbb50481e33c7e45aef552d66c922f4078"
//         };

//         var currentRootFolderPath = currentAsset.Folder.Path;
//         var duplicatedAssetByHashList = _duplicatedAssets.Where(x => x.Any(y => y.Asset.Hash == currentAsset.Hash));
//         var duplicatedAssetByHash = duplicatedAssetByHashList.FirstOrDefault();

//         var assetSelected = duplicatedAssetByHash?.FirstOrDefault(x => x?.Asset.Folder.Path == currentRootFolderPath && x?.Asset.FileName == currentAsset.FileName);

//         var assetsToDelete = duplicatedAssetByHash?.Where(x => x != null && x != assetSelected).ToList() ?? new List<DuplicatedAssetViewModel>();

//         var duplicatedAssetViewModelList = _duplicatedSetViewModel1.Cast<DuplicatedAssetViewModel>().ToList();

//         assetsToDelete.Should().NotBeEquivalentTo(duplicatedAssetViewModelList);
//     }

//     [Test]
//     [Trait("Category", "DeleteAllDuplicatedAssetsByHash")]
//     public void AssetsToDelete_Should_NotContain_The_DuplicatedAsset_Selected()
//     {
//         Folder folder = new() { FolderId = "10", Path = "D:\\Inexistent Folder10" };

//         var currentAsset = new Asset()
//         {
//             FileName = "FileName1",
//             FolderId = _folder1.FolderId,
//             Folder = _folder1,
//             Hash = "0b6d010f85544871c307bb3a96028402f55fa29094908cdd0f74a8ec8d3fc3d4fbec995d98b89aafef3dcf5581c018fbb50481e33c7e45aef552d66c922f4078"
//         };

//         var currentRootFolderPath = currentAsset.Folder.Path;
//         var duplicatedAssetByHashList = _duplicatedAssets.Where(x => x.Any(y => y.Asset.Hash == currentAsset.Hash));
//         var duplicatedAssetByHash = duplicatedAssetByHashList.FirstOrDefault();

//         var assetSelected = duplicatedAssetByHash?.FirstOrDefault(x => x?.Asset.Folder.Path == currentRootFolderPath && x?.Asset.FileName == currentAsset.FileName);

//         var assetsToDelete = duplicatedAssetByHash?.Where(x => x != null && x != assetSelected).ToList() ?? new List<DuplicatedAssetViewModel>();

//         var duplicatedAssetViewModelList = _duplicatedSetViewModel1.Cast<DuplicatedAssetViewModel>().Where(x => x != null && x != assetSelected).ToList();

//         assetsToDelete.Should().BeEquivalentTo(duplicatedAssetViewModelList);
//     }

//     [Test]
//     [Trait("Category", "DeleteAllDuplicatedAssetsByHash")]
//     public void AssetsToDelete_Should_NotBeEquivalentTo_An_Other_DuplicatedAssets()
//     {
//         Folder folder = new() { FolderId = "10", Path = "D:\\Inexistent Folder10" };

//         var currentAsset = new Asset()
//         {
//             FileName = "FileName1",
//             FolderId = _folder1.FolderId,
//             Folder = _folder1,
//             Hash = "0b6d010f85544871c307bb3a96028402f55fa29094908cdd0f74a8ec8d3fc3d4fbec995d98b89aafef3dcf5581c018fbb50481e33c7e45aef552d66c922f4078"
//         };

//         var currentRootFolderPath = currentAsset.Folder.Path;
//         var duplicatedAssetByHashList = _duplicatedAssets.Where(x => x.Any(y => y.Asset.Hash == currentAsset.Hash));
//         var duplicatedAssetByHash = duplicatedAssetByHashList.FirstOrDefault();

//         var assetSelected = duplicatedAssetByHash?.FirstOrDefault(x => x?.Asset.Folder.Path == currentRootFolderPath && x?.Asset.FileName == currentAsset.FileName);

//         var assetsToDelete = duplicatedAssetByHash?.Where(x => x != null && x != assetSelected).ToList() ?? new List<DuplicatedAssetViewModel>();

//         var duplicatedAssetViewModelList = _duplicatedSetViewModel2.Cast<DuplicatedAssetViewModel>().Where(x => x != null && x != assetSelected).ToList();

//         assetsToDelete.Should().NotBeEquivalentTo(duplicatedAssetViewModelList);
//     }

//     [Test]
//     [Trait("Category", "DeleteEveryDuplicatedAssets")]
//     public void ShouldNot_Delete_DuplicatedAssets_FromExemptFolder()
//     {
//         var exemptedAssets = new List<DuplicatedAssetViewModel>
//         {
//             _duplicatedAssetViewModel1,
//             _duplicatedAssetViewModel3
//         };

//         var isContainingDuplicatesFromExemptFolder = false;
//         var duplicatedAssetsFiltered = _duplicatedAssets.Where(x => x != null).SelectMany(x => x).Where(y => y != null && y.Asset.Folder.Path != pathLocationToExemptTheFolder).ToList();

//         var assetsToDelete = duplicatedAssetsFiltered.Join(exemptedAssets,
//             x => x.Asset.Hash,
//             y => y.Asset.Hash,
//             (x, y) => x)
//             .ToList();

//         isContainingDuplicatesFromExemptFolder = assetsToDelete.Any(x => x.Asset.Folder.Path == pathLocationToExemptTheFolder);
//         isContainingDuplicatesFromExemptFolder.Should().BeFalse();
//         assetsToDelete.Should().Contain(_duplicatedAssetViewModel2);
//         assetsToDelete.Should().Contain(_duplicatedAssetViewModel4);
//     }

//     [Test]
//     [Trait("Category", "DeleteEveryDuplicatedAssets")]
//     public void ShouldNot_Delete_DuplicatedAssets_WhichIsNotIncluded_In_The_ExemptedFolder()
//     {
//         var exemptedAssets = new List<DuplicatedAssetViewModel>
//         {
//             _duplicatedAssetViewModel1,
//             _duplicatedAssetViewModel3
//         };

//         var isContainingDuplicatesFromExemptFolder = false;
//         var duplicatedAssetsFiltered = _duplicatedAssets.Where(x => x != null).SelectMany(x => x).Where(y => y != null && y.Asset.Folder.Path != pathLocationToExemptTheFolder).ToList();

//         var assetsToDelete = duplicatedAssetsFiltered.Join(exemptedAssets,
//             x => x.Asset.Hash,
//             y => y.Asset.Hash,
//             (x, y) => x)
//             .ToList();

//         isContainingDuplicatesFromExemptFolder = assetsToDelete.Any(x => x.Asset.Folder.Path == pathLocationToExemptTheFolder);
//         isContainingDuplicatesFromExemptFolder.Should().BeFalse();
//         assetsToDelete.Should().NotContain(_duplicatedAssetViewModel5);
//         assetsToDelete.Should().NotContain(_duplicatedAssetViewModel6);
//     }
// }
