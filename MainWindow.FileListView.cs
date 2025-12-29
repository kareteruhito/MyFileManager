using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Data;

/*
* FileListViewの初期化とイベントハンドラ
*/

namespace MyFileManager;

public partial class MainWindow : Window
{
    // FileListViewのカレントディレクトリを取得
    private string GetFileListViewCurrentDirectory()
    {
        var currentDirectory = FileListView.Tag as string;
        return currentDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
    // FileListViewのカレントディレクトリを設定
    private void SetFileListViewCurrentDirectory(string path)
    {
        var currentDirectory = GetFileListViewCurrentDirectory();
        if (currentDirectory == path) return;
        
        FileListView.Tag = path;
        AddressTextBox.Text = path;

        ReloadFileListView(path);
    }
    // FileListViewの内容を再読み込み
    private async void ReloadFileListView(string? path = null)
    {
        if (path is null)
        {
            path = GetFileListViewCurrentDirectory();
        }
        
        var sw = Stopwatch.StartNew();
                FileListView.Visibility = Visibility.Collapsed;
                StatusText.Text = "Loading...";

        Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Render,
            new Action(() => { })
        );

        var fileItems = new ObservableCollection<FileItem>();

        var dirTask = Task.Run(() => FileSystemUtil.GetDirs(path));

        var dirs = await dirTask;
        
        var fileTask = Task.Run(() => FileSystemUtil.GetFiles(path));
        foreach (var dir in dirs)
        {
            fileItems.Add(new FileItem
            {
                Name = Path.GetFileName(dir),
                FullPath = dir,
                Type = "Directory",
                Size = 0
            });
        }

        var files = await fileTask;
        foreach (var file in files)
        {
            var info = new FileInfo(file);
            fileItems.Add(new FileItem
            {
                Name = info.Name,
                FullPath = info.FullName,
                Type = "File",
                Size = info.Length
            });
        }
        FileListView.ItemsSource = fileItems;
        FileListView.UpdateLayout();

        FileListView.Visibility = Visibility.Visible;
        sw.Stop();
        StatusText.Text = $"LoadingTime : {sw.ElapsedMilliseconds} ms";
    }

    // ダブルクリック処理
    private void FileListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (FileListView.SelectedItem is FileItem item)
        {
            if (item.Type == "Directory")
            {
                UpdateCurrentDirectory(item.FullPath);
            }
            else
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = item.FullPath,
                    UseShellExecute = true
                });
            }
        }
    }

    /* 右クリックメニュー処理 */
    private FileItem? SelectedItem =>
        FileListView.SelectedItem as FileItem;
    private void FileListView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var element = e.OriginalSource as DependencyObject;

        while (element != null && element is not ListViewItem)
        {
            element = VisualTreeHelper.GetParent(element);
        }

        if (element is ListViewItem item)
        {
            item.IsSelected = true;
            item.Focus();
        }
    }

    /*
    * 列ヘッダクリックでソート
    */
    private ListSortDirection _nameSortDirection = ListSortDirection.Ascending;
    private ListSortDirection _sizeSortDirection = ListSortDirection.Ascending;
    private void NameHeader_Click(object sender, RoutedEventArgs e)
    {
        var view = CollectionViewSource.GetDefaultView(FileListView.ItemsSource);
        if (view == null) return;

        view.SortDescriptions.Clear();
        // 第1キー：種類
        view.SortDescriptions.Add(
            new SortDescription(nameof(FileItem.Type), _nameSortDirection));
        // 第2キー：名前
        view.SortDescriptions.Add(
            new SortDescription(nameof(FileItem.Name), _nameSortDirection));

        // 次回クリック用に反転
        _nameSortDirection =
            _nameSortDirection == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;
    }
    private void SizeHeader_Click(object sender, RoutedEventArgs e)
    {
        var view = CollectionViewSource.GetDefaultView(FileListView.ItemsSource);
        if (view == null) return;

        view.SortDescriptions.Clear();
        // 第1キー：種類
        view.SortDescriptions.Add(
            new SortDescription(nameof(FileItem.Type), _sizeSortDirection));
        // 第2キー：サイズ
        view.SortDescriptions.Add(
            new SortDescription(nameof(FileItem.Size), _sizeSortDirection));

        // 次回クリック用に反転
        _sizeSortDirection =
            _sizeSortDirection == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;
    }

}