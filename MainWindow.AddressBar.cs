using System.IO;
using System.Windows;
using System.Windows.Input;

/*
* AddressBarの初期化とイベントハンドラ
*/

namespace MyFileManager;

public partial class MainWindow : Window
{

    private void UpButton_Click(object sender, RoutedEventArgs e)
    {
        var path = GetFileListViewCurrentDirectory();
        var parent = Path.GetDirectoryName(path) ?? path;
        UpdateCurrentDirectory(parent);
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
            UpdateCurrentDirectory(path);
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
}