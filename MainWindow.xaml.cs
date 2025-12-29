using System.Runtime.CompilerServices;
using System.Windows;

namespace MyFileManager;

public partial class MainWindow : Window
{
    // 前回のカレントディレクトリを保存・復元のためのパス
    private static string GetLastDirectoryPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return System.IO.Path.Combine(appData, "MyFileManager", "last_directory.txt");
    }
    // 前回のカレントディレクトリを取得
    private static string LoadLastDirectory()
    {
        var path = GetLastDirectoryPath();
        if (System.IO.File.Exists(path))
        {
            return System.IO.File.ReadAllText(path);
        }
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
    // 次回用にカレントディレクトリを保存
    private static void SaveLastDirectory(string path)
    {
        var filePath = GetLastDirectoryPath();
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
        var path = LoadLastDirectory();

        // コマンドライン引数でディレクトリが指定されていればそちらを優先
        string[] args = Environment.GetCommandLineArgs();
        if (args.Length > 1)
        {
            var cmdPath = args[1];
            if (System.IO.Directory.Exists(cmdPath))
            {
                path = cmdPath;
            }
        }

        // カレントディレクトリ初期化
        UpdateCurrentDirectory(path);
    }
    /* クロージング イベントハンドラ */
    public void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // カレントディレクトリを保存
        var path = GetFileListViewCurrentDirectory();
        SaveLastDirectory(path);
    }

    /* カレントディレクトリの変更 */
    private void UpdateCurrentDirectory(string path)
    {
        // AddressBar更新
        AddressTextBox.Text = path;

        // FileListView更新
        SetFileListViewCurrentDirectory(path);
    }
}