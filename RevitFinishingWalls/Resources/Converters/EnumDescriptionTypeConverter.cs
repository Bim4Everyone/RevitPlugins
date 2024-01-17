using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

using dosymep.WPF.Extensions;

namespace RevitFinishingWalls.Resources.Converters {
    internal class EnumDescriptionTypeConverter : MarkupExtension, IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((Enum) value).GetDescription();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }
}
