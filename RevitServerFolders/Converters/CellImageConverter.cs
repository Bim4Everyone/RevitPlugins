using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;

using RevitServerFolders.ViewModels;

namespace RevitServerFolders.Converters {
    [ValueConversion(typeof(ModelObjectViewModel), typeof(ImageSource))]
    internal sealed class CellImageConverter : IValueConverter {
        public ImageSource Empty { get; set; }
        public ImageSource Model { get; set; }
        public ImageSource Folder { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is ModelObjectViewModel modelObjectViewModel) {
                if(modelObjectViewModel.IsFolder) {
                    return Folder;
                }

                return modelObjectViewModel.Name.EndsWith(".rvt", StringComparison.OrdinalIgnoreCase)
                    ? Model
                    : Empty;
            }

            return Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
