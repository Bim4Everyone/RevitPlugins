using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace dosymep.WPF.Converters {
    public class NullToStringConverter : MarkupExtension, IValueConverter {
        /// <summary> 
        /// Вывод, когда значение Null
        /// </summary>
        public string WhenNull { get; set; } = "<Нет данных>";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var stringValue = value?.ToString();
            return string.IsNullOrEmpty(stringValue) ? WhenNull : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }
}
