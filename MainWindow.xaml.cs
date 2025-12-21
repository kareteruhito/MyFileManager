using System.Windows;

namespace MyFileManager;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // FolderTreeの初期化
        DataContext = this;
        LoadRoots();

        // FileListViewの初期化
        SetFileListViewDirectory(_currentDirectory);

        // AddressBarの初期化
        SetAddressBarCurrentDirectory(_currentDirectory);

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
        if (CurrentDirectory != path)
        {
            SetAddressBarCurrentDirectory(path);
        }

        // FileListView更新
        if (_currentDirectory != path)
        {
            SetFileListViewDirectory(path);
        }
    }
}