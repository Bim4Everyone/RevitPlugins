using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

using RevitBatchPrint.ViewModels;

namespace RevitBatchPrint.Converters {
    public class SelectedAlbumsConverter : MarkupExtension, IValueConverter {
        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value != null)
                return new List<object>((IEnumerable<object>) value);
            return null;
        }
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            ObservableCollection<PrintAlbumViewModel> result = new ObservableCollection<PrintAlbumViewModel>();
            var enumerable = (List<object>) value;
            if(enumerable != null)
                foreach(object item in enumerable)
                    result.Add((PrintAlbumViewModel) item);
            return result;
        }
    }
}
