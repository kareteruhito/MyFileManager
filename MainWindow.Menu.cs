/*
* メニュー関連の処理
*/

using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MyFileManager;

public partial class MainWindow : Window
{

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
    readonly private FileClipboard _clipboard = new ();

    // クリップボードに選択中のファイルをセット
    void SetClipboard(FileOperation op)
    {
        var paths = GetSelectedPaths();
        if (paths.Count == 0) return;

        _clipboard.Paths.Clear();
        _clipboard.Paths.AddRange(paths);
        _clipboard.Operation = op;
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

    // 仮名ディレクトリ名生成
    static private string CreateUniqueFolderName(string basePath, string baseName = "NewFolder")
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
        string parent = GetFileListViewCurrentDirectory();
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

            ReloadFileListView(null); // カレントディレクトリを変更していない・リロード
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

            ReloadFileListView(null); // カレントディレクトリを変更していない・リロード
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "名前の変更エラー");
        }
    }





    /* メインメニューイベントハンドラ ここから */
    private void Menu_OnNewWindow(object sender, RoutedEventArgs e)
    {
        new MainWindow().Show();
    }

    private void Menu_OnExit(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Menu_OnRefresh(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Refreshed";
    }

    private void Menu_OnAbout(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Explorer Sample\nWPF Menu + StatusBar",
            "About",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    /* メインメニューイベントハンドラ ここまで */






    /* コンテキストメニューイベントハンドラ ここから */

    // メニュー：コピー
    void Menu_Copy_Click(object sender, RoutedEventArgs e)
    {
        SetClipboard(FileOperation.Copy);
    }
    // メニュー：切り取り
    void Menu_Cut_Click(object sender, RoutedEventArgs e)
    {
        SetClipboard(FileOperation.Cut);
    }
    // メニュー：貼り付け
    void Menu_Paste_Click(object sender, RoutedEventArgs e)
    {
        if (_clipboard.Operation == FileOperation.None) return;

        string destDir = GetFileListViewCurrentDirectory(); // カレントディレクトリを取得

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
        ReloadFileListView(null); // カレントディレクトリ変更していない・リロード
    }

    // メニュー：開く
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

    // メニュー：エクスプローラーで表示
    private void Menu_OpenInExplorer_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedItem == null) return;

        Process.Start("explorer.exe", $"/select,\"{SelectedItem.FullPath}\"");
    }

    // メニュー：削除
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

        ReloadFileListView(null); // カレントディレクトリ変更していない・リロード
    }
    // メニュー：新しいフォルダ作成
    private void Menu_NewFolder_Click(object sender, RoutedEventArgs e)
    {
        CreateNewDirectory();
    }
    // メニュー：名前変更
    private void Menu_Rename_Click(object sender, RoutedEventArgs e)
    {
        if (FileListView.SelectedItem is FileItem item)
            RenameItem(item);
    }
    
    /* コンテキストメニューイベントハンドラ ここまで */


}