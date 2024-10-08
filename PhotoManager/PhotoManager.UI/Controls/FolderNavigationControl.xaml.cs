using log4net;
using PhotoManager.Domain;
using PhotoManager.Infrastructure;
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
public partial class FolderNavigationControl : UserControl
{
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private object placeholderNode = null;
    public event EventHandler FolderSelected;
    public string SelectedPath { get; set; }
    private bool isInitializing = true;

    public FolderNavigationControl()
    {
        InitializeComponent();
    }

    public ApplicationViewModel ViewModel
    {
        get { return (ApplicationViewModel)DataContext; }
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

    // TODO: When a new folder is catalogued, this control should be notified so it can display it.
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

                item.Items.Add(placeholderNode);
                item.Expanded += new RoutedEventHandler(Item_Expanded);
                foldersTreeView.Items.Add(item);
            }

            GoToFolder(SelectedPath);
            isInitializing = false;
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void AddSubItems(TreeViewItem item, bool includeHidden)
    {
        try
        {
            item.Items.Clear();

            Folder[] folders = ViewModel.GetSubFolders((Folder)item.Tag, includeHidden);
            folders = folders.OrderBy(f => f.Name).ToArray();

            foreach (Folder folder in folders)
            {
                TreeViewItem subitem = new()
                {
                    Header = folder.Name,
                    Tag = folder
                };

                subitem.Expanded += new RoutedEventHandler(Item_Expanded);
                item.Items.Add(subitem);
                AddSubItems(subitem, includeHidden);
            }
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void Item_Expanded(object sender, RoutedEventArgs e)
    {
        if (isInitializing)
            return;

        TreeViewItem item = (TreeViewItem)sender;

        if (LacksSubItems(item))
        {
            // TODO: SHOULD ASK THE USER IF HE WANTS TO SEE HIDDEN FOLDERS. -> No // TODO: Remove includeHidden everywhere
            AddSubItems(item, false);
        }
    }

    private bool LacksSubItems(TreeViewItem item)
    {
        return item.Items.Count == 1 && item.Items[0] == placeholderNode;
    }

    private void FoldersTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        try
        {
            TreeView tree = (TreeView)sender;
            TreeViewItem selectedTreeViewItem = ((TreeViewItem)tree.SelectedItem);

            if (selectedTreeViewItem == null)
                return;

            if (selectedTreeViewItem.Tag is Folder folder)
            {
                SelectedPath = folder.Path;
                FolderSelected?.Invoke(this, new EventArgs());
            }
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    public void GoToFolder(string folderFullPath)
    {
        foreach (var item in foldersTreeView.Items)
        {
            TreeViewItem treeViewItem = (TreeViewItem)item;
            // TODO: SHOULD ASK THE USER IF HE WANTS TO SEE HIDDEN FOLDERS.
            GoToFolder(treeViewItem, folderFullPath, false);
        }
    }

    private void GoToFolder(TreeViewItem item, string folderFullPath, bool includeHidden)
    {
        if (item.Tag is Folder folder)
        {
            if (folderFullPath.StartsWith(folder.Path, StringComparison.InvariantCultureIgnoreCase))
            {
                if (LacksSubItems(item))
                {
                    AddSubItems(item, includeHidden);
                }

                item.IsExpanded = true;

                if (folder.Path == folderFullPath)
                {
                    item.IsSelected = true;
                    item.BringIntoView();
                }
                else
                {
                    foreach (var subItem in item.Items)
                    {
                        TreeViewItem treeViewItem = (TreeViewItem)subItem;
                        GoToFolder(treeViewItem, folderFullPath, includeHidden);
                    }
                }
            }
        }
    }
}
