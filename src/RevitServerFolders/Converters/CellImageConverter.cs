using System;
using System.Globalization;
using System.Windows.Data;

using RevitServerFolders.ViewModels;

namespace RevitServerFolders.Converters;
[ValueConversion(typeof(ModelObjectViewModel), typeof(object))]
internal sealed class CellImageConverter : IValueConverter {
    public object Empty { get; set; }
    public object Model { get; set; }
    public object Folder { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if(value is ModelObjectViewModel modelObjectViewModel) {
            return modelObjectViewModel.IsFolder
                ? Folder
                : modelObjectViewModel.Name.EndsWith(".rvt", StringComparison.OrdinalIgnoreCase)
                ? Model
                : Empty;
        }

        return Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
