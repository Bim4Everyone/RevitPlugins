using System;
using System.Globalization;
using System.Windows.Data;

using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SystemParams;

using Wpf.Ui.Controls;

namespace RevitPlatformSettings.Converters;

internal sealed class RevitParamImageConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if(value is SharedParam) {
            return SymbolRegular.ShareAndroid24;
        } else if(value is ProjectParam) {
            return SymbolRegular.DocumentFlowchart24;
        } else if(value is SystemParam) {
            return SymbolRegular.System24;
        }

        return default;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
