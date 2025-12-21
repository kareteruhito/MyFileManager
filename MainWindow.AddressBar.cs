using System.IO;
using System.Windows;
using System.Windows.Input;

/*
* AddressBarの初期化とイベントハンドラ
*/

namespace MyFileManager;

public partial class MainWindow : Window
{
    public string CurrentDirectory { get; private set; } = "";

/*
    public MainWindow()
    {
        InitializeComponent();

        // 初期ディレクトリ
        SetCurrentDirectory(@"C:\");
    }
*/

    private void MoveButton_Click(object sender, RoutedEventArgs e)
    {
        Navigate(AddressTextBox.Text);
    }

    private void AddressTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        // Enterキーで移動
        if (e.Key == Key.Enter)
        {
            Navigate(AddressTextBox.Text);
        }
    }

    private void Navigate(string path)
    {
        if (Directory.Exists(path))
        {
            SetCurrentDirectory(path);
        }
        else
        {
            MessageBox.Show(
                "ディレクトリが存在しません。",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void SetCurrentDirectory(string path)
    {
        CurrentDirectory = path;
        AddressTextBox.Text = path;

        // 本来はここで FileListView / ListView を更新する
        // LoadFiles(path);
        //System.Diagnostics.Debug.WriteLine($"Navigated to: {path}");    // 確認用
        UpdateCurrentDirectory(path);
    }
}