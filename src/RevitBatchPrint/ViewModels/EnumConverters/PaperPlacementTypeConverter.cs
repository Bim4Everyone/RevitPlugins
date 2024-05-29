using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

using Autodesk.Revit.DB;

namespace RevitBatchPrint.ViewModels.EnumConverters {
    [ValueConversion(typeof(PaperPlacementType), typeof(IEnumerable<string>))]
    internal class PaperPlacementTypeConverter : MarkupExtension, IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var placementType = (PaperPlacementType?) value;
            switch(placementType) {
                case PaperPlacementType.Center:
                    return "Центр";
                case PaperPlacementType.Margins:
                    return "Смещение угла";
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var placementType = (string) value;
            switch(placementType) {
                case "Центр":
                    return PaperPlacementType.Center;
                case "Смещение угла":
                    return PaperPlacementType.Margins;
                default:
                    return null;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }

    internal class PaperPlacementTypeExtension : MarkupExtension {
        public override object ProvideValue(IServiceProvider serviceProvider) {
            return new[] {"Центр", "Смещение угла"};
        }
    }
}