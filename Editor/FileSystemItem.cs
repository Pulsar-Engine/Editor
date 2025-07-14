using System.Collections.Generic;

namespace PulsarApp
{
    public class FileSystemItem
    {
        public required string Name { get; set; }
        public required string FullPath { get; set; }
        public bool IsDirectory { get; set; }
        public List<FileSystemItem> Children { get; set; } = new List<FileSystemItem>();
    }
}