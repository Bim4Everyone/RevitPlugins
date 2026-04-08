using System;
using System.Globalization;

using Autodesk.Revit.DB;

namespace RevitSuperfilter.Extensions;

internal static class ElementExtensions {
    public static string GetValueOrDefault(this Parameter parameter, IFormatProvider formatProvider = null) {
        string value = parameter.AsValueString();
        if(!string.IsNullOrEmpty(value)) {
            return value;
        }
        
        formatProvider ??= CultureInfo.CurrentCulture;

        return parameter.StorageType switch {
            StorageType.Integer => parameter.AsInteger().ToString(formatProvider),
            StorageType.Double => parameter.AsDouble().ToString(formatProvider),
            StorageType.String => parameter.AsString(),
            StorageType.ElementId => parameter.AsValueString(),
            _ => null
        };
    }
}
