using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System;
using System.Globalization;
using Avalonia.Platform;

namespace Editeur.Models
{
    public class FileSystemIconConverter : IValueConverter
    {
        public static FileSystemIconConverter Instance { get; } = new FileSystemIconConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isDirectory)
            {
                var icon = isDirectory 
                    ? "avares://Editeur/Assets/folder.png" 
                    : "avares://Editeur/Assets/file.png";
                
                try
                {
                    return new Bitmap(AssetLoader.Open(new Uri(icon)));
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}