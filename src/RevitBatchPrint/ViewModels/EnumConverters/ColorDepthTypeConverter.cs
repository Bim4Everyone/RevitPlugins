using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

using Autodesk.Revit.DB;

namespace RevitBatchPrint.ViewModels.EnumConverters {
    [ValueConversion(typeof(ColorDepthType), typeof(string))]
    internal class ColorDepthTypeConverter : MarkupExtension, IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var colorDepthType = (ColorDepthType?) value;
            switch(colorDepthType) {
                case ColorDepthType.Color:
                    return "Цвет";
                case ColorDepthType.GrayScale:
                    return "Оттенки серого";
                case ColorDepthType.BlackLine:
                    return "Черные линии";
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var marginType = (string) value;
            switch(marginType) {
                case "Цвет":
                    return ColorDepthType.Color;
                case "Оттенки серого":
                    return ColorDepthType.GrayScale;
                case "Черные линии":
                    return ColorDepthType.BlackLine;
                default:
                    return null;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }

    internal class ColorDepthTypeExtension : MarkupExtension {
        public override object ProvideValue(IServiceProvider serviceProvider) {
            return new[] {"Цвет", "Оттенки серого", "Черные линии"};
        }
    }
}