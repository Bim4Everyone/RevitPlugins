using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

using dosymep.Bim4Everyone;

using RevitRefreshLinks.ViewModels;

namespace RevitRefreshLinks.Resources;
internal class PathInfoImageConverter : IValueConverter {
    private static readonly string _iconsFolderPath =
        $"pack://application:,,,/RevitRefreshLinks_{ModuleEnvironment.RevitVersion};component/Resources/Icons/";

    private static readonly BitmapImage _emptyFolder = GetImg("folder.empty.png");
    private static readonly BitmapImage _folder = GetImg("folder.png");
    private static readonly BitmapImage _file = GetImg("file.png");

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if(value is DirectoryViewModel dir) {
            return dir.Content.Count > 0 ? _folder : _emptyFolder;
        } else if(value is FileViewModel) {
            return _file;
        }

        return default;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }

    private static BitmapImage GetImg(string iconName) {
        return new BitmapImage(new Uri(_iconsFolderPath + iconName, UriKind.Absolute));
    }
}
