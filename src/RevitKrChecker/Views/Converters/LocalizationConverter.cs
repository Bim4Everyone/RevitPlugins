using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using dosymep.SimpleServices;

namespace RevitKrChecker.Views.Converters {
    public class LocalizationConverter : Freezable, IValueConverter {
        protected override Freezable CreateInstanceCore() {
            return new LocalizationConverter();
        }
        public static readonly DependencyProperty LocalizationServiceProperty =
            DependencyProperty.Register(
                "LocalizationService",
                typeof(ILocalizationService),
                typeof(LocalizationConverter),
                new FrameworkPropertyMetadata(null)
                );

        public ILocalizationService LocalizationService {
            get { return GetValue(LocalizationServiceProperty) as ILocalizationService; }
            set { SetValue(LocalizationServiceProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is string keyValue) {
                return LocalizationService?.GetLocalizedString($"ReportWindow.{keyValue}") ?? value;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
