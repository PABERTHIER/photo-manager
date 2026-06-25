using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PhotoManager.Domain;
using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels;
using System.Diagnostics.CodeAnalysis;

namespace PhotoManager.UI.Controls;

/// <summary>
/// User control rendering the folder tree used to navigate asset directories.
/// </summary>
[ExcludeFromCodeCoverage]
public partial class FolderNavigationControl : UserControl
{
    private readonly ILogger<FolderNavigationControl> _logger;
    private bool _isInitialized;
    private FolderNavigationViewModel? _subscribedViewModel;

    public FolderNavigationControl()
    {
        _logger = App.ServiceProvider?.GetService<ILogger<FolderNavigationControl>>()
                  ?? NullLogger<FolderNavigationControl>.Instance;
        InitializeComponent();
        SelectedPath = string.Empty;
        DataContextChanged += UserControl_DataContextChanged;
    }

    public event EventHandler? FolderSelected;

    public string SelectedPath { get; set; }

    private FolderNavigationViewModel? ViewModel => DataContext as FolderNavigationViewModel;

    public void Initialize()
    {
        try
        {
            if (ViewModel is not { } viewModel)
            {
                _isInitialized = false;
                return;
            }

            FoldersTreeView.Items.Clear();
            Folder[] rootFolders = viewModel.ApplicationViewModel.GetRootCatalogFolders();

            foreach (Folder folder in rootFolders.OrderBy(static folder => folder.Name))
            {
                FoldersTreeView.Items.Add(CreateItem(folder, viewModel.ApplicationViewModel));
            }

            GoToFolder(SelectedPath);
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        FoldersTreeView = this.FindControl<TreeView>(nameof(FoldersTreeView))
            ?? throw new InvalidOperationException("FoldersTreeView control was not found.");
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        InitializeIfReady();
    }

    private void UserControl_DataContextChanged(object? sender, EventArgs e)
    {
        InitializeIfReady();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnsubscribeFromViewModel();
        base.OnDetachedFromVisualTree(e);
    }

    private void InitializeIfReady()
    {
        if (ViewModel is not { } viewModel)
        {
            return;
        }

        Initialize();
        SubscribeToViewModel(viewModel);
    }

    private void SubscribeToViewModel(FolderNavigationViewModel viewModel)
    {
        if (ReferenceEquals(_subscribedViewModel, viewModel))
        {
            return;
        }

        UnsubscribeFromViewModel();
        viewModel.ApplicationViewModel.FolderAdded += ViewModel_FolderAdded;
        viewModel.ApplicationViewModel.FolderRemoved += ViewModel_FolderRemoved;
        _subscribedViewModel = viewModel;
    }

    private void UnsubscribeFromViewModel()
    {
        if (_subscribedViewModel == null)
        {
            return;
        }

        _subscribedViewModel.ApplicationViewModel.FolderAdded -= ViewModel_FolderAdded;
        _subscribedViewModel.ApplicationViewModel.FolderRemoved -= ViewModel_FolderRemoved;
        _subscribedViewModel = null;
    }

    private void ViewModel_FolderAdded(object sender, FolderAddedEventArgs e)
    {
        RefreshFolders();
    }

    private void ViewModel_FolderRemoved(object sender, FolderRemovedEventArgs e)
    {
        RefreshFolders();
    }

    private void RefreshFolders()
    {
        if (ViewModel is not { } viewModel)
        {
            return;
        }

        viewModel.ApplicationViewModel.SetIsRefreshingFolders(true);

        try
        {
            Initialize();
        }
        finally
        {
            viewModel.ApplicationViewModel.SetIsRefreshingFolders(false);
        }
    }

    private void FoldersTreeView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (!_isInitialized)
            {
                return;
            }

            if (FoldersTreeView.SelectedItem is TreeViewItem { Tag: Folder folder })
            {
                SelectedPath = folder.Path;
                FolderSelected?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private TreeViewItem CreateItem(Folder folder, ApplicationViewModel applicationViewModel)
    {
        TreeViewItem item = new()
        {
            Header = folder.Name,
            Tag = folder
        };

        Folder[] folders = applicationViewModel.GetSubFolders(folder);

        foreach (Folder childFolder in folders.OrderBy(static childFolder => childFolder.Name))
        {
            item.Items.Add(CreateItem(childFolder, applicationViewModel));
        }

        return item;
    }

    private void GoToFolder(string folderFullPath)
    {
        foreach (object? item in FoldersTreeView.Items)
        {
            if (item is TreeViewItem treeViewItem)
            {
                GoToFolder(treeViewItem, folderFullPath);
            }
        }
    }

    private void GoToFolder(TreeViewItem item, string folderFullPath)
    {
        if (item.Tag is not Folder folder ||
            !folderFullPath.StartsWith(folder.Path, StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }

        item.IsExpanded = true;

        if (folder.Path == folderFullPath)
        {
            item.IsSelected = true;
            return;
        }

        foreach (object? subItem in item.Items)
        {
            if (subItem is TreeViewItem treeViewItem)
            {
                GoToFolder(treeViewItem, folderFullPath);
            }
        }
    }
}
