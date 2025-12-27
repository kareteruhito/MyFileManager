using System.Windows;

namespace MyFileManager;

public partial class RenameDialog : Window
{
    public string ResultName => NameBox.Text;

    public RenameDialog(string currentName)
    {
        InitializeComponent();
        NameBox.Text = currentName;
        NameBox.SelectAll();
        Loaded += (_, _) => NameBox.Focus();
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
            return;

        DialogResult = true;
    }
}
