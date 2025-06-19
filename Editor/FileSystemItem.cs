using System.Collections.Generic;

namespace YourApp
{
    public class FileSystemItem
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
        public List<FileSystemItem> Children { get; set; } = new();
    }
}