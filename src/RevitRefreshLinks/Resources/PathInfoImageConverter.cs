using System;
using System.Globalization;
using System.Windows.Data;

using RevitRefreshLinks.ViewModels;

using Wpf.Ui.Controls;

namespace RevitRefreshLinks.Resources;
internal class PathInfoImageConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if(value is DirectoryViewModel dir) {
            return SymbolRegular.Folder24;
        } else if(value is FileViewModel) {
            return SymbolRegular.Building24;
        }

        return default;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}
