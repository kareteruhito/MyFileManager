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

    private void UpButton_Click(object sender, RoutedEventArgs e)
    {
        var path = Path.GetDirectoryName(CurrentDirectory) ?? CurrentDirectory;
        SetAddressBarCurrentDirectory(path);
    }

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
            SetAddressBarCurrentDirectory(path);
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

    // アドレスバーのカレントディレクトリ設定
    private void SetAddressBarCurrentDirectory(string path)
    {
        if (CurrentDirectory == path) return;

        CurrentDirectory = path;
        AddressTextBox.Text = path;

        UpdateCurrentDirectory(path);
    }
}