using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

using Autodesk.Revit.DB;

namespace RevitBatchPrint.ViewModels.EnumConverters {
    [ValueConversion(typeof(RasterQualityType), typeof(string))]
    internal class RasterQualityTypeConverter : MarkupExtension, IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var colorDepthType = (RasterQualityType?) value;
            switch(colorDepthType) {
                case RasterQualityType.Low:
                    return "Низкое";
                case RasterQualityType.Medium:
                    return "Среднее";
                case RasterQualityType.High:
                    return "Высокое";
                case RasterQualityType.Presentation:
                    return "Презентационное";
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var marginType = (string) value;
            switch(marginType) {
                case "Низкое":
                    return RasterQualityType.Low;
                case "Среднее":
                    return RasterQualityType.Medium;
                case "Высокое":
                    return RasterQualityType.High;
                case "Презентационное":
                    return RasterQualityType.Presentation;
                default:
                    return null;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }

    internal class RasterQualityTypeExtension : MarkupExtension {
        public override object ProvideValue(IServiceProvider serviceProvider) {
            return new[] {"Низкое", "Среднее", "Высокое", "Презентационное"};
        }
    }
}