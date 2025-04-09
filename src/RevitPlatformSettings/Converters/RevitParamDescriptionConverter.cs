using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;

namespace RevitPlatformSettings.Converters;

internal sealed class RevitParamDescriptionConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return string.Join(" | ", GetValues(value).Where(item => !string.IsNullOrEmpty(item)));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

    private static IEnumerable<string> GetValues(object value) {
        if(value is RevitParam revitParam) {
            if(value is SharedParam sharedParam) {
                yield return sharedParam.Guid.ToString();
            }
            
            yield return revitParam.StorageType.ToString();
#if REVIT_2020
            yield return revitParam.UnitType.ToString();
#else
            yield return revitParam.UnitType == ForgeTypeIdExtensions.EmptyForgeTypeId
                ? "Empty"
                : revitParam.UnitType.TypeId;
#endif

            yield return revitParam.Description;
        }
    }
}
