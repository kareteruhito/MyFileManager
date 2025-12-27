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

    /*
    * コピー・カット＆ペースト処理(アプリ内クリップボード)
    */

    // クリップボードの内容
    enum FileOperation
    {
        None,
        Copy,
        Cut
    }

    // ファイルクリップボード
    class FileClipboard
    {
        public FileOperation Operation { get; set; } = FileOperation.None;
        public List<string> Paths { get; } = new();
    }
    
    // 選択中のファイルを取得
    List<string> GetSelectedPaths()
    {
        return FileListView.SelectedItems
            .Cast<FileItem>()
            .Select(x => x.FullPath)
            .ToList();
    }
    private FileClipboard _clipboard = new FileClipboard();
    // 「コピー」メニュー項目クリック
    void Copy_Click(object sender, RoutedEventArgs e)
    {
        SetClipboard(FileOperation.Copy);
    }
    // 「切り取り」メニュー項目クリック
    void Cut_Click(object sender, RoutedEventArgs e)
    {
        SetClipboard(FileOperation.Cut);
    }
    // クリップボードに選択中のファイルをセット
    void SetClipboard(FileOperation op)
    {
        var paths = GetSelectedPaths();
        if (paths.Count == 0) return;

        _clipboard.Paths.Clear();
        _clipboard.Paths.AddRange(paths);
        _clipboard.Operation = op;
    }
    // 「貼り付け」メニュー項目クリック
    void Paste_Click(object sender, RoutedEventArgs e)
    {
        if (_clipboard.Operation == FileOperation.None) return;

        string destDir = _currentDirectory; // カレントディレクトリ

        foreach (var src in _clipboard.Paths)
        {
            string name = Path.GetFileName(src);
            string dest = Path.Combine(destDir, name);

            if (File.Exists(dest) || Directory.Exists(dest))
            {
                // 同名が存在する場合はスキップ
                continue;
            }            

            if (_clipboard.Operation == FileOperation.Copy)
            {
                if (Directory.Exists(src))
                {
                    // ディレクトリ
                    // 未実装
                    continue;
                }
                else if (File.Exists(src))
                {
                    // ファイル
                    File.Copy(src, dest, overwrite: false);
                }                
            }
            else if (_clipboard.Operation == FileOperation.Cut)
            {
                if (Directory.Exists(src))
                {
                    // ディレクトリ
                    Directory.Move(src, dest);
                }
                else if (File.Exists(src))
                {
                    // ファイル
                    File.Move(src, dest);
                }                
            }
        }

        _clipboard.Paths.Clear();
        _clipboard.Operation = FileOperation.None;

        // 既存の一覧更新処理
        SetFileListViewDirectory(_currentDirectory);
    }
    // コンテキストメニュー表示前処理
    void FileContextMenu_Opening(object sender, RoutedEventArgs e)
    {
        var menu = (ContextMenu)sender;

        var copy = (MenuItem)menu.Items[0];
        var cut  = (MenuItem)menu.Items[1];
        var paste = (MenuItem)menu.Items[3];

        // メニュー項目の有効/無効設定

        // ディレクトリはコピー不可とする

        var paths = FileListView.SelectedItems
            .Cast<FileItem>()
            .Select(x => x.FullPath)
            .ToList();

        bool hasFile = false;
        bool hasDirectory = false;

        foreach (var path in paths)
        {
            if (File.Exists(path))
                hasFile = true;
            else if (Directory.Exists(path))
                hasDirectory = true;
        }

        copy.IsEnabled =
            paths.Count > 0 &&
            hasFile &&
            !hasDirectory;

        cut.IsEnabled =
            paths.Count > 0;

        paste.IsEnabled =
            _clipboard.Operation != FileOperation.None &&
            paths.Count == 0;
    }
    // 右クリックで項目を選択状態にする
    void FileListView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        DependencyObject? current = e.OriginalSource as DependencyObject;

        while (current != null && current is not ListViewItem)
        {
            current = VisualTreeHelper.GetParent(current);
        }

        if (current is ListViewItem item)
        {
            item.IsSelected = true;
        }
    }
    /*
    * 新しいいフォルダ作成・名前変更
    */
    // 新しいフォルダ作成
    private void NewFolder_Click(object sender, RoutedEventArgs e)
    {
        CreateNewDirectory();
    }
    // 名前変更
    private void Rename_Click(object sender, RoutedEventArgs e)
    {
        if (FileListView.SelectedItem is FileItem item)
            RenameItem(item);
    }
    // 仮名ディレクトリ名生成
    private string CreateUniqueFolderName(string basePath, string baseName = "NewFolder")
    {
        string path = Path.Combine(basePath, baseName);
        int index = 1;

        while (Directory.Exists(path))
        {
            path = Path.Combine(basePath, $"{baseName} ({index})");
            index++;
        }
        return path;
    }
    // 名前変更ダイアログ表示
    private void CreateNewDirectory()
    {
        string parent = CurrentDirectory;
        string tempPath = CreateUniqueFolderName(parent);

        try
        {
            Directory.CreateDirectory(tempPath);

            var dlg = new RenameDialog(Path.GetFileName(tempPath))
            {
                Owner = this
            };

            if (dlg.ShowDialog() == true)
            {
                string newPath = Path.Combine(parent, dlg.ResultName);
                if (tempPath != newPath)
                    Directory.Move(tempPath, newPath);
            }
            else
            {
                // キャンセル時は削除
                Directory.Delete(tempPath);
            }

            SetFileListViewDirectory(_currentDirectory);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "ディレクトリ作成エラー");
        }
    }
    // 名前変更処理
    private void RenameItem(FileItem item)
    {
        var dlg = new RenameDialog(item.Name)
        {
            Owner = this
        };

        if (dlg.ShowDialog() != true)
            return;

        string newPath = Path.Combine(
            Path.GetDirectoryName(item.FullPath)!,
            dlg.ResultName);

        try
        {
            if (Directory.Exists(item.FullPath))
                Directory.Move(item.FullPath, newPath);
            else if (File.Exists(item.FullPath))
                File.Move(item.FullPath, newPath);

            SetFileListViewDirectory(_currentDirectory);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "名前の変更エラー");
        }
    }

}