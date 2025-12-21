using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

/*
* FolderTreeの初期化とイベントハンドラ
*/

namespace MyFileManager;

public partial class MainWindow : Window
{
    public ObservableCollection<DirectoryNode> RootNodes { get; }
        = new ObservableCollection<DirectoryNode>();
/*
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        LoadRoots();
    }
*/
    private void LoadRoots()
    {
        RootNodes.Clear();

        foreach (var drive in FileSystemUtil.GetDrives())
        {
            var node = new DirectoryNode(drive);
            node.EnsureDummy();

            RootNodes.Add(node);
        }
    }

    private void FolderTreeItem_Expanded(object sender, RoutedEventArgs e)
    {
        if (sender is not TreeViewItem item) return;
        if (item.DataContext is not DirectoryNode node) return;

        node.LoadChildren();
    }

    private void FolderTree_SelectedItemChanged(
        object sender,
        RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is DirectoryNode node)
        {
            Title = node.FullPath;
            // ListView更新など
            UpdateCurrentDirectory(node.FullPath);
        }
    }

}
