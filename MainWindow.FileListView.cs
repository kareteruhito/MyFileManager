using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Data;

/*
* FileListViewの初期化とイベントハンドラ
*/

namespace MyFileManager;

public partial class MainWindow : Window
{
    private string _currentDirectory = @"C:\";

    private async void SetFileListViewDirectory(string path)
    {
var sw = Stopwatch.StartNew();
        FileListView.Visibility = Visibility.Collapsed;
        StatusText.Text = "Loading...";

Dispatcher.Invoke(
    System.Windows.Threading.DispatcherPriority.Render,
    new Action(() => { })
);
        _currentDirectory = path;
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

    // 選択変更（イベントドリブン）
    private void FileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FileListView.SelectedItem is FileItem item)
        {
            //System.Diagnostics.Debug.WriteLine($"Selected: {item.FullPath}");
        }
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
            element = System.Windows.Media.VisualTreeHelper.GetParent(element);
        }

        if (element is ListViewItem item)
        {
            item.IsSelected = true;
            item.Focus();
        }
    }

    private void Menu_Open_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedItem == null) return;

        if (SelectedItem.Type == "Directory")
        {
            UpdateCurrentDirectory(SelectedItem.FullPath);
        }
        else
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = SelectedItem.FullPath,
                UseShellExecute = true
            });
        }
    }

    private void Menu_OpenInExplorer_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedItem == null) return;

        Process.Start("explorer.exe", $"/select,\"{SelectedItem.FullPath}\"");
    }

    private void Menu_Delete_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedItem == null) return;

        if (MessageBox.Show(
            $"{SelectedItem.Name} を削除しますか？",
            "確認",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        if (SelectedItem.Type == "Directory")
            Directory.Delete(SelectedItem.FullPath, true);
        else
            File.Delete(SelectedItem.FullPath);

        SetFileListViewDirectory(_currentDirectory);
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