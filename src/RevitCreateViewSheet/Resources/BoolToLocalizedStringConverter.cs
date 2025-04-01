using System;
using System.Globalization;
using System.Windows.Data;

using dosymep.SimpleServices;

namespace RevitCreateViewSheet.Resources {
    [ValueConversion(typeof(bool), typeof(string))]
    internal class BoolToLocalizedStringConverter : IValueConverter {
        private readonly ILocalizationService _localizationService;

        public BoolToLocalizedStringConverter(ILocalizationService localizationService) {
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is bool boolValue) {
                return boolValue
                    ? _localizationService.GetLocalizedString("EntityState.Exist")
                    : _localizationService.GetLocalizedString("EntityState.New");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
