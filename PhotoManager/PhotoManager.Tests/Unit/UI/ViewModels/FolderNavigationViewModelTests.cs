﻿//using Autofac;
//using Autofac.Extras.Moq;
//using FluentAssertions;
//using Xunit;

//namespace PhotoManager.Tests.Unit.UI.ViewModels;

//public class FolderNavigationViewModelTests
//{
//    [Test]
//    public void ViewModelTest()
//    {
//        using var mock = AutoMock.GetLoose();
//        mock.Mock<IApplication>().Setup(a => a.GetInitialFolderPath()).Returns(@"D:\Data");

//        Folder sourceFolder = new() { Id = Guid.NewGuid(), Path = @"D:\Data\Folder1" };
//        Folder selectedFolder = new() { Id = Guid.NewGuid(), Path = @"D:\Data\Folder2" };

//        var viewModel = mock.Container.Resolve<FolderNavigationViewModel>(
//            new NamedParameter("sourceFolder", sourceFolder),
//            new NamedParameter("lastSelectedFolder", selectedFolder),
//            new NamedParameter("recentTargetPaths", new List<string>()));

//        viewModel.TargetPath = @"D:\Data\Folder2";
//        viewModel.HasConfirmed = true;

//        viewModel.SourceFolder.Path.Should().Be(@"D:\Data\Folder1");
//        viewModel.LastSelectedFolder.Path.Should().Be(@"D:\Data\Folder2");
//        viewModel.TargetPath.Should().Be(@"D:\Data\Folder2");
//        viewModel.SelectedFolder.Path.Should().Be(@"D:\Data\Folder2");
//        viewModel.HasConfirmed.Should().BeTrue();
//    }

//    [Theory]
//    [InlineData(@"D:\Data\Folder1", @"D:\Data\Folder1", false)]
//    [InlineData(@"D:\Data\Folder1", @"D:\Data\Folder2", true)]
//    public void CanConfirmTest(string sourcePath, string selectedPath, bool expected)
//    {
//        using var mock = AutoMock.GetLoose();
//        mock.Mock<IApplication>().Setup(a => a.GetInitialFolderPath()).Returns(@"D:\Data");

//        Folder sourceFolder = new() { Id = Guid.NewGuid(), Path = sourcePath };

//        var viewModel = mock.Container.Resolve<FolderNavigationViewModel>(
//            new NamedParameter("sourceFolder", sourceFolder),
//            new NamedParameter("lastSelectedFolder", null),
//            new NamedParameter("recentTargetPaths", new List<string>()));

//        viewModel.TargetPath = selectedPath;
//        viewModel.CanConfirm.Should().Be(expected);
//    }

//    [Test]
//    public void CanConfirmNullSourceFolderTest()
//    {
//        using var mock = AutoMock.GetLoose();
//        mock.Mock<IApplication>().Setup(a => a.GetInitialFolderPath()).Returns(@"D:\Data");

//        Folder sourceFolder = null;

//        var viewModel = mock.Container.Resolve<FolderNavigationViewModel>(
//            new NamedParameter("sourceFolder", sourceFolder),
//            new NamedParameter("lastSelectedFolder", null),
//            new NamedParameter("recentTargetPaths", new List<string>()));

//        viewModel.TargetPath = @"D:\Data\Folder2";
//        viewModel.CanConfirm.Should().BeFalse();
//    }

//    [Test]
//    public void CanConfirmNullTargetPathTest()
//    {
//        using var mock = AutoMock.GetLoose();
//        mock.Mock<IApplication>().Setup(a => a.GetInitialFolderPath()).Returns(@"D:\Data");

//        Folder sourceFolder = new() { Id = Guid.NewGuid(), Path = @"D:\Data\Folder1" };

//        var viewModel = mock.Container.Resolve<FolderNavigationViewModel>(
//            new NamedParameter("sourceFolder", sourceFolder),
//            new NamedParameter("lastSelectedFolder", null),
//            new NamedParameter("recentTargetPaths", new List<string>()));

//        viewModel.TargetPath = null;
//        viewModel.CanConfirm.Should().BeFalse();
//    }
//}
