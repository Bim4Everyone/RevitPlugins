using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

using Autodesk.Revit.DB;

namespace RevitBatchPrint.ViewModels.EnumConverters {
    public class HiddenLineViewsTypeConverter : MarkupExtension, IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var hiddenLineViewsType = (HiddenLineViewsType?) value;
            switch(hiddenLineViewsType) {
                case HiddenLineViewsType.RasterProcessing:
                return "Растровая обработка";
                case HiddenLineViewsType.VectorProcessing:
                return "Векторная обработка";
                default:
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var hiddenLineViewsType = (string) value;
            switch(hiddenLineViewsType) {
                case "Растровая обработка":
                return HiddenLineViewsType.RasterProcessing;
                case "Векторная обработка":
                return HiddenLineViewsType.VectorProcessing;
                default:
                return null;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return new[] { "Растровая обработка", "Векторная обработка" };
        }
    }
}