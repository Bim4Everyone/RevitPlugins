using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

using Autodesk.Revit.DB;

namespace RevitBatchPrint.ViewModels.EnumConverters {
    [ValueConversion(typeof(MarginType), typeof(IEnumerable<string>))]
    internal class MarginTypeConverter : MarkupExtension, IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var marginType = (MarginType?) value;
            switch(marginType) {
                case MarginType.NoMargin:
                    return "Без полей";
                case MarginType.PrinterLimit:
                    return "Пределы принтера";
                case MarginType.UserDefined:
                    return "Пользовательские";
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var marginType = (string) value;
            switch(marginType) {
                case "Без полей":
                    return MarginType.NoMargin;
                case "Пределы принтера":
                    return MarginType.PrinterLimit;
                case "Пользовательские":
                    return MarginType.UserDefined;
                default:
                    return null;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }

    internal class MarginTypeExtension : MarkupExtension {
        public override object ProvideValue(IServiceProvider serviceProvider) {
            return new[] {"Без полей", "Пределы принтера", "Пользовательские"};
        }
    }
}