using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace RevitCopyInteriorSpecs.Views.Converters {
    public class NumberWithOffsetConverter : MarkupExtension, IValueConverter {
        /// <summary> 
        /// Вывод, когда значение bool == True
        /// </summary>
        public double Offset { get; set; } = 0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            // Проверяем, что значение является double
            if(value is double originalNumber) {
                // Изменяем значение на указанное учитывая разницу
                return originalNumber + Offset;
            }
            // Если значение не double, возвращаем его без изменений
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            // Обратное преобразование не требуется
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }
}
