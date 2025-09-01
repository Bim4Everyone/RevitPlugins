using System;
using System.Windows.Media.Imaging;

using dosymep.Bim4Everyone;

namespace RevitRefreshLinks.Resources;
internal static class IconResources {
    private static readonly string _iconsFolderPath
        = $"pack://application:,,,/RevitRefreshLinks_{ModuleEnvironment.RevitVersion};component/Resources/Icons/";

    public static BitmapImage HomeImg { get; } = GetImg("home.png");
    public static BitmapImage ArrowUpImg { get; } = GetImg("arrow.up.png");
    public static BitmapImage ArrowLeftImg { get; } = GetImg("arrow.left.png");
    public static BitmapImage ArrowRightImg { get; } = GetImg("arrow.right.png");
    public static BitmapImage SearchImg { get; } = GetImg("search.png");
    public static BitmapImage UpdateImg { get; } = GetImg("update.png");


    private static BitmapImage GetImg(string iconName) {
        return new BitmapImage(new Uri(_iconsFolderPath + iconName, UriKind.Absolute));
    }
}
