using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace RevitMarkPlacement.Extensions;

internal static class BitmapExtensions {
    public static BitmapSource ConvertToBitmapSource(this Bitmap bitmap) {
        using MemoryStream memory = new();
        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
        
        memory.Position = 0;

        BitmapImage bitmapImage = new();
        bitmapImage.BeginInit();
       
        bitmapImage.StreamSource = memory;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
       
        bitmapImage.EndInit();
        return bitmapImage;
    }
}
