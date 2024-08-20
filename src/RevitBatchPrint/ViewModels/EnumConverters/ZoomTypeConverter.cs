using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

using Autodesk.Revit.DB;

namespace RevitBatchPrint.ViewModels.EnumConverters {
    [ValueConversion(typeof(ZoomType), typeof(IEnumerable<string>))]
    internal class ZoomTypeConverter : MarkupExtension, IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var zoomType = (ZoomType?) value;
            switch(zoomType) {
                case ZoomType.Zoom:
                    return "Масштаб";
                case ZoomType.FitToPage:
                    return "Вписать";
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var zoomType = (string) value;
            switch(zoomType) {
                case "Масштаб":
                    return ZoomType.Zoom;
                case "Вписать":
                    return ZoomType.FitToPage;
                default:
                    return null;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }

    internal class ZoomTypeExtension : MarkupExtension {
        public override object ProvideValue(IServiceProvider serviceProvider) {
            return new[] {"Масштаб", "Вписать"};
        }
    }
}