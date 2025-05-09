using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;

namespace Editeur.ViewModels
{
    public class FileItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsDirectory { get; set; }
        public ObservableCollection<FileItem> Children { get; } = new ObservableCollection<FileItem>();
        
        public Bitmap Icon
        {
            get
            {
                // Vous devrez implémenter la logique pour obtenir les icônes
                // Ceci est un exemple simplifié
                return null;
            }
        }
    }
}