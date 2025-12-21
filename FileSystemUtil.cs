using System.IO;

public static class FileSystemUtil
{
    // ここにファイルシステム関連のユーティリティメソッドを追加できます
    public static string[] GetDirs(string path)
    {
        return Directory
            .GetDirectories(path)
            .Select(d => new DirectoryInfo(d))
            .Where(di =>
                !di.Attributes.HasFlag(FileAttributes.Hidden) &&
                !di.Attributes.HasFlag(FileAttributes.System) &&
                !di.Name.StartsWith("."))
            .Select(di => di.FullName)
            .ToArray();
    }

    public static string[] GetFiles(string path)
    {
        return Directory
            .GetFiles(path)
            .Select(f => new FileInfo(f))
            .Where(fi =>
                !fi.Attributes.HasFlag(FileAttributes.Hidden) &&
                !fi.Attributes.HasFlag(FileAttributes.System) &&
                !fi.Name.StartsWith("."))
            .Select(fi => fi.FullName)
            .ToArray();
    }
    public static string[] GetDrives()
    {
        return DriveInfo
            .GetDrives()
            .Where(drive =>
                drive.IsReady &&
                (drive.DriveType.HasFlag(DriveType.Fixed)||drive.DriveType.HasFlag(DriveType.Network))
            )
            .Select(drive => drive.RootDirectory.FullName)
            .ToArray();
    }

}