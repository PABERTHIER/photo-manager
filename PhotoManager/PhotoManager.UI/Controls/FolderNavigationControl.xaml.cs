using log4net;
using PhotoManager.Domain;
using PhotoManager.Infrastructure;
using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace PhotoManager.UI.Controls;

/// <summary>
/// Interaction logic for FolderNavigationControl.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class FolderNavigationControl
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    private readonly object? _placeholderNode = null;
    private bool _isInitializing = true;

    public event EventHandler? FolderSelected;

    public FolderNavigationControl()
    {
        InitializeComponent();

        SelectedPath = string.Empty;
    }

    public ApplicationViewModel ViewModel => (ApplicationViewModel)DataContext;

    public string SelectedPath { get; set; }

    public void Initialize()
    {
        try
        {
            foldersTreeView.Items.Clear();
            Folder[] rootFolders = ViewModel.GetRootCatalogFolders();

            foreach (Folder folder in rootFolders)
            {
                TreeViewItem item = new()
                {
                    Header = folder.Name,
                    Tag = folder
                };

                item.Items.Add(_placeholderNode);
                item.Expanded += Item_Expanded;
                foldersTreeView.Items.Add(item);
            }

            GoToFolder(SelectedPath);
            _isInitializing = false;
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        Initialize();
        ViewModel.FolderAdded += ViewModel_FolderAdded;
        ViewModel.FolderRemoved += ViewModel_FolderRemoved;
    }

    private void ViewModel_FolderAdded(object sender, FolderAddedEventArgs e)
    {
        ViewModel.IsRefreshingFolders = true;
        Initialize();
        ViewModel.IsRefreshingFolders = false;
    }

    private void ViewModel_FolderRemoved(object sender, FolderRemovedEventArgs e)
    {
        ViewModel.IsRefreshingFolders = true;
        Initialize();
        ViewModel.IsRefreshingFolders = false;
    }

    private void Item_Expanded(object sender, RoutedEventArgs e)
    {
        if (_isInitializing)
        {
            return;
        }

        TreeViewItem item = (TreeViewItem)sender;

        if (LacksSubItems(item))
        {
            AddSubItems(item);
        }
    }

    private void FoldersTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        try
        {
            TreeView tree = (TreeView)sender;
            TreeViewItem selectedTreeViewItem = (TreeViewItem)tree.SelectedItem;

            if (selectedTreeViewItem == null)
            {
                return;
            }

            // Even if SelectedPath == folder.Path -> source, need it to display assets at startup
            if (selectedTreeViewItem.Tag is Folder folder)
            {
                SelectedPath = folder.Path;
                FolderSelected?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private void GoToFolder(string folderFullPath)
    {
        foreach (object? item in foldersTreeView.Items)
        {
            TreeViewItem treeViewItem = (TreeViewItem)item;

            GoToFolder(treeViewItem, folderFullPath);
        }
    }

    private void GoToFolder(TreeViewItem item, string folderFullPath)
    {
        if (item.Tag is Folder folder)
        {
            if (folderFullPath.StartsWith(folder.Path, StringComparison.InvariantCultureIgnoreCase))
            {
                if (LacksSubItems(item))
                {
                    AddSubItems(item);
                }

                item.IsExpanded = true;

                if (folder.Path == folderFullPath)
                {
                    item.IsSelected = true;
                    item.BringIntoView();
                }
                else
                {
                    foreach (object? subItem in item.Items)
                    {
                        TreeViewItem treeViewItem = (TreeViewItem)subItem;
                        GoToFolder(treeViewItem, folderFullPath);
                    }
                }
            }
        }
    }

    private void AddSubItems(TreeViewItem item)
    {
        try
        {
            item.Items.Clear();

            Folder[] folders = ViewModel.GetSubFolders((Folder)item.Tag);
            folders = [..folders.OrderBy(f => f.Name)];

            foreach (Folder folder in folders)
            {
                TreeViewItem subItem = new()
                {
                    Header = folder.Name,
                    Tag = folder
                };

                subItem.Expanded += Item_Expanded;
                item.Items.Add(subItem);
                AddSubItems(subItem);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private bool LacksSubItems(TreeViewItem item)
    {
        return item.Items.Count == 1 && item.Items[0] == _placeholderNode;
    }
}
