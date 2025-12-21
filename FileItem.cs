/*
* FileItemクラスの定義
*/

namespace MyFileManager;
public class FileItem
{
    public string Name { get; set; } = "";
    public string FullPath { get; set; } = "";
    public string Type { get; set; } = "";   // File / Directory
    public long Size { get; set; }
}