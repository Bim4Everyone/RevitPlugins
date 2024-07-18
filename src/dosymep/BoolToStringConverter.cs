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
            return boolValue.HasValue ? WhenTrue : WhenFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }
}
