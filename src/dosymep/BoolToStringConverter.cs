using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace dosymep.WPF.Converters {
    public class BoolToStringConverter : MarkupExtension, IValueConverter {
        /// <summary> 
        /// Вывод, когда значение bool == True
        /// </summary>
        public string WhenTrue { get; set; } = "Да";

        /// <summary>
        /// Вывод, когда значение bool == False
        /// </summary>
        public string WhenFalse { get; set; } = "Нет";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            bool? boolValue = (bool?) value;
            return boolValue == true ? WhenTrue : WhenFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value?.Equals(WhenTrue) == true) {
                return true;
            }

            if(value?.Equals(WhenFalse) == true) {
                return false;
            }

            return default;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }
}
