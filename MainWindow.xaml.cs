using System.Runtime.CompilerServices;
using System.Windows;

namespace MyFileManager;

public partial class MainWindow : Window
{
    // 前回のカレントディレクトリを保存・復元のためのパス
    private static string getLastDirectoryPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return System.IO.Path.Combine(appData, "MyFileManager", "last_directory.txt");
    }
    // 前回のカレントディレクトリを取得
    private static string loadLastDirectory()
    {
        var path = getLastDirectoryPath();
        if (System.IO.File.Exists(path))
        {
            return System.IO.File.ReadAllText(path);
        }
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
    // 次回用にカレントディレクトリを保存
    private static void saveLastDirectory(string path)
    {
        var filePath = getLastDirectoryPath();
        var dir = System.IO.Path.GetDirectoryName(filePath);
        if (dir != null && !System.IO.Directory.Exists(dir))
        {
            System.IO.Directory.CreateDirectory(dir);
        }
        System.IO.File.WriteAllText(filePath, path);
    }
    /* コンストラクタ */
    public MainWindow()
    {
        InitializeComponent();

        this.Closing += Window_Closing;

        // FolderTreeの初期化
        DataContext = this;
        LoadRoots();

        // 前回のカレントディレクトリを復元
        _fileListViewCurrentDirectory = loadLastDirectory();

        // FileListViewの初期化
        SetFileListViewDirectory(_fileListViewCurrentDirectory);

        // AddressBarの初期化
        SetAddressBarCurrentDirectory(_fileListViewCurrentDirectory);

    }
    /* クロージング イベントハンドラ */
    public void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // カレントディレクトリを保存
        saveLastDirectory(_fileListViewCurrentDirectory);
    }

    /* メニューイベントハンドラ */
    private void OnNewWindow(object sender, RoutedEventArgs e)
    {
        new MainWindow().Show();
    }

    private void OnExit(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnRefresh(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Refreshed";
    }

    private void OnAbout(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Explorer Sample\nWPF Menu + StatusBar",
            "About",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    /* ここまでメニューイベントハンドラ */

    /* カレントディレクトリの変更 */
    private void UpdateCurrentDirectory(string path)
    {
        // AddressBar更新
        if (AddressBarCurrentDirectory != path)
        {
            SetAddressBarCurrentDirectory(path);
        }

        // FileListView更新
        if (_fileListViewCurrentDirectory != path)
        {
            SetFileListViewDirectory(path);
        }
    }
}