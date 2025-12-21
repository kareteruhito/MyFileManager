using System.Collections.ObjectModel;
using System.IO;

/*
* FoldeerTree用のディレクトリノード
*/

namespace MyFileManager;
public sealed class DirectoryNode
{
    public string Name { get; }
    public string FullPath { get; }

    public ObservableCollection<DirectoryNode> Children { get; }
        = new ObservableCollection<DirectoryNode>();

    public bool IsLoaded { get; private set; }
    public bool IsDummy { get; }

    private DirectoryNode(string path, bool isDummy)
    {
        FullPath = path;
        IsDummy = isDummy;

        Name = isDummy
            ? string.Empty
            : Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar));

        if (string.IsNullOrEmpty(Name))
            Name = path;
    }

    public DirectoryNode(string path)
        : this(path, isDummy: false)
    {
    }

    public static DirectoryNode CreateDummy(string parentPath)
        => new DirectoryNode(parentPath, isDummy: true);

    public void EnsureDummy()
    {
        if (Children.Count == 0)
        {
            Children.Add(CreateDummy(FullPath));
        }
    }

    public void LoadChildren()
    {
        if (IsLoaded) return;

        Children.Clear();
        IsLoaded = true;

        try
        {
            foreach (var dir in FileSystemUtil.GetDirs(FullPath))
            {
                var child = new DirectoryNode(dir);
                child.EnsureDummy();
                Children.Add(child);
            }
        }
        catch
        {
            // Explorer同様、例外は無視
        }
    }
}
