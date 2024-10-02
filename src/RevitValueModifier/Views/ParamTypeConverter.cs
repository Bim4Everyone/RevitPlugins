using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xml.Linq;

using dosymep.SimpleServices;

using RevitValueModifier.Models;

namespace RevitValueModifier.Views {
    public class ParamTypeConverter : MarkupExtension, IValueConverter {
        public ILocalizationService LocalizationService { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var paramType = (RevitParamType)value;
            if(LocalizationService is null) {
                return paramType.ToString();
            }
            switch(paramType) {
                case RevitParamType.SystemParameter:
                    return LocalizationService.GetLocalizedString("MainWindow.SystemParameter");
                case RevitParamType.SharedParameter:
                    return LocalizationService.GetLocalizedString("MainWindow.SharedParameter");
                case RevitParamType.ProjectParameter:
                    return LocalizationService.GetLocalizedString("MainWindow.ProjectParameter");
                default:
                    return paramType.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }
}
