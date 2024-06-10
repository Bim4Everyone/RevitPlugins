using System;
using System.Globalization;

namespace RevitReinforcementCoefficient.Views.Converters {
    public class BoolToString : ConvertorBase<BoolToString> {
        private string _whenTrue = "Да";
        private string _whenFalse = "Нет";

        /// <summary> 
        /// Вывод, когда значение bool == True
        /// </summary>
        public string WhenTrue {
            set {
                _whenTrue = value;
            }
            private get {
                return _whenTrue;
            }
        }

        /// <summary>
        /// Вывод, когда значение bool == False
        /// </summary>
        public string WhenFalse {
            set {
                _whenFalse = value;
            }
            private get {
                return _whenFalse;
            }
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (bool) value == true ? WhenTrue : WhenFalse;
        }
    }
}
