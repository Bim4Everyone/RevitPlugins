using System;
using System.Globalization;

namespace RevitReinforcementCoefficient.Views.Converters {
    public class NullToString : ConvertorBase<NullToString> {
        private string _whenNull = "<Нет данных>";

        /// <summary> 
        /// Вывод, когда значение Null
        /// </summary>
        public string WhenNull {
            set {
                _whenNull = value;
            }
            private get {
                return _whenNull;
            }
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value is string ? (string.IsNullOrEmpty((string) value) ? WhenNull : value) : value;
        }
    }
}
