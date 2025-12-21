using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;

/*
* FileListViewの初期化とイベントハンドラ
*/

namespace MyFileManager;

public partial class MainWindow : Window
{
    public ObservableCollection<FileItem> FileItems { get; }
        = new ObservableCollection<FileItem>();

    private string _currentDirectory = @"C:\";
/*
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        LoadDirectory(_currentDirectory);
    }
*/

    private void LoadDirectory(string path)
    {
        FileItems.Clear();

        foreach (var dir in FileSystemUtil.GetDirs(path))
        {
            FileItems.Add(new FileItem
            {
                Name = Path.GetFileName(dir),
                FullPath = dir,
                Type = "Directory",
                Size = 0
            });
        }

        foreach (var file in FileSystemUtil.GetFiles(path))
        {
            var info = new FileInfo(file);
            FileItems.Add(new FileItem
            {
                Name = info.Name,
                FullPath = info.FullName,
                Type = "File",
                Size = info.Length
            });
        }
    }

    // 選択変更（イベントドリブン）
    private void FileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FileListView.SelectedItem is FileItem item)
        {
            Debug.WriteLine($"Selected: {item.FullPath}");
        }
    }

    // ダブルクリック処理
    private void FileListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (FileListView.SelectedItem is FileItem item)
        {
            if (item.Type == "Directory")
            {
                //_currentDirectory = item.FullPath;
                //LoadDirectory(_currentDirectory);
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
            //LoadDirectory(SelectedItem.FullPath);
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

        LoadDirectory(_currentDirectory);
    }

}